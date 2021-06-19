using System;

namespace Waher.Networking.XMPP.RDP
{
	/// <summary>
	/// Event arguments for tile events.
	/// </summary>
	public class TileEventArgs : EventArgs
	{
		private readonly RemoteDesktopSession session;
		private readonly string tileBase64;
		private readonly int x;
		private readonly int y;

		/// <summary>
		/// Event arguments for tile events.
		/// </summary>
		/// <param name="Session">Remote Desktop session.</param>
		/// <param name="X">Tile X-coordinate of remote desktop screen.</param>
		/// <param name="Y">Tile Y-coordinate of remote desktop screen.</param>
		/// <param name="TileBase64">PNG of tile being updated, base64-encoded.</param>
		public TileEventArgs(RemoteDesktopSession Session, int X, int Y, string TileBase64)
		{
			this.session = Session;
			this.x = X;
			this.y = Y;
			this.tileBase64 = TileBase64;
		}

		/// <summary>
		/// Remote Desktop session.
		/// </summary>
		public RemoteDesktopSession Session => this.session;

		/// <summary>
		/// PNG of tile being updated, base64-encoded.
		/// </summary>
		public string TileBase64 => this.tileBase64;

		/// <summary>
		/// Tile X-coordinate of remote desktop screen.
		/// </summary>
		public int X => this.x;

		/// <summary>
		/// Tile Y-coordinate of remote desktop screen.
		/// </summary>
		public int Y => this.y;
	}
}
