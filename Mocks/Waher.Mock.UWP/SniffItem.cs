using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Waher.Events;

namespace Waher.Mock
{
	/// <summary>
	/// Sniff item type.
	/// </summary>
	public enum SniffItemType
	{
		/// <summary>
		/// Data received.
		/// </summary>
		DataReceived,

		/// <summary>
		/// Data transmitted.
		/// </summary>
		DataTransmitted,

		/// <summary>
		/// Text received.
		/// </summary>
		TextReceived,

		/// <summary>
		/// Text transmitted.
		/// </summary>
		TextTransmitted,

		/// <summary>
		/// Information.
		/// </summary>
		Information,

		/// <summary>
		/// Warning.
		/// </summary>
		Warning,

		/// <summary>
		/// Error.
		/// </summary>
		Error,

		/// <summary>
		/// Exception.
		/// </summary>
		Exception
	}

	/// <summary>
	/// Represents one item in a sniffer output.
	/// </summary>
	public class SniffItem : ColorableItem
	{
		private SniffItemType type;
		private DateTime timestamp;
		private string message;
		private byte[] data;

		/// <summary>
		/// Represents one item in a sniffer output.
		/// </summary>
		/// <param name="Type">Type of sniff record.</param>
		/// <param name="Message">Message</param>
		/// <param name="Data">Optional binary data.</param>
		/// <param name="ForegroundColor">Foreground Color</param>
		/// <param name="BackgroundColor">Background Color</param>
		public SniffItem(SniffItemType Type, string Message, byte[] Data, Color ForegroundColor, Color BackgroundColor)
			: base(ForegroundColor, BackgroundColor)
		{
			this.type = Type;
			this.timestamp = DateTime.Now;
			this.message = Message;
			this.data = Data;
		}

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp { get { return this.timestamp; } }

		/// <summary>
		/// Sniff item type.
		/// </summary>
		public SniffItemType Type { get { return this.type; } }

		/// <summary>
		/// Time of day of event, as a string.
		/// </summary>
		public string Time { get { return this.timestamp.ToString("T"); } }

		/// <summary>
		/// Message
		/// </summary>
		public string Message { get { return this.message; } }

		/// <summary>
		/// Optional binary data.
		/// </summary>
		public byte[] Data { get { return this.data; } }

	}
}
