using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Dsn
{
	/// <summary>
	/// Delivery Status information, as defined in RFC 3464:
	/// 
	/// https://tools.ietf.org/html/rfc3464
	/// </summary>
	public class DeliveryStatus
	{
		private readonly PerMessageFields perMessage;
		private readonly PerRecipientFields[] perRecipients;
		private readonly string text;

		/// <summary>
		/// Delivery Status information, as defined in RFC 3464:
		/// 
		/// https://tools.ietf.org/html/rfc3464
		/// </summary>
		/// <param name="Text">Text representation of the status message.</param>
		/// <param name="PerMessage">Information about message</param>
		/// <param name="PerRecipients">Information about recipients</param>
		public DeliveryStatus(string Text, PerMessageFields PerMessage,
			PerRecipientFields[] PerRecipients)
		{
			this.text = Text;
			this.perMessage = PerMessage;
			this.perRecipients = PerRecipients;
		}

		/// <summary>
		/// Text representation of the status message.
		/// </summary>
		public string Text => this.text;

		/// <summary>
		/// Information about message
		/// </summary>
		public PerMessageFields PerMessage => this.perMessage;

		/// <summary>
		/// Information about recipients
		/// </summary>
		public PerRecipientFields[] PerRecipients => this.perRecipients;
	}
}
