"""Tests for euromod.eu_euromod.calibrate, on synthetic data: no model is needed."""
import json

import numpy as np
import pandas as pd
import pytest

from euromod.eu_euromod import EU27, EURO_AREA_19, eu27_systems
from euromod.eu_euromod import calibrate
from euromod.eu_euromod.calibrate import (
    HOUSEHOLD_TYPES, benefit_ratio_by_quantile, binned_means, calibrate_hsv,
    family_composition, fit_diagnostics, fit_hsv, gini_coefficients, household_sample,
    hsv, income_shares, load_estimates, marginal_hsv, net_tax_rate_at_mean_income, pool,
    sample_characteristics, save_estimates, schedule, select_households, simulate,
    tax_rates_by_quantile, to_calibration)
from euromod.eu_euromod.calibrate.data_preparation import _dataset_file
from euromod.eu_euromod.calibrate.weighted import gini, weighted_mean, weighted_percentile

NO_TRIM = dict(trim=(0, 100))


# --- synthetic EUROMOD output -------------------------------------------------

def person(idhh, age, earnings, dms=1, yse=0, parent=0, dwt=100.0, benefit=0.0):
    """One individual with a 20% tax on earnings."""
    return {"idhh": idhh, "dag": age, "dms": dms, "dwt": dwt, "yse": yse,
            "idmother": parent, "idfather": 0,
            "ils_earns": earnings, "ils_origy": earnings, "ils_origrepy": earnings,
            "ils_taxsim": 0.2 * earnings, "ils_benmt": benefit, "ils_bennt": 0.0,
            "ils_dispy": 0.8 * earnings + benefit}


def output(*people):
    return pd.DataFrame(list(people))


def households(n=400, lambda_=0.2, tau=0.15, noise=0.0, seed=0):
    """A household sample that follows the HSV schedule, exactly when noise is 0."""
    rng = np.random.default_rng(seed)
    income = rng.lognormal(0, 0.6, n)
    dwt = rng.uniform(50, 150, n)
    income = income / weighted_mean(income, dwt)
    disposable = (1 - lambda_) * income ** (1 - tau) * np.exp(noise * rng.normal(size=n))
    married = rng.random(n) < 0.45
    return pd.DataFrame({
        "idhh": np.arange(n), "dwt": dwt, "hh_size": rng.integers(1, 5, n),
        "head_age": rng.integers(25, 61, n), "dms": np.where(married, 2, 1),
        "married": married, "n_children": rng.integers(0, 3, n),
        "ils_origrepy": 3000 * income, "ils_dispy": 3000 * disposable,
        "ils_benmt": 100 * rng.random(n),
        "income_norm": income, "disposable_norm": disposable,
        "net_tax_rate": 1 - disposable / income})


# --- data preparation ---------------------------------------------------------

class TestHouseholdSample:
    def test_sums_members_and_picks_the_top_earner_as_head(self):
        sample = household_sample(output(
            person(1, 38, 1000, dms=1), person(1, 40, 3000, dms=2),
            person(1, 10, 0, parent=5)), **NO_TRIM)
        row = sample.iloc[0]
        assert len(sample) == 1 and row["hh_size"] == 3
        assert row["ils_origrepy"] == 4000 and row["ils_dispy"] == 3200
        assert row["head_age"] == 40 and row["married"]
        assert row["n_children"] == 1

    def test_equal_earnings_go_to_the_older_member(self):
        sample = household_sample(output(person(1, 30, 2000, dms=1),
                                         person(1, 50, 2000, dms=2)), **NO_TRIM)
        assert sample.iloc[0]["head_age"] == 50

    def test_a_full_tie_goes_to_the_member_listed_first(self):
        first_single = output(person(1, 45, 1500, dms=1), person(1, 45, 1500, dms=2))
        first_married = output(person(1, 45, 1500, dms=2), person(1, 45, 1500, dms=1))
        assert not household_sample(first_single, **NO_TRIM).iloc[0]["married"]
        assert household_sample(first_married, **NO_TRIM).iloc[0]["married"]

    def test_head_age_window_includes_both_ends(self):
        sample = household_sample(output(person(1, 24, 2000), person(2, 25, 2000),
                                         person(3, 60, 2000), person(4, 61, 2000)), **NO_TRIM)
        assert list(sample["idhh"]) == [2, 3]
        wider = household_sample(output(person(1, 24, 2000), person(4, 61, 2000)),
                                 head_age=(20, 65), **NO_TRIM)
        assert list(wider["idhh"]) == [1, 4]

    def test_self_employed_households_are_dropped_unless_asked(self):
        data = output(person(1, 40, 2000), person(2, 40, 2000), person(2, 38, 500, yse=300))
        assert list(household_sample(data, **NO_TRIM)["idhh"]) == [1]
        kept = household_sample(data, drop_self_employed=False, **NO_TRIM)
        assert list(kept["idhh"]) == [1, 2]

    def test_children_need_a_parent_and_to_be_18_or_under(self):
        sample = household_sample(output(
            person(1, 45, 3000), person(1, 18, 0, parent=9), person(1, 19, 0, parent=9),
            person(1, 12, 0)), **NO_TRIM)
        assert sample.iloc[0]["n_children"] == 1

    def test_households_without_positive_income_are_dropped(self):
        sample = household_sample(output(person(1, 40, 2000), person(2, 40, 0)), **NO_TRIM)
        assert list(sample["idhh"]) == [1]

    def test_incomes_are_in_multiples_of_the_weighted_mean(self):
        sample = household_sample(output(person(1, 40, 1000, dwt=300), person(2, 40, 3000, dwt=100)),
                                  **NO_TRIM)
        assert sample.attrs["mean_income"] == pytest.approx(1500)
        assert list(sample["income_norm"]) == pytest.approx([1000 / 1500, 2.0])
        assert weighted_mean(sample["income_norm"], sample["dwt"]) == pytest.approx(1.0)
        assert list(sample["net_tax_rate"]) == pytest.approx([0.2, 0.2])
        assert list(sample["disposable_norm"]) == pytest.approx([800 / 1500, 2400 / 1500])

    def test_benefits_lower_the_net_tax_rate_but_not_the_tax_rate(self):
        sample = household_sample(output(person(1, 40, 1000, benefit=500)), **NO_TRIM)
        assert sample.iloc[0]["net_tax_rate"] == pytest.approx(1 - 1300 / 1000)
        assert sample.iloc[0]["tax_rate"] == pytest.approx(0.2)

    def test_trimming_drops_the_tails_before_the_mean_is_taken(self):
        data = output(*[person(i, 40, 1000 * i) for i in range(1, 11)])
        sample = household_sample(data, trim=(15, 85))
        assert list(sample["idhh"]) == [2, 3, 4, 5, 6, 7, 8, 9]
        assert sample.attrs["mean_income"] == pytest.approx(5500)

    def test_a_missing_variable_is_named(self):
        with pytest.raises(ValueError, match="yse"):
            household_sample(output(person(1, 40, 2000)).drop(columns="yse"))

    def test_disposable_income_before_employee_contributions(self):
        data = output(person(1, 40, 1000))
        with pytest.raises(ValueError, match="ils_sicee"):
            household_sample(data, disposable="ils_dispy_sicee", **NO_TRIM)
        sample = household_sample(data.assign(ils_sicee=100.0), disposable="ils_dispy_sicee",
                                  **NO_TRIM)
        assert sample.iloc[0]["net_tax_rate"] == pytest.approx(1 - 900 / 1000)


