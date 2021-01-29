using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace EM_BackEnd
{
    public class EM_BackEndResponder
    {
        public string responderKey = null;
        public Dictionary<string, Action<HttpContext, EM_BackEndResponder>> responses = new Dictionary<string, Action<HttpContext, EM_BackEndResponder>>();

        public EM_BackEndResponder(string _responderKey)
        {
            responderKey = _responderKey == null ? Guid.NewGuid().ToString() : _responderKey;
        }

        public virtual bool BuildResponse_FromResource(HttpContext context, string r) { return false; }
    }
}
