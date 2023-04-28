﻿using Client.Patch;
using ClientApp;
using ClientKit.Demux;
using ClientKit.Demux.Connection;
using ClientKit.UbiServices.Public;
using ClientKit.UbiServices.Records;
using Google.Protobuf;
using Newtonsoft.Json;
using RestSharp;
using SharedLib.Shared;
using System.IO.Pipes;
using System.Reflection.Metadata;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Debug.isDebug = true;
            var client = new RestClient("https://local-ubiservices.ubi.com:7777/store/?p=0");
            var request = new RestRequest();

            request.AddHeader("authorization", $"ubi_v1 t=VDF6UWlacDJvTDdCOE5IcXR3bHNYRE8yeDlvWjkvYzh6M083R0hER0hGaz0=");

            var rsp = ClientKit.UbiServices.Rest.GetString(client,request);

            File.WriteAllText("resp", rsp);

            var reg = V3.Register("testuser", "testuser", "testuser");
            if (reg != null)
            {
                var UserID = reg.UserId;
                Console.WriteLine(UserID);
            }
            var login = V3.Login("testuser", "testuser");
            if (login != null)
            {
                var ticket = login.Ticket;
                Console.WriteLine(ticket);
                Debug.isDebug = true;
                Socket socket = new();
                var patch = socket.GetPatch();

                if (patch.LatestVersion != socket.ClientVersion)
                {
                    Console.WriteLine("Your client is outdated!\nDo you wanna update?");
                    Console.WriteLine("\nY/y = Yes | N/n = No (If you dont say it just exit)");

                    var choice = Console.ReadLine();
                    switch (choice.ToLower())
                    {
                        case "y":
                            Patcher.Main(patch);
                            Restarter.Restart("");
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    Console.WriteLine("Your Client is up-to-date!");
                }
                socket.PushVersion();
                Console.WriteLine(socket.Authenticate(ticket));
                Console.WriteLine(socket.IsAuthed);
                if (args.Contains("install") || args.Contains("download"))
                {
                    args = args.Append("-ticket").Append(ticket).ToArray();
                    Downloader.Program.Main(args, socket);
                }
                else
                {
                    OwnershipConnection ownershipConnection = new(socket);
                    var x = ownershipConnection.Initialize();
                    var sig = x.OwnedGamesContainer.Signature.ToBase64();
                    var tmp = ownershipConnection.RegisterTempOwnershipToken(sig);
                    Console.WriteLine(tmp.ProductIds.Count);
                }

                Console.ReadLine();
                socket.Close();
            }

            /*
            var x = callbacktest.getcontext();
            Console.WriteLine("yey context! " + x);
            callbacktest.updatecontext(x);
            Console.WriteLine("update! " +x);
            callbacktest.Use(x);
            Console.WriteLine("use! " + x);
            callbacktest.updatecontext(x);
            Console.WriteLine("update! " + x);
            callbacktest.freecontext(x);
            Console.WriteLine("free! " + x);*/
            /*
            var reg = V3.Register("slejm","slejm","slejm");
            if (reg != null)
            {
                var UserID = reg.UserId;

            }
           

            var login = V3.Login("slejm", "slejm");
            if (login != null)
            {
                var ticket = login.Ticket;
                Debug.isDebug = true;
                Socket socket = new();
                socket.PushVersion();
                socket.VersionCheck();
                Console.WriteLine(socket.Authenticate(ticket));

                OwnershipConnection ow = new(socket);
                var games = ow.GetOwnedGames();
                if (games != null)
                {
                    foreach (var x in games)
                    {
                        Console.WriteLine(x.ToString());

                    }

                }




                Console.ReadLine();

                socket.Close();
            }*/
        }

        /*
         
                     Console.WriteLine("Hello, World!"); 
            new Thread(VTest).Start();
            
            Console.WriteLine("Bye, World!");
         
         */

        public static void VTest()
        {
            var pipeServer = new NamedPipeServerStream("custom_r2_pipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte);
            Console.WriteLine("server created");
            //new Thread(BTest).Start();
            pipeServer.WaitForConnection();
            Console.WriteLine("con waited");
            if (pipeServer.IsConnected)
            {
                byte[] buffer = new byte[4];
                int count = pipeServer.Read(buffer, 0, 4);
                if (count == 4)
                {
                    Console.WriteLine("4");
                    var _InternalReadedLenght = FormatLength(BitConverter.ToUInt32(buffer, 0));
                    var _InternalReaded = new byte[(int)_InternalReadedLenght];
                    Console.WriteLine(_InternalReadedLenght);
                    int readed = pipeServer.Read(_InternalReaded, 0, (int)_InternalReadedLenght);
                    Console.WriteLine(readed);
                    var upstream = FormatDataNoLength<Uplay.Demux.Upstream>(_InternalReaded);
                    if (upstream != null)
                    {
                        Console.WriteLine("not null");
                        Console.WriteLine(upstream);
                        if (!string.IsNullOrEmpty(upstream.Request.ServiceRequest.Service))
                        {
                            Console.WriteLine(upstream.Request.ServiceRequest.Service);
                            Uplay.Demux.Downstream downstream = new()
                            {
                                Response = new()
                                {
                                    RequestId = ReqId,
                                    ServiceRsp = new()
                                    {
                                        Success = true,
                                        Data = ByteString.CopyFrom(new Uplay.Uplaydll.Rsp() { InstallerRsp = new() { InitInstallerRsp = new() { Success = true } } }.ToByteArray())
                                    }
                                }
                            };
                            pipeServer.Write(FormatUpstream(upstream.ToByteArray()));
                            pipeServer.Flush();
                        }
                    }
                    else
                    {
                        Console.WriteLine("whyy");
                    }
                }
            }
            pipeServer.Disconnect();
            pipeServer.Dispose();
        }

        public static T? FormatData<T>(byte[] bytes) where T : IMessage<T>, new()
        {
            try
            {
                if (bytes == null)
                    return default;

                byte[] buffer = new byte[4];

                using var ms = new MemoryStream(bytes);
                ms.Read(buffer, 0, 4);
                var responseLength = FormatLength(BitConverter.ToUInt32(buffer, 0));
                if (responseLength == 0)
                    return default;

                MessageParser<T> parser = new(() => new T());
                return parser.ParseFrom(ms);
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static T? FormatDataNoLength<T>(byte[] bytes) where T : IMessage<T>, new()
        {
            try
            {
                if (bytes == null)
                    return default;

                MessageParser<T> parser = new(() => new T());
                return parser.ParseFrom(bytes);
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static uint ReqId = uint.MinValue;
        public static byte[] FormatUpstream(byte[] rawMessage)
        {
            BlobWriter blobWriter = new(4);
            blobWriter.WriteUInt32BE((uint)rawMessage.Length);
            var returner = blobWriter.ToArray().Concat(rawMessage).ToArray();
            blobWriter.Clear();
            return returner;
        }

        public static uint FormatLength(uint length)
        {
            BlobWriter blobWriter = new(4);
            blobWriter.WriteUInt32BE(length);
            var returner = BitConverter.ToUInt32(blobWriter.ToArray());
            blobWriter.Clear();
            return returner;
        }
    }
}