class TestSimulate:
    def test_dataset_is_found_flat_or_per_country(self, tmp_path):
        (tmp_path / "PT").mkdir()
        (tmp_path / "flat.txt").write_text("x")
        (tmp_path / "PT" / "nested.txt").write_text("x")
        assert _dataset_file(str(tmp_path), "PT", "flat").endswith("flat.txt")
        assert _dataset_file(str(tmp_path), "PT", "nested").endswith("nested.txt")
        with pytest.raises(FileNotFoundError, match="absent.txt"):
            _dataset_file(str(tmp_path), "PT", "absent")

    def test_cached_output_is_reused_without_loading_a_model(self, tmp_path):
        stored = pd.DataFrame({"idhh": [1, 2]})
        stored.to_pickle(tmp_path / "PT_2022.pkl")
        (tmp_path / "PT_2022.json").write_text(json.dumps(
            {"dataset": "PT_data", "constants": {"$rate|": "0.5"}, "addons": [], "switches": []}))
        reused = simulate("no/model", "no/data", "PT_2022", constants={"$rate": 0.5},
                          cache_dir=str(tmp_path))
        pd.testing.assert_frame_equal(reused, stored)

    def test_cache_made_with_other_constants_is_refused(self, tmp_path):
        pd.DataFrame({"idhh": [1]}).to_pickle(tmp_path / "PT_2022.pkl")
        (tmp_path / "PT_2022.json").write_text(json.dumps(
            {"dataset": "PT_data", "constants": {}, "addons": [], "switches": []}))
        with pytest.raises(ValueError, match="different settings"):
            simulate("no/model", "no/data", "PT_2022", constants={"$rate": 0.5},
                     cache_dir=str(tmp_path))


# --- estimation ---------------------------------------------------------------

class TestSchedule:
    def test_lambda_is_the_rate_at_mean_income(self):
        assert hsv(1.0, 0.2, 0.15) == pytest.approx(0.2)

    def test_tau_zero_is_a_flat_rate(self):
        assert hsv(np.array([0.5, 1, 3]), 0.2, 0.0) == pytest.approx([0.2, 0.2, 0.2])

    def test_marginal_rate_is_the_slope_of_the_tax_paid(self):
        income, step = 1.3, 1e-6
        tax = lambda y: y * hsv(y, 0.2, 0.15)
        slope = (tax(income + step) - tax(income - step)) / (2 * step)
        assert marginal_hsv(income, 0.2, 0.15) == pytest.approx(slope, rel=1e-6)


class TestFitHsv:
    def test_recovers_the_parameters_of_exact_data(self):
        fit = fit_hsv(households(lambda_=0.2, tau=0.15))
        assert fit["lambda"] == pytest.approx(0.2, abs=1e-12)
        assert fit["tau"] == pytest.approx(0.15, abs=1e-12)
        assert fit["r2"] == pytest.approx(1.0) and fit["n"] == 400

    def test_matches_weighted_least_squares_solved_directly(self):
        sample = households(noise=0.2)
        root_w = np.sqrt(sample["dwt"].to_numpy())
        design = np.column_stack([root_w, np.log(sample["income_norm"]) * root_w])
        intercept, slope = np.linalg.lstsq(
            design, np.log(sample["disposable_norm"]) * root_w, rcond=None)[0]
        fit = fit_hsv(sample)
        assert fit["lambda"] == pytest.approx(1 - np.exp(intercept), rel=1e-12)
        assert fit["tau"] == pytest.approx(1 - slope, rel=1e-12)

    def test_the_scale_of_the_weights_does_not_matter(self):
        sample = households(noise=0.2)
        assert fit_hsv(sample.assign(dwt=sample["dwt"] * 1000)) == pytest.approx(fit_hsv(sample))

    def test_standard_errors(self):
        fit = fit_hsv(households(noise=0.2))
        assert 0 < fit["se_tau"] < 0.05
        assert fit["se_lambda"] == pytest.approx((1 - fit["lambda"]) * fit["se_intercept"])

    def test_fixed_lambda_estimates_only_tau(self):
        fit = fit_hsv(households(lambda_=0.2, tau=0.15), lambda_fixed=0.2)
        assert fit["lambda"] == 0.2 and fit["tau"] == pytest.approx(0.15, abs=1e-12)
        assert fit["r2_uncentered"] == pytest.approx(1.0)
        assert "se_lambda" not in fit

    def test_a_wrong_fixed_lambda_moves_tau(self):
        sample = households(lambda_=0.2, tau=0.15, noise=0.1)
        assert fit_hsv(sample, lambda_fixed=0.3)["tau"] != pytest.approx(fit_hsv(sample)["tau"])


