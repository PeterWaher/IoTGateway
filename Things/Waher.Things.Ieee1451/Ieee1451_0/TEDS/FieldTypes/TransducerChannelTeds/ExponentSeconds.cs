using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.TransducerChannelTeds
{
	/// <summary>
	/// TEDS The exponent for seconds (§6.5.2.12)
	/// </summary>
	public class ExponentSeconds : TedsRecord
    {
		/// <summary>
		/// TEDS The exponent for seconds (§6.5.2.12)
		/// </summary>
		public ExponentSeconds()
            : base()
        {
        }

		/// <summary>
		/// Unit Exponent
		/// </summary>
		public byte Exponent { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
        {
            return RecordTypeId.Class == 3 && RecordTypeId.Type == 55 ? Grade.Perfect : Grade.NotAtAll;
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
			State.Units.Seconds = RawValue.NextUInt8();

            return new ExponentSeconds()
            {
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
				RawValue = RawValue.Body,
				Exponent = State.Units.Seconds
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
        }
    }
}
