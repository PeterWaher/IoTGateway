﻿using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.TransducerChannelTeds
{
	/// <summary>
	/// TEDS Sample definition data model significant bits (§6.5.2.26)
	/// </summary>
	public class SampleDefinitionDataModelSignificantBits : TedsRecord
	{
		/// <summary>
		/// TEDS Sample definition data model significant bits (§6.5.2.26)
		/// </summary>
		public SampleDefinitionDataModelSignificantBits()
			: base()
		{
		}

		/// <summary>
		/// Sample definition Data model significant bits.
		/// </summary>
		public ushort DataModelSignificantBits;

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
		{
			return RecordTypeId.Class == 3 && RecordTypeId.Type == 42 ? Grade.Perfect : Grade.NotAtAll;
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
			State.SampleDefinition.DataModelSignificantBits = RawValue.NextUInt16(nameof(SampleDefinitionDataModelSignificantBits));

			return new SampleDefinitionDataModelSignificantBits()
			{
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
				RawValue = RawValue.Body,
				DataModelSignificantBits = State.SampleDefinition.DataModelSignificantBits
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
		}

		/// <summary>
		/// Appends record details to sniffer output.
		/// </summary>
		/// <param name="SnifferOutput">Sniffer output.</param>
		public override void AppendDetails(StringBuilder SnifferOutput)
		{
			SnifferOutput.Append("DataModelSignificantBits=");
			SnifferOutput.AppendLine(this.DataModelSignificantBits.ToString());
		}
	}
}
