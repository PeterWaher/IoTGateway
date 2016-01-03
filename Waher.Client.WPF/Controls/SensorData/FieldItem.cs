using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Waher.Client.WPF.Model;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Controls.SensorData
{
	/// <summary>
	/// Represents one item in a sensor data readout.
	/// </summary>
	public class FieldItem : ColorableItem
	{
		private Field field;

		/// <summary>
		/// Represents one item in a sniffer output.
		/// </summary>
		/// <param name="Field">Sensor data field.</param>
		/// <param name="ForegroundColor">Foreground Color</param>
		/// <param name="BackgroundColor">Background Color</param>
		public FieldItem(Field Field, Color ForegroundColor, Color BackgroundColor)
			: base(ForegroundColor, BackgroundColor)
		{
			this.field = Field;
		}

		/// <summary>
		/// Sensor data field.
		/// </summary>
		public Field Field { get { return this.field; } }

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public string Timestamp
		{
			get
			{
				return this.field.Timestamp.ToShortDateString() + ", " + this.field.Timestamp.ToLongTimeString();
			}
		}

		/// <summary>
		/// Field Name
		/// </summary>
		public string FieldName { get { return this.field.Name; } }

		/// <summary>
		/// Value
		/// </summary>
		public string Value { get { return this.field.ValueString; } }

		/// <summary>
		/// Unit
		/// </summary>
		public string Unit
		{
			get
			{
				QuantityField Q = this.field as QuantityField;
				if (Q != null)
					return Q.Unit;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Status
		/// </summary>
		public string Status
		{
			get
			{
				if (this.statusString == null)
				{
					StringBuilder Output = null;

					foreach (Enum Value in Enum.GetValues(typeof(FieldQoS)))
					{
						if (!this.field.QoS.HasFlag(Value))
							continue;

						if (Output == null)
							Output = new StringBuilder();
						else
							Output.Append(", ");

						switch ((FieldQoS)Value)
						{
							case FieldQoS.Missing: Output.Append("Missing"); break;
							case FieldQoS.InProgress: Output.Append("In Progress"); break;
							case FieldQoS.AutomaticEstimate: Output.Append("Automatic Estimate"); break;
							case FieldQoS.ManualEstimate: Output.Append("Manual Estimate"); break;
							case FieldQoS.ManualReadout: Output.Append("ManualReadout"); break;
							case FieldQoS.AutomaticReadout: Output.Append("AutomaticReadout"); break;
							case FieldQoS.TimeOffset: Output.Append("Time Offset"); break;
							case FieldQoS.Warning: Output.Append("Warning"); break;
							case FieldQoS.Error: Output.Append("Error"); break;
							case FieldQoS.Signed: Output.Append("Signed"); break;
							case FieldQoS.Invoiced: Output.Append("Invoiced"); break;
							case FieldQoS.EndOfSeries: Output.Append("EndOfSeries"); break;
							case FieldQoS.PowerFailure: Output.Append("PowerFailure"); break;
							case FieldQoS.InvoiceConfirmed: Output.Append("InvoiceConfirmed"); break;
							default: Output.Append(Value.ToString()); break;
						}
					}

					this.statusString = Output.ToString();
				}

				return this.statusString;
			}
		}

		private string statusString = null;

		/// <summary>
		/// Type
		/// </summary>
		public string Type
		{
			get
			{
				if (this.typeString == null)
				{
					StringBuilder Output = null;

					foreach (Enum Value in Enum.GetValues(typeof(FieldType)))
					{
						if (!this.field.Type.HasFlag(Value))
							continue;

						if (Output == null)
							Output = new StringBuilder();
						else
							Output.Append(", ");

						switch ((FieldType)Value)
						{
							case FieldType.Momentary: Output.Append("Momentary"); break;
							case FieldType.Identity: Output.Append("Identity"); break;
							case FieldType.Status: Output.Append("Status"); break;
							case FieldType.Computed: Output.Append("Computed"); break;
							case FieldType.Peak: Output.Append("Peak"); break;
							case FieldType.HistoricalSecond: Output.Append("Historical (Second)"); break;
							case FieldType.HistoricalMinute: Output.Append("Historical (Minute)"); break;
							case FieldType.HistoricalHour: Output.Append("Historical (Hour)"); break;
							case FieldType.HistoricalDay: Output.Append("Historical (Day)"); break;
							case FieldType.HistoricalWeek: Output.Append("Historical (Week)"); break;
							case FieldType.HistoricalMonth: Output.Append("Historical (Month)"); break;
							case FieldType.HistoricalQuarter: Output.Append("Historical (Quarter)"); break;
							case FieldType.HistoricalYear: Output.Append("Historical (Year)"); break;
							case FieldType.HistoricalOther: Output.Append("Historical (Other)"); break;
							default: Output.Append(Value.ToString()); break;
						}
					}

					this.typeString = Output.ToString();
				}

				return this.typeString;
			}
		}

		private string typeString = null;

	}
}
