#pragma once
#define clrLibrary_API __declspec(dllimport)
#include <string>
#include <vector>
extern "C" clrLibrary_API int test_hannes();
clrLibrary_API int runEM(string system, string pathEM, string dataSetId, string& error_message, string pathData, string country, string pathOutput, double input_arr[], int length, vector<string> variables, vector<double*>& out, vector<vector<string>>& variables_out, string TU_output, string varsOutputRequested, string ILoutputRequested);