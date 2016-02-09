using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Represents a time value. Time values adhere to the type specified by xsd:time.
	///
	/// Difference between xsd:time and xsd:duration is that:
	///		xsd:duration represents the relative span between twostamps in time.
	///		xsd:time represents an absolute time or clock in the 24-hour clock.
	/// </summary>
	public class TimeField : Field
	{
		private TimeSpan value;

		/// <summary>
		/// Represents a time value. Time values adhere to the type specified by xsd:time.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
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
		public TimeField(ThingReference Thing, DateTime Timestamp, string Name, TimeSpan Value, FieldType Type, FieldQoS QoS, bool Writable, string Module, params LocalizationStep[] StringIdSteps)
			: base(Thing, Timestamp, Name, Type, QoS, Writable, Module, StringIdSteps)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a time value. Time values adhere to the type specified by xsd:time.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
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
		public TimeField(ThingReference Thing, DateTime Timestamp, string Name, TimeSpan Value, FieldType Type, FieldQoS QoS, bool Writable, string Module, params int[] StringIds)
			: base(Thing, Timestamp, Name, Type, QoS, Writable, Module, StringIds)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a time value. Time values adhere to the type specified by xsd:time.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public TimeField(ThingReference Thing, DateTime Timestamp, string Name, TimeSpan Value, FieldType Type, FieldQoS QoS, string Module, params LocalizationStep[] StringIdSteps)
			: base(Thing, Timestamp, Name, Type, QoS, Module, StringIdSteps)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a time value. Time values adhere to the type specified by xsd:time.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public TimeField(ThingReference Thing, DateTime Timestamp, string Name, TimeSpan Value, FieldType Type, FieldQoS QoS, string Module, params int[] StringIds)
			: base(Thing, Timestamp, Name, Type, QoS, Module, StringIds)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a time value. Time values adhere to the type specified by xsd:time.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		public TimeField(ThingReference Thing, DateTime Timestamp, string Name, TimeSpan Value, FieldType Type, FieldQoS QoS, bool Writable)
			: base(Thing, Timestamp, Name, Type, QoS, Writable)
		{
			this.value = Value;
		}

		/// <summary>
		/// Represents a time value. Time values adhere to the type specified by xsd:time.
		///
		/// Difference between xsd:time and xsd:duration is that:
		///		xsd:duration represents the relative span between twostamps in time.
		///		xsd:time represents an absolute time or clock in the 24-hour clock.
		/// </summary>
		/// <param name="Thing">Reference to the thing to which the field belongs.</param>
		/// <param name="Timestamp">Timestamp of field value.</param>
		/// <param name="Name">Field Name.</param>
		/// <param name="Value">Field Value.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		public TimeField(ThingReference Thing, DateTime Timestamp, string Name, TimeSpan Value, FieldType Type, FieldQoS QoS)
			: base(Thing, Timestamp, Name, Type, QoS)
		{
			this.value = Value;
		}

		/// <summary>
		/// Field Value
		/// </summary>
		public TimeSpan Value 
		{
			get { return this.value; }
			set { this.value = value; } 
		}

		/// <summary>
		/// String representation of field value.
		/// </summary>
		public override string ValueString
		{
			get { return this.value.ToString(); }
		}

		/// <summary>
		/// Provides a string identifying the data type of the field. Should conform to field value data types specified in XEP-0323, if possible:
		/// http://xmpp.org/extensions/xep-0323.html#fieldvaluetypes
		/// </summary>
		public override string FieldDataTypeName
		{
			get { return "time"; }
		}

	}
}
