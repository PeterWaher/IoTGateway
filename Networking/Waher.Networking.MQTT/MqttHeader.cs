using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.MQTT
{
	/// <summary>
	/// Contains information in the MQTT header.
	/// </summary>
	public struct MqttHeader
	{
		/// <summary>
		/// Control packet type.
		/// </summary>
		public MqttControlPacketType ControlPacketType;

		/// <summary>
		/// Quality of Service level.
		/// </summary>
		public MqttQualityOfService QualityOfService;

		/// <summary>
		/// Retain flag.
		/// </summary>
		public bool Retain;

		/// <summary>
		/// Duplicate delivery flag.
		/// </summary>
		public bool Duplicate;

		/// <summary>
		/// The Remaining Length is the number of bytes remaining within the current packet, including data in the 
		/// variable header and the payload. The Remaining Length does not include the bytes used to encode the Remaining Length.
		/// </summary>
		public int RemainingLength;

		/// <summary>
		/// The variable header component of many of the Control Packet types includes a 2 byte Packet Identifier field.
		/// </summary>
#pragma warning disable
		public ushort PacketIdentifier;
#pragma warning enable

		/// <summary>
		/// Parses a header.
		/// </summary>
		/// <param name="Input">Binary input.</param>
		/// <returns>Header.</returns>
		public static MqttHeader Parse(BinaryInput Input)
		{
			MqttHeader Result = new MqttHeader();

			// Fixed part:

			byte b = Input.ReadByte();

			Result.ControlPacketType = (MqttControlPacketType)(b >> 4);
			Result.Retain = (b & 1) != 0;
			Result.QualityOfService = (MqttQualityOfService)((b >> 1) & 3);
			Result.Duplicate = (b & 8) != 0;

			b = Input.ReadByte();

			int Offset = 0;

			Result.RemainingLength = b & 127;
			while ((b & 128) != 0)
			{
				b = Input.ReadByte();
				Offset += 7;
				Result.RemainingLength |= (b & 127) << Offset;
			}

			// Variable part:

			switch (Result.ControlPacketType)
			{
				case MqttControlPacketType.CONNECT:
				case MqttControlPacketType.CONNACK:
				case MqttControlPacketType.PINGREQ:
				case MqttControlPacketType.PINGRESP:
				case MqttControlPacketType.DISCONNECT:
				case MqttControlPacketType.PUBLISH:
				default:
					Result.PacketIdentifier = 0;
					break;

				case MqttControlPacketType.PUBACK:
				case MqttControlPacketType.PUBREC:
				case MqttControlPacketType.PUBREL:
				case MqttControlPacketType.PUBCOMP:
				case MqttControlPacketType.SUBSCRIBE:
				case MqttControlPacketType.SUBACK:
				case MqttControlPacketType.UNSUBSCRIBE:
				case MqttControlPacketType.UNSUBACK:
					Result.PacketIdentifier = Input.ReadByte();
					Result.PacketIdentifier <<= 8;
					Result.PacketIdentifier |= Input.ReadByte();

					Result.RemainingLength -= 2;
					break;
			}

			return Result;
		}
	}
}
