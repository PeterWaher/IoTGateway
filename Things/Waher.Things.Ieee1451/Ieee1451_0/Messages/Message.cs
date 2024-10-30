using Waher.Networking.Sniffers;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Message
	/// </summary>
	public abstract class Message : Binary
	{
		/// <summary>
		/// Empty UUID (16 zero bytes)
		/// </summary>
		public static readonly byte[] EmptyUuid = new byte[16];

		/// <summary>
		/// IEEE 1451.0 Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="NetworkServiceId">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		/// <param name="Sniffable">Sniffable interface on which the message was received.</param>
		public Message(NetworkServiceType NetworkServiceType, byte NetworkServiceId,
			MessageType MessageType, byte[] Body, byte[] Tail, ISniffable Sniffable)
			: base(Body, Sniffable, false)
		{
			this.NetworkServiceType = NetworkServiceType;
			this.NetworkServiceId = NetworkServiceId;
			this.MessageType = MessageType;
			this.Tail = Tail;
		}

		/// <summary>
		/// Network Service Type
		/// </summary>
		public NetworkServiceType NetworkServiceType { get; }

		/// <summary>
		/// Network Service ID
		/// </summary>
		public byte NetworkServiceId { get; }

		/// <summary>
		/// Name of <see cref="NetworkServiceId"/>
		/// </summary>
		public abstract string NetworkServiceIdName { get; }

		/// <summary>
		/// Message Type
		/// </summary>
		public MessageType MessageType { get; }

		/// <summary>
		/// Bytes that are received after the body.
		/// </summary>
		public byte[] Tail { get; }

		/// <summary>
		/// Parses an Application ID from the message.
		/// </summary>
		/// <returns>App ID information.</returns>
		public ChannelAddress NextAppId() => this.NextChannelId(true, false, false, false);

		/// <summary>
		/// Parses an NCAP ID from the message.
		/// </summary>
		/// <param name="AppId">If App ID should be parsed.</param>
		/// <returns>NCAP ID information.</returns>
		public ChannelAddress NextNcapId(bool AppId) => this.NextChannelId(AppId, true, false, false);

		/// <summary>
		/// Parses an TIM ID from the message.
		/// </summary>
		/// <param name="AppId">If App ID should be parsed.</param>
		/// <returns>TIM ID information.</returns>
		public ChannelAddress NextTimId(bool AppId) => this.NextChannelId(AppId, true, true, false);

		/// <summary>
		/// Parses a Channel ID from the message.
		/// </summary>
		/// <param name="AppId">If App ID should be parsed.</param>
		/// <returns>Channel ID information.</returns>
		public ChannelAddress NextChannelId(bool AppId) => this.NextChannelId(AppId, true, true, true);

		/// <summary>
		/// Parses a Channel ID from the message.
		/// </summary>
		/// <param name="AppId">If App ID should be parsed.</param>
		/// <param name="NcapId">If NCAP ID should be parsed.</param>
		/// <param name="TimId">If TIM ID should be parsed.</param>
		/// <param name="ChannelId">If Channel ID should be parsed.</param>
		/// <returns>Channel ID information.</returns>
		public ChannelAddress NextChannelId(bool AppId, bool NcapId, bool TimId, bool ChannelId)
		{
			return new ChannelAddress()
			{
				ApplicationId = AppId ? this.NextUuid(nameof(AppId)) : null,
				NcapId = NcapId ? this.NextUuid(nameof(NcapId)) : null,
				TimId = TimId ? this.NextUuid(nameof(TimId)) : null,
				ChannelId = ChannelId ? this.NextUInt16(nameof(ChannelId)) : (ushort)0
			};
		}
	}
}
