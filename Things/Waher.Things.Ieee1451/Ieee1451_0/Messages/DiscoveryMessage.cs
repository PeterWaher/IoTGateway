using System.IO;
using System;
using Waher.Things.Metering;
using System.Text;
using Waher.Networking.Sniffers;

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
		/// <param name="Sniffable">Sniffable interface on which the message was received.</param>
		public DiscoveryMessage(NetworkServiceType NetworkServiceType, DiscoveryService DiscoveryService,
			MessageType MessageType, byte[] Body, byte[] Tail, ISniffable Sniffable)
			: base(NetworkServiceType, (byte)DiscoveryService, MessageType, Body, Tail, Sniffable)
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
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="NcapId">NCAP ID</param>
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
		/// Serializes a TIM discovery response.
		/// </summary>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="NcapId">NCAP ID</param>
		/// <param name="TimIds">TIM IDs</param>
		/// <param name="TimNames">Names of TIMs</param>
		/// <returns>Binary serialization.</returns>
		public static byte[] SerializeResponse(ushort ErrorCode, byte[] NcapId,
			byte[][] TimIds, string[] TimNames)
		{
			if (NcapId is null || NcapId.Length != 16)
				throw new ArgumentException("Invalid NCAP UUID.", nameof(NcapId));

			if (TimIds is null || TimIds.Length == 0)
				throw new ArgumentException("No TIM IDs.", nameof(TimIds));

			int i, c = TimIds.Length;
			if (TimNames.Length != c)
				throw new ArgumentException("Invalid number of TIM names.", nameof(TimNames));

			for (i = 0; i < c; i++)
			{
				if (TimIds[i] is null || TimIds[i].Length != 16)
					throw new ArgumentException("Invalid TIM UUID.", nameof(TimIds));
			}

			using (MemoryStream ms = new MemoryStream())
			{
				ms.WriteByte((byte)(ErrorCode >> 8));
				ms.WriteByte((byte)ErrorCode);
				ms.Write(MeteringTopology.Root.ObjectId.ToByteArray(), 0, 16); // App ID
				ms.Write(NcapId, 0, 16); // NCAP ID

				ms.WriteByte((byte)(c >> 8));
				ms.WriteByte((byte)c);

				for (i = 0; i < c; i++)
					ms.Write(TimIds[i], 0, 16); // TIM ID

				for (i = 0; i < c; i++)
				{
					byte[] Bin = Encoding.UTF8.GetBytes(TimNames[i]);
					ms.Write(Bin, 0, Bin.Length);
					ms.WriteByte(0);
				}

				return Ieee1451Parser.SerializeMessage(DiscoveryService.NCAPTIMDiscovery, MessageType.Reply, ms.ToArray());
			}
		}

		/// <summary>
		/// Serializes a Transducer channel discovery response.
		/// </summary>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="NcapId">NCAP ID</param>
		/// <param name="TimId">TIM ID</param>
		/// <param name="ChannelIds">Transfucer channel IDs</param>
		/// <param name="ChannelNames">Names of transducer channels</param>
		/// <returns>Binary serialization.</returns>
		public static byte[] SerializeResponse(ushort ErrorCode, byte[] NcapId,
			byte[] TimId, ushort[] ChannelIds, string[] ChannelNames)
		{
			if (NcapId is null || NcapId.Length != 16)
				throw new ArgumentException("Invalid NCAP UUID.", nameof(NcapId));

			if (TimId is null || TimId.Length != 16)
				throw new ArgumentException("Invalid TIM UUID.", nameof(TimId));

			if (ChannelIds is null || ChannelIds.Length == 0)
				throw new ArgumentException("No Transducer channel IDs.", nameof(ChannelIds));

			int i, c = ChannelIds.Length;
			if (ChannelNames.Length != c)
				throw new ArgumentException("Invalid number of Transducer channel names.", nameof(ChannelNames));

			using (MemoryStream ms = new MemoryStream())
			{
				ms.WriteByte((byte)(ErrorCode >> 8));
				ms.WriteByte((byte)ErrorCode);
				ms.Write(MeteringTopology.Root.ObjectId.ToByteArray(), 0, 16); // App ID
				ms.Write(NcapId, 0, 16); // NCAP ID
				ms.Write(TimId, 0, 16); // TIM ID

				ms.WriteByte((byte)(c >> 8));
				ms.WriteByte((byte)c);

				for (i = 0; i < c; i++)
				{
					ushort ChannelId = ChannelIds[i];

					ms.WriteByte((byte)(ChannelId >> 8));
					ms.WriteByte((byte)ChannelId);
				}

				for (i = 0; i < c; i++)
				{
					byte[] Bin = Encoding.UTF8.GetBytes(ChannelNames[i]);
					ms.Write(Bin, 0, Bin.Length);
					ms.WriteByte(0);
				}

				return Ieee1451Parser.SerializeMessage(DiscoveryService.NCAPTIMTransducerDiscovery, MessageType.Reply, ms.ToArray());
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
						ErrorCode = this.NextUInt16(nameof(ErrorCode));
					else
						ErrorCode = 0;

					switch (this.DiscoveryService)
					{
						case DiscoveryService.NCAPAnnouncement:
							Channel = this.NextNcapId(false);
							string Name = this.NextString(nameof(Name));
							AddressType AddressType = this.NextUInt8<AddressType>(nameof(AddressType));
							byte[] Address = this.NextUInt8Array(nameof(Address));
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
							Name = this.NextString(nameof(Name));
							Data = new DiscoveryDataEntity(Channel, Name);
							return true;

						case DiscoveryService.NCAPTIMDeparture:
							Channel = this.NextTimId(false);
							Data = new DiscoveryData(Channel);
							return true;

						case DiscoveryService.NCAPTIMTransducerAnnouncement:
							Channel = this.NextChannelId(false);
							Name = this.NextString(nameof(Name));
							Data = new DiscoveryDataEntity(Channel, Name);
							return true;

						case DiscoveryService.NCAPTIMTransducerDeparture:
							Channel = this.NextChannelId(false);
							Data = new DiscoveryData(Channel);
							return true;

						case DiscoveryService.NCAPDiscovery:
							Channel = this.NextNcapId(true);
							Name = this.NextString(nameof(Name));
							AddressType = this.NextUInt8<AddressType>(nameof(AddressType));
							Address = this.NextUInt8Array(nameof(Address));
							Data = new DiscoveryDataIpEntity(Channel, Name, AddressType, Address);
							return true;

						case DiscoveryService.NCAPTIMDiscovery:
							Channel = this.NextNcapId(true);
							ushort Nr = this.NextUInt16(nameof(Nr));
							byte[][] Ids = this.NextUuidArray(nameof(Ids), Nr);
							string[] Names = this.NextStringArray(nameof(Names), Nr);
							Data = new DiscoveryDataEntities(Channel, Names, Ids);
							return true;

						case DiscoveryService.NCAPTIMTransducerDiscovery:
							Channel = this.NextTimId(true);
							Nr = this.NextUInt16(nameof(Nr));
							ushort[] Channels = this.NextUInt16Array(nameof(Channels), Nr);
							Names = this.NextStringArray(nameof(Names), Nr);
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
