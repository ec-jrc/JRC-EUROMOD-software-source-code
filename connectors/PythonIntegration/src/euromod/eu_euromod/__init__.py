"""
Tools that are specific to the JRC's EUROMOD model of the 27 EU member states.

The rest of the ``euromod`` package is a connector to the EUROMOD *software*: it
works with any model built for that software. The modules in this package are
different. They rely on the conventions of one model, the EU model maintained by
the Joint Research Centre: its income lists (``ils_dispy``, ``ils_origrepy``, ...),
its variable names (``dwt``, ``dms``, ``yse``, ...) and its system names
(``PT_2022``). They will not work on a model that does not follow these
conventions, and each module states exactly what it needs.

This package is not imported by ``euromod`` itself. Import what you need, e.g.::

    from euromod.eu_euromod.calibrate import prepare_sample, fit_hsv

Modules
-------
calibrate
    Calibrating a parametric tax-benefit function on EUROMOD output.
"""

__all__ = ["EU27", "EURO_AREA_19", "EURO_AREA_20", "euro_area", "eu27_systems"]

#: Country codes of the 27 EU member states, as used in the EU model.
EU27 = ("AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "EL", "ES", "FI", "FR",
        "HR", "HU", "IE", "IT", "LT", "LU", "LV", "MT", "NL", "PL", "PT", "RO",
        "SE", "SI", "SK")

#: Euro-area members up to 2022. Croatia joined in 2023.
EURO_AREA_19 = ("AT", "BE", "CY", "DE", "EE", "EL", "ES", "FI", "FR", "IE", "IT",
                "LT", "LU", "LV", "MT", "NL", "PT", "SI", "SK")

#: Euro-area members from 2023.
EURO_AREA_20 = ("AT", "BE", "CY", "DE", "EE", "EL", "ES", "FI", "FR", "HR", "IE",
                "IT", "LT", "LU", "LV", "MT", "NL", "PT", "SI", "SK")


def euro_area(year):
    """
    Euro-area members in a policy year (from 2015, when Lithuania joined).

    Parameters
    ----------
    year : :obj:`int`
        Policy year.

    Returns
    -------
    tuple of :obj:`str`
        :data:`EURO_AREA_19` up to 2022, :data:`EURO_AREA_20` from 2023.
    """
    if year < 2015:
        raise ValueError("euro_area() knows the members from 2015 on.")
    return EURO_AREA_19 if year <= 2022 else EURO_AREA_20


def eu27_systems(year):
    """
    Baseline system names of the 27 member states for one policy year.

    Parameters
    ----------
    year : :obj:`int`
        Policy year.

    Returns
    -------
    list of :obj:`str`
        ``["AT_2022", "BE_2022", ..., "SK_2022"]`` for ``year=2022``.
    """
    return [f"{country}_{year}" for country in EU27]
