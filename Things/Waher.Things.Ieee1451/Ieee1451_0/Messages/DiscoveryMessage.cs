using System.IO;
using System;
using Waher.Things.Metering;

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
		/// Serializes a request for transducer data.
		/// </summary>
		/// <param name="Service">Discovery service</param>
		/// <returns>Binary serialization.</returns>
		public static byte[] SerializeRequest(DiscoveryService Service)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				ms.Write(MeteringTopology.Root.ObjectId.ToByteArray(), 0, 16); // App ID

				return Ieee1451Parser.SerializeMessage(Service, MessageType.Command, ms.ToArray());
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
