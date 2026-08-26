__license__='''
Copyright 2024 European Commission
*
Licensed under the EUPL, Version 1.2;
You may not use this work except in compliance with the Licence.
You may obtain a copy of the Licence at:

*
   https://joinup.ec.europa.eu/software/page/eupl
*

Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the Licence for the specific language governing permissions and limitations under the Licence.
'''

import clr
import os
import System as SystemCs
import polars as pl

_console_hardened = False


def harden_dotnet_console() -> None:
    """Detach the .NET Console from the process's console handle.

    The EUROMOD engine prints progress and also prints every warning to the
    Windows console. In a process without a valid console handle -- a service, a
    detached or background launch, some notebook kernels -- that write raises
    ``IOException: The handle is invalid``, turning a harmless engine warning
    into a failed simulation.

    Redirecting Console.Out/Error to ``TextWriter.Null`` removes the fragile
    channel. Nothing is lost: warnings are still delivered through the
    structured simulation result, which surfaces them as errors/warnings.

    Idempotent and best-effort. Set ``EUROMOD_KEEP_CONSOLE=1`` to skip, e.g.
    when watching engine progress interactively.
    """
    global _console_hardened
    if _console_hardened:
        return
    if os.environ.get("EUROMOD_KEEP_CONSOLE", "").strip().lower() in ("1", "true", "yes", "on"):
        _console_hardened = True
        return
    try:
        SystemCs.Console.SetOut(SystemCs.IO.TextWriter.Null)
        SystemCs.Console.SetError(SystemCs.IO.TextWriter.Null)
    except Exception:
        pass
    _console_hardened = True


def convert_list_of_str(list_to_convert):
    list_cs = SystemCs.Collections.Generic.List[SystemCs.String]()
    for s in list_to_convert:
        if type(s) != str:
            raise TypeError("type of elements of list must be a string")
        list_cs.Add(s)
    
    return list_cs
def is_iterable(variable):
    try:
        iter(variable)
        return True
    except TypeError:
        return False
def convert_numeric_columns_to_float64(df: pl.DataFrame) -> pl.DataFrame:
    # Create a list to hold the expressions to cast numeric columns to Float64
    cast_expressions = []

    for name, dtype in zip(df.columns, df.dtypes):
        # Check if the column is a numeric type (integer or float)
        if dtype in [pl.Int8, pl.Int16, pl.Int32, pl.Int64, pl.UInt8, pl.UInt16, pl.UInt32, pl.UInt64, pl.Float32, pl.Float64]:
            # Cast to Float64 and add to the list of expressions
            cast_expressions.append(df[name].cast(pl.Float64))
        

    # Select all columns with the cast expressions applied
    float_df = df.select(cast_expressions)
    return float_df