using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Networking.HTTP.WebSockets;
using Waher.Security;

namespace Waher.IoTGateway
{
	/// <summary>
	/// Web-socket binding method for the <see cref="ClientEvents"/> class. It allows clients connect to the gateway using web-sockets to
	/// get events.
	/// </summary>
	public class ClientEventsWebSocket : WebSocketListener
	{
		private static readonly string serverId = Hashes.BinaryToString(Gateway.NextBytes(32));

		/// <summary>
		/// Resource managing asynchronous events to web clients.
		/// </summary>
		public ClientEventsWebSocket()
			: base("/ClientEventsWS", true, 10 * 1024 * 1024, 10 * 1024 * 1024, "ls")
		{
			this.Accept += ClientEventsWebSocket_Accept;
			this.Connected += ClientEventsWebSocket_Connected;
		}

		private void ClientEventsWebSocket_Connected(object Sender, WebSocketEventArgs e)
		{
			e.Socket.Closed += Socket_Closed;
			e.Socket.Disposed += Socket_Disposed;
			e.Socket.TextReceived += Socket_TextReceived;
		}

		private void Socket_TextReceived(object Sender, WebSocketTextEventArgs e)
		{
			if (JSON.Parse(e.Payload) is Dictionary<string, object> Obj &&
				Obj.TryGetValue("cmd", out object Value) && Value is string Command)
			{
				switch (Command)
				{
					case "Register":
						if (Obj.TryGetValue("tabId", out object O1) && O1 is string TabID &&
							Obj.TryGetValue("location", out object O2) && O2 is string Location)
						{
							e.Socket.Tag = new Info()
							{
								Location = Location,
								TabID = TabID
							};

							ClientEvents.RegisterWebSocket(e.Socket, Location, TabID);
							ClientEvents.PushEvent(new string[] { TabID }, "CheckServerInstance", serverId, false);
						}
						break;

					case "Unregister":
						this.Close(e.Socket);
						break;

					case "Ping":
						if (e.Socket.Tag is Info Info)
							ClientEvents.Ping(Info.TabID);
						break;
				}
			}
		}

		private class Info
		{
			public string Location;
			public string TabID;
		}

		private void Socket_Disposed(object sender, EventArgs e)
		{
			if (sender is WebSocket WebSocket)
				this.Close(WebSocket);
		}

		private void Socket_Closed(object Sender, WebSocketClosedEventArgs e)
		{
			this.Close(e.Socket);
		}

		private void Close(WebSocket Socket)
		{
			if (Socket.Tag is Info Info)
			{
				ClientEvents.UnregisterWebSocket(Socket, Info.Location, Info.TabID);
				Socket.Tag = null;
			}
		}

		private void ClientEventsWebSocket_Accept(object Sender, WebSocketEventArgs e)
		{
			HttpFieldCookie Cookie;

			if ((Cookie = e.Socket.HttpRequest.Header.Cookie) is null ||
				string.IsNullOrEmpty(Cookie["HttpSessionID"]))
			{
				throw new ForbiddenException("HTTP Session required.");
			}
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get
			{
				return false;
			}
		}


	}
}
