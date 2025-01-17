﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using MoonlapseNetworking;
using MoonlapseClient.UserStates;
using MoonlapseNetworking.Packets;

namespace MoonlapseClient
{
    public class NetworkController
    {
        TcpClient _client;

        public string Hostname { get; private set; }
        public int Port { get; private set; }

        public string ErrorMessage { get; private set; }

        public UserState CurrentState { get; set; }

        public NetworkController(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
            _client = new TcpClient();
            CurrentState = new EntryState(this);
        }

        public async Task Start()
        {
            try
            {
                await _client.ConnectAsync(Hostname, Port);
            }
            catch (Exception)
            {
                ErrorMessage = "Unable to connect to server";
            }

            var stream = _client.GetStream();
            var sr = new StreamReader(stream);

            // main read loop
            while (true)
            {
                try
                {
                    string line;

                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        CurrentState.HandlePacketFromString(line);
                    }
                }
                catch (Exception)
                {
                    ErrorMessage = "Connection to server lost";
                }
                
            }
        }

        public void SendPacket(Packet p)
        {
            try
            {
                var sw = new StreamWriter(_client.GetStream());
                sw.WriteLine(p.ToString());
                sw.Flush();
            }
            catch (Exception)
            {
                ErrorMessage = "Connection to server lost";
            }
        }
    }
}
