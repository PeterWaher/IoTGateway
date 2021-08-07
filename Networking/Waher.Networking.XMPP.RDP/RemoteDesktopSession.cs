using System;
using System.Text;
using Waher.Content.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP.RDP
{
	/// <summary>
	/// Enumeration identifying mouse button being used.
	/// </summary>
	public enum MouseButton
	{
		/// <summary>
		/// Left mouse button
		/// </summary>
		Left,

		/// <summary>
		/// Middle mouse button
		/// </summary>
		Middle,

		/// <summary>
		/// Right mouse button
		/// </summary>
		Right
	}

	/// <summary>
	/// Maintains the client-side state of a Remote Desktop Session.
	/// </summary>
	public class RemoteDesktopSession
	{
		private readonly string sessionId;
		private readonly string remoteJid;
		private readonly RemoteDesktopClient client;
		private RemoteDesktopSessionState state = RemoteDesktopSessionState.Starting;
		private ScreenInfo[] screens = new ScreenInfo[0];
		private string deviceName = string.Empty;
		private int bitsPerPixel = 0;
		private int left = 0;
		private int top = 0;
		private int width = 0;
		private int height = 0;
		private int tileSize = 0;

		/// <summary>
		/// Maintains the client-side state of a Remote Desktop Session.
		/// </summary>
		/// <param name="SessionId">Session ID</param>
		/// <param name="RemoteJid">Remote JID</param>
		/// <param name="Client">Remote Desktop Client</param>
		public RemoteDesktopSession(string SessionId, string RemoteJid, RemoteDesktopClient Client)
		{
			this.sessionId = SessionId;
			this.remoteJid = RemoteJid;
			this.client = Client;
		}

		/// <summary>
		/// Session ID
		/// </summary>
		public string SessionId => this.sessionId;

		/// <summary>
		/// Remote JID
		/// </summary>
		public string RemoteJid => this.remoteJid;

		/// <summary>
		/// Remote Desktop Client
		/// </summary>
		public RemoteDesktopClient Client => this.client;

		/// <summary>
		/// Session state changed.
		/// </summary>
		public RemoteDesktopSessionState State
		{
			get => this.state;
			internal set
			{
				if (this.state != value)
				{
					this.state = value;

					try
					{
						this.StateChanged?.Invoke(this, EventArgs.Empty);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		/// <summary>
		/// Event raised when state has been changed.
		/// </summary>
		public event EventHandler StateChanged;

		/// <summary>
		/// Available remote screens.
		/// </summary>
		public ScreenInfo[] Screens
		{
			get => this.screens;
			internal set => this.screens = value;
		}

		/// <summary>
		/// Bits per pixel.
		/// </summary>
		public int BitsPerPixel
		{
			get => this.bitsPerPixel;
			internal set => this.bitsPerPixel = value;
		}

		/// <summary>
		/// Left coordinate
		/// </summary>
		public int Left
		{
			get => this.left;
			internal set => this.left = value;
		}

		/// <summary>
		/// Top coordinate
		/// </summary>
		public int Top
		{
			get => this.top;
			internal set => this.top = value;
		}

		/// <summary>
		/// Width of screen
		/// </summary>
		public int Width
		{
			get => this.width;
			internal set => this.width = value;
		}

		/// <summary>
		/// Height of screen
		/// </summary>
		public int Height
		{
			get => this.height;
			internal set => this.height = value;
		}

		/// <summary>
		/// Tile size
		/// </summary>
		public int TileSize
		{
			get => this.tileSize;
			internal set => this.tileSize = value;
		}

		/// <summary>
		/// Name of device
		/// </summary>
		public string DeviceName
		{
			get => this.deviceName;
			internal set => this.deviceName = value;
		}

		internal void UpdateTile(int X, int Y, string TileBase64)
		{
			try
			{
				this.TileUpdated?.Invoke(this, new TileEventArgs(this, X, Y, TileBase64));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when a tile on the remote desktop has been updated.
		/// </summary>
		public event EventHandler<TileEventArgs> TileUpdated;

		/// <summary>
		/// Reports the mouse having moved to a given position.
		/// </summary>
		/// <param name="X">X-coodrinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		public void MouseMoved(int X, int Y)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<mouseMoved xmlns='");
			Xml.Append(RemoteDesktopClient.RemoteDesktopNamespace);
			Xml.Append("' sid='");
			Xml.Append(XML.Encode(this.sessionId));
			Xml.Append("' x='");
			Xml.Append(X.ToString());
			Xml.Append("' y='");
			Xml.Append(Y.ToString());
			Xml.Append("'/>");

			this.SendMessage(Xml.ToString());
		}

		private void SendMessage(string Xml)
		{
			this.client.E2E.SendMessage(this.client.Client, E2ETransmission.NormalIfNotE2E, QoSLevel.Unacknowledged, MessageType.Normal,
				string.Empty, this.remoteJid, Xml, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Reports the mouse having been pressed down.
		/// </summary>
		/// <param name="X">X-coodrinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Button">Mouse button being pressed.</param>
		public void MouseDown(int X, int Y, MouseButton Button)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<mouseDown xmlns='");
			Xml.Append(RemoteDesktopClient.RemoteDesktopNamespace);
			Xml.Append("' sid='");
			Xml.Append(XML.Encode(this.sessionId));
			Xml.Append("' x='");
			Xml.Append(X.ToString());
			Xml.Append("' y='");
			Xml.Append(Y.ToString());
			Xml.Append("' b='");
			Xml.Append(Button.ToString());
			Xml.Append("'/>");

			this.SendMessage(Xml.ToString());
		}

		/// <summary>
		/// Reports the mouse having been released up.
		/// </summary>
		/// <param name="X">X-coodrinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Button">Mouse button being released.</param>
		public void MouseUp(int X, int Y, MouseButton Button)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<mouseUp xmlns='");
			Xml.Append(RemoteDesktopClient.RemoteDesktopNamespace);
			Xml.Append("' sid='");
			Xml.Append(XML.Encode(this.sessionId));
			Xml.Append("' x='");
			Xml.Append(X.ToString());
			Xml.Append("' y='");
			Xml.Append(Y.ToString());
			Xml.Append("' b='");
			Xml.Append(Button.ToString());
			Xml.Append("'/>");

			this.SendMessage(Xml.ToString());
		}

		/// <summary>
		/// Reports the mouse wheel having been turned.
		/// </summary>
		/// <param name="X">X-coodrinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Delta">Wheel delta</param>
		public void MouseWheel(int X, int Y, int Delta)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<mouseWheel xmlns='");
			Xml.Append(RemoteDesktopClient.RemoteDesktopNamespace);
			Xml.Append("' sid='");
			Xml.Append(XML.Encode(this.sessionId));
			Xml.Append("' x='");
			Xml.Append(X.ToString());
			Xml.Append("' y='");
			Xml.Append(Y.ToString());
			Xml.Append("' d='");
			Xml.Append(Delta.ToString());
			Xml.Append("'/>");

			this.SendMessage(Xml.ToString());
		}

		/// <summary>
		/// Reports a key having been pressed.
		/// </summary>
		/// <param name="KeyCode">Key Code.</param>
		public void KeyDown(int KeyCode)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<keyDown xmlns='");
			Xml.Append(RemoteDesktopClient.RemoteDesktopNamespace);
			Xml.Append("' sid='");
			Xml.Append(XML.Encode(this.sessionId));
			Xml.Append("' key='");
			Xml.Append(KeyCode.ToString());
			Xml.Append("'/>");

			this.SendMessage(Xml.ToString());
		}

		/// <summary>
		/// Reports a key having been released.
		/// </summary>
		/// <param name="KeyCode">Key Code.</param>
		public void KeyUp(int KeyCode)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<keyUp xmlns='");
			Xml.Append(RemoteDesktopClient.RemoteDesktopNamespace);
			Xml.Append("' sid='");
			Xml.Append(XML.Encode(this.sessionId));
			Xml.Append("' key='");
			Xml.Append(KeyCode.ToString());
			Xml.Append("'/>");

			this.SendMessage(Xml.ToString());
		}
	}
}
