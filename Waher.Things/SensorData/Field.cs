using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Base class for all sensor data fields.
	/// </summary>
	public abstract class Field
	{
		private LocalizationStep[] stringIdSteps;
		private FieldType type;
		private FieldQoS qos;
		private string name;
		private string module;
		private bool writable;

		/// <summary>
		/// Base class for all sensor data fields.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public Field(string Name, FieldType Type, FieldQoS QoS, bool Writable, string Module, params LocalizationStep[] StringIdSteps)
		{
			this.name = Name;
			this.type = Type;
			this.qos = QoS;
			this.writable = Writable;
			this.module = Module;
			this.stringIdSteps = StringIdSteps;
		}

		/// <summary>
		/// Base class for all sensor data fields.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public Field(string Name, FieldType Type, FieldQoS QoS, bool Writable, string Module, params int[] StringIds)
		{
			this.name = Name;
			this.type = Type;
			this.qos = QoS;
			this.writable = Writable;
			this.module = Module;
			this.stringIdSteps = Convert(StringIds);
		}

		/// <summary>
		/// Base class for all sensor data fields.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public Field(string Name, FieldType Type, FieldQoS QoS, string Module, params LocalizationStep[] StringIdSteps)
		{
			this.name = Name;
			this.type = Type;
			this.qos = QoS;
			this.writable = false;
			this.module = Module;
			this.stringIdSteps = StringIdSteps;
		}

		/// <summary>
		/// Base class for all sensor data fields.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Module">Language Module for localization purposes.</param>
		/// <param name="StringIdSteps">String ID steps.</param>
		public Field(string Name, FieldType Type, FieldQoS QoS, string Module, params int[] StringIds)
		{
			this.name = Name;
			this.type = Type;
			this.qos = QoS;
			this.writable = false;
			this.module = Module;
			this.stringIdSteps = Convert(StringIds);
		}

		/// <summary>
		/// Base class for all sensor data fields.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		/// <param name="Writable">If the field is writable, i.e. corresponds to a control parameter.</param>
		public Field(string Name, FieldType Type, FieldQoS QoS, bool Writable)
		{
			this.name = Name;
			this.type = Type;
			this.qos = QoS;
			this.writable = Writable;
			this.module = string.Empty;
			this.stringIdSteps = null;
		}

		/// <summary>
		/// Base class for all sensor data fields.
		/// </summary>
		/// <param name="Name">Field Name.</param>
		/// <param name="Type">Field Type flags.</param>
		/// <param name="QoS">Quality of Service flags.</param>
		public Field(string Name, FieldType Type, FieldQoS QoS)
		{
			this.name = Name;
			this.type = Type;
			this.qos = QoS;
			this.writable = false;
			this.module = string.Empty;
			this.stringIdSteps = null;
		}

		private static LocalizationStep[] Convert(int[] StringIds)
		{
			int i, c = StringIds.Length;
			LocalizationStep[] Result = new LocalizationStep[c];

			for (i = 0; i < c; i++)
				Result[i] = new LocalizationStep(StringIds[i]);

			return Result;
		}

		/// <summary>
		/// Array of Language String ID steps. List can be null. Strings can be null if no seeds or modules are used.
		/// 
		/// Localization algorithm is defined in:
		/// http://xmpp.org/extensions/xep-0323.html#localization
		/// </summary>
		public LocalizationStep[] StringIdSteps { get { return this.stringIdSteps; } }

		/// <summary>
		/// Field Type flags.
		/// </summary>
		public FieldType Type { get { return this.type; } }

		/// <summary>
		/// Field Quality of Service flags.
		/// </summary>
		public FieldQoS QoS { get { return this.qos; } }

		/// <summary>
		/// Unlocalized field name.
		/// </summary>
		public string Name { get { return this.name; } }

		/// <summary>
		/// Default language module, if explicit language modules are not specified in the language steps.
		/// </summary>
		public string Module { get { return this.module; } }

		/// <summary>
		/// If the field corresponds to a control parameter with the same name.
		/// </summary>
		public bool Writable { get { return this.writable; } }

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			sb.Append(this.name);
			sb.Append(": ");
			sb.Append(this.ValueString);
			sb.Append(" (");

			if (this.writable)
				this.Append(sb, "Writable", ref First);

			if ((this.type & FieldType.Computed) != 0)
				this.Append(sb, "Computed", ref First);

			if ((this.type & FieldType.Identity) != 0)
				this.Append(sb, "Identity", ref First);

			if ((this.type & FieldType.Momentary) != 0)
				this.Append(sb, "Momentary", ref First);

			if ((this.type & FieldType.Peak) != 0)
				this.Append(sb, "Peak", ref First);

			if ((this.type & FieldType.Status) != 0)
				this.Append(sb, "Status", ref First);

			if ((this.type & FieldType.HistoricalSecond) != 0)
				this.Append(sb, "HistoricalSecond", ref First);

			if ((this.type & FieldType.HistoricalMinute) != 0)
				this.Append(sb, "HistoricalMinute", ref First);

			if ((this.type & FieldType.HistoricalHour) != 0)
				this.Append(sb, "HistoricalHour", ref First);

			if ((this.type & FieldType.HistoricalDay) != 0)
				this.Append(sb, "HistoricalDay", ref First);

			if ((this.type & FieldType.HistoricalWeek) != 0)
				this.Append(sb, "HistoricalWeek", ref First);

			if ((this.type & FieldType.HistoricalMonth) != 0)
				this.Append(sb, "HistoricalMonth", ref First);

			if ((this.type & FieldType.HistoricalQuarter) != 0)
				this.Append(sb, "HistoricalQuarter", ref First);

			if ((this.type & FieldType.HistoricalYear) != 0)
				this.Append(sb, "HistoricalYear", ref First);

			if ((this.type & FieldType.HistoricalQuarter) != 0)
				this.Append(sb, "HistoricalQuarter", ref First);

			if ((this.type & FieldType.HistoricalOther) != 0)
				this.Append(sb, "HistoricalOther", ref First);

			if ((this.qos & FieldQoS.Missing) != 0)
				this.Append(sb, "Missing", ref First);

			if ((this.qos & FieldQoS.InProgress) != 0)
				this.Append(sb, "InProgress", ref First);

			if ((this.qos & FieldQoS.AutomaticEstimate) != 0)
				this.Append(sb, "AutomaticEstimate", ref First);

			if ((this.qos & FieldQoS.ManualEstimate) != 0)
				this.Append(sb, "ManualEstimate", ref First);

			if ((this.qos & FieldQoS.ManualReadout) != 0)
				this.Append(sb, "ManualReadout", ref First);

			if ((this.qos & FieldQoS.AutomaticReadout) != 0)
				this.Append(sb, "AutomaticReadout", ref First);

			if ((this.qos & FieldQoS.TimeOffset) != 0)
				this.Append(sb, "TimeOffset", ref First);

			if ((this.qos & FieldQoS.Warning) != 0)
				this.Append(sb, "Warning", ref First);

			if ((this.qos & FieldQoS.Error) != 0)
				this.Append(sb, "Error", ref First);

			if ((this.qos & FieldQoS.Signed) != 0)
				this.Append(sb, "Signed", ref First);

			if ((this.qos & FieldQoS.Invoiced) != 0)
				this.Append(sb, "Invoiced", ref First);

			if ((this.qos & FieldQoS.EndOfSeries) != 0)
				this.Append(sb, "EndOfSeries", ref First);

			if ((this.qos & FieldQoS.PowerFailure) != 0)
				this.Append(sb, "PowerFailure", ref First);

			if ((this.qos & FieldQoS.InvoiceConfirmed) != 0)
				this.Append(sb, "InvoiceConfirmed", ref First);

			sb.Append(")");

			return sb.ToString();
		}

		private void Append(StringBuilder sb, string s, ref bool First)
		{
			if (First)
				First = false;
			else
				sb.Append(", ");

			sb.Append(s);
		}

		/// <summary>
		/// String representation of field value.
		/// </summary>
		public abstract string ValueString
		{
			get;
		}
	}
}