class TestHouseholdTypes:
    def test_married_and_single_split_the_sample(self):
        sample = households()
        married, single = (select_households(sample, t) for t in ("married", "single"))
        assert len(married) + len(single) == len(select_households(sample, "all")) == len(sample)
        parts = [select_households(sample, t) for t in HOUSEHOLD_TYPES if "children" in t]
        assert sum(len(part) for part in parts) == len(sample)

    def test_any_condition_can_be_used(self):
        sample = households()
        assert (select_households(sample, "hh_size == 1")["hh_size"] == 1).all()

    def test_pool_keeps_every_household_and_names_the_country(self):
        pooled = pool({"AT_2022": households(100, seed=1), "BE_2022": households(50, seed=2)})
        assert len(pooled) == 150
        assert pooled["country"].value_counts().to_dict() == {"AT": 100, "BE": 50}


class TestDiagnostics:
    def test_exact_data_fit_without_error(self):
        sample = households()
        result = fit_diagnostics(sample, fit_hsv(sample))
        assert result["rmse"] == pytest.approx(0, abs=1e-9)
        assert result["mae"] == pytest.approx(0, abs=1e-9)
        # a bin's mean rate is not the rate at its mean income, so these are small, not zero
        assert result["mae_binned"] <= result["rmse_binned"] < 0.005
        assert result["gini_fitted"] == pytest.approx(result["gini_disposable"])
        assert result["gini_disposable"] < result["gini_gross"]

    def test_net_tax_rate_at_mean_income_reads_the_schedule_at_one(self):
        sample = households(lambda_=0.2, tau=0.15, n=4000)
        assert net_tax_rate_at_mean_income(sample) == pytest.approx(0.2, abs=1e-3)

    def test_without_households_near_the_mean_the_closest_one_is_used(self):
        sample = households().query("income_norm > 1.5")
        closest = sample.loc[sample["income_norm"].idxmin(), "net_tax_rate"]
        assert net_tax_rate_at_mean_income(sample) == closest

    def test_bins_cover_every_household(self):
        sample = households()
        binned = binned_means(sample, bins=20)
        assert binned["n"].sum() == len(sample) and len(binned) <= 20
        assert binned["dwt_sum"].sum() == pytest.approx(sample["dwt"].sum())


class TestCalibrateHsv:
    @pytest.fixture(autouse=True)
    def synthetic_samples(self, monkeypatch):
        seeds = {system: seed for seed, system in enumerate(eu27_systems(2022))}
        monkeypatch.setattr(calibrate, "prepare_sample",
                            lambda model, data, system, **_: households(60, seed=seeds[system]))

    def test_one_row_per_system_and_household_type(self):
        table = calibrate_hsv("model", "data", ["AT_2022", "BE_2022"])
        assert len(table) == 2 * len(HOUSEHOLD_TYPES)
        assert table.loc[("AT_2022", "all"), "tau"] == pytest.approx(0.15)

    def test_pooled_group_needs_all_its_countries(self):
        with pytest.raises(ValueError, match="BE"):
            calibrate_hsv("model", "data", ["AT_2022"], pooled={"core": ("AT", "BE")})
        table = calibrate_hsv("model", "data", ["AT_2022", "BE_2022"], pooled={"core": ("AT", "BE")},
                              household_types={"all": None})
        assert table.loc[("core", "all"), "n"] == 120

    def test_euro_area_is_a_subset_of_the_union(self):
        assert set(EURO_AREA_19) < set(EU27) and len(EU27) == 27 and len(EURO_AREA_19) == 19


