using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.MQTT
{
	public enum MqttControlPacketType
	{
		/// <summary>
		/// Client to Server.
		/// Client request to connect to Server
		/// </summary>
		CONNECT = 1,

		/// <summary>
		/// Server to Client
		/// Connect acknowledgment
		/// </summary>
		CONNACK = 2,

		/// <summary>
		/// Client to Server or Server to Client
		/// Publish message
		/// </summary>
		PUBLISH = 3,

		/// <summary>
		/// Client to Server or Server to Client 
		/// Publish acknowledgment
		/// </summary>
		PUBACK = 4,

		/// <summary>
		/// Client to Server or Server to Client 
		/// Publish received (assured delivery part 1)
		/// </summary>
		PUBREC = 5,

		/// <summary>
		/// Client to Server or Server to Client 
		/// Publish release (assured delivery part 2)
		/// </summary>
		PUBREL = 6,

		/// <summary>
		/// Client to Server or Server to Client  
		/// Publish complete (assured delivery part 3)
		/// </summary>
		PUBCOMP = 7,

		/// <summary>
		/// Client to Server
		/// Client subscribe request
		/// </summary>
		SUBSCRIBE = 8,

		/// <summary>
		/// Server to Client
		/// Subscribe acknowledgment
		/// </summary>
		SUBACK = 9,

		/// <summary>
		/// Client to Server
		/// Unsubscribe request
		/// </summary>
		UNSUBSCRIBE = 10,

		/// <summary>
		/// Server to Client
		/// Unsubscribe acknowledgment
		/// </summary>
		UNSUBACK = 11,

		/// <summary>
		/// Client to Server
		/// PING request
		/// </summary>
		PINGREQ = 12,

		/// <summary>
		/// Server to Client 
		/// PING response
		/// </summary>
		PINGRESP = 13,

		/// <summary>
		/// Client to Server
		/// Client is disconnecting
		/// </summary>
		DISCONNECT = 14
	}
}
