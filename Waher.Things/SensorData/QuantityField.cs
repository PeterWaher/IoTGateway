using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking;

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
		/// Represents a physical quantity value.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public QuantityField(string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS, bool Writable, string Module,
			params LocalizationStep[] StringIdSteps)
			: base(Name, Type, QoS, Writable, Module, StringIdSteps)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Represents a physical quantity value.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public QuantityField(string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS, bool Writable, string Module,
			params int[] StringIds)
			: base(Name, Type, QoS, Writable, Module, StringIds)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Represents a physical quantity value.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public QuantityField(string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS, string Module,
			params LocalizationStep[] StringIdSteps)
			: base(Name, Type, QoS, Module, StringIdSteps)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Represents a physical quantity value.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public QuantityField(string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS, string Module, params int[] StringIds)
			: base(Name, Type, QoS, Module, StringIds)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Represents a physical quantity value.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		public QuantityField(string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS, bool Writable)
			: base(Name, Type, QoS, Writable)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Represents a physical quantity value.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		public QuantityField(string Name, double Value, byte NrDecimals, string Unit, FieldType Type, FieldQoS QoS)
			: base(Name, Type, QoS)
		{
			this.value = Value;
			this.nrDecimals = NrDecimals;
			this.unit = Unit;
		}

		/// <summary>
		/// Field Value
		/// </summary>
		public double Value { get { return this.value; } }

		/// <summary>
		/// Number of decimals.
		/// </summary>
		public byte NrDecimals { get { return this.nrDecimals; } }

		/// <summary>
		/// Unit
		/// </summary>
		public string Unit { get { return this.unit; } }

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

	}
}
