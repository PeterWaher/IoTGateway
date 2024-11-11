using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Content;
using Waher.Networking;
using Waher.Script.Units;
using Waher.Security;
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
		/// <param name="ComLayer">Sniffable interface on which the message was received.</param>
		public TransducerAccessMessage(NetworkServiceType NetworkServiceType, TransducerAccessService TransducerAccessService,
			MessageType MessageType, byte[] Body, byte[] Tail, ICommunicationLayer ComLayer)
			: base(NetworkServiceType, (byte)TransducerAccessService, MessageType, Body, Tail, ComLayer)
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
		/// <param name="ChannelTeds">Channel TEDS, if available.</param>
		/// <param name="PreferredUnit">Preferred unit.</param>
		/// <param name="ErrorCode">Error code, if available.</param>
		/// <param name="Data">Transducer Data, if successful.</param>
		/// <returns>If able to parse transducer data.</returns>
		public bool TryParseTransducerData(ThingReference Thing, Teds ChannelTeds, Unit PreferredUnit, out ushort ErrorCode, out TransducerData Data)
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
				this.Position = 0;
				if (this.MessageType == MessageType.Reply)
					ErrorCode = this.NextUInt16(nameof(ErrorCode));
				else
					ErrorCode = 0;

				ChannelAddress ChannelInfo = this.NextChannelId(true);
				List<Field> Fields = new List<Field>();

				switch (this.TransducerAccessService)
				{
					case TransducerAccessService.SyncReadTransducerSampleDataFromAChannelOfATIM:
						string Value = this.NextString(nameof(Value));
						DateTime Timestamp = this.NextTime(nameof(Timestamp)).ToDateTime();

						if (CommonTypes.TryParse(Value, out double NumericValue, out byte NrDecimals))
						{
							string FieldName = "Value";
							string Unit = string.Empty;

							if (!(ChannelTeds is null))
							{
								FieldName = ChannelTeds.FieldName ?? FieldName;

								if (!(PreferredUnit is null) &&
									!(ChannelTeds.FieldUnit is null) &&
									Script.Units.Unit.TryConvert(NumericValue, ChannelTeds.FieldUnit, NrDecimals, PreferredUnit, out double ConvertedValue, out byte NrDec2))
								{
									NumericValue = ConvertedValue;
									Unit = PreferredUnit.ToString();
									NrDecimals = NrDec2;
								}
								else
									Unit = ChannelTeds.Unit;
							}

							Fields.Add(new QuantityField(Thing, Timestamp, FieldName,
								NumericValue, NrDecimals, Unit, FieldType.Momentary,
								FieldQoS.AutomaticReadout));
						}
						else
						{
							Fields.Add(new StringField(Thing, Timestamp, "Value", Value,
								FieldType.Momentary, FieldQoS.AutomaticReadout));
						}

						// TODO: Historic value vs. Momentary value
						break;

					default:
						return false;
				}

				Data = new TransducerData(ChannelInfo, Fields.ToArray());

				if (!(ChannelTeds is null))
					this.data = Data;

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
				SamplingMode = this.NextUInt8<SamplingMode>(nameof(SamplingMode));
				TimeoutSeconds = this.NextTimeDurationSeconds(nameof(TimeoutSeconds));

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
		/// <param name="SnifferOutput">Optional sniffer output.</param>
		/// <returns>Binary serialization.</returns>
		public static byte[] SerializeRequest(byte[] NcapId, byte[] TimId, ushort ChannelId,
			SamplingMode SamplingMode, double TimeoutSeconds, StringBuilder SnifferOutput)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				byte[] AppId = MeteringTopology.Root.ObjectId.ToByteArray();

				ms.Write(AppId, 0, 16);
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

				byte[] Result = Ieee1451Parser.SerializeMessage(
					TransducerAccessService.SyncReadTransducerSampleDataFromAChannelOfATIM,
					MessageType.Command, ms.ToArray(), SnifferOutput);

				if (!(SnifferOutput is null))
				{
					SnifferOutput.Append("App ID: ");
					SnifferOutput.AppendLine(Hashes.BinaryToString(AppId));
					SnifferOutput.Append("NCAP ID: ");
					SnifferOutput.AppendLine(Hashes.BinaryToString(NcapId ?? EmptyUuid));
					SnifferOutput.Append("TIM ID: ");
					SnifferOutput.AppendLine(Hashes.BinaryToString(TimId ?? EmptyUuid));
					SnifferOutput.Append("Channel ID: ");
					SnifferOutput.AppendLine(ChannelId.ToString());
					SnifferOutput.Append("Sampling Mode: ");
					SnifferOutput.AppendLine(SamplingMode.ToString());
					SnifferOutput.Append("Timeout (s): ");
					SnifferOutput.AppendLine(TimeoutSeconds.ToString());
				}

				return Result;
			}
		}

		/// <summary>
		/// Serializes a request for transducer data.
		/// </summary>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="NcapId">NCAP ID</param>
		/// <param name="TimId">TIM ID, or null if none.</param>
		/// <param name="ChannelId">Channel ID, or 0 if none.</param>
		/// <param name="Value">String-representation of value.</param>
		/// <param name="Timestamp">Timestamp of value.</param>
		/// <param name="SnifferOutput">Optional sniffer output.</param>
		/// <returns>Binary serialization.</returns>
		public static byte[] SerializeResponse(ushort ErrorCode, byte[] NcapId, byte[] TimId, ushort ChannelId,
			string Value, DateTime Timestamp, StringBuilder SnifferOutput)
		{
			if (NcapId is null)
				NcapId = new byte[16];
			else if (NcapId.Length != 16)
				throw new ArgumentException("Invalid NCAP UUID.", nameof(NcapId));

			if (TimId is null)
				TimId = new byte[16];
			else if (TimId.Length != 16)
				throw new ArgumentException("Invalid TIM UUID.", nameof(TimId));

			using (MemoryStream ms = new MemoryStream())
			{
				byte[] AppId = MeteringTopology.Root.ObjectId.ToByteArray();

				ms.WriteByte((byte)(ErrorCode >> 8));
				ms.WriteByte((byte)ErrorCode);
				ms.Write(AppId, 0, 16);
				ms.Write(NcapId, 0, 16);
				ms.Write(TimId, 0, 16);
				ms.WriteByte((byte)(ChannelId >> 8));
				ms.WriteByte((byte)ChannelId);

				byte[] Bin = Encoding.UTF8.GetBytes(Value);
				ms.Write(Bin, 0, Bin.Length);
				ms.WriteByte(0);

				Time Time = new Time(Timestamp);

				ms.WriteByte((byte)(Time.Seconds >> 40));
				ms.WriteByte((byte)(Time.Seconds >> 32));
				ms.WriteByte((byte)(Time.Seconds >> 24));
				ms.WriteByte((byte)(Time.Seconds >> 16));
				ms.WriteByte((byte)(Time.Seconds >> 8));
				ms.WriteByte((byte)Time.Seconds);
				ms.WriteByte((byte)(Time.NanoSeconds >> 24));
				ms.WriteByte((byte)(Time.NanoSeconds >> 16));
				ms.WriteByte((byte)(Time.NanoSeconds >> 8));
				ms.WriteByte((byte)Time.NanoSeconds);

				byte[] Result = Ieee1451Parser.SerializeMessage(
					TransducerAccessService.SyncReadTransducerSampleDataFromAChannelOfATIM,
					MessageType.Reply, ms.ToArray(), SnifferOutput);

				if (!(SnifferOutput is null))
				{
					SnifferOutput.Append("App ID: ");
					SnifferOutput.AppendLine(Hashes.BinaryToString(AppId));
					SnifferOutput.Append("NCAP ID: ");
					SnifferOutput.AppendLine(Hashes.BinaryToString(NcapId));
					SnifferOutput.Append("TIM ID: ");
					SnifferOutput.AppendLine(Hashes.BinaryToString(TimId));
					SnifferOutput.Append("Channel ID: ");
					SnifferOutput.AppendLine(ChannelId.ToString());
					SnifferOutput.Append("Value: ");
					SnifferOutput.AppendLine(Value);
					SnifferOutput.Append("Timestamp: ");
					SnifferOutput.AppendLine(Timestamp.ToString());
				}

				return Result;
			}
		}
	}
}
