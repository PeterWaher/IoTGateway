using System;
using System.Collections.Generic;
using System.IO;
using Waher.Content;
using Waher.Things.Metering;
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

				ChannelAddress ChannelInfo = this.NextChannelId(true);
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
		/// Tries to parse a Transfucer Access request from the message.
		/// </summary>
		/// <param name="Channel">Channel information.</param>
		/// <param name="SamplingMode">Sampling mode requested.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		/// <returns>If able to parse a TEDS object.</returns>
		public bool TryParseRequest(out ChannelAddress Channel, out SamplingMode SamplingMode,
			out double TimeoutSeconds)
		{
			try
			{
				Channel = this.NextChannelId(true);
				SamplingMode = (SamplingMode)this.NextUInt8();
				TimeoutSeconds = this.NextTimeDurationSeconds();

				return true;
			}
			catch (Exception)
			{
				Channel = null;
				SamplingMode = 0;
				TimeoutSeconds = 0;

				return false;
			}
		}

		/// <summary>
		/// Serializes a request for transducer data.
		/// </summary>
		/// <param name="NcapId">NCAP ID</param>
		/// <param name="TimId">TIM ID, or null if none.</param>
		/// <param name="ChannelId">Channel ID, or 0 if none.</param>
		/// <param name="SamplingMode">Sampling mode.</param>
		/// <param name="TimeoutSeconds">Timeout, in seconds.</param>
		/// <returns>Binary serialization.</returns>
		public static byte[] SerializeRequest(byte[] NcapId, byte[] TimId, ushort ChannelId,
			SamplingMode SamplingMode, double TimeoutSeconds)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				ms.Write(MeteringTopology.Root.ObjectId.ToByteArray(), 0, 16); // App ID
				ms.Write(NcapId ?? EmptyUuid, 0, 16);
				ms.Write(TimId ?? EmptyUuid, 0, 16);
				ms.WriteByte((byte)(ChannelId >> 8));
				ms.WriteByte((byte)ChannelId);
				ms.WriteByte((byte)SamplingMode);

				TimeoutSeconds *= 1e9 * 65536;
				ulong l = (ulong)TimeoutSeconds;
				byte[] Bin = BitConverter.GetBytes(l);
				Array.Reverse(Bin);
				ms.Write(Bin, 0, 8);

				return Ieee1451Parser.SerializeMessage(
					TransducerAccessService.SyncReadTransducerSampleDataFromAChannelOfATIM,
					MessageType.Command, ms.ToArray());
			}
		}
	}
}
