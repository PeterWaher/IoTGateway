using System;
using System.Collections.Generic;
using System.Text;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Delegate for internal readout field events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void InternalReadoutFieldsEventHandler(object Sender, InternalReadoutFieldsEventArgs e);

	/// <summary>
	/// Event arguments for internal readout field events.
	/// </summary>
	public class InternalReadoutFieldsEventArgs : InternalReadoutEventArgs
	{
		private IEnumerable<Field> fields;

		/// <summary>
		/// Event arguments for internal readout field events.
		/// </summary>
		/// <param name="Done">If the readout is done.</param>
		/// <param name="State">State object used in original request.</param>
		public InternalReadoutFieldsEventArgs(bool Done, IEnumerable<Field> Fields, object State)
			: base(Done, State)
		{
			this.fields = Fields;
		}

		/// <summary>
		/// New fields reported.
		/// </summary>
		public IEnumerable<Field> Fields { get { return this.fields; } }
	}
}
