#pragma once
public ref class DataHandler {
public:
	void addData(Dictionary<String^, cli::array<double, 2>^>^% data, Dictionary<String^, List<String^>^>^% dataHeader);
	void clearData();
	Dictionary<String^, cli::array<double, 2>^>^ getData();
	Dictionary<String^, List<String^>^>^ getHeader();

private:
	Dictionary<String^, cli::array<double, 2>^>^ returnedData = gcnew Dictionary<String^, cli::array<double, 2>^>();
	Dictionary<String^, List<String^>^>^ returnedVars = gcnew Dictionary<String^, List<String^>^>();

};



