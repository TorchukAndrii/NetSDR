﻿using System.Net;

namespace NetSdrCore.Contracts;

public interface ITcpCommunicationClient
{
    bool IsConnected { get; }

    Task ConnectAsync(IPAddress host, int port = 50000);

    Task DisconnectAsync();

    Task SendAsync(byte[] data);

    Task<byte[]> ReceiveAsync();
}