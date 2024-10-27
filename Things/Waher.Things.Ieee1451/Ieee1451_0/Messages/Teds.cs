using System.Collections.Generic;
using System;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS;
using Waher.Things.SensorData;
using Waher.Security;
using Waher.Script.Units;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
    /// <summary>
    /// IEEE 1451.0 TEDS
    /// </summary>
    public class Teds
	{
		/// <summary>
		/// IEEE 1451.0 TEDS
		/// </summary>
		/// <param name="ChannelInfo">Address information.</param>
		/// <param name="Records">TLV records available in TEDS</param>
		public Teds(ChannelAddress ChannelInfo, TedsRecord[] Records)
		{
			this.ChannelInfo = ChannelInfo;
			this.Records = Records;
			this.FieldName = null;
			this.FieldUnit = null;
			this.Unit = string.Empty;

			foreach (TedsRecord Record in Records)
			{
				if (Record is TEDS.FieldTypes.TransducerChannelTeds.PhysicalUnits TedsUnits)
				{
					PhysicalUnits Units = TedsUnits.Units;

					this.FieldUnit = Units.TryCreateUnit();
					if (!(this.FieldUnit is null))
					{
						this.Unit = this.FieldUnit.ToString();
						if (Script.Units.Unit.TryGetCategory(this.FieldUnit, out IUnitCategory Category))
							this.FieldName = Category.Name;
					}
				}
			}

		}

		/// <summary>
		/// TLV records available in TEDS
		/// </summary>
		public TedsRecord[] Records { get; }

		/// <summary>
		/// Address information.
		/// </summary>
		public ChannelAddress ChannelInfo { get; }

		/// <summary>
		/// Field name of value, if found, otherwise null.
		/// </summary>
		public string FieldName { get; }

		/// <summary>
		/// Field unit of value, if found, otherwise null.
		/// </summary>
		public Unit FieldUnit { get; }

		/// <summary>
		/// Field unit of value, if found, otherwise the empty string.
		/// </summary>
		public string Unit { get; }

		/// <summary>
		/// Gets the information in the record, as an array of fields.
		/// </summary>
		/// <param name="Thing">Thing associated with fields.</param>
		/// <param name="Timestamp">Timestamp of fields.</param>
		/// <returns>Array of fields.</returns>
		public Field[] GetFields(ThingReference Thing, DateTime Timestamp)
		{
			List<Field> Fields = new List<Field>();
			this.AddFields(Thing, Timestamp, Fields);
			return Fields.ToArray();
		}

		/// <summary>
		/// Adds fields to a collection of fields.
		/// </summary>
		/// <param name="Thing">Thing associated with fields.</param>
		/// <param name="Timestamp">Timestamp of fields.</param>
		/// <param name="Fields">Parsed fields.</param>
		public virtual void AddFields(ThingReference Thing, DateTime Timestamp, List<Field> Fields)
		{
			Fields.Add(new StringField(Thing, Timestamp, "Application ID",
				Hashes.BinaryToString(this.ChannelInfo.ApplicationId),
				FieldType.Identity, FieldQoS.AutomaticReadout));

			Fields.Add(new StringField(Thing, Timestamp, "NCAP ID",
				Hashes.BinaryToString(this.ChannelInfo.NcapId),
				FieldType.Identity, FieldQoS.AutomaticReadout));

			Fields.Add(new StringField(Thing, Timestamp, "TIM ID", 
				Hashes.BinaryToString(this.ChannelInfo.TimId),
				FieldType.Identity, FieldQoS.AutomaticReadout));

			Fields.Add(new Int32Field(Thing, Timestamp, "Channel ID", this.ChannelInfo.ChannelId,
				FieldType.Identity, FieldQoS.AutomaticReadout));

			foreach (TedsRecord Record in this.Records)
				Record.AddFields(Thing, Timestamp, Fields, this);
		}
	}
}
