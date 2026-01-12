#include "pch.h"
#include "euromoderror.h"

namespace Euromod {
    EuromodError::EuromodError(bool isWarning, const std::string& message) {
        this->isWarning = isWarning;
        this->message = message;
    }

    bool EuromodError::getIsWarning() const {
        return isWarning;
    }

    std::string EuromodError::getMessage() const {
        return message;
    }
}