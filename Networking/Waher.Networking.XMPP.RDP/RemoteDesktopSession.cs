using System;
using Waher.Events;

namespace Waher.Networking.XMPP.RDP
{
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
	}
}
