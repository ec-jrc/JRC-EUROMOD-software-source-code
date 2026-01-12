#include "stata_communication.h"
char local_error_message[] = "_errorMessageEM";
std::ostringstream oss_error;
ST_retcode printMessage(string message) {
    const char* message_ch = message.c_str();
    int len = message.length();
    for (int i = 0; i < len; i += 80) {
        char buffer[81] = { 0 }; // initialize all elements to 0
        int num_chars = min(80, len - i);
        strncpy_s(buffer, sizeof(buffer), &message_ch[i], num_chars);
        SF_display(buffer);
    }
    return 0;
}


void printErrorMessage(string message) {
    const char* message_ch = message.c_str();
    int len = message.length();
    for (int i = 0; i < len; i += 80) {
        char buffer[81] = { 0 }; // initialize all elements to 0
        int num_chars = min(80, len - i);
        strncpy_s(buffer, sizeof(buffer), &message_ch[i], num_chars);
        SF_error(buffer);
    }
    oss_error << message_ch;  
}

void setErrorMessage() {
    SF_macro_save(local_error_message, (char*)oss_error.str().c_str());
}

void clearErrorMessage() {
    oss_error.clear();
}