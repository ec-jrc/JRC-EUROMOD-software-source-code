"""
CalibrationOrganiser — hierarchical storage and manifest for Calibration objects.

Organises calibrations by **country** and **year** into a folder tree::

    root/
    ├── manifest.json
    ├── BE/
    │   └── 2022/
    │       ├── HSV_taxben.json
    │       └── HSV_tax.json
    └── AT/
        └── 2022/
            └── HSV_taxben.json

The manifest is a self-contained JSON that lists every calibration with its
estimates, labels, and metadata so users can browse results without loading
individual files.
"""

from __future__ import annotations

import json
import os
from datetime import datetime, timezone
from typing import Iterator

from .calibrate import Calibration


# ── helpers ──────────────────────────────────────────────────────────────────

def _rel_path(country: str, year: int, name: str) -> str:
    """Build the relative JSON path for a calibration entry."""
    return f"{country}/{year}/{name}.json"


def _manifest_entry(
    cal: Calibration,
    country: str,
    year: int,
    name: str,
) -> dict:
    """Build a rich manifest record from a Calibration."""
    return {
        "country": country,
        "year": year,
        "name": name,
        "path": _rel_path(country, year, name),
        "system": cal.system,
        "model_version": cal.model_version,
        "dataset_id": cal.dataset_id,
        "functional_form": str(cal.functional_form),
        "function_type": cal.function_type,
        "target_variable": cal.target_variable,
        "data_vars": cal.data_vars,
        "parameter_vars": cal.parameter_vars,
        "estimates": cal.estimates,
        "variable_labels": cal.variable_labels,
        "group_filters": cal.group_filters,
    }


# ── Country/Year view helpers ───────────────────────────────────────────────

class _YearView:
    """
    Dict-like view into a single country-year bucket.

    Supports ``view["HSV_taxben"]`` to retrieve (and lazily load) a
    :class:`Calibration`.
    """

    def __init__(self, organiser: "CalibrationOrganiser", country: str, year: int):
        self._org = organiser
        self._country = country
        self._year = year

    def __getitem__(self, name: str) -> Calibration:
        return self._org.get(self._country, self._year, name)

    def __contains__(self, name: str) -> bool:
        store = self._org._store
        return (
            self._country in store
            and self._year in store[self._country]
            and name in store[self._country][self._year]
        )

    def keys(self) -> list[str]:
        return self._org.names(self._country, self._year)

    def __repr__(self) -> str:
        names = self.keys()
        lines = [
            f"CalibrationOrganiser  {self._country} / {self._year}",
            f"{len(names)} calibration(s) available:",
            "",
        ]
        for nm in names:
            entry = self._org._manifest[self._country][self._year][nm]
            form = entry.get("functional_form", "?")
            target = entry.get("target_variable", "?")
            lines.append(f"  • \"{nm}\"")
            lines.append(f"      target = {target}")
            lines.append(f"      form   = {form}")
            est = entry.get("estimates", {})
            for grp, params in est.items():
                pstr = ", ".join(f"{k}={v:.4f}" for k, v in params.items())
                lines.append(f"      [{grp}] {pstr}")
            labels = entry.get("variable_labels", {})
            if labels:
                lstr = ", ".join(f"{k}: {v}" for k, v in labels.items())
                lines.append(f"      labels: {lstr}")
        lines.append("")
        lines.append("Usage:")
        lines.append(f'  cal = org["{self._country}"][{self._year}]["<name>"]')
        return "\n".join(lines)


class _CountryView:
    """
    Dict-like view into a single country.

    Supports ``view[2022]`` to get a :class:`_YearView` and
    ``view.years()`` to list available years.
    """

    def __init__(self, organiser: "CalibrationOrganiser", country: str):
        self._org = organiser
        self._country = country

    def __getitem__(self, year: int) -> _YearView:
        store = self._org._store
        if self._country not in store or year not in store[self._country]:
            raise KeyError(
                f"No calibrations for {self._country}/{year}. "
                f"Available years: {self.years()}"
            )
        return _YearView(self._org, self._country, year)

    def __contains__(self, year: int) -> bool:
        store = self._org._store
        return self._country in store and year in store[self._country]

    def years(self) -> list[int]:
        return self._org.years(self._country)

    def __repr__(self) -> str:
        yrs = self.years()
        total = sum(
            len(self._org.names(self._country, y)) for y in yrs
        )
        lines = [
            f"CalibrationOrganiser  {self._country}",
            f"{len(yrs)} year(s), {total} calibration(s) total",
            "",
        ]
        for y in yrs:
            names = self._org.names(self._country, y)
            lines.append(f"  {y}  ({len(names)}):  {', '.join(names)}")
        lines.append("")
        lines.append("Usage:")
        lines.append(f'  org["{self._country}"][<year>]          '
                     f'→ list calibrations for that year')
        lines.append(f'  org["{self._country}"][<year>]["<name>"] '
                     f'→ load a specific calibration')
        return "\n".join(lines)


# ── Main class ───────────────────────────────────────────────────────────────

