"""How run() reads its constantsToOverwrite argument. No model is needed."""
import pytest

from euromod.core import System


def convert(constants):
    return System._get_constantsToOverwrite(None, constants)


def test_name_and_group_pairs_are_accepted():
    converted = convert({("$f_h_cpi", "2022"): "1000", ("$tin_rate", ""): "0.5"})
    assert converted.Count == 2


@pytest.mark.parametrize("key", ["$tin_rate", ("$tin_rate",), ("$f_h_cpi", "2022", "x"), 5])
def test_a_key_that_is_not_a_pair_is_refused(key):
    # A plain string used to be taken apart into its first two characters, so the
    # constant was silently not overridden.
    with pytest.raises(TypeError, match="constantsToOverwrite"):
        convert({key: "0.5"})


def test_no_constants():
    assert convert(None) is None
