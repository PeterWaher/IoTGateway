using System.Collections.Generic;
using System;
using Waher.Things.Ieee1451.Ieee1451_0.TEDS;
using Waher.Things.SensorData;
using Waher.Security;

namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
    /// <summary>
    /// IEEE 1451.0 TEDS
    /// </summary>
    public class Ieee1451_0Teds
	{
		/// <summary>
		/// IEEE 1451.0 TEDS
		/// </summary>
		/// <param name="ChannelInfo">Address information.</param>
		/// <param name="Records">TLV records available in TEDS</param>
		public Ieee1451_0Teds(Ieee1451_0ChannelId ChannelInfo, TedsRecord[] Records)
		{
			this.ChannelInfo = ChannelInfo;
			this.Records = Records;
		}

		/// <summary>
		/// TLV records available in TEDS
		/// </summary>
		public TedsRecord[] Records { get; }

		/// <summary>
		/// Address information.
		/// </summary>
		public Ieee1451_0ChannelId ChannelInfo { get; }

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
				Record.AddFields(Thing, Timestamp, Fields);
		}
	}
}
