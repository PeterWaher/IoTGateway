using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Things.Ieee1451.Ieee1451_0.Model;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// IEEE 1451.0 Transducer Access Message
	/// </summary>
	public class TransducerAccessMessage : Message
	{
		private TransducerData data;
		private ushort errorCode;

		/// <summary>
		/// IEEE 1451.0 Transducer Access Message
		/// </summary>
		/// <param name="NetworkServiceType">Network Service Type</param>
		/// <param name="TransducerAccessService">Network Service ID</param>
		/// <param name="MessageType">Message Type</param>
		/// <param name="Body">Binary Body</param>
		/// <param name="Tail">Bytes that are received after the body.</param>
		public TransducerAccessMessage(NetworkServiceType NetworkServiceType, TransducerAccessService TransducerAccessService, 
			MessageType MessageType, byte[] Body, byte[] Tail)
			: base(NetworkServiceType, (byte)TransducerAccessService, MessageType, Body, Tail)
		{
			this.TransducerAccessService = TransducerAccessService;
		}

		/// <summary>
		/// Transducer Access Service
		/// </summary>
		public TransducerAccessService TransducerAccessService { get; }

		/// <summary>
		/// Name of <see cref="Message.NetworkServiceId"/>
		/// </summary>
		public override string NetworkServiceIdName => this.TransducerAccessService.ToString();

		/// <summary>
		/// Tries to parse Transducer Data from the message.
		/// </summary>
		/// <param name="Thing">Thing associated with fields.</param>
		/// <param name="ErrorCode">Error code, if available.</param>
		/// <param name="Data">Transducer Data, if successful.</param>
		/// <returns>If able to parse transducer data.</returns>
		public bool TryParseTransducerData(ThingReference Thing, out ushort ErrorCode, 
			out TransducerData Data)
		{
			if (!(this.data is null))
			{
				ErrorCode = this.errorCode;
				Data = this.data;
				return true;
			}

			Data = null;

			try
			{
				if (this.MessageType == MessageType.Reply)
					ErrorCode = this.NextUInt16();
				else
					ErrorCode = 0;

				ChannelAddress ChannelInfo = this.NextChannelId();
				List<Field> Fields = new List<Field>();

				switch (this.TransducerAccessService)
				{
					case TransducerAccessService.SyncReadTransducerSampleDataFromAChannelOfATIM:
						string Value = this.NextString();
						DateTime Timestamp = this.NextTime().ToDateTime();

						if (CommonTypes.TryParse(Value, out double NumericValue, out byte NrDecimals))
						{
							Fields.Add(new QuantityField(Thing, Timestamp, "Value", 
								NumericValue, NrDecimals, string.Empty, FieldType.Momentary,
								FieldQoS.AutomaticReadout));
						}
						else
						{
							Fields.Add(new StringField(Thing, Timestamp, "Value", Value,
								FieldType.Momentary, FieldQoS.AutomaticReadout));
						}

						// TODO: Unit
						// TODO: Name
						// TODO: Historic value vs. Momentary value
						break;

					default:
						return false;
				}

				this.data = Data = new TransducerData(ChannelInfo, Fields.ToArray());
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
		/// Process incoming message.
		/// </summary>
		/// <param name="Client">Client interface.</param>
		public override Task ProcessIncoming(IClient Client)
		{
			switch(this.MessageType)
			{
				case MessageType.Command:
					return Client.TransducerAccessCommand(this);

				case MessageType.Reply:
					return Client.TransducerAccessReply(this);

				case MessageType.Announcement:
					return Client.TransducerAccessAnnouncement(this);

				case MessageType.Notification:
					return Client.TransducerAccessNotification(this);

				case MessageType.Callback:
					return Client.TransducerAccessCallback(this);

				default:
					return Task.CompletedTask;
			}
		}
	}
}
