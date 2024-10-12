using System.Threading.Tasks;
using Waher.Things.Ieee1451.Ieee1451_0.Model;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Message
	/// </summary>
	public abstract class Message : Binary
	{
		/// <summary>
		/// IEEE 1451.0 Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="NetworkServiceId">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public Message(NetworkServiceType NetworkServiceType, byte NetworkServiceId,
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
		public ApplicationAddress NextAppId()
		{
			return new ApplicationAddress()
			{
				ApplicationId = this.NextUuid()
			};
		}

		/// <summary>
		/// Parses an NCAP ID from the message.
		/// </summary>
		/// <returns>NCAP ID information.</returns>
		public NcapAddress NextNcapId()
		{
			return new NcapAddress()
			{
				ApplicationId = this.NextUuid(),
				NcapId = this.NextUuid()
			};
		}

		/// <summary>
		/// Parses an TIM ID from the message.
		/// </summary>
		/// <returns>TIM ID information.</returns>
		public TimAdress NextTimId()
		{
			return new TimAdress()
			{
				ApplicationId = this.NextUuid(),
				NcapId = this.NextUuid(),
				TimId = this.NextUuid()
			};
		}

		/// <summary>
		/// Parses a Channel ID from the message.
		/// </summary>
		/// <returns>Channel ID information.</returns>
		public ChannelAddress NextChannelId()
		{
			return new ChannelAddress()
			{
				ApplicationId = this.NextUuid(),
				NcapId = this.NextUuid(),
				TimId = this.NextUuid(),
				ChannelId = this.NextUInt16()
			};
		}

		/// <summary>
		/// Process incoming message.
		/// </summary>
		/// <param name="Client">Client interface.</param>
		public abstract Task ProcessIncoming(IClient Client);
	}
}
