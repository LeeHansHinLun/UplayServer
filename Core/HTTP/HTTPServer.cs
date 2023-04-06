﻿using Core.HTTP;
using NetCoreServer;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;

namespace Core
{
    public class HTTPServer
    {
        static HttpsBackendServer server;
        public static void Start()
        {
            var context = new SslContext(SslProtocols.Tls12, Utils.GetCert("services", "local-ubiservices.ubi.com"));
            server = new HttpsBackendServer(context, IPAddress.Parse(Config.HTTPS_Ip), Config.HTTPS_Port);
            Console.WriteLine("[HTTPS] Server Started");
            server.Start();
        }

        public static void Stop()
        {
            server.Stop();
            Console.WriteLine("[HTTPS] Server Stopped");
        }

        public static HttpsBackendServer GetServer()
        {
            return server;
        }

        public class HttpsBackendServer : HttpsServer
        {
            public HttpsBackendServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            HttpsBackendSession session;

            public HttpsBackendSession GetSession()
            {
                return session;
            }

            protected override SslSession CreateSession()
            {
                session = new HttpsBackendSession(this);

                return session;
            }
        }

        public class HttpsBackendSession : HttpsSession
        {
            public HttpsBackendSession(HttpsServer server) : base(server) { }

            HttpRequest _request;

            public HttpRequest LastRequest()
            {
                return _request;
            }

            protected override void OnReceivedRequest(HttpRequest request)
            {
                Extra.PluginHandle.PluginsHttpRequest(request, this);
                _request = request;
                // Show HTTP request content
                Dictionary<string, string> Headers = new();
                for (int i = 0; i < request.Headers; i++)
                {
                    var headerpart = request.Header(i);
                    Headers.Add(headerpart.Item1.ToLower(), headerpart.Item2);
                }
                string key = request.Url;
                key = Uri.UnescapeDataString(key);
                //Console.WriteLine("\n" +  request.Method  + "\n"+ key);
                // Process HTTP request methods
                if (request.Method == "HEAD")
                    SendResponseAsync(Response.MakeHeadResponse());
                else if (request.Method == "GET")
                {
                    byte[] contentBytes = { };
                    var content = "";
                    bool handled = false;
                    var contentType = "text/html; charset=UTF-8";
                    switch (key)
                    {
                        case "v3/users/me":
                            break;
                    }
                    //Dynamic HTTPs:
                    if (key.StartsWith("/v2/applications/"))
                    {
                        //HTTP.Applications.v2.GET
                    }
                    if (key.StartsWith("/v1/applications/"))
                    {
                        //HTTP.Applications.v1.GET
                    }
                    if (key.StartsWith("/v1/spaces/"))
                    {
                        //HTTP.Spaces.v1.GET
                    }
                    if (key.StartsWith("/download/"))
                    {
                        contentBytes = DownloadHandler.DownloadHandlerCallback(key, out contentType);
                        handled = true;
                    }
                    if (key.StartsWith("/patch/"))
                    {
                        contentBytes = PatchHandler.PatchHandlerCallback(key, out contentType);
                        handled = true;
                    }

                    if (!handled)
                        Console.WriteLine("\n" + request.Method + "\n" + key);

                    if (contentBytes.Length == 0)
                    {
                        SendResponseAsync(Response.MakeGetResponse(content, contentType));
                    }
                    else
                    {
                        SendResponseAsync(Response.MakeGetResponse(contentBytes, contentType));
                    }

                }
                else if ((request.Method == "POST") || (request.Method == "PUT"))
                {
                    string value = request.Body;
                    Console.WriteLine(value);

                    string content = "{\"ok\":true}";
                    string contentType = "application/json; charset=UTF-8";
                    bool isfailed = false;
                    switch (key)
                    {
                        case "/v3/profiles/register":
                            content = Register.RegisterCallback(Headers, request.Body, out contentType);
                            break;
                        case "/user2session":
                            //Only needed if you using legit uplay client
                            content = User2SessionHandler.Callback(Headers, request.Body, out contentType);
                            break;
                        case "/v3/profiles/sessions":
                            content = Sessions.SessionsCallback(Headers, request.Body, out contentType, out isfailed);
                            break;
                    }
                    Console.WriteLine(isfailed);
                    Console.WriteLine(contentType);
                    if (isfailed)
                    {
                        SendResponseAsync(Response.MakeErrorResponse("Something is wrong!"));
                    }
                    else
                    {
                        SendResponseAsync(Response.MakeGetResponse(content, contentType));

                    }
                }
                else if (request.Method == "DELETE")
                {
                    SendResponseAsync(Response.MakeOkResponse());
                }
                else if (request.Method == "OPTIONS")
                    SendResponseAsync(Response.MakeOptionsResponse());
                else
                    SendResponseAsync(Response.MakeErrorResponse("Unsupported HTTP method: " + request.Method));
            }

            protected override void OnReceivedRequestError(HttpRequest request, string error)
            {
                Console.WriteLine($"Request error: {error}");
            }

            protected override void OnError(SocketError error)
            {
                Console.WriteLine($"HTTPS session caught an error: {error}");
            }
        }
    }
}
