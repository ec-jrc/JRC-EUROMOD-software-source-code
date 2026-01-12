#pragma once
#ifdef CLR_LIBRARY
#define clrLibrary_API __declspec(dllexport)

#else
#define clrLibrary_API __declspec(dllimport)

#endif
#ifndef EURMODERROR_H
#define EURMODERROR_H



#include <string>



namespace Euromod {
    class clrLibrary_API EuromodError {
    private:
        bool isWarning;
        std::string message;

    public:
        EuromodError(bool isWarning, const std::string& message);
        bool getIsWarning() const;
        std::string getMessage() const;
    };

}


#endif