class TestEstimatesFiles:
    def test_a_table_of_estimates_survives_a_file(self, tmp_path, monkeypatch):
        monkeypatch.setattr(calibrate, "prepare_sample",
                            lambda model, data, system, **_: households(60, noise=0.1))
        table = calibrate_hsv("model", "data", ["AT_2022", "BE_2022"])
        path = str(tmp_path / "estimates.json")
        save_estimates(table, path, model="test", constants={"$rate": 0.5})
        loaded = load_estimates(path)
        pd.testing.assert_frame_equal(loaded, table, check_exact=True)
        assert loaded.attrs == {"model": "test", "constants": {"$rate": 0.5}}

    def test_the_published_estimates_ship_with_the_package(self):
        baseline = load_estimates()
        systems = baseline.index.get_level_values("system").unique()
        assert set(systems) == set(eu27_systems(2022)) | {"EA"}
        assert len(baseline) == 28 * len(HOUSEHOLD_TYPES)
        portugal = baseline.loc[("PT_2022", "all")]
        assert portugal["lambda"] == pytest.approx(0.1925, abs=5e-5)
        assert portugal["tau"] == pytest.approx(0.1631, abs=5e-5)
        assert baseline.attrs["policy_year"] == 2022
        assert baseline[["lambda", "tau", "se_tau", "r2", "n"]].notna().all().all()

    def test_a_row_of_estimates_becomes_a_schedule(self):
        baseline = load_estimates()
        row = baseline.loc[("PT_2022", "married")]
        married = schedule(baseline, "PT_2022", "married")
        assert married.net_tax_rate(1.0) == pytest.approx(row["lambda"])
        assert married([0.5, 2.0]) == pytest.approx(hsv([0.5, 2.0], row["lambda"], row["tau"]))
        assert schedule(baseline, "EA")(1.0) == pytest.approx(baseline.loc[("EA", "all"), "lambda"])

    def test_a_schedule_gives_taxes_and_incomes(self):
        flat = schedule(pd.DataFrame({"lambda": [0.2], "tau": [0.0]},
                                     index=pd.MultiIndex.from_tuples([("XX_2022", "all")])), "XX_2022")
        income = np.array([0.5, 1.0, 3.0])
        assert flat.net_taxes(income) == pytest.approx(0.2 * income)
        assert flat.disposable_income(income) == pytest.approx(0.8 * income)
        assert flat.marginal_rate(income) == pytest.approx([0.2, 0.2, 0.2])

        progressive = schedule(load_estimates(), "PT_2022")
        assert progressive.disposable_income(income) == pytest.approx(
            (1 - progressive.lambda_) * income ** (1 - progressive.tau))
        assert progressive.net_taxes(income) + progressive.disposable_income(income) == pytest.approx(income)


class TestToCalibration:
    def test_evaluates_like_the_schedule_and_survives_json(self, tmp_path):
        pytest.importorskip("sympy")
        pytest.importorskip("numba")
        from euromod.calibrate import Calibration

        sample = households(noise=0.1)
        fits = {name: fit_hsv(select_households(sample, name)) for name in ("all", "married")}
        constants = {"$rate": 0.5, ("$f_cpi", "2023"): 1.02}
        cal = to_calibration(fits, system="PT_2022", constants=constants)

        income = np.array([0.5, 1.0, 2.0])
        married = fits["married"]
        assert cal(income, "married") == pytest.approx(hsv(income, married["lambda"], married["tau"]))
        assert cal.group_filters["married"] == "SELECT * FROM self WHERE married"

        cal.to_json(str(tmp_path / "cal.json"))
        loaded = Calibration.from_json(None, str(tmp_path / "cal.json"))
        assert loaded.constantsToOverwrite == {("$rate", ""): "0.5", ("$f_cpi", "2023"): "1.02"}
        assert loaded(income) == pytest.approx(cal(income))

    def test_a_single_fit_becomes_the_group_all(self):
        pytest.importorskip("sympy")
        pytest.importorskip("numba")
        cal = to_calibration(fit_hsv(households()), system="PT_2022")
        assert list(cal.estimates) == ["all"]


# --- descriptives and weighted statistics -------------------------------------

class TestWeighted:
    def test_percentile_is_one_of_the_values(self):
        values, weights = np.array([10.0, 20.0, 30.0, 40.0]), np.array([1.0, 1.0, 1.0, 1.0])
        assert weighted_percentile(values, weights, 0) == 10
        assert weighted_percentile(values, weights, 50) == 20
        assert weighted_percentile(values, weights, 100) == 40
        assert weighted_percentile(values, np.array([1.0, 1.0, 1.0, 9.0]), 50) == 40

    def test_gini_of_equal_incomes_is_about_zero_and_rises_with_concentration(self):
        ones = np.ones(1000)
        assert gini(ones, ones) == pytest.approx(0, abs=1e-5)
        spread = np.arange(1, 1001, dtype=float)
        assert gini(spread, ones) == pytest.approx(1 / 3, abs=1e-3)
        assert gini(spread ** 3, ones) > gini(spread, ones)


