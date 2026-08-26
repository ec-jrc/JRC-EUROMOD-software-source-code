"""Sphinx directive rendering the methodology registry.

The real documentation of each methodology is the ``description`` string on its
``MethodSpec`` — 50-odd lines of prose stating what the transformation does and
what it assumes. Copying that into a markdown page would create two versions of
the same claim, and the copy would be the one people read after it went stale.

So the page renders from the registry instead. ``.. method-reference::`` walks
``registry.list_specs()`` at build time and emits a section per methodology with
its description, the shock channels and metrics it consumes, what it needs from
the dataset and from the model, and its scenario params. Editing a method
updates the docs.

This is only possible because importing ``euromod_linking`` does not touch .NET:
the package's own ``euromod`` import is lazy, inside ``session.get_model``. The
docs therefore build on a runner with no EUROMOD installation.

Usage in a MyST page::

    ```{eval-rst}
    .. method-reference::
    ```

Take one method by naming it, and drop the parts a hand-written page already
covers::

    ```{eval-rst}
    .. method-reference:: lma_labour_alignment
       :no-description:
       :no-title:
    ```
"""

from docutils import nodes
from docutils.statemachine import StringList
from sphinx.util.docutils import SphinxDirective
from sphinx.util.nodes import nested_parse_with_titles

__version__ = "1.0"


def _rows(spec):
    """The spec's fields as (label, value) pairs, skipping what is empty.

    Order is deliberate: what the methodology consumes, then what it demands of
    the inputs, then what it does to them."""
    addon_entries, switch_entries = spec.addon_requirements or ((), ())
    addons = [" / ".join(str(x) for x in entry) if isinstance(entry, (list, tuple)) else str(entry)
              for entry in addon_entries]
    switches = [f"``{e[0]}`` = {bool(e[1])}" if isinstance(e, (list, tuple)) else f"``{e}``"
                for e in switch_entries]

    def code_list(values):
        return ", ".join(f"``{v}``" for v in values)

    rows = [
        ("Shock channels", code_list(spec.channels_consumed)),
        ("Metrics", code_list(spec.metrics_consumed) if spec.metrics_consumed
         else "any input variable or income list"),
        ("Population cells", spec.cell_variables),
        ("Required input columns", code_list(spec.dataset_requirements)),
        ("Required add-ons", ", ".join(f"``{a}``" for a in addons)),
        ("Required extensions", ", ".join(switches)),
        ("Earliest EUROMOD release", f"``{spec.min_model_release}``"
         if spec.min_model_release else ""),
        ("Columns added to the input", code_list(spec.injected_columns)),
        ("Restructures rows", "yes — the baseline is rebuilt on the same rows so the two "
                              "runs stay observation-paired" if spec.restructures_rows else ""),
    ]
    return [(label, value) for label, value in rows if value]


def _params(spec):
    """Scenario ``params`` as (name, requiredness, description) triples."""
    schema = spec.params_schema or {}
    required = set(schema.get("required") or ())
    out = []
    for name, sub in (schema.get("properties") or {}).items():
        bits = [str(sub.get("description") or "")]
        if "default" in sub:
            bits.append(f"Default ``{sub['default']}``.")
        out.append((name, "required" if name in required else "optional", " ".join(bits).strip()))
    return out


def _flag(argument):
    """docutils option validator for a valueless flag."""
    return True


class IncomeLists(SphinxDirective):
    """Render the ``ils_udb_*`` catalogue as a table, from the package's own
    ``INCOME_LISTS``, so the documented names cannot drift from the shipped ones.

    Usage::

        ```{eval-rst}
        .. income-lists::
        ```
    """

    has_content = False
    required_arguments = 0

    def run(self):
        from euromod_linking.methods.scale_variables import INCOME_LISTS

        lines = []
        for group, entries in INCOME_LISTS.items():
            scalable = "scalable" in group
            lines += [f"**{group[0].upper()}{group[1:]}**", ""]
            lines += [".. list-table::", "   :header-rows: 1", "   :widths: 24 56 20", ""]
            lines += ["   * - Income list", "     - Concept", "     - Scaling it"]
            for name, concept in entries.items():
                effect = ("Scales the recorded input variables"
                          if scalable else "Mostly recomputed — see below")
                lines += [f"   * - ``{name}``", f"     - {concept}", f"     - {effect}"]
            lines += [""]

        node = nodes.section()
        node.document = self.state.document
        nested_parse_with_titles(self.state, StringList(lines), node)
        return node.children


class MethodReference(SphinxDirective):
    """Render registered methodologies as reST.

    With no argument, every registered method. With one, just that method —
    ``lma_labour_alignment`` or the bare name for the highest version."""

    has_content = False
    required_arguments = 0
    optional_arguments = 1
    final_argument_whitespace = False
    option_spec = {"no-description": _flag, "no-title": _flag, "no-params": _flag}

    def run(self):
        from euromod_linking import registry

        if self.arguments:
            specs = [registry.resolve(self.arguments[0])]
        else:
            specs = registry.list_specs()

        lines = []
        for spec in specs:
            if "no-title" not in self.options:
                title = spec.name
                lines += [f".. _methodology-{spec.name}:", ""]
                lines += [title, "-" * len(title), ""]
                lines += [spec.summary, ""]

            for label, value in _rows(spec):
                lines += [f":{label}: {value}"]
            lines += [""]

            if "no-description" not in self.options:
                lines += ["**What it does**", "", spec.description, ""]

            params = _params(spec)
            if params and "no-params" not in self.options:
                lines += ["**Scenario params**", ""]
                for name, req, description in params:
                    lines += [f"``{name}`` ({req})", f"   {description}" if description else "   —", ""]

        node = nodes.section()
        node.document = self.state.document
        # nested_parse_with_titles, not nested_parse: the generated content has
        # a section per methodology, and plain nested parsing rejects titles.
        nested_parse_with_titles(self.state, StringList(lines), node)
        return node.children


def setup(app):
    app.add_directive("method-reference", MethodReference)
    app.add_directive("income-lists", IncomeLists)
    return {"version": __version__, "parallel_read_safe": True, "parallel_write_safe": True}
