﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Minecraft
{
    class Server
    {
        private PacketFactory packetFactory = new PacketFactory();

        private List<Connection> clients = new List<Connection>();
        private object clientsLock = new object();
        private Thread connectionsThread;
        private int port;
        private string address;
        private TcpListener tcpServer;
        private bool isRunning;

        private World world;
        private Game game;

        public void Start(Game game, string address, int port)
        {
            this.game = game;
            this.address = address;
            this.port = port;

            world = new World(game);

            connectionsThread = new Thread(ListenForConnections);
            connectionsThread.IsBackground = true;
            connectionsThread.Start();
        }

        public void GenerateMap()
        {
            world.GenerateTestMap();
        }

        public void AddHook(IEventHook hook)
        {
            world.AddEventHooks(hook);
        }

        public World GetWorldInstance()
        {
            return world;
        }

        private void ListenForConnections()
        {
            tcpServer = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            tcpServer.Start();
            Logger.Info("Started listening for connections on " + GetType());
            isRunning = true;
            while (isRunning)
            {
                TcpClient client = tcpServer.AcceptTcpClient();
                Logger.Info("Server accepted new client.");
                NetworkStream stream = client.GetStream();
                Connection clientConnection = new Connection
                {
                    client = client,
                    netStream = stream,
                    reader = new BinaryReader(stream),
                    writer = new BinaryWriter(stream),
                    bufferedStream = new NetBufferedStream(new BufferedStream(stream))
                };
                ServerNetHandler netHandler = new ServerNetHandler(game, clientConnection);
                clientConnection.netHandler = netHandler;

                lock (clientsLock)
                {
                    clients.Add(clientConnection);
                }

                Logger.Info("Writing chunk data to stream.");
                foreach (KeyValuePair<Vector2, Chunk> kv in world.loadedChunks)
                {
                    clientConnection.SendPacket(new ChunkDataPacket(kv.Value));
                }

                BroadcastPacket(new ChatPacket("Someone joined!"));             
            }

            Logger.Warn("Server is closing down. Closing connections to all clients.");
            lock (clientsLock)
            {
                clients.ForEach(c => c.Close());
            }
            tcpServer.Stop();
            Logger.Info("Server closed.");
        }

        public void Update(Game game)
        {
            //Check for console input in a non-blocking way
            if (Console.KeyAvailable)
            {
                string input = Console.ReadLine();
                BroadcastPacket(new ChatPacket(input));
            }

            lock (clientsLock)
            {
                foreach (Connection client in clients)
                {
                    if (!client.netStream.DataAvailable)
                    {
                        continue;
                    }

                    Packet packet = packetFactory.ReadPacket(client.reader);
                    Logger.Info("Server received packet " + packet.ToString());
                    packet.Process(client.netHandler);
                }
            }
        }

        public void BroadcastPacket(Packet packet)
        {
            lock (clientsLock)
            {
                Logger.Info("Server broadcasting packet [" + packet.GetType() + "]");
                clients.ForEach(c => c.SendPacket(packet));
            }
        }

        public void Stop()
        {
            isRunning = false;
        }
    }
}