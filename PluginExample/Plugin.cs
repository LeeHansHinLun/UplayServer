﻿using Core.DemuxResponders;
using Core.Extra.Interfaces;
using NetCoreServer;
using System.Composition;
using System.Net.Security;

namespace Plugin
{
    [Export(typeof(IPlugin))]
    public class Plugin : IPlugin, IDisposable
    {
        public uint Priority => uint.MaxValue;

        public string Name => "Plugin Example";

        public Core.JSON.Plugin PluginExtra => new()
        {
            Mode = "Test",
            PluginType = Core.JSON.Plugin.pluginType.Extra,
            Version = "0.0.0.1"
        };

        public Plugin()
        {
            Console.WriteLine("Welcome from " + Name + " !");
        }

        public void Dispose()
        {

        }

        public void Initialize()
        {

        }

        public void DemuxParseInitFinish(DemuxServer demux)
        {

        }

        public void DemuxDataReceived(int ClientNumb, SslStream sslStream, byte[] receivedData)
        {

        }

        public void HttpRequest(HttpRequest request, HttpsSession session)
        {
            if (request.Url.Contains("ok"))
            {
                session.SendResponseAsync(session.Response.MakeGetResponse("Success!"));
            }
        }

        public void ShutDown()
        {
            Console.WriteLine("Goodbye!");
        }

        public void DemuxDataReceivedCustom(int ClientNumb, byte[] receivedData, string Protoname)
        {

        }
    }
}