class CalibrationOrganiser:
    """
    Hierarchical container that tracks :class:`Calibration` objects by
    *country*, *year*, and *name*.

    Calibrations live in memory (after :meth:`add`) and can be exported to
    a structured folder tree with :meth:`export`.  A manifest JSON is
    written alongside the calibration files so that users can browse
    available results without loading every file.

    Loading from disk is **lazy**: :meth:`from_folder` reads only the
    manifest; individual ``Calibration`` objects are instantiated on the
    first call to :meth:`get` (or the ``[]`` accessor).

    Examples
    --------
    >>> org = CalibrationOrganiser()
    >>> org.add(cal, country="BE", year=2022, name="HSV_taxben")
    >>> org.add(cal2, country="BE", year=2022, name="HSV_tax")
    >>> org.export("calibrations")
    # writes calibrations/manifest.json, calibrations/BE/2022/*.json

    >>> org2 = CalibrationOrganiser.from_folder("calibrations")
    >>> org2["BE"][2022]["HSV_taxben"]          # lazy-loads the JSON
    >>> org2.list_calibrations(country="BE")    # no disk I/O
    """

    # ── construction ────────────────────────────────────────────────────

    def __init__(self) -> None:
        # _store[country][year][name] → Calibration | None (None = lazy)
        self._store: dict[str, dict[int, dict[str, Calibration | None]]] = {}
        # _manifest mirrors _store but keeps the rich dict entries
        self._manifest: dict[str, dict[int, dict[str, dict]]] = {}
        # root folder, set by from_folder() for lazy loading
        self._root: str | None = None

    # ── add / remove ────────────────────────────────────────────────────

    def add(
        self,
        cal: Calibration,
        *,
        country: str,
        year: int,
        name: str,
    ) -> None:
        """
        Register a calibration in the organiser.

        Parameters
        ----------
        cal : Calibration
            A fitted calibration object.
        country : str
            ISO-2 country code (e.g. ``"BE"``, ``"AT"``).
        year : int
            Policy year.
        name : str
            Short descriptive name (e.g. ``"HSV_taxben"``).
            Used as the JSON filename stem.

        Raises
        ------
        ValueError
            If an entry with the same (country, year, name) already exists.
        """
        bucket = self._store.setdefault(country, {}).setdefault(year, {})
        if name in bucket:
            raise ValueError(
                f"Calibration '{name}' already exists for {country}/{year}. "
                f"Use remove() first to replace it."
            )
        bucket[name] = cal
        entry = _manifest_entry(cal, country, year, name)
        self._manifest.setdefault(country, {}).setdefault(year, {})[name] = entry

    def remove(self, country: str, year: int, name: str) -> None:
        """Remove a calibration entry.  Raises ``KeyError`` if not found."""
        try:
            del self._store[country][year][name]
            del self._manifest[country][year][name]
        except KeyError:
            raise KeyError(
                f"No calibration '{name}' for {country}/{year}."
            ) from None
        # tidy up empty nesting
        if not self._store[country][year]:
            del self._store[country][year]
            del self._manifest[country][year]
        if not self._store[country]:
            del self._store[country]
            del self._manifest[country]

    # ── access ──────────────────────────────────────────────────────────

    def get(self, country: str, year: int, name: str) -> Calibration:
        """
        Retrieve a calibration, loading it lazily from disk if needed.

        Raises ``KeyError`` if the (country, year, name) triple is unknown.
        """
        try:
            cal = self._store[country][year][name]
        except KeyError:
            raise KeyError(
                f"No calibration '{name}' for {country}/{year}."
            ) from None

        if cal is None:
            # Lazy load from disk
            if self._root is None:
                raise RuntimeError(
                    f"Calibration '{name}' ({country}/{year}) has not been "
                    f"loaded and no root folder is set for lazy loading."
                )
            entry = self._manifest[country][year][name]
            path = os.path.join(self._root, entry["path"])
            cal = Calibration.from_json(None, path)
            self._store[country][year][name] = cal
        return cal

    def __getitem__(self, country: str) -> _CountryView:
        """``organiser["BE"]`` → :class:`_CountryView`."""
        if country not in self._store:
            raise KeyError(
                f"Country '{country}' not in organiser. "
                f"Available: {self.countries}"
            )
        return _CountryView(self, country)

    # ── enumeration ─────────────────────────────────────────────────────

    @property
    def countries(self) -> list[str]:
        """Sorted list of country codes present in the organiser."""
        return sorted(self._store.keys())

    def years(self, country: str) -> list[int]:
        """Sorted list of years available for *country*."""
        try:
            return sorted(self._store[country].keys())
        except KeyError:
            return []

    def names(self, country: str, year: int) -> list[str]:
        """Sorted list of calibration names for *country* / *year*."""
        try:
            return sorted(self._store[country][year].keys())
        except KeyError:
            return []

    def list_calibrations(
        self,
        country: str | None = None,
        year: int | None = None,
    ) -> list[dict]:
        """
        Return manifest entries, optionally filtered.

        Each entry is a dict with keys: ``country``, ``year``, ``name``,
        ``path``, ``system``, ``functional_form``, ``function_type``,
        ``target_variable``, ``data_vars``, ``parameter_vars``,
        ``estimates``, ``variable_labels``, ``group_filters``.

        No disk I/O is performed.
        """
        results: list[dict] = []
        for cty, year_dict in self._manifest.items():
            if country is not None and cty != country:
                continue
            for yr, name_dict in year_dict.items():
                if year is not None and yr != year:
                    continue
                results.extend(name_dict.values())
        return results

    def __len__(self) -> int:
        return sum(
            len(names)
            for years in self._store.values()
            for names in years.values()
        )

    def __iter__(self) -> Iterator[tuple[str, int, str]]:
        """Iterate over ``(country, year, name)`` triples."""
        for country in sorted(self._store):
            for year in sorted(self._store[country]):
                for name in sorted(self._store[country][year]):
                    yield country, year, name

    # ── export ──────────────────────────────────────────────────────────

    def export(self, root: str) -> str:
        """
        Write all calibrations and the manifest to disk.

        Creates the folder tree ``root/country/year/name.json`` and
        writes ``root/manifest.json``.

        Parameters
        ----------
        root : str
            Root directory.  Created if it does not exist.

        Returns
        -------
        str
            Absolute path to the written ``manifest.json``.
        """
        root = os.path.abspath(root)
        now = datetime.now(timezone.utc).isoformat(timespec="seconds")

        # Write individual calibration files
        for country, year, name in self:
            cal = self._store[country][year][name]
            if cal is None:
                # Lazy entry — re-export the original file from the manifest
                # without loading; simply copy the data we already have.
                # But to be safe, we load it to ensure a clean round-trip.
                cal = self.get(country, year, name)

            folder = os.path.join(root, country, str(year))
            os.makedirs(folder, exist_ok=True)
            cal.to_json(os.path.join(folder, f"{name}.json"))

        # Build flat manifest list
        entries = self.list_calibrations()

        manifest = {
            "created": now,
            "updated": now,
            "count": len(entries),
            "calibrations": entries,
        }

        # If a manifest already exists, preserve the original 'created' stamp
        manifest_path = os.path.join(root, "manifest.json")
        if os.path.isfile(manifest_path):
            try:
                with open(manifest_path, "r") as f:
                    old = json.load(f)
                manifest["created"] = old.get("created", now)
            except (json.JSONDecodeError, OSError):
                pass

        with open(manifest_path, "w") as f:
            json.dump(manifest, f, indent=2)

        self._root = root
        return manifest_path

    # ── import ──────────────────────────────────────────────────────────

    @classmethod
    def from_folder(cls, root: str) -> "CalibrationOrganiser":
        """
        Load an organiser from a manifest on disk.

        Only the manifest is read; individual ``Calibration`` objects are
        loaded lazily on first access via :meth:`get` or the ``[]``
        accessor.

        Parameters
        ----------
        root : str
            Folder containing ``manifest.json`` and the country/year
            sub-folders.

        Returns
        -------
        CalibrationOrganiser
        """
        root = os.path.abspath(root)
        manifest_path = os.path.join(root, "manifest.json")
        with open(manifest_path, "r") as f:
            manifest_data = json.load(f)

        org = cls()
        org._root = root

        for entry in manifest_data.get("calibrations", []):
            country = entry["country"]
            year = int(entry["year"])
            name = entry["name"]

            # Populate store with None (lazy placeholder)
            org._store.setdefault(country, {}).setdefault(year, {})[name] = None
            # Store the manifest entry for list_calibrations() and lazy loading
            org._manifest.setdefault(country, {}).setdefault(year, {})[name] = entry

        return org

    # ── display ─────────────────────────────────────────────────────────

    def __repr__(self) -> str:
        n = len(self)
        if n == 0:
            return "CalibrationOrganiser(empty)"

        lines = [f"CalibrationOrganiser  ({n} calibration(s))"]
        lines.append("-" * 80)
        lines.append(
            f"{'Country':<10} {'Year':<6} {'Name':<25} "
            f"{'Type':<20} {'Target'}"
        )
        lines.append("-" * 80)

        for country, year, name in self:
            entry = self._manifest[country][year][name]
            ftype = entry.get("function_type", "?")
            target = entry.get("target_variable", "?")
            lines.append(
                f"{country:<10} {year:<6} {name:<25} {ftype:<20} {target}"
            )

        lines.append("-" * 80)
        lines.append("")
        lines.append("Usage:")
        lines.append('  org["<country>"]                        '
                     '→ browse years for a country')
        lines.append('  org["<country>"][<year>]                '
                     '→ list calibrations for that year')
        lines.append('  org["<country>"][<year>]["<name>"]       '
                     '→ load a specific calibration')
        lines.append('  org.list_calibrations(country=, year=)  '
                     '→ filter manifest entries (no disk I/O)')
        return "\n".join(lines)
