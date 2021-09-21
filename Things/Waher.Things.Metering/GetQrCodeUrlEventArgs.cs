using System;

namespace Waher.Things.Metering
{
	/// <summary>
	/// Event arguments for events that request an URL to a QR code.
	/// </summary>
	public class GetQrCodeUrlEventArgs : EventArgs
	{
		private readonly string text;
		private string url;

		/// <summary>
		/// Event arguments for events that request an URL to a QR code.
		/// </summary>
		/// <param name="Text">Text to encode.</param>
		public GetQrCodeUrlEventArgs(string Text)
		{
			this.text = Text;
			this.url = null;
		}

		/// <summary>
		/// Text to encode.
		/// </summary>
		public string Text => this.text;

		/// <summary>
		/// QR Code URL.
		/// </summary>
		public string Url
		{
			get => this.url;
			set => this.url = value;
		}
	}
}
