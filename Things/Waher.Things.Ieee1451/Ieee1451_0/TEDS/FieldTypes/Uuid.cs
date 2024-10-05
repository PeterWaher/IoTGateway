using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes
{
	/// <summary>
	/// TEDS Universal unique identifier (§6.4.2.2)
	/// </summary>
	public class Uuid : TedsRecord
	{
		/// <summary>
		/// TEDS Universal unique identifier (§6.4.2.2)
		/// </summary>
		public Uuid()
			: base()
		{
		}

		/// <summary>
		/// Unique identifier
		/// </summary>
		public byte[] Identity { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="FieldType">TEDS field type.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(byte FieldType)
		{
			return FieldType == 4 ? Grade.Perfect : Grade.NotAtAll;
		}

		/// <summary>
		/// Parses a TEDS record.
		/// </summary>
		/// <param name="Type">Field Type</param>
		/// <param name="RawValue">Raw Value of record</param>
		/// <returns>Parsed TEDS record.</returns>
		public override TedsRecord Parse(byte Type, Ieee1451_0Binary RawValue)
		{
			return new Uuid()
			{
				Type = Type,
				RawValue = RawValue.Body,
				Identity = RawValue.NextUInt8Array(16)
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
			Fields.Add(new StringField(Thing, Timestamp, "IEEE 1451 UUID", 
				Hashes.BinaryToString(this.Identity), SensorData.FieldType.Identity, 
				FieldQoS.AutomaticReadout));
		}
	}
}
