using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.TransducerChannelTeds
{
	/// <summary>
	/// TEDS TransducerChannel write setup time (§6.5.2.34)
	/// </summary>
	public class WriteSetupTime : TedsRecord
	{
		/// <summary>
		/// TEDS TransducerChannel write setup time (§6.5.2.34)
		/// </summary>
		public WriteSetupTime()
			: base()
		{
		}

		/// <summary>
		/// This field contains the minimum time (tws), expressed in seconds, 
		/// between the end of a write data frame and the application of a trigger.
		/// </summary>
		public float Value { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
		{
			return RecordTypeId.Class == 3 && RecordTypeId.Type == 21 ? Grade.Perfect : Grade.NotAtAll;
		}

		/// <summary>
		/// Parses a TEDS record.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <param name="RawValue">Raw Value of record</param>
		/// <param name="State">Current parsing state.</param>
		/// <returns>Parsed TEDS record.</returns>
		public override TedsRecord Parse(ClassTypePair RecordTypeId, Binary RawValue, ParsingState State)
		{
			return new WriteSetupTime()
			{
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
				RawValue = RawValue.Body,
				Value = RawValue.NextSingle(nameof(WriteSetupTime))
			};
		}

		/// <summary>
		/// Adds fields to a collection of fields.
		/// </summary>
		/// <param name="Thing">Thing associated with fields.</param>
		/// <param name="Timestamp">Timestamp of fields.</param>
		/// <param name="Fields">Parsed fields.</param>
		/// <param name="Teds">TEDS containing records.</param>
		public override void AddFields(ThingReference Thing, DateTime Timestamp, List<Field> Fields, Teds Teds)
		{
			Fields.Add(new QuantityField(Thing, Timestamp, "Write Setup Time", this.Value,
				Math.Min(CommonTypes.GetNrDecimals(this.Value), (byte)2), "s",
				FieldType.Status, FieldQoS.AutomaticReadout));
		}

		/// <summary>
		/// Appends record details to sniffer output.
		/// </summary>
		/// <param name="SnifferOutput">Sniffer output.</param>
		public override void AppendDetails(StringBuilder SnifferOutput)
		{
			SnifferOutput.Append("WriteSetupTime=");
			SnifferOutput.AppendLine(this.Value.ToString());
		}
	}
}
