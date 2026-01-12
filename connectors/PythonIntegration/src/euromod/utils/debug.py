import pandas as pd
def create_debug_df(df,nth_person=0 ,var=None,values=[]):
    """
    Copies a DataFrame len(values) times and shifts the specified ID columns by a large constant to make them unique.
    
    Parameters:
    - df: The original pandas DataFrame.
    - N: The number of times to copy the DataFrame.
    - input_ids: The list of column names that need to have unique IDs.
    - shift_constant: The constant used to shift the ID values.
    
    Returns:
    - A new DataFrame with N copies of the original, with unique IDs.
    """

    
    input_ids = ['idhh','idperson','idfather'	,'idmother','idpartner','idorighh','idorigperson']
    copies = []
    shift_constant = int(df.idperson.max()) +1
    for i in range(len(values)):
        # Create a copy of the original DataFrame
        df_copy = df.copy()

        # Shift the ID columns by a constant times the copy index to ensure uniqueness
        for id_col in input_ids:
            if id_col in df_copy.columns:
                df_copy[id_col] += shift_constant * i*(df_copy[id_col] > 0)
        df_copy.loc[nth_person,var] = values[i]
        # Append the modified copy to the list
        copies.append(df_copy)

    # Concatenate all the copies into a single DataFrame
    result_df = pd.concat(copies, ignore_index=True)

    return result_df


