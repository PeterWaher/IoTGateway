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
	public class ErrorItem : ColorableItem
	{
		private ThingError error;

		/// <summary>
		/// Represents one item in a sensor data output.
		/// </summary>
		/// <param name="Error">Error message.</param>
		public ErrorItem(ThingError Error)
			: base(Colors.Yellow, Colors.Red)
		{
			this.error = Error;
		}

		/// <summary>
		/// Error message.
		/// </summary>
		public ThingError Error { get { return this.error; } }

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public string Timestamp
		{
			get
			{
				return this.error.Timestamp.ToShortDateString() + ", " + this.error.Timestamp.ToLongTimeString();
			}
		}

		/// <summary>
		/// Field Name
		/// </summary>
		public string FieldName { get { return string.Empty; } }

		/// <summary>
		/// Value
		/// </summary>
		public string Value
		{
			get
			{
				return this.error.ErrorMessage;
			}
		}

		/// <summary>
		/// Unit
		/// </summary>
		public string Unit
		{
			get
			{
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
				return "Error";
			}
		}

		/// <summary>
		/// Type
		/// </summary>
		public string Type
		{
			get
			{
				return string.Empty;
			}
		}

		public string Alignment
		{
			get
			{
				return "Left";
			}
		}

	}
}
