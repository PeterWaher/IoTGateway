using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.MetaTeds
{
    /// <summary>
    /// TEDS Transducer module self-test time requirement (§6.4.2.6)
    /// </summary>
    public class SelfTestTime : TedsRecord
    {
        /// <summary>
        /// TEDS Transducer module self-test time requirement (§6.4.2.6)
        /// </summary>
        public SelfTestTime()
            : base()
        {
        }

        /// <summary>
        /// This field contains the maximum time, in seconds, required to execute 
        /// the self-test.
        /// </summary>
        public float Time { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
        {
            return RecordTypeId.Class == 1 && RecordTypeId.Type == 12 ? Grade.Perfect : Grade.NotAtAll;
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
            return new SelfTestTime()
            {
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
				RawValue = RawValue.Body,
                Time = RawValue.NextSingle(nameof(SelfTestTime))
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
			Fields.Add(new QuantityField(Thing, Timestamp, "Self-Test Time",
				this.Time, Math.Min(CommonTypes.GetNrDecimals(this.Time), (byte)2), "s",
                FieldType.Status, FieldQoS.AutomaticReadout));
        }
    }
}
