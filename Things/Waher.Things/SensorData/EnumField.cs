﻿using System;
using Waher.Persistence.Attributes;
using Waher.Runtime.Inventory;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Represents a enum value.
	/// </summary>
	public class EnumField : Field
	{
		private Enum value;
		private string valueString;
		private string type;

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		public EnumField()
			: base(null, DateTime.MinValue, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout)
		{
			this.valueString = string.Empty;
			this.type = string.Empty;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, Enum Value, FieldType Type, FieldQoS QoS, bool Writable, string Module, params LocalizationStep[] StringIdSteps)
			: base(Thing, Timestamp, Name, Type, QoS, Writable, Module, StringIdSteps)
		{
			this.value = Value;
			this.valueString = Value.ToString();
			this.type = Value.GetType().FullName;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIds">String IDs.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, Enum Value, FieldType Type, FieldQoS QoS, bool Writable, 
			string Module, params int[] StringIds)
			: base(Thing, Timestamp, Name, Type, QoS, Writable, Module, StringIds)
		{
			this.value = Value;
			this.valueString = Value.ToString();
			this.type = Value.GetType().FullName;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, Enum Value, FieldType Type, FieldQoS QoS, string Module, params LocalizationStep[] StringIdSteps)
			: base(Thing, Timestamp, Name, Type, QoS, Module, StringIdSteps)
		{
			this.value = Value;
			this.valueString = Value.ToString();
			this.type = Value.GetType().FullName;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIds">String IDs.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, Enum Value, FieldType Type, FieldQoS QoS, string Module, 
			params int[] StringIds)
			: base(Thing, Timestamp, Name, Type, QoS, Module, StringIds)
		{
			this.value = Value;
			this.valueString = Value.ToString();
			this.type = Value.GetType().FullName;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, Enum Value, FieldType Type, FieldQoS QoS, bool Writable)
			: base(Thing, Timestamp, Name, Type, QoS, Writable)
		{
			this.value = Value;
			this.valueString = Value.ToString();
			this.type = Value.GetType().FullName;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, Enum Value, FieldType Type, FieldQoS QoS)
			: base(Thing, Timestamp, Name, Type, QoS)
		{
			this.value = Value;
			this.valueString = Value.ToString();
			this.type = Value.GetType().FullName;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="ValueString">String-representation of enumeration value.</param>
		/// <param name="EnumerationType">Enumeration type.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, string ValueString, string EnumerationType, FieldType Type, FieldQoS QoS, bool Writable, string Module, params LocalizationStep[] StringIdSteps)
			: base(Thing, Timestamp, Name, Type, QoS, Writable, Module, StringIdSteps)
		{
			this.valueString = ValueString;
			this.type = EnumerationType;

			if (!Types.TryParseEnum(this.type, this.valueString, out this.value))
				this.value = null;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="ValueString">String-representation of enumeration value.</param>
		/// <param name="EnumerationType">Enumeration type.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIds">String IDs.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, string ValueString, string EnumerationType, FieldType Type, FieldQoS QoS, bool Writable, 
			string Module, params int[] StringIds)
			: base(Thing, Timestamp, Name, Type, QoS, Writable, Module, StringIds)
		{
			this.valueString = ValueString;
			this.type = EnumerationType;

			if (!Types.TryParseEnum(this.type, this.valueString, out this.value))
				this.value = null;
		}



		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="ValueString">String-representation of enumeration value.</param>
		/// <param name="EnumerationType">Enumeration type.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, string ValueString, string EnumerationType, FieldType Type, FieldQoS QoS, string Module, params LocalizationStep[] StringIdSteps)
			: base(Thing, Timestamp, Name, Type, QoS, Module, StringIdSteps)
		{
			this.valueString = ValueString;
			this.type = EnumerationType;

			if (!Types.TryParseEnum(this.type, this.valueString, out this.value))
				this.value = null;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="ValueString">String-representation of enumeration value.</param>
		/// <param name="EnumerationType">Enumeration type.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIds">String IDs.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, string ValueString, string EnumerationType, FieldType Type, FieldQoS QoS, 
			string Module, params int[] StringIds)
			: base(Thing, Timestamp, Name, Type, QoS, Module, StringIds)
		{
			this.valueString = ValueString;
			this.type = EnumerationType;

			if (!Types.TryParseEnum(this.type, this.valueString, out this.value))
				this.value = null;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="ValueString">String-representation of enumeration value.</param>
		/// <param name="EnumerationType">Enumeration type.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, string ValueString, string EnumerationType, FieldType Type, FieldQoS QoS, bool Writable)
			: base(Thing, Timestamp, Name, Type, QoS, Writable)
		{
			this.valueString = ValueString;
			this.type = EnumerationType;

			if (!Types.TryParseEnum(this.type, this.valueString, out this.value))
				this.value = null;
		}

		/// <summary>
		/// Represents a enum value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="ValueString">String-representation of enumeration value.</param>
		/// <param name="EnumerationType">Enumeration type.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		public EnumField(ThingReference Thing, DateTime Timestamp, string Name, string ValueString, string EnumerationType, FieldType Type, FieldQoS QoS)
			: base(Thing, Timestamp, Name, Type, QoS)
		{
			this.valueString = ValueString;
			this.type = EnumerationType;

			if (!Types.TryParseEnum(this.type, this.valueString, out this.value))
				this.value = null;
		}

		/// <summary>
		/// Field Value, if available. If null, <see cref="ValueString"/> contains the string representation and
		/// <see cref="Type"/> contains the name of the enumeration type.
		/// </summary>
		[ShortName("v")]
		public Enum Value
		{
			get => this.value;
			set
			{
				this.value = value;
				this.valueString = this.value.ToString();
			}
		}

		/// <summary>
		/// Enumeration Type.
		/// </summary>
		[ShortName("et")]
		public string EnumerationType 
		{
			get => this.type;
			set => this.type = value;
		}

		/// <summary>
		/// Field value, boxed as an object reference.
		/// </summary>
		public override object ObjectValue => this.value;

		/// <summary>
		/// String representation of field value.
		/// </summary>
		public override string ValueString => this.valueString;

		/// <summary>
		/// Provides a string identifying the data type of the field. Should conform to field value data types specified by the Neuro-Foundation, if possible:
		/// https://neuro-foundation.io/SensorData.md#fields
		/// </summary>
		public override string FieldDataTypeName
		{
			get { return "e"; }
		}

		/// <summary>
		/// Reference value. Can be used for change calculations, as outlined in 
		/// http://www.xmpp.org/extensions/inbox/iot-events.html#changeconditions.
		/// 
		/// Possible values are either double values or string values.
		/// </summary>
		public override object ReferenceValue
		{
			get
			{
				return this.value.ToString();
			}
		}

	}
}
