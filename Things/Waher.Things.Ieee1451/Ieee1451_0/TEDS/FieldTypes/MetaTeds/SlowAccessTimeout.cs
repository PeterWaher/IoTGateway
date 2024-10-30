using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.MetaTeds
{
    /// <summary>
    /// TEDS Slow access time-out (§6.4.2.5)
    /// </summary>
    public class SlowAccessTimeout : TedsRecord
    {
        /// <summary>
        /// TEDS Slow access time-out (§6.4.2.5)
        /// </summary>
        public SlowAccessTimeout()
            : base()
        {
        }

        /// <summary>
        /// The slow access time-out field contains the time interval, in seconds, after 
        /// which an action for which the lack of a reply following the receipt of a 
        /// command may be interpreted as a failed operation.
        /// </summary>
        public float Timeout { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
        {
            return RecordTypeId.Class == 1 && RecordTypeId.Type == 11 ? Grade.Perfect : Grade.NotAtAll;
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
            return new SlowAccessTimeout()
            {
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
				RawValue = RawValue.Body,
                Timeout = RawValue.NextSingle(nameof(SlowAccessTimeout))
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
			Fields.Add(new QuantityField(Thing, Timestamp, "Slow Access Timeout",
                this.Timeout, Math.Min(CommonTypes.GetNrDecimals(this.Timeout), (byte)2), "s",
                FieldType.Status, FieldQoS.AutomaticReadout));
        }
    }
}
