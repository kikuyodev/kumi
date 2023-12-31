﻿namespace Kumi.Game.Online;

public class ServerConfiguration
{
    /// <summary>
    /// The base URL for the central REST API.
    /// </summary>
    public string APIUri { get; set; } = string.Empty;

    /// <summary>
    /// The base URL for the central SignalR server.
    /// </summary>
    public string WebsocketUri { get; set; } = string.Empty;

    /// <summary>
    /// The base URL for the website.
    /// </summary>
    public string WebsiteUri { get; set; } = string.Empty;
}
