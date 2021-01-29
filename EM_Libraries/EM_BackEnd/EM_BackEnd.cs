using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace EM_BackEnd
{
    public class BackEnd
    {
        private const int STARTING_PORT = 5000;
        private IWebHost host = null;
        private int port = 0;
        bool isRunning = false;

        /// <summary> The currently active port for the EM_BackEnd host server. </summary>
        public int Port { get { return port; } }
        /// <summary> The current state of the EM_BackEnd host server. </summary>
        public bool IsRunning { get { return isRunning; } }

        public static Dictionary<string, EM_BackEndResponder> backEndResponders = new Dictionary<string, EM_BackEndResponder>();

        /// <summary> Starts or restarts the EM_BackEnd host server </summary>
        public void Start(Dictionary<string, EM_BackEndResponder> _backEndResponders = null, string wwwrootPath = null, int startingPort = STARTING_PORT)
        {
            if (isRunning) Stop();
            host = null; port = GetAvailablePort(startingPort); if (port == 0) return;
            var builder = CreateWebHostBuilder(wwwrootPath);
            host = builder.Build(); host.StartAsync(); isRunning = true;
            if (_backEndResponders != null)
                foreach (var ber in _backEndResponders) AddResponder(ber.Key, ber.Value);
        }

        /// <summary> Stops the host server for the EM_BackEnd. </summary>
        public void Stop(List<string> removeResponderKeys = null)
        {
            if (host != null) { host.StopAsync(); host.Dispose(); host = null; }
            port = 0; isRunning = false;
            if (removeResponderKeys != null) RemoveResponders(removeResponderKeys);
        }

        public void AddResponder(string key, EM_BackEndResponder responder, bool replace = false)
        {
            if (backEndResponders.ContainsKey(key) && replace) backEndResponders.Remove(key);
            if (!backEndResponders.ContainsKey(key)) backEndResponders.Add(key, responder);
        }

        public void RemoveResponders(List<string> keys)
        {
            foreach (string key in keys) backEndResponders.Remove(key);
        }

        public class ResponseError { public string errorMessage = string.Empty; }
        public static void WriteResponseError(HttpContext context, string error)
        {
            // the java-script-function that takes the response is looking for 'errorMessage' in what-ever-class
            context.Response.WriteAsync(JsonConvert.SerializeObject(new ResponseError() { errorMessage = "Back-End error: " + error }));
        }

        /// <summary> Creates and returns a new host (server) for the EM_LightBackEnd </summary>
        /// <param name="port">The port on which the host will listen.</param>
        /// <returns>The host builder.</returns>
        private IWebHostBuilder CreateWebHostBuilder(string wwwrootPath)
        {
            WebHostBuilder webHostBuilder = new WebHostBuilder();
            webHostBuilder.UseKestrel()
                          .CaptureStartupErrors(true)
                          .ConfigureKestrel((context, options) => { options.ListenLocalhost(port); })
                          .UseStartup<Startup>();
            if (wwwrootPath != null) webHostBuilder.UseContentRoot(wwwrootPath);
            return webHostBuilder;
        }

        /// <summary> Checks for used ports and retrieves the first free port. </summary>
        /// <returns>The free port or 0 if it did not find a free port.</returns>
        private int GetAvailablePort(int startingPort)
        {
            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            // Getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);
            // Getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);
            // Getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);
            portArray.Sort();
            for (int i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;
            return 0;
        }
    }
}
