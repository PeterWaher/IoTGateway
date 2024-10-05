using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Things.Ieee1451.Ieee1451_0.Messages;
using Waher.Things.SensorData;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS
{
	/// <summary>
	/// Represents one record in a TEDS
	/// </summary>
	public class TedsRecord : IFieldType
	{
		/// <summary>
		/// Represents one record in a TEDS
		/// </summary>
		public TedsRecord()
		{
		}

		/// <summary>
		/// TEDS Record Type
		/// </summary>
		public byte Type { get; set; }

		/// <summary>
		/// TEDS Record value
		/// </summary>
		public byte[] RawValue { get; set; }

		/// <summary>
		/// Field Type
		/// </summary>
		public virtual int FieldType => -1;

		/// <summary>
		/// Parses a TEDS record.
		/// </summary>
		/// <param name="Type">Field Type</param>
		/// <param name="RawValue">Raw Value of record</param>
		/// <returns>Parsed TEDS record.</returns>
		public virtual TedsRecord Parse(byte Type, Ieee1451_0Binary RawValue)
		{
			return new TedsRecord()
			{
				Type = Type,
				RawValue = RawValue.Body
			};
		}

		/// <summary>
		/// How well the class supports a specific TEDS field type.
		/// </summary>
		/// <param name="FieldType">TEDS field type.</param>
		/// <returns>Suppoer grade.</returns>
		public virtual Grade Supports(byte FieldType)
		{
			return Grade.NotAtAll;
		}

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
			Fields.Add(new StringField(Thing, Timestamp, this.Type.ToString("X2") + ", Raw",
				Convert.ToBase64String(this.RawValue), SensorData.FieldType.Identity,
				FieldQoS.AutomaticReadout));
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return Convert.ToBase64String(this.RawValue);
		}
	}
}
