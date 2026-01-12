#include "pch.h"
#include "CommandProcessing.h"
#include "utilities.h"
int splitString(const std::string& input, string& EM_constant, string& EM_group) {
    const string input_trimmed = ltrim(rtrim(input));
    std::regex re("([^\\(]*)(?:\\((\\d+)\\))?"); // Regex to match group number within digits

    std::smatch result;
    std::regex_match(input, result, re);
    // if size == 5 than this corresponds to the case where there is a group specified
    if (result.size() == 3) {
        if (!result[2].str().empty()) {
            EM_group = result[2];
        }
        else { EM_group = to_string(INT_MIN); } //INT_MIN is the default for EM

        EM_constant = result[1];
        return 0;
    }
    else {
        return -1;
    }
}
// Default return

int getRevalueArguments(const std::string& s, Dictionary<Tuple<String^, String^>^, String^>^% constantsToOverwrite) {
    std::regex equalRe("(.*)=(')(.*)(')");
    std::smatch match;
    string EM_constant;
    string EM_group;
    if (std::regex_match(s, match, equalRe)) {
        // match[0] is the full match, match[1] and on are the groups
        if (match.size() == 5) {
            if (splitString(match[1], EM_constant, EM_group) != 0) return -1;

        }
        else
        {
            return -1;
        }

        constantsToOverwrite[gcnew Tuple<String^,String^ >(marshal_as<String^>(EM_constant), marshal_as<String^>(EM_group))] = marshal_as<String^>(string(match[3]));
    }
    else
        return -1;
    return 0;
}



int processCommands(const std::string& s, Dictionary<Tuple<String^, String^>^, String^>^% constantsToOverwrite) {
    std::string token;
    std::istringstream tokenStream(ltrim(s));
    char c, last_char = 0;
    int quote_count = 0;
    int splitable_count = 2;
    bool quoted = false;
    bool recently_trimmed = false;
    char last_non_space = 0;
    while (tokenStream.get(c)) {
        if (c == '\'') {
            quote_count++;
            quoted = !quoted;
            token += c;
            if (splitable_count == quote_count) {
                // Add $-sign here because STATA does not handle $ signs in macro because it's the operator to refer to a global
                if (getRevalueArguments("$" + token, constantsToOverwrite) != 0)  return -1;
                //tokens.push_back(token);
                token.clear();
                splitable_count += 2;
            }
        }
        else if (!std::isspace(c)) {
            if ((isalpha(c) || c == '$') && !quoted && recently_trimmed && last_non_space != '\'') return -1;
            token += c;
        }
        // FIX bug
        last_char = c;
        if (isspace(c) && !quoted) recently_trimmed = true;
        else { recently_trimmed = false; last_non_space = c; }
    }

    //getRevalueArguments(token, constants, groups, formulas);
    if (constantsToOverwrite->Count == 0 || quote_count % 2 != 0 || !token.empty()) {
        return -1;
    }
    return 0;
}

int createDictionary(const std::string& s, Dictionary<String^, String^>^% dict)
{
    std::string token, key, value;
    std::istringstream tokenStream(s);
    char c, last_char = 0;
    int quote_count = 0;
    bool mapped = false, separated = false;
    bool quoted = false;
    char last_non_space = 0;
    while (tokenStream.get(c)) {
        if (c == '\'') {
            quoted = !quoted;
            quote_count++;
            if (quote_count > 0 && !quoted) {
                if (quote_count % 4 != 0) key = token;
                else dict[marshal_as<String^>(key)] = marshal_as<String^>(token);
                token.clear();
            }
            else {
                // if you are entering quote mode then arguments should have been separated by a comma or mapped by a colon
                if (quote_count % 4 == 3) {

                    if (!mapped) return -1;
                    else mapped = false;
                }
                if (quote_count > 4 && quote_count % 4 - 1 == 0) {

                    if (!separated) return -1;
                    else separated = false;
                }

            }
            continue;
        }
        if (quoted) {
            token += c;
        }
        else {
            if (isspace(c))  continue;

            if (quote_count % 4 != 0) {
                if (c == ':') {
                    if (!mapped) mapped = true;
                    else return -1;
                }
                else return -1;
            }
            else
            {
                if (c == ',') {
                    if (!separated) separated = true;
                    else return -1;
                }
                else return -1;
            }
        }

    }
    if (!token.empty()) return -1;
    return 0;
}

