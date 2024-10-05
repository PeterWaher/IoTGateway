using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.TransducerChannelTeds
{
	/// <summary>
	/// TEDS TransducerChannel warm-up time (§6.5.2.37)
	/// </summary>
	public class WarmUpTime : TedsRecord
	{
		/// <summary>
		/// TEDS TransducerChannel warm-up time (§6.5.2.37)
		/// </summary>
		public WarmUpTime()
			: base()
		{
		}

		/// <summary>
		/// This field contains the period of time, expressed in seconds, in which the 
		/// TransducerChannel stabilizes its performance to predefined tolerances, as 
		/// specified in Uncertainty under worst-case conditions(see 6.5.2.7), after 
		/// the application of power to the TransducerChannel
		/// </summary>
		public float Value { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
		{
			return RecordTypeId.Class == 3 && RecordTypeId.Type == 24 ? Grade.Perfect : Grade.NotAtAll;
		}

		/// <summary>
		/// Parses a TEDS record.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <param name="RawValue">Raw Value of record</param>
		/// <param name="State">Current parsing state.</param>
		/// <returns>Parsed TEDS record.</returns>
		public override TedsRecord Parse(ClassTypePair RecordTypeId, Ieee1451_0Binary RawValue, ParsingState State)
		{
			return new WarmUpTime()
			{
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
				RawValue = RawValue.Body,
				Value = RawValue.NextSingle()
			};
		}

		/// <summary>
		/// Adds fields to a collection of fields.
		/// </summary>
		/// <param name="Thing">Thing associated with fields.</param>
		/// <param name="Timestamp">Timestamp of fields.</param>
		/// <param name="Fields">Parsed fields.</param>
		public override void AddFields(ThingReference Thing, DateTime Timestamp, List<Field> Fields)
		{
			Fields.Add(new QuantityField(Thing, Timestamp, "Warm-Up Time", this.Value,
				Math.Min(CommonTypes.GetNrDecimals(this.Value), (byte)2), "s",
				FieldType.Status, FieldQoS.AutomaticReadout));
		}
	}
}
