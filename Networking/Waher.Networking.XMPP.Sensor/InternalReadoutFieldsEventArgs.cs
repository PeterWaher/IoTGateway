﻿using System.Collections.Generic;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Event arguments for internal readout field events.
	/// </summary>
	public class InternalReadoutFieldsEventArgs : InternalReadoutEventArgs
	{
		private readonly IEnumerable<Field> fields;

		/// <summary>
		/// Event arguments for internal readout field events.
		/// </summary>
		/// <param name="Done">If the readout is done.</param>
		/// <param name="Fields">Fields.</param>
		/// <param name="State">State object used in original request.</param>
		public InternalReadoutFieldsEventArgs(bool Done, IEnumerable<Field> Fields, object State)
			: base(Done, State)
		{
			this.fields = Fields;
		}

		/// <summary>
		/// New fields reported.
		/// </summary>
		public IEnumerable<Field> Fields => this.fields;
	}
}