int createDictionaryListOfStr(const std::string& s, Dictionary<String^, List<String^>^>^% dict)
{
    std::string token, key, value;
    std::istringstream tokenStream(s);
    char c, last_char = 0;
    int quote_count = 0;
    bool mapped = false, separated = false;
    bool quoted = false;
    char last_non_space = 0;
    List<String^>^ values = gcnew List<String^>();
    while (tokenStream.get(c)) {
        if (c == '\'') {
            quoted = !quoted;
            quote_count++;
            if (quote_count > 0 && !quoted) {
                if (quote_count % 4 != 0) key = token;
                else {
                    values->Add(marshal_as<String^>(token));
                    dict[marshal_as<String^>(key)] = values;
                    token.clear();
                    values = gcnew List<String^>();
                    continue;
                }
            }
            else {
                // if you are entering quote mode then arguments should have been separated by a comma or mapped by a colon
                if (quote_count % 4 == 3) {

                    if (!mapped) return -1;
                    else mapped = false;
                }
                if (quote_count > 4 && quote_count % 4 - 1 == 0) {

                    if (!separated) return -1;
                    else separated = false;
                }

            }
            continue;
        }
        if (quoted) {
            if (isspace(c)) continue;
            if (c == ',') {
                values->Add(marshal_as<String^>(token));
                token.clear();
                continue;
            }
            token += c;
        }
        else {
            if (isspace(c))  continue;

            if (quote_count % 4 != 0) {
                if (c == ':') {
                    if (!mapped) mapped = true;
                    else return -1;
                }
                else return -1;
            }
            else
            {
                if (c == ',') {
                    if (!separated) separated = true;
                    else return -1;
                }
                else return -1;
            }
        }

    }
    if (!token.empty()) return -1;
    return 0;
}


int createListOfTupleOfStr_And_ListOfStr(const std::string& s, List<Tuple<String^, List<String^>^>^>^% out)
{
    std::string token, key, value;
    std::istringstream tokenStream(s);
    char c, last_char = 0;
    int quote_count = 0;
    bool mapped = false, separated = false;
    bool quoted = false;
    char last_non_space = 0;
    List<String^>^ values = gcnew List<String^>();
    while (tokenStream.get(c)) {
        if (c == '\'') {
            quoted = !quoted;
            quote_count++;
            if (quote_count > 0 && !quoted) {
                if (quote_count % 4 != 0) {
                    key = token;
                    token.clear();
                }
                else {
                    values->Add(marshal_as<String^>(token));
                    Tuple<String^, List<String^>^>^ temp = gcnew Tuple<String^, List<String^>^>(marshal_as<String^>(key), values);

                    out->Add(temp);
                    token.clear();
                    values = gcnew List<String^>();
                    continue;
                }
            }
            else {
                // if you are entering quote mode then arguments should have been separated by a comma or mapped by a colon
                if (quote_count % 4 == 3) {

                    if (!mapped) return -1;
                    else mapped = false;
                }
                if (quote_count > 4 && quote_count % 4 - 1 == 0) {

                    if (!separated) return -1;
                    else separated = false;
                }

            }
            continue;
        }
        if (quoted) {
            if (isspace(c)) continue;
            if (c == ',') {
                values->Add(marshal_as<String^>(token));
                token.clear();
                continue;
            }
            token += c;
        }
        else {
            if (isspace(c))  continue;

            if (quote_count % 4 != 0) {
                if (c == ':') {
                    if (!mapped) mapped = true;
                    else return -1;
                }
                else return -1;
            }
            else
            {
                if (c == ',') {
                    if (!separated) separated = true;
                    else return -1;
                }
                else return -1;
            }
        }

    }
    if (!token.empty()) return -1;
    return 0;
}


int parseList(const std::string& s, List<String^>^% list, bool removeAllSpaces) {
    std::istringstream tokenStream(s);
    std::string token;
    char c, last_char = 0;
    int quote_count = 0;
    bool recently_separated;
    while (tokenStream.get(c)) {
        if (c == ',') {
            recently_separated = true;
            if (!token.empty()) list->Add(marshal_as<String^>(rtrim(ltrim(token))));
            token.clear();
        }
        else if (c == ' ' && removeAllSpaces) {
            continue;
        }
        else {
            token += c;
        }

    }
    if (!token.empty()) list->Add(marshal_as<String^>(rtrim(ltrim(token))));
    return 0;
}
