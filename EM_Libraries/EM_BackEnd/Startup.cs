using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace EM_BackEnd
{
    public class Startup
    {
        long countRequests = 0;

        public void Configure(IApplicationBuilder app)
        {
            app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = new List<string> { "home.html" } });
            app.UseStaticFiles();
            app.Run(async (context) =>
            {
                countRequests++;

                string request = context.Request.Path.Value.Substring(1); // remove leading slash
                if (BuildResponse_FromResource(context, request)) return;
                foreach (var backEndResponder in BackEnd.backEndResponders)
                    foreach (var response in backEndResponder.Value.responses)
                        if ($"{backEndResponder.Key}_{response.Key}" == request)
                        { response.Value(context, backEndResponder.Value); return; }
                // If nothing else matched, produce an error message
                BuildResponse_Error(context, request);
            });
        }

        private void BuildResponse_Error(HttpContext context, string r)
        {
            context.Response.WriteAsync("{\"errorMessage\":\"Invalid Request: "+ context.Request.GetDisplayUrl() +"\"}");
        }

        private bool BuildResponse_FromResource(HttpContext context, string r)
        {
            foreach (var backEndResponder in BackEnd.backEndResponders)
                if (backEndResponder.Value.BuildResponse_FromResource(context, r)) return true;

            string ext = Path.GetExtension(r).Replace(".","");
            string objName = r;
            if (ext == "css" || ext == "js")
            {
                objName = r.Replace(".", "_");
            }
            else if (ext == "png")
            {
                objName = r.Substring(0, r.Length - 4);
            }
            object obj = Resources.ResourceManager.GetObject(objName);


            if (obj != null)
            {
                if (ext == "css") context.Response.ContentType = "text/css";
                else if (ext == "js") context.Response.ContentType = "text/javascript";
                else if (ext == "png") context.Response.ContentType = "image/png";

                if (ext == "css" || ext == "js")
                {
                    context.Response.WriteAsync(obj as string);
                    return true;
                }
                else if (ext == "png")
                {
                    byte[] img = obj as byte[];
                    context.Response.Body.WriteAsync(img, 0, (int)img.Length);
                    return true;
                }
            }
            return false;
        }

        private void BuildResponse_Default(HttpContext context, IServerAddressesFeature serverAddressesFeature)
        {
            try
            {
                bool hasForm = context.Request.HasFormContentType && context.Request.Form.Count > 0;
                context.Response.ContentType = "text/html";
                context.Response.WriteAsync("<!DOCTYPE html><html lang=\"en\"><head><title></title></head><body><p>Hosted by Kestrel</p>");
                if (serverAddressesFeature != null)
                    context.Response.WriteAsync("<p>Listening on the following addresses: " + string.Join(", ", serverAddressesFeature.Addresses) + "</p>");
                context.Response.WriteAsync($"<p>Request URL: {context.Request.GetDisplayUrl()}<p>");
                if (hasForm && context.Request.Form.Count > 0)
                {
                    context.Response.WriteAsync("<p>Form submitted! (POST)");
                    foreach (string key in context.Request.Form.Keys) context.Response.WriteAsync($"<br> - Field [{key}]: '{context.Request.Form[key]}'");
                    context.Response.WriteAsync("<br></p>");
                }
                context.Response.WriteAsync($"<p>Number of total requests: {countRequests}</p>");
                context.Response.WriteAsync("</body></html>");
            }
            catch (Exception exception) { BackEnd.WriteResponseError(context, exception.Message); }
        }
    }
}
