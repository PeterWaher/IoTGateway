namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Message
	/// </summary>
	public abstract class Ieee1451_0Message : Ieee1451_0Binary
	{
		/// <summary>
		/// IEEE 1451.0 Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="NetworkServiceId">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public Ieee1451_0Message(NetworkServiceType NetworkServiceType, byte NetworkServiceId,
			MessageType MessageType, byte[] Body, byte[] Tail)
			: base(Body)
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
		public Ieee1451_0AppId NextAppId()
		{
			return new Ieee1451_0AppId()
			{
				ErrorCode = this.NextUInt16(),
				ApplicationId = this.NextGuid()
			};
		}

		/// <summary>
		/// Parses an NCAP ID from the message.
		/// </summary>
		/// <returns>NCAP ID information.</returns>
		public Ieee1451_0NcapId NextNcapId()
		{
			return new Ieee1451_0NcapId()
			{
				ErrorCode = this.NextUInt16(),
				ApplicationId = this.NextGuid(),
				NcapId = this.NextGuid()
			};
		}

		/// <summary>
		/// Parses an TIM ID from the message.
		/// </summary>
		/// <returns>TIM ID information.</returns>
		public Ieee1451_0TimId NextTimId()
		{
			return new Ieee1451_0TimId()
			{
				ErrorCode = this.NextUInt16(),
				ApplicationId = this.NextGuid(),
				NcapId = this.NextGuid(),
				TimId = this.NextGuid()
			};
		}

		/// <summary>
		/// Parses a Channel ID from the message.
		/// </summary>
		/// <returns>Channel ID information.</returns>
		public Ieee1451_0ChannelId NextChannelId()
		{
			return new Ieee1451_0ChannelId()
			{
				ErrorCode = this.NextUInt16(),
				ApplicationId = this.NextGuid(),
				NcapId = this.NextGuid(),
				TimId = this.NextGuid(),
				ChannelId = this.NextUInt16()
			};
		}
	}
}
