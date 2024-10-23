using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS
{
	/// <summary>
	/// Basic interface for TEDS field types.
	/// </summary>
	public interface IFieldType : IProcessingSupport<ClassTypePair>
	{
		/// <summary>
		/// Parses a TEDS record.
		/// </summary>
		/// <param name="RecordTypeId">Record type identifier</param>
		/// <param name="RawValue">Raw Value of record</param>
		/// <param name="State">Current parsing state.</param>
		/// <returns>Parsed TEDS record.</returns>
		TedsRecord Parse(ClassTypePair RecordTypeId, Binary RawValue, ParsingState State);

		/// <summary>
		/// Adds fields to a collection of fields.
		/// </summary>
		/// <param name="Thing">Thing associated with fields.</param>
		/// <param name="Timestamp">Timestamp of fields.</param>
		/// <param name="Fields">Parsed fields.</param>
		/// <param name="Teds">TEDS containing records.</param>
		void AddFields(ThingReference Thing, DateTime Timestamp, List<Field> Fields, Teds Teds);
	}
}
