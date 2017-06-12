using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.MQTT
{
	/// <summary>
	/// MQTT Quality of Service level.
	/// </summary>
	public enum MqttQualityOfService
	{
		/// <summary>
		/// The message is delivered according to the capabilities of the underlying network. 
		/// No response is sent by the receiver and no retry is performed by the sender. 
		/// The message arrives at the receiver either once or not at all. 
		/// </summary>
		AtMostOnce = 0,

		/// <summary>
		/// This quality of service ensures that the message arrives at the receiver at least once. 
		/// A QoS 1 PUBLISH Packet has a Packet Identifier in its variable header and is acknowledged 
		/// by a PUBACK Packet. Section 2.3.1 provides more information about Packet Identifiers.
		/// </summary>
		AtLeastOnce = 1,

		/// <summary>
		/// This is the highest quality of service, for use when neither loss nor duplication of messages are acceptable.
		/// There is an increased overhead associated with this quality of service.
		/// A QoS 2 message has a Packet Identifier in its variable header. Section 2.3.1 provides more information about 
		/// Packet Identifiers. The receiver of a QoS 2 PUBLISH Packet acknowledges receipt with a two-step acknowledgement process. 
		/// </summary>
		ExactlyOnce = 2
	}
}
