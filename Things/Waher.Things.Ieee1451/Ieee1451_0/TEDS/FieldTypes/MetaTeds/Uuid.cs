using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.MetaTeds
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
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
        {
            return RecordTypeId.Class == 1 && RecordTypeId.Type == 4 ? Grade.Perfect : Grade.NotAtAll;
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
            return new Uuid()
            {
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
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
		/// <param name="Teds">TEDS containing records.</param>
		public override void AddFields(ThingReference Thing, DateTime Timestamp, List<Field> Fields, Teds Teds)
		{
			Fields.Add(new StringField(Thing, Timestamp, "IEEE 1451 UUID",
                Hashes.BinaryToString(this.Identity), FieldType.Identity, 
                FieldQoS.AutomaticReadout));
        }
    }
}
