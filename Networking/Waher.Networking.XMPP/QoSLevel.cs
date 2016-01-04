using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Quality of Service Level for asynchronous messages. Support for QoS Levels must be supported by the recipient, and is defined in:
	/// http://xmpp.org/extensions/inbox/qos.html
	/// </summary>
	public enum QoSLevel
	{
		/// <summary>
		/// To send a message that is received at most once to its destination, a normal message stanza is used, as defined by the XMPP protocol itself. 
		/// The delivery of the message is not guaranteed.
		/// </summary>
		Unacknowledged,

		/// <summary>
		/// To send a message that is received at least once to its destination, acknowledged service can be used.
		/// The message is delivered using IQ-based request/response stanzas, and therefore requires twice as many
		/// stanzas to be sent, as compared to the <see cref="Unacknowledged"/> service. However, the payload is
		/// only sent in the first request.
		/// It is not guaranteed that the message is delivered only once.
		/// </summary>
		Acknowledged,

		/// <summary>
		/// To send a message that is received exactly once to its destination, assured service can be used.
		/// The message is delivered using two IQ-based request/response stanzas, and therefore requires twice as many
		/// stanzas to be sent, as compared to the <see cref="Acknowledged"/> service. However, the payload is
		/// only sent in the first request.
		/// </summary>
		Assured
	}
}
