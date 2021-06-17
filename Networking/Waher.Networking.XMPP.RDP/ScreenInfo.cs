using System;

namespace Waher.Networking.XMPP.RDP
{
	/// <summary>
	/// Information about a remote screen.
	/// </summary>
	public class ScreenInfo
	{
		private readonly string deviceName;
		private readonly int bitsPerPixel;
		private readonly int left;
		private readonly int top;
		private readonly int width;
		private readonly int height;
		private readonly bool primary;

		/// <summary>
		/// Information about a remote screen.
		/// </summary>
		/// <param name="Primary">If the screen is the primary screen.</param>
		/// <param name="BitsPerPixel">Bits per pixel.</param>
		/// <param name="Left">Left coordinate</param>
		/// <param name="Top">Top coordinate</param>
		/// <param name="Width">Width of screen</param>
		/// <param name="Height">Height of screen</param>
		/// <param name="DeviceName">Name of device</param>
		public ScreenInfo(bool Primary, int BitsPerPixel, int Left, int Top, int Width, int Height, string DeviceName)
		{
			this.primary = Primary;
			this.bitsPerPixel = BitsPerPixel;
			this.left = Left;
			this.top = Top;
			this.width = Width;
			this.height = Height;
			this.deviceName = DeviceName;
		}

		/// <summary>
		/// If the screen is the primary screen.
		/// </summary>
		public bool Primary => this.primary;

		/// <summary>
		/// Bits per pixel.
		/// </summary>
		public int BitsPerPixel => this.bitsPerPixel;

		/// <summary>
		/// Left coordinate
		/// </summary>
		public int Left => this.left;

		/// <summary>
		/// Top coordinate
		/// </summary>
		public int Top => this.top;

		/// <summary>
		/// Width of screen
		/// </summary>
		public int Width => this.width;

		/// <summary>
		/// Height of screen
		/// </summary>
		public int Height => this.height;

		/// <summary>
		/// Name of device
		/// </summary>
		public string DeviceName => this.deviceName;
	}
}