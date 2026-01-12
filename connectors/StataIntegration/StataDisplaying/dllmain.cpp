// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "Header.h"
#include "stplugin.h"
using namespace std;
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
}



