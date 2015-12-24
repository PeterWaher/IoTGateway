using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Represents a duration value. Duration values adhere to the type specified by xsd:duration.
	///
	/// Difference between xsd:time and xsd:duration is that:
	///		xsd:duration represents the relative span between twostamps in time.
	///		xsd:time represents an absolute time or clock in the 24-hour clock.
	/// </summary>
	public class DurationField : Field
	{
		private Duration value;

		/// <summary>
		/// Represents a duration value. Duration values adhere to the type specified by xsd:duration.
		/// </summary>
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public DurationField(string Name, Duration Value, FieldType Type, FieldQoS QoS, bool Writable, string Module, params LocalizationStep[] StringIdSteps)
			: base(Name, Type, QoS, Writable, Module, StringIdSteps)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a duration value. Duration values adhere to the type specified by xsd:duration.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public DurationField(string Name, Duration Value, FieldType Type, FieldQoS QoS, bool Writable, string Module, params int[] StringIds)
			: base(Name, Type, QoS, Writable, Module, StringIds)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a duration value. Duration values adhere to the type specified by xsd:duration.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public DurationField(string Name, Duration Value, FieldType Type, FieldQoS QoS, string Module, params LocalizationStep[] StringIdSteps)
			: base(Name, Type, QoS, Module, StringIdSteps)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a duration value. Duration values adhere to the type specified by xsd:duration.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public DurationField(string Name, Duration Value, FieldType Type, FieldQoS QoS, string Module, params int[] StringIds)
			: base(Name, Type, QoS, Module, StringIds)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a duration value. Duration values adhere to the type specified by xsd:duration.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		public DurationField(string Name, Duration Value, FieldType Type, FieldQoS QoS, bool Writable)
			: base(Name, Type, QoS, Writable)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a duration value. Duration values adhere to the type specified by xsd:duration.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		public DurationField(string Name, Duration Value, FieldType Type, FieldQoS QoS)
			: base(Name, Type, QoS)
		{
			this.value = Value;
		}

		/// <summary>
		/// Field Value
		/// </summary>
		public Duration Value { get { return this.value; } }

		/// <summary>
		/// String representation of field value.
		/// </summary>
		public override string ValueString
		{
			get { return this.value.ToString(); }
		}

	}
}
