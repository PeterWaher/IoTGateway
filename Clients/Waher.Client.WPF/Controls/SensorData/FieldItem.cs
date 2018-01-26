using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Waher.Client.WPF.Model;
using Waher.Content;
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
		private QuantityField quantityField;

		/// <summary>
		/// Represents one item in a sensor data readout.
		/// </summary>
		/// <param name="Field">Sensor data field.</param>
		public FieldItem(Field Field)
			: base(Colors.Black, Colors.White)
		{
			this.field = Field;
			this.quantityField = Field as QuantityField;

			FieldQoS QoS = Field.QoS;

			if (QoS.HasFlag(FieldQoS.InvoiceConfirmed) || QoS.HasFlag(FieldQoS.Invoiced))
				this.BackgroundColor = Colors.Gold;
			else if (QoS.HasFlag(FieldQoS.EndOfSeries))
				this.BackgroundColor = Colors.LightBlue;
			else if (QoS.HasFlag(FieldQoS.Signed))
				this.BackgroundColor = Colors.LightGreen;
			else if (QoS.HasFlag(FieldQoS.Error))
				this.BackgroundColor = Colors.LightPink;
			else if (QoS.HasFlag(FieldQoS.PowerFailure) || QoS.HasFlag(FieldQoS.TimeOffset) || QoS.HasFlag(FieldQoS.Warning))
				this.BackgroundColor = Colors.LightYellow;
			else if (QoS.HasFlag(FieldQoS.Missing) || QoS.HasFlag(FieldQoS.InProgress))
				this.BackgroundColor = Colors.LightGray;
			else if (QoS.HasFlag(FieldQoS.AutomaticEstimate) || QoS.HasFlag(FieldQoS.ManualEstimate))
				this.BackgroundColor = Colors.WhiteSmoke;
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
		public string Value
		{
			get
			{
				if (this.quantityField != null)
					return CommonTypes.Encode(this.quantityField.Value, this.quantityField.NrDecimals);
				else
					return this.field.ValueString;
			}
		}

		/// <summary>
		/// Unit
		/// </summary>
		public string Unit
		{
			get
			{
				if (this.quantityField != null)
					return this.quantityField.Unit;
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
							case FieldQoS.ManualReadout: Output.Append("Manual Readout"); break;
							case FieldQoS.AutomaticReadout: Output.Append("Automatic Readout"); break;
							case FieldQoS.TimeOffset: Output.Append("Time Offset"); break;
							case FieldQoS.Warning: Output.Append("Warning"); break;
							case FieldQoS.Error: Output.Append("Error"); break;
							case FieldQoS.Signed: Output.Append("Signed"); break;
							case FieldQoS.Invoiced: Output.Append("Invoiced"); break;
							case FieldQoS.EndOfSeries: Output.Append("End of Series"); break;
							case FieldQoS.PowerFailure: Output.Append("Power Failure"); break;
							case FieldQoS.InvoiceConfirmed: Output.Append("Invoice Confirmed"); break;
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
							case FieldType.Historical: Output.Append("Historical"); break;
							default: Output.Append(Value.ToString()); break;
						}
					}

					this.typeString = Output.ToString();
				}

				return this.typeString;
			}
		}

		private string typeString = null;

		public string Alignment
		{
			get
			{
				if (this.field is QuantityField ||
					this.field is Int32Field ||
					this.field is Int64Field)
				{
					return "Right";
				}
				else if (this.field is BooleanField)
					return "Center";
				else
					return "Left";
			}
		}

	}
}
