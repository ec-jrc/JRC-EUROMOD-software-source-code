from core import System,Model

import os
import polars as pl
import sympy 
from typing import Dict,List
import numba as nb
import json
import scipy as sp
import numpy as np

class Calibration:
    def __init__(self,model_version, system: str, dataset_id: str,
                 functional_form: sympy.Expr,
                 data_vars: List[str], parameter_vars: List[str],
                 estimates: Dict[str,Dict[str,float]], target_variable,constantsToOverwrite: dict = {}):
        self.system = system
        self.model_version = model_version
        self.target_variable = target_variable
        self.dataset_id = dataset_id
        self.constantsToOverwrite = constantsToOverwrite
        self.data_vars = data_vars
        self.parameter_vars = parameter_vars
        self.functional_form = functional_form
        self.pretty_function = dict()
        for k,v in estimates.items(): 
            repl_dict = {sympy.Symbol(param): round(float(value), 4) for param, value in v.items()}
            repl_form = self.functional_form.xreplace(repl_dict)
            self.pretty_function[k] = repl_form
        self.estimates = estimates #group dependent parameter estimates
        # Create the base lambda function from sympy
        all_vars = self.data_vars + self.parameter_vars
        self.base_lambda = sympy.lambdify(all_vars, self.functional_form, 'numpy')
        
        # Try to create numba-compiled version if possible
        try:
            # Create a wrapper function that numba can compile
            self.numba_function = nb.njit(self.base_lambda)
        except Exception:
            # Fall back to regular lambda if numba compilation fails
            self.numba_function = self.base_lambda  

        
        # Generate group-specific lambda functions with proper closure
        self.group_lambdas = {}
        for group_key, param_estimates in self.estimates.items():
            # Create a lambda that captures the parameter estimates for this group
            param_values = [param_estimates[param] for param in self.parameter_vars]
            # Use proper closure to avoid late binding issues
            self.group_lambdas[group_key] = (lambda params: lambda *data_args: self.numba_function(*data_args, *params))(param_values)

    def get_value_function(self,group_key: str,data : Dict[str,np.ndarray]):
        group_lambda = self.group_lambdas.get(group_key)
        data_listed = [data[x] for x in self.data_vars]
        if group_lambda is not None:
            return lambda *args: group_lambda(*data_listed)
        raise ValueError(f"No value function found for group: {group_key}")

    def to_json(self,path: str):
        with open(path,"w") as json_file:
            json.dump({
                "model_version": self.model_version,
                "system": self.system,
                "dataset_id": self.dataset_id,
                "functional_form": str(self.functional_form),
                "constantsToOverwrite": self.constantsToOverwrite,
                "parameter_vars": self.parameter_vars,
                "data_vars": self.data_vars,
                "estimates": self.estimates 
            }, json_file)
    @staticmethod
    def from_json(system: System,path: str):  
        with open(path,"r") as json_file:
            data = json.load(json_file)
            functional_form = sympy.sympify(data["functional_form"])
            return Calibration(
                data["model_version"],
                data["system"],
                data["dataset_id"],
                functional_form,
                data["data_vars"],
                data["parameter_vars"],
                data["estimates"],
                data["constantsToOverwrite"]
            )
    def get_estimates(self):
        return self.estimates
    
    def __repr__(self):
        s = f"Callibration for system {self.system} using dataset {self.dataset_id}:\n"
        for k,v in self.pretty_function.items() :
            s += f"\t Group {k}: {v}\n"
        return s

    def evaluate(self,
                 system: System,
                 output_data: pl.DataFrame,
                 target_variable: str,
                 group_filters: dict[str, pl.Expr] | None = None,
                 dataset_id: str | None = None) -> tuple[pl.DataFrame, pl.DataFrame]:
        """
        Compare fitted vs actual target values per group.
        Returns (metrics_df, comparison_df).
        """
        if dataset_id is None:
            dataset_id = self.dataset_id
        if group_filters is None or group_filters == {}:
            # If no filters provided, evaluate on full sample for each calibrated group
            group_filters = {g: pl.lit(True) for g in self.estimates.keys()}

        

        if target_variable not in output_data.columns:
            raise ValueError(f"Target variable '{target_variable}' missing from simulation outputs.")

        # Ensure numeric types
        needed = set(self.data_vars + [target_variable])
        cast_exprs = [pl.col(c).cast(pl.Float64) for c in needed if c in output_data.columns]
        if cast_exprs:
            output_data = output_data.with_columns(cast_exprs)

        metrics_rows = []
        comparison_frames: list[pl.DataFrame] = []

        for group_name, filt in group_filters.items():
            if group_name not in self.estimates:
                continue  # Skip groups without calibrated params
            group_df = output_data.filter(filt)
            if group_df.height == 0:
                continue

            data_arrays = [group_df[v].to_numpy() for v in self.data_vars]
            # Use stored parameter order
            params = [self.estimates[group_name][p] for p in self.parameter_vars]

            try:
                fitted = self.numba_function(*data_arrays, *params)
            except Exception:
                # Fallback to base lambda if numba fails dynamically
                fitted = self.base_lambda(*data_arrays, *params)

            fitted = np.asarray(fitted, dtype=np.float64)
            actual = group_df[target_variable].to_numpy()
            residual = fitted - actual

            n = actual.size
            mse = float(np.mean(residual**2))
            rmse = float(np.sqrt(mse))
            mae = float(np.mean(np.abs(residual)))
            ss_res = float(np.sum(residual**2))
            ss_tot = float(np.sum((actual - actual.mean())**2))
            r2 = float(1 - ss_res / ss_tot) if ss_tot > 0 else float("nan")

            metrics_rows.append({
                "group": group_name,
                "n": n,
                "mse": mse,
                "rmse": rmse,
                "mae": mae,
                "r2": r2
            })

            comparison_frames.append(pl.DataFrame({
                "group": [group_name] * n,
                target_variable: actual,
                "fitted": fitted,
                "residual": residual
            }))

        metrics_df = pl.DataFrame(metrics_rows) if metrics_rows else pl.DataFrame({"group": [], "n": [], "mse": [], "rmse": [], "mae": [], "r2": []})
        comparison_df = pl.concat(comparison_frames, how="vertical") if comparison_frames else pl.DataFrame({"group": [], target_variable: [], "fitted": [], "residual": []})

        return metrics_df, comparison_df