class TestDescriptives:
    def test_sample_characteristics(self):
        sample = pd.DataFrame({"dwt": [1.0, 3.0], "head_age": [30, 50], "married": [True, False],
                               "n_children": [2, 0], "hh_size": [4, 1]})
        result = sample_characteristics(sample)
        assert result == {"n": 2, "head_age": 45.0, "share_married": 0.25,
                          "share_children": 0.25, "children_per_family": 2.0, "hh_size": 1.75}

    def test_income_shares_add_up_and_rise(self):
        shares = income_shares(households())
        assert shares.sum().to_dict() == pytest.approx({"gross": 100, "disposable": 100})
        assert shares["gross"].is_monotonic_increasing and len(shares) == 5

    def test_a_progressive_schedule_lowers_the_gini(self):
        result = gini_coefficients(households(tau=0.15))
        assert result["disposable"] < result["gross"]

    def test_tax_rates_by_decile(self):
        sample = households(tau=0.15)
        rates = tax_rates_by_quantile(sample)
        assert list(rates.columns) == list(HOUSEHOLD_TYPES) and len(rates) == 10
        assert rates["all"].is_monotonic_increasing
        overall = (rates["all"] * sample["dwt"].groupby(
            pd.qcut(sample["income_norm"], 10, labels=False)).sum().to_numpy()).sum()
        assert overall / sample["dwt"].sum() == pytest.approx(
            100 * weighted_mean(sample["net_tax_rate"], sample["dwt"]), rel=0.02)

    def test_benefit_ratio_total_is_the_weighted_mean(self):
        sample = households()
        table = benefit_ratio_by_quantile(sample)
        ratio = sample["ils_benmt"] / sample["ils_origrepy"]
        assert table.loc["all", "ratio"] == pytest.approx(100 * weighted_mean(ratio, sample["dwt"]))
        deciles = table.drop("all")
        assert (deciles["ratio"] * deciles["weight"]).sum() / deciles["weight"].sum() == \
            pytest.approx(table.loc["all", "ratio"])

    def test_family_composition(self):
        sample = pd.DataFrame({"dwt": [1.0, 1.0, 2.0], "married": [True, False, False],
                               "n_children": [1, 0, 1]})
        assert family_composition(sample) == {
            "children_among_single": pytest.approx(2 / 3), "children_among_married": 1.0,
            "single_among_children": pytest.approx(2 / 3),
            "married_among_children": pytest.approx(1 / 3)}


# --- against a real model -----------------------------------------------------

@pytest.mark.live
def test_baseline_and_reform_on_the_eu_model():
    """Needs EUROMOD_MODEL_PATH and EUROMOD_DATA_PATH (the folder with the input
    datasets). Raising Portugal's top income-tax rates must raise progressivity."""
    import os
    model, data = (os.environ.get(name, "").strip()
                   for name in ("EUROMOD_MODEL_PATH", "EUROMOD_DATA_PATH"))
    if not (model and data):
        pytest.skip("EUROMOD_MODEL_PATH and EUROMOD_DATA_PATH not set")
    from euromod.eu_euromod.calibrate import prepare_sample

    baseline = prepare_sample(model, data, "PT_2022")
    reform = prepare_sample(model, data, "PT_2022", constants={"$tin00_rate9": 0.58})
    fit = fit_hsv(baseline)
    assert 0 < fit["lambda"] < 0.5 and 0 < fit["tau"] < 0.5 and fit["r2"] > 0.8
    assert len(reform) == len(baseline)
    level = fit["lambda"]
    assert fit_hsv(reform, lambda_fixed=level)["tau"] > fit_hsv(baseline, lambda_fixed=level)["tau"]

    with pytest.raises(ValueError, match="PT_2022"):
        prepare_sample(model, data, "PT_1850")
