using System;
using System.Windows.Media;
using Waher.Client.WPF.Model;

namespace Waher.Client.WPF.Controls.Sniffers
{
	public enum SniffItemType
	{
		DataReceived,
		DataTransmitted,
		TextReceived,
		TextTransmitted,
		Information,
		Warning,
		Error,
		Exception
	}

	/// <summary>
	/// Represents one item in a sniffer output.
	/// </summary>
	/// <param name="Timestamp">Timestamp of event.</param>
	/// <param name="Type">Type of sniff record.</param>
	/// <param name="Message">Message</param>
	/// <param name="Data">Optional binary data.</param>
	/// <param name="ForegroundColor">Foreground Color</param>
	/// <param name="BackgroundColor">Background Color</param>
	public class SniffItem(DateTime Timestamp, SniffItemType Type, string Message, 
		byte[] Data, Color ForegroundColor, Color BackgroundColor) 
		: ColorableItem(ForegroundColor, BackgroundColor)
	{
		private readonly SniffItemType type = Type;
		private readonly DateTime timestamp = Timestamp;
		private readonly string message = Message;
		private readonly byte[] data = Data;

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp => this.timestamp;

		/// <summary>
		/// Sniff item type.
		/// </summary>
		public SniffItemType Type => this.type;

		/// <summary>
		/// Time of day of event, as a string.
		/// </summary>
		public string Time { get { return this.timestamp.ToLongTimeString(); } }

		/// <summary>
		/// Message
		/// </summary>
		public string Message => this.message;

		/// <summary>
		/// Optional binary data.
		/// </summary>
		public byte[] Data => this.data;

	}
}