def calibrate_function(system,data,dataset_id,target_variable,base,functional_form: sympy.Expr,group_filters: dict[str,pl.Expr]={},constantsToOverwrite: dict = {} ):
    """"
    Calibrate the functional form for the specified dataset and target variable.
    """
    simulation = system.run(data,dataset_id,constantsToOverwrite=constantsToOverwrite)
    output_data = simulation.outputs[0]
    # Validate functional form
    if not isinstance(functional_form, sympy.Expr):
        raise ValueError("functional_form must be a sympy expression")
    
    parameter_vars = [str(sym) for sym in functional_form.free_symbols if str(sym) not in output_data.columns]
    data_vars = [str(sym) for sym in functional_form.free_symbols if str(sym) in output_data.columns]

    fn_lambda = sympy.lambdify(data_vars + parameter_vars, functional_form)
    if group_filters == {}:
        group_filters = {"all": pl.lit(True)}

    numba_function = nb.njit(fn_lambda)
    def objective_function(params, group_data):
        # Compute the difference between the target variable and the functional form output
        data_listed = [group_data[var].to_numpy() for var in data_vars]

        model_output = numba_function(*data_listed ,  *params.tolist())
        target_values = group_data[target_variable].to_numpy()
        scale = len(target_values)*max(target_values)
        return np.sum((model_output - target_values)**2) / scale

    estimates = dict()
    for group_name,group_filter in group_filters.items():
        group_data = output_data.filter(group_filter)
      

        initial_params = np.ones(len(parameter_vars)) * 0.001

        # Use Nelder-Mead with strict tolerances
        result = sp.optimize.minimize(
            lambda params: objective_function(params, group_data),
            initial_params,
            method="Nelder-Mead",
            options={"maxiter": 10000}
        )

        if result.success:
            calibrated_params = dict(zip(parameter_vars, [float(x) for x in result.x]))
            estimates[group_name] = calibrated_params
        else:
            continue


    return Calibration(system.parent.model.model_path,system.name,dataset_id,functional_form,data_vars,parameter_vars,estimates,constantsToOverwrite), output_data

if __name__ == "__main__":
    model_path= r"R:\B2\04 - EUROMOD JRC\06 - Working area\01 - Common\15 - Release checks\2025Q1\model\EUROMOD_MASTER_VERSION_J0.51"
    mod = Model(model_path)
    repository = r"R:\B2\04 - EUROMOD JRC\01 - Repository\03 - Datasets\All data\All countries"
    #example calibration
    import pandas as pd
    dataset = mod["HU"][-1].bestmatch_datasets[0].name
    df_pandas = pd.read_csv(os.path.join(repository,dataset + ".txt"),sep="\t")
    df = pl.from_pandas(df_pandas)
    group_filters = {
        "single_person_hh": pl.col("idhh").count().over("idhh") == 1,
        "multi_person_hh": pl.col("idhh").count().over("idhh") > 1,
        "all": pl.lit(True)
    }
    calibration,output_data = calibrate_function(
        mod["HU"][-1],
        df,
        dataset,
        "ils_taxin",
        None,
        sympy.sympify("ils_base_tin - g*ils_base_tin**(1-tau)"),
        group_filters
    )
    print(calibration)

    evaluation = calibration.evaluate(
        mod["HU"][-1],
        output_data,
        "ils_taxin",
        group_filters
    )

    print(evaluation)

    calibration2,output_data = calibrate_function(
        mod["HU"][-1],
        df,
        dataset,
        "ils_taxin",
        None,
        sympy.sympify("3*a_3*(ils_base_tin/1000)**3 + 2*a_2*(ils_base_tin/1000)**2 + a_1*(ils_base_tin/1000) + a_0"),
        group_filters
    )
    print(calibration2)

    evaluation2 = calibration2.evaluate(
        mod["HU"][-1],
        output_data,
        "ils_taxin",
        group_filters
    )

    print(evaluation2)
