using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.P2P;
using Waher.Runtime.Cache;

namespace Waher.Networking.XMPP.RDP
{
	/// <summary>
	/// Remote Desktop Client
	/// </summary>
	public class RemoteDesktopClient : XmppExtension
	{
		private readonly Cache<string, RemoteDesktopSession> sessions;
		private readonly EndpointSecurity e2e;

		/// <summary>
		/// http://waher.se/rdp/1.0
		/// </summary>
		public const string RemoteDesktopNamespace = "http://waher.se/rdp/1.0";

		/// <summary>
		/// Remote Desktop Client
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="E2e">End-to-end encryption</param>
		public RemoteDesktopClient(XmppClient Client, EndpointSecurity E2e)
			: base(Client)
		{
			this.e2e = E2e;

			this.sessions = new Cache<string, RemoteDesktopSession>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromMinutes(15));
			this.sessions.Removed += Sessions_Removed;

			Client.RegisterMessageHandler("started", RemoteDesktopNamespace, this.StartedMessageHandler, true);
			Client.RegisterMessageHandler("stopped", RemoteDesktopNamespace, this.StoppedMessageHandler, false);
			Client.RegisterMessageHandler("tile", RemoteDesktopNamespace, this.TileMessageHandler, false);
			Client.RegisterMessageHandler("tiles", RemoteDesktopNamespace, this.TilesMessageHandler, false);
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			this.client.UnregisterMessageHandler("started", RemoteDesktopNamespace, this.StartedMessageHandler, true);
			this.client.UnregisterMessageHandler("stopped", RemoteDesktopNamespace, this.StoppedMessageHandler, false);
			this.client.UnregisterMessageHandler("tile", RemoteDesktopNamespace, this.TileMessageHandler, false);
			this.client.UnregisterMessageHandler("tiles", RemoteDesktopNamespace, this.TilesMessageHandler, false);

			this.sessions.Dispose();
			base.Dispose();
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[0];

		/// <summary>
		/// End-to-end encryption
		/// </summary>
		public EndpointSecurity E2E => this.e2e;

		/// <summary>
		/// Starts a Remote Desktop session.
		/// </summary>
		/// <param name="To">Full JID of remote client.</param>
		/// <returns>Remote Desktop Session object.</returns>
		public Task<RemoteDesktopSession> StartSessionAsync(string To)
		{
			return this.StartSessionAsync(To, Guid.NewGuid());
		}

		/// <summary>
		/// Starts a Remote Desktop session.
		/// </summary>
		/// <param name="To">Full JID of remote client.</param>
		/// <param name="SessionGuid">Session ID to use.</param>
		/// <returns>Remote Desktop Session object.</returns>
		public Task<RemoteDesktopSession> StartSessionAsync(string To, Guid SessionGuid)
		{
			StringBuilder sb = new StringBuilder();
			string SessionId = SessionGuid.ToString();

			sb.Append("<start xmlns='");
			sb.Append(RemoteDesktopNamespace);
			sb.Append("'>");
			sb.Append(SessionId);
			sb.Append("</start>");

			TaskCompletionSource<RemoteDesktopSession> Result = new TaskCompletionSource<RemoteDesktopSession>();

			this.e2e.SendIqSet(this.client, E2ETransmission.NormalIfNotE2E, To, sb.ToString(), (sender, e) =>
			{
				if (e.Ok)
				{
					RemoteDesktopSession Session = new RemoteDesktopSession(SessionId, To, this);
					this.sessions[SessionId] = Session;
					Result.TrySetResult(Session);
				}
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to start Remote Desktop Session."));

				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		private void Sessions_Removed(object Sender, CacheItemEventArgs<string, RemoteDesktopSession> e)
		{
			e.Value.State = RemoteDesktopSessionState.Stopped;
		}

		/// <summary>
		/// Stops a Remote Desktop session.
		/// </summary>
		/// <param name="To">Full JID of remote client.</param>
		/// <param name="SessionId">Session ID</param>
		/// <returns>Remote Desktop Session object.</returns>
		public Task StopSessionAsync(string To, string SessionId)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("<stop xmlns='");
			sb.Append(RemoteDesktopNamespace);
			sb.Append("'>");
			sb.Append(SessionId);
			sb.Append("</stop>");

			if (this.sessions.TryGetValue(SessionId, out RemoteDesktopSession Session) && Session.State != RemoteDesktopSessionState.Stopped)
				Session.State = RemoteDesktopSessionState.Stopping;

			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();

			this.e2e.SendIqSet(this.client, E2ETransmission.NormalIfNotE2E, To, sb.ToString(), (sender, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(true);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to stop Remote Desktop Session."));

				return Task.CompletedTask;
			}, null);

			return Result.Task;
		}

		private Task StartedMessageHandler(object State, MessageEventArgs e)
		{
			List<ScreenInfo> Screens = new List<ScreenInfo>();
			string SessionId = XML.Attribute(e.Content, "sessionId");

			if (!this.sessions.TryGetValue(SessionId, out RemoteDesktopSession Session))
				return Task.CompletedTask;

			Session.DeviceName = XML.Attribute(e.Content, "deviceName");
			Session.BitsPerPixel = XML.Attribute(e.Content, "bitsPerPixel", 0);
			Session.Left = XML.Attribute(e.Content, "left", 0);
			Session.Top = XML.Attribute(e.Content, "top", 0);
			Session.Width = XML.Attribute(e.Content, "width", 0);
			Session.Height = XML.Attribute(e.Content, "height", 0);
			Session.TileSize = XML.Attribute(e.Content, "tileSize", 0);

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "screen" && E.NamespaceURI == RemoteDesktopNamespace)
				{
					string DeviceName2 = XML.Attribute(E, "deviceName");
					int BitsPerPixel2 = XML.Attribute(E, "bitsPerPixel", 0);
					int Left2 = XML.Attribute(E, "left", 0);
					int Top2 = XML.Attribute(E, "top", 0);
					int Width2 = XML.Attribute(E, "width", 0);
					int Height2 = XML.Attribute(E, "height", 0);
					bool Primary = XML.Attribute(E, "primary", false);

					Screens.Add(new ScreenInfo(Primary, BitsPerPixel2, Left2, Top2, Width2, Height2, DeviceName2));
				}
			}

			Session.Screens = Screens.ToArray();
			Session.State = RemoteDesktopSessionState.Started;

			Log.Informational("Remote desktop session started.",
				new KeyValuePair<string, object>("RemoteJid", Session.RemoteJid),
				new KeyValuePair<string, object>("SessionId", Session.SessionId));

			return Task.CompletedTask;
		}

		private Task StoppedMessageHandler(object State, MessageEventArgs e)
		{
			string SessionId = XML.Attribute(e.Content, "sessionId");
			string Reason = XML.Attribute(e.Content, "reason");

			if (!this.sessions.TryGetValue(SessionId, out RemoteDesktopSession Session))
				return Task.CompletedTask;

			Session.State = RemoteDesktopSessionState.Stopped;
			this.sessions.Remove(SessionId);

			Log.Informational("Remote desktop session stopped.",
				new KeyValuePair<string, object>("RemoteJid", Session.RemoteJid),
				new KeyValuePair<string, object>("SessionId", Session.SessionId),
				new KeyValuePair<string, object>("Reason", Reason));

			return Task.CompletedTask;
		}

		private Task TileMessageHandler(object State, MessageEventArgs e)
		{
			RemoteDesktopSession Session = null;
			string TileBase64 = e.Content.InnerText;
			int X = 0;
			int Y = 0;

			foreach (XmlAttribute Attr in e.Content.Attributes)
			{
				switch (Attr.Name)
				{
					case "sessionId":
						if (!this.sessions.TryGetValue(Attr.Value, out Session))
							return Task.CompletedTask;
						break;

					case "x":
						if (!int.TryParse(Attr.Value, out X))
							return Task.CompletedTask;
						break;

					case "y":
						if (!int.TryParse(Attr.Value, out Y))
							return Task.CompletedTask;
						break;
				}
			}

			if (Session is null || TileBase64 is null)
				return Task.CompletedTask;

			Session.UpdateTile(X, Y, TileBase64);

			return Task.CompletedTask;
		}

		private Task TilesMessageHandler(object State, MessageEventArgs e)
		{
			string SessionId = XML.Attribute(e.Content, "sessionId");
			if (!this.sessions.TryGetValue(SessionId, out RemoteDesktopSession Session))
				return Task.CompletedTask;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "tile" && E.NamespaceURI == e.Content.NamespaceURI)
				{
					string TileBase64 = E.InnerText;
					int X = 0;
					int Y = 0;

					foreach (XmlAttribute Attr in E.Attributes)
					{
						switch (Attr.Name)
						{
							case "x":
								if (!int.TryParse(Attr.Value, out X))
									continue;
								break;

							case "y":
								if (!int.TryParse(Attr.Value, out Y))
									continue;
								break;
						}
					}

					Session.UpdateTile(X, Y, TileBase64);
				}
			}

			return Task.CompletedTask;
		}
	}
}
