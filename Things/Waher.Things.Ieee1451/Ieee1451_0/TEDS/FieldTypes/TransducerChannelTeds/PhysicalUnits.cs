﻿using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Units;
using Waher.Security;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.TransducerChannelTeds
{
	/// <summary>
	/// TEDS Physical units (§6.5.2.6)
	/// </summary>
	public class PhysicalUnits : TedsRecord
	{
		/// <summary>
		/// TEDS Physical units (§6.5.2.6)
		/// </summary>
		public PhysicalUnits()
			: base()
		{
		}

		/// <summary>
		/// TEDS Physical units (§6.5.2.6)
		/// </summary>
		/// <param name="Units">Physical units to encode.</param>
		public PhysicalUnits(Messages.PhysicalUnits Units)
			: base(3, 12, new byte[]
			{
				(byte)Units.Interpretation,
				Units.Radians,
				Units.Steradians,
				Units.Meters,
				Units.Kilograms,
				Units.Seconds,
				Units.Amperes,
				Units.Kelvins,
				Units.Moles,
				Units.Candelas
			})
		{
		}

		/// <summary>
		/// Units
		/// </summary>
		public Messages.PhysicalUnits Units { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
		{
			return RecordTypeId.Class == 3 && RecordTypeId.Type == 12 ? Grade.Perfect : Grade.NotAtAll;
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
			State.Units = RawValue.NextPhysicalUnits(nameof(PhysicalUnits));

			return new PhysicalUnits()
			{
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
				RawValue = RawValue.Body,
				Units = State.Units
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
			SnifferOutput.Append("PhysicalUnits=");

			Unit Unit = this.Units.TryCreateUnit();
			if (Unit is null)
				SnifferOutput.AppendLine(Hashes.BinaryToString(this.RawValue, true));
			else
				SnifferOutput.AppendLine(Unit.ToString());
		}
	}
}
