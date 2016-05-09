using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Persistence.Attributes;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Represents a physical quantity value.
	/// </summary>
	public class QuantityField : Field
	{
		private string unit;
		private double value;
		private byte nrDecimals;

		/// <summary>
		/// Represents a 64-bit integer value.
		/// </summary>
		public QuantityField()
			: base(null, DateTime.MinValue, string.Empty, FieldType.Momentary, FieldQoS.AutomaticReadout)
		{
			this.value = 0;
			this.unit = string.Empty;
			this.nrDecimals = 0;
		}

		/// <summary>
		/// Represents a physical quantity value.
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
		public QuantityField(ThingReference Thing, DateTime Timestamp, string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS, bool Writable, string Module,
			params LocalizationStep[] StringIdSteps)
			: base(Thing, Timestamp, Name, Type, QoS, Writable, Module, StringIdSteps)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Represents a physical quantity value.
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
		public QuantityField(ThingReference Thing, DateTime Timestamp, string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS, bool Writable, string Module,
			params int[] StringIds)
			: base(Thing, Timestamp, Name, Type, QoS, Writable, Module, StringIds)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Represents a physical quantity value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public QuantityField(ThingReference Thing, DateTime Timestamp, string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS, string Module,
			params LocalizationStep[] StringIdSteps)
			: base(Thing, Timestamp, Name, Type, QoS, Module, StringIdSteps)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Represents a physical quantity value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public QuantityField(ThingReference Thing, DateTime Timestamp, string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS, string Module, params int[] StringIds)
			: base(Thing, Timestamp, Name, Type, QoS, Module, StringIds)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Represents a physical quantity value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		public QuantityField(ThingReference Thing, DateTime Timestamp, string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS, bool Writable)
			: base(Thing, Timestamp, Name, Type, QoS, Writable)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Represents a physical quantity value.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		public QuantityField(ThingReference Thing, DateTime Timestamp, string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS)
			: base(Thing, Timestamp, Name, Type, QoS)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Field Value
		/// </summary>
		[ShortName("v")]
		public double Value 
		{
			get { return this.value; }
			set { this.value = value; } 
		}

		/// <summary>
		/// Number of decimals.
		/// </summary>
		[ShortName("d")]
		[DefaultValue(0)]
		public byte NrDecimals 
		{
			get { return this.nrDecimals; }
			set { this.nrDecimals = value; } 
		}

		/// <summary>
		/// Unit
		/// </summary>
		[ShortName("u")]
		[DefaultValueStringEmpty]
		public string Unit 
		{
			get { return this.unit; }
			set { this.unit = value; } 
		}

		/// <summary>
		/// String representation of field value.
		/// </summary>
		public override string ValueString
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(CommonTypes.Encode(this.value, this.nrDecimals));

				if (!string.IsNullOrEmpty(this.unit))
				{
					sb.Append(' ');
					sb.Append(this.unit);
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// Provides a string identifying the data type of the field. Should conform to field value data types specified in XEP-0323, if possible:
		/// http://xmpp.org/extensions/xep-0323.html#fieldvaluetypes
		/// </summary>
		public override string FieldDataTypeName
		{
			get { return "numeric"; }
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
				return this.value;
			}
		}

	}
}
