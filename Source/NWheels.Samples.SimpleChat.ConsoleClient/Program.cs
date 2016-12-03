﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NWheels.Client;
using NWheels.Endpoints;
using NWheels.Extensions;
using NWheels.Samples.SimpleChat.Contracts;

namespace NWheels.Samples.SimpleChat.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("--poc"))
            {
                PocMain(args);
                return;
            }

            var framework = ClientSideFramework.CreateWithDefaultConfiguration();
            var clientFactory = framework.Components.Resolve<DuplexTcpClientFactory>();
            var client = new ChatClient();

            Console.WriteLine("Now connecting to chat server.");
            Console.WriteLine("HELP > while in chat, type your message and hit ENTER to send.");
            Console.WriteLine("HELP > to leave, type Q and hit ENTER.");

            client.Server = clientFactory.CreateServerProxy<IChatServiceApi, IChatClientApi>(
                new ChatClient(),
                serverHostname: "localhost",
                serverPort: 9797,
                serverPingInterval: TimeSpan.FromSeconds(1));

            Thread.Sleep(20);
            client.Server.Hello(myName: "PID#" + Process.GetCurrentProcess().Id);

            while (true)
            {
                var text = Console.ReadLine();
                
                if (text == null || text.Trim().EqualsIgnoreCase("Q"))
                {
                    client.Server.GoodBye();
                    break;
                }
            }

            Console.WriteLine("Shutting down.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void PocMain(string[] args)
        {
            Console.WriteLine("****** Starting PoC.");

            var serverArgIndex = Array.IndexOf(args, "--server");
            if (serverArgIndex < 0 || serverArgIndex >= args.Length - 1)
            {
                throw new ArgumentException("server must be specified in the format --server host:port");
            }

            var serverAddress = args[serverArgIndex + 1];
            var serverUri = new Uri("tcp://" + serverAddress);

            TcpPoc.Server server = null;
            TcpPoc.Client client = null;
            TcpPoc.Session serverSession = null;

            if (args.Contains("--run-server"))
            {
                var serverIp = (Uri.CheckHostName(serverUri.Host) == UriHostNameType.IPv4 ? serverUri.Host : null);
                server = new TcpPoc.Server(serverIp, serverUri.Port, onClientConnected: (session) => {
                    serverSession = session;
                    session.MessageReceived += (session2, message) => {
                        Console.WriteLine(Encoding.UTF8.GetString(message));
                    };
                });
            }

            if (args.Contains("--run-client"))
            {
                client = new TcpPoc.Client(serverUri.Host, serverUri.Port);
                Task.Factory.StartNew(() => TcpPoc.RunScenario2(client, serverSession));
            }

            Console.WriteLine("****** PoC is running. Hit ENTER to quit . . .");
            Console.WriteLine();

            Console.ReadLine();

            if (client != null)
            {
                client.Dispose();
            }

            if (server != null)
            {
                server.Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("****** PoC shut down completed.");
        }
    }
}
