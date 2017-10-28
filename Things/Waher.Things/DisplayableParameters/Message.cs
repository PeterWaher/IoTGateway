
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Things;

namespace Waher.Things.DisplayableParameters
{
	/// <summary>
	/// Type of message.
	/// </summary>
	public enum MessageType
	{
		/// <summary>
		/// Information message.
		/// </summary>
		Information,

		/// <summary>
		/// Warning message.
		/// </summary>
		Warning,

		/// <summary>
		/// Error message.
		/// </summary>
		Error
	}

	/// <summary>
	/// Contains information about a message logged on a node.
	/// </summary>
	public class Message
	{
		private DateTime timestamp;
		private MessageType type;
		private string eventId;
		private string body;

		/// <summary>
		/// Contains information about a message logged on a node.
		/// </summary>
		/// <param name="Timestamp">Message Timestamp.</param>
		/// <param name="Type">Type of message.</param>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public Message(DateTime Timestamp, MessageType Type, string EventId, string Body)
		{
			this.timestamp = Timestamp;
			this.type = Type;
			this.eventId = EventId;
			this.body = Body;
		}

		/// <summary>
		/// Timestamp
		/// </summary>
		public DateTime Timestamp
		{
			get { return this.timestamp; }
		}

		/// <summary>
		/// Message Type
		/// </summary>
		public MessageType Type
		{
			get { return this.type; }
		}

		/// <summary>
		/// Optional Event ID.
		/// </summary>
		public string EventId
		{
			get { return this.eventId; }
		}

		/// <summary>
		/// Message body.
		/// </summary>
		public string Body
		{
			get { return this.body; }
		}
		
		/// <summary>
		/// Exports the message to XML.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		public void Export(StringBuilder Xml)
		{
			Xml.Append("<message timestamp='");
			Xml.Append(XML.Encode(this.timestamp));
			Xml.Append("' type='");
			Xml.Append(this.type.ToString());

			if (!string.IsNullOrEmpty(this.eventId))
			{
				Xml.Append("' eventId='");
				Xml.Append(this.eventId);
			}

			Xml.Append("'>");
			Xml.Append(XML.Encode(this.body));
			Xml.Append("</message>");
		}
	}
}
