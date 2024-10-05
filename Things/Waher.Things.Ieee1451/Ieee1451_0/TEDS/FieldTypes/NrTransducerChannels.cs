using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes
{
	/// <summary>
	/// TEDS Number of implemented TransducerChannels (§6.4.2.7)
	/// </summary>
	public class NrTransducerChannels : TedsRecord
	{
		/// <summary>
		/// TEDS Number of implemented TransducerChannels (§6.4.2.7)
		/// </summary>
		public NrTransducerChannels()
			: base()
		{
		}

		/// <summary>
		/// This field contains the number of TransducerChannels implemented in this TIM.
		/// </summary>
		public int NrChannels { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="FieldType">TEDS field type.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(byte FieldType)
		{
			return FieldType == 13 ? Grade.Perfect : Grade.NotAtAll;
		}

		/// <summary>
		/// Parses a TEDS record.
		/// </summary>
		/// <param name="Type">Field Type</param>
		/// <param name="RawValue">Raw Value of record</param>
		/// <returns>Parsed TEDS record.</returns>
		public override TedsRecord Parse(byte Type, Ieee1451_0Binary RawValue)
		{
			return new NrTransducerChannels()
			{
				Type = Type,
				RawValue = RawValue.Body,
				NrChannels = RawValue.NextUInt16()
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
			Fields.Add(new Int32Field(Thing, Timestamp, "#Channels", this.NrChannels, 
				SensorData.FieldType.Identity, FieldQoS.AutomaticReadout));
		}
	}
}
