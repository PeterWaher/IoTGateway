using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes.TransducerNameTeds
{
	/// <summary>
	/// TEDS Content (§6.11.2.3)
	/// </summary>
	public class Content : TedsRecord
	{
		/// <summary>
		/// TEDS Content (§6.11.2.3)
		/// </summary>
		public Content()
			: base()
		{
		}

		/// <summary>
		/// TEDS Content (§6.11.2.3)
		/// </summary>
		/// <param name="Name">Entity name.</param>
		public Content(string Name)
			: base(12, 5, Encoding.UTF8.GetBytes(Name))
		{
		}

		/// <summary>
		/// Name, either as text, or base64-encoded binary
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="RecordTypeId">Record Type identifier.</param>
		/// <returns>Suppoer grade.</returns>
		public override Grade Supports(ClassTypePair RecordTypeId)
		{
			return RecordTypeId.Class == 12 && RecordTypeId.Type == 5 ? Grade.Perfect : Grade.NotAtAll;
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
			string Name;

			if (State.NameFormatText)
				Name = Encoding.UTF8.GetString(RawValue.Body);
			else
				Name = Convert.ToBase64String(RawValue.Body);

			return new Content()
			{
				Class = RecordTypeId.Class,
				Type = RecordTypeId.Type,
				RawValue = RawValue.Body,
				Name = Name
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
			Fields.Add(new StringField(Thing, Timestamp, "Name", this.Name,
				FieldType.Identity, FieldQoS.AutomaticReadout));
		}

		/// <summary>
		/// Appends record details to sniffer output.
		/// </summary>
		/// <param name="SnifferOutput">Sniffer output.</param>
		public override void AppendDetails(StringBuilder SnifferOutput)
		{
			SnifferOutput.Append("Content=");
			SnifferOutput.AppendLine(this.Name);
		}
	}
}
