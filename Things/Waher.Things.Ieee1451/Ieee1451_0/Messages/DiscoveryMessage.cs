using System.IO;
using System;
using Waher.Things.Metering;
using Waher.Things.Ieee1451.Ieee1451_1_6;
using System.Text;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Discovery Message
	/// </summary>
	public class DiscoveryMessage : Message
	{
		/// <summary>
		/// IEEE 1451.0 Discovery Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="DiscoveryService">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public DiscoveryMessage(NetworkServiceType NetworkServiceType, DiscoveryService DiscoveryService,
			MessageType MessageType, byte[] Body, byte[] Tail)
			: base(NetworkServiceType, (byte)DiscoveryService, MessageType, Body, Tail)
		{
			this.DiscoveryService = DiscoveryService;
		}

		/// <summary>
		/// Discovery Service
		/// </summary>
		public DiscoveryService DiscoveryService { get; }

		/// <summary>
		/// Name of <see cref="Message.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.DiscoveryService.ToString();

		/// <summary>
		/// Serializes an NCAP Discovery request.
		/// </summary>
		/// <returns>Binary serialization.</returns>
		public static byte[] SerializeRequest()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				ms.Write(MeteringTopology.Root.ObjectId.ToByteArray(), 0, 16); // App ID

				return Ieee1451Parser.SerializeMessage(DiscoveryService.NCAPDiscovery, MessageType.Command, ms.ToArray());
			}
		}

		/// <summary>
		/// Serializes an NCAP TIM Discovery request.
		/// </summary>
		/// <param name="NcapId">NCAP ID</param>
		/// <returns>Binary serialization.</returns>
		public static byte[] SerializeRequest(byte[] NcapId)
		{
			if (NcapId is null || NcapId.Length != 16)
				throw new ArgumentException("Invalid NCAP UUID.", nameof(NcapId));

			using (MemoryStream ms = new MemoryStream())
			{
				ms.Write(MeteringTopology.Root.ObjectId.ToByteArray(), 0, 16); // App ID
				ms.Write(NcapId, 0, 16); // NCAP ID

				return Ieee1451Parser.SerializeMessage(DiscoveryService.NCAPTIMDiscovery, MessageType.Command, ms.ToArray());
			}
		}

		/// <summary>
		/// Serializes an NCAP TIM Transducer Discovery request.
		/// </summary>
		/// <param name="NcapId">NCAP ID</param>
		/// <param name="TimId">TIM ID</param>
		/// <returns>Binary serialization.</returns>
		public static byte[] SerializeRequest(byte[] NcapId, byte[] TimId)
		{
			if (NcapId is null || NcapId.Length != 16)
				throw new ArgumentException("Invalid NCAP UUID.", nameof(NcapId));

			if (TimId is null || TimId.Length != 16)
				throw new ArgumentException("Invalid TIM UUID.", nameof(TimId));

			using (MemoryStream ms = new MemoryStream())
			{
				ms.Write(MeteringTopology.Root.ObjectId.ToByteArray(), 0, 16); // App ID
				ms.Write(NcapId, 0, 16); // NCAP ID
				ms.Write(TimId, 0, 16); // TIM ID

				return Ieee1451Parser.SerializeMessage(DiscoveryService.NCAPTIMTransducerDiscovery, MessageType.Command, ms.ToArray());
			}
		}

		/// <summary>
		/// Serializes an NCAP discovery response.
		/// </summary>
		/// <param name="NcapId">NCAP ID</param>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="Name">Name of NCAP</param>
		/// <returns>Binary serialization.</returns>
		public static byte[] SerializeResponse(ushort ErrorCode, byte[] NcapId,
			string Name)
		{
			if (NcapId is null || NcapId.Length != 16)
				throw new ArgumentException("Invalid NCAP UUID.", nameof(NcapId));

			using (MemoryStream ms = new MemoryStream())
			{
				ms.WriteByte((byte)(ErrorCode >> 8));
				ms.WriteByte((byte)ErrorCode);
				ms.Write(MeteringTopology.Root.ObjectId.ToByteArray(), 0, 16); // App ID
				ms.Write(NcapId, 0, 16); // NCAP ID

				byte[] Bin = Encoding.UTF8.GetBytes(Name);
				ms.Write(Bin, 0, Bin.Length);
				ms.WriteByte(0);
				ms.WriteByte(1);    // IPv4
				ms.WriteByte(127);
				ms.WriteByte(0);
				ms.WriteByte(0);
				ms.WriteByte(1);

				return Ieee1451Parser.SerializeMessage(DiscoveryService.NCAPDiscovery, MessageType.Reply, ms.ToArray());
			}
		}

		/// <summary>
		/// Tries to parse a Discovery message.
		/// </summary>
		/// <param name="ErrorCode">Error code, if available.</param>
		/// <param name="Data">Parsed data.</param>
		/// <returns>If able to parse a Discovery message.</returns>
		public bool TryParseMessage(out ushort ErrorCode, out DiscoveryData Data)
		{
			try
			{
				ChannelAddress Channel;

				if (this.MessageType == MessageType.Command)
				{
					ErrorCode = 0;

					switch (this.DiscoveryService)
					{
						case DiscoveryService.NCAPDiscovery:
							Channel = this.NextAppId();
							break;

						case DiscoveryService.NCAPTIMDiscovery:
							Channel = this.NextNcapId(true);
							break;

						case DiscoveryService.NCAPTIMTransducerDiscovery:
							Channel = this.NextTimId(true);
							break;

						default:
							Data = null;
							return false;
					}

					Data = new DiscoveryData(Channel);
					return true;
				}
				else
				{
					if (this.MessageType == MessageType.Reply)
						ErrorCode = this.NextUInt16();
					else
						ErrorCode = 0;

					switch (this.DiscoveryService)
					{
						case DiscoveryService.NCAPAnnouncement:
							Channel = this.NextNcapId(false);
							string Name = this.NextString();
							AddressType AddressType = (AddressType)this.NextUInt8();
							byte[] Address = this.NextUInt8Array();
							Data = new DiscoveryDataIpEntity(Channel, Name, AddressType, Address);
							return true;

						case DiscoveryService.NCAPDeparture:
							Channel = this.NextNcapId(false);
							Data = new DiscoveryData(Channel);
							return true;

						case DiscoveryService.NCAPAbandonment:
							Channel = this.NextNcapId(false);
							Data = new DiscoveryData(Channel);
							return true;

						case DiscoveryService.NCAPTIMAnnouncement:
							Channel = this.NextTimId(false);
							Name = this.NextString();
							Data = new DiscoveryDataEntity(Channel, Name);
							return true;

						case DiscoveryService.NCAPTIMDeparture:
							Channel = this.NextTimId(false);
							Data = new DiscoveryData(Channel);
							return true;

						case DiscoveryService.NCAPTIMTransducerAnnouncement:
							Channel = this.NextChannelId(false);
							Name = this.NextString();
							Data = new DiscoveryDataEntity(Channel, Name);
							return true;

						case DiscoveryService.NCAPTIMTransducerDeparture:
							Channel = this.NextChannelId(false);
							Data = new DiscoveryData(Channel);
							return true;

						case DiscoveryService.NCAPDiscovery:
							Channel = this.NextNcapId(true);
							Name = this.NextString();
							AddressType = (AddressType)this.NextUInt8();
							Address = this.NextUInt8Array();
							Data = new DiscoveryDataIpEntity(Channel, Name, AddressType, Address);
							return true;

						case DiscoveryService.NCAPTIMDiscovery:
							Channel = this.NextNcapId(true);
							ushort Nr = this.NextUInt16();
							byte[][] Ids = this.NextUuidArray(Nr);
							string[] Names = this.NextStringArray(Nr);
							Data = new DiscoveryDataEntities(Channel, Names, Ids);
							return true;

						case DiscoveryService.NCAPTIMTransducerDiscovery:
							Channel = this.NextTimId(true);
							Nr = this.NextUInt16();
							ushort[] Channels = this.NextUInt16Array(Nr);
							Names = this.NextStringArray(Nr);
							Data = new DiscoveryDataChannels(Channel, Names, Channels);
							return true;

						default:
							Data = null;
							return false;
					}
				}
			}
			catch (Exception)
			{
				ErrorCode = 0xffff;
				Data = null;

				return false;
			}
		}

	}
}
