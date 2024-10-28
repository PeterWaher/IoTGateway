using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.TransducerChannelTeds
{
	/// <summary>
	/// TEDS Design operational lower range limit (§6.5.2.18)
	/// </summary>
	public class LowerLimit : TedsRecord
	{
		/// <summary>
		/// TEDS Design operational lower range limit (§6.5.2.18)
		/// </summary>
		public LowerLimit()
			: base()
		{
		}

		/// <summary>
		/// For sensors, this shall be the lowest valid value for TransducerChannel data 
		/// that the TransducerChannel is designed to provide after correction is applied.
		/// </summary>
		public float Value { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
		{
			return RecordTypeId.Class == 3 && RecordTypeId.Type == 13 ? Grade.Perfect : Grade.NotAtAll;
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
			return new LowerLimit()
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
		/// <param name="Teds">TEDS containing records.</param>
		public override void AddFields(ThingReference Thing, DateTime Timestamp, List<Field> Fields, Teds Teds)
		{
			Fields.Add(new QuantityField(Thing, Timestamp, (Teds?.FieldName ?? "Value") + ", Lower Limit", this.Value,
				Math.Min(CommonTypes.GetNrDecimals(this.Value), (byte)2), Teds?.Unit ?? string.Empty,
				FieldType.Status, FieldQoS.AutomaticReadout));
		}
	}
}
