using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.TransducerChannelTeds
{
	/// <summary>
	/// TEDS Sample definition data model length (§6.5.2.25)
	/// </summary>
	public class SampleDefinitionDataModelLength : TedsRecord
	{
		/// <summary>
		/// TEDS Sample definition data model length (§6.5.2.25)
		/// </summary>
		public SampleDefinitionDataModelLength()
			: base()
		{
		}

		/// <summary>
		/// Sample definition Data model length.
		/// </summary>
		public byte DataModelLength;

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
		{
			return RecordTypeId.Class == 3 && RecordTypeId.Type == 41 ? Grade.Perfect : Grade.NotAtAll;
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
			State.SampleDefinition.DataModelLength = RawValue.NextUInt8(null);

			if (RawValue.HasSniffers)
				RawValue.SniffValue(nameof(this.DataModelLength), State.SampleDefinition.DataModelLength.ToString());

			return new SampleDefinitionDataModelLength()
			{
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
				RawValue = RawValue.Body,
				DataModelLength = State.SampleDefinition.DataModelLength
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
			SnifferOutput.Append("DataModelLength=");
			SnifferOutput.AppendLine(this.DataModelLength.ToString());
		}
	}
}
