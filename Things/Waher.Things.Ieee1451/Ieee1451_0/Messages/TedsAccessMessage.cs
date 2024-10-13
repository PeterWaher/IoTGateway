using System;
using System.Threading.Tasks;
using Waher.Content.Putters;
using Waher.Things.Ieee1451.Ieee1451_0.Model;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 TEDS Access Message
	/// </summary>
	public class TedsAccessMessage : Message
	{
		private Teds data;
		private ushort errorCode;

		/// <summary>
		/// IEEE 1451.0 TEDS Access Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="TedsAccessService">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public TedsAccessMessage(NetworkServiceType NetworkServiceType, TedsAccessService TedsAccessService,
			MessageType MessageType, byte[] Body, byte[] Tail)
			: base(NetworkServiceType, (byte)TedsAccessService, MessageType, Body, Tail)
		{
			this.TedsAccessService = TedsAccessService;
		}

		/// <summary>
		/// TEDS Access Service
		/// </summary>
		public TedsAccessService TedsAccessService { get; }

		/// <summary>
		/// Name of <see cref="Message.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.TedsAccessService.ToString();

		/// <summary>
		/// Tries to parse a TEDS from the message.
		/// </summary>
		/// <param name="ErrorCode">Error code, if available.</param>
		/// <param name="Teds">TEDS object, if successful.</param>
		/// <returns>If able to parse a TEDS object.</returns>
		public bool TryParseTeds(out ushort ErrorCode, out Teds Teds)
		{
			return this.TryParseTeds(true, out ErrorCode, out Teds);
		}

		/// <summary>
		/// Tries to parse a TEDS from the message.
		/// </summary>
		/// <param name="CheckChecksum">If checksum should be checked.</param>
		/// <param name="ErrorCode">Error code, if available.</param>
		/// <param name="Teds">TEDS object, if successful.</param>
		/// <returns>If able to parse a TEDS object.</returns>
		public bool TryParseTeds(bool CheckChecksum, out ushort ErrorCode, out Teds Teds)
		{
			if (!(this.data is null))
			{
				ErrorCode = this.errorCode;
				Teds = this.data;
				return true;
			}

			Teds = null;

			try
			{
				if (this.MessageType == MessageType.Reply)
					ErrorCode = this.NextUInt16();
				else
					ErrorCode = 0;

				ChannelAddress ChannelInfo = this.NextChannelId();
				uint TedsOffset = this.NextUInt32();
				int Start = this.Position;
				uint Len = this.NextUInt32();
				if (Len < 2)
					return this.MessageType == MessageType.Command;

				Len -= 2;
				if (Len > int.MaxValue)
					return false;

				byte[] Data = this.NextUInt8Array((int)Len);
				ushort CheckSum = 0;

				while (Start < this.Position)
					CheckSum += this.Body[Start++];

				CheckSum ^= 0xffff;

				ushort CheckSum2 = this.NextUInt16();
				if (CheckChecksum && CheckSum != CheckSum2)
					return false;

				Binary TedsBlock = new Binary(Data);
				ParsingState State = new ParsingState();

				this.data = Teds = new Teds(ChannelInfo, TedsBlock.ParseTedsRecords(State));
				this.errorCode = ErrorCode;
				return true;
			}
			catch (Exception)
			{
				ErrorCode = 0xffff;
				return false;
			}
		}

		/// <summary>
		/// Tries to parse a TEDS request from the message.
		/// </summary>
		/// <param name="Channel">Channel information.</param>
		/// <param name="TedsAccesCode">What TED is requested.</param>
		/// <param name="TedsOffset">TEDS offset.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		/// <returns>If able to parse a TEDS object.</returns>
		public bool TryParseRequest(out ChannelAddress Channel, out TedsAccesCode TedsAccesCode, 
			out uint TedsOffset, out double TimeoutSeconds)
		{
			try
			{
				Channel = this.NextChannelId();
				TedsAccesCode = (TedsAccesCode)this.NextUInt8();
				TedsOffset = this.NextUInt32();
				TimeoutSeconds = this.NextTimeDurationSeconds();

				return true;
			}
			catch (Exception)
			{
				Channel = null;
				TedsAccesCode = 0;
				TedsOffset = 0;
				TimeoutSeconds = 0;

				return false;
			}
		}

		/// <summary>
		/// Process incoming message.
		/// </summary>
		/// <param name="Client">Client interface.</param>
		public override Task ProcessIncoming(IClient Client)
		{
			switch (this.MessageType)
			{
				case MessageType.Command:
					return Client.TedsAccessCommand(this);

				case MessageType.Reply:
					return Client.TedsAccessReply(this);

				case MessageType.Announcement:
					return Client.TedsAccessAnnouncement(this);

				case MessageType.Notification:
					return Client.TedsAccessNotification(this);

				case MessageType.Callback:
					return Client.TedsAccessCallback(this);

				default:
					return Task.CompletedTask;
			}
		}
	}
}
