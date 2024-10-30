using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.MetaTeds
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
		/// TEDS Number of implemented TransducerChannels (§6.4.2.7)
		/// </summary>
		public NrTransducerChannels(int NrChannels)
			: base(1, 13, new byte[] 
			{
				(byte)(NrChannels >> 8),
				(byte)NrChannels
			})
		{
			if (NrChannels < 0 || NrChannels > ushort.MaxValue)
				throw new ArgumentOutOfRangeException(nameof(NrChannels));

			this.NrChannels = NrChannels;
		}

		/// <summary>
		/// This field contains the number of TransducerChannels implemented in this TIM.
		/// </summary>
		public int NrChannels { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
        {
            return RecordTypeId.Class == 1 && RecordTypeId.Type == 13 ? Grade.Perfect : Grade.NotAtAll;
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
            return new NrTransducerChannels()
            {
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
				RawValue = RawValue.Body,
                NrChannels = RawValue.NextUInt16(nameof(NrTransducerChannels))
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
			Fields.Add(new Int32Field(Thing, Timestamp, "#Channels", this.NrChannels,
                FieldType.Identity, FieldQoS.AutomaticReadout));
        }
    }
}
