using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Delegate for internal readout error events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task InternalReadoutErrorsEventHandler(object Sender, InternalReadoutErrorsEventArgs e);

	/// <summary>
	/// Event arguments for internal readout error events.
	/// </summary>
	public class InternalReadoutErrorsEventArgs : InternalReadoutEventArgs
	{
		private readonly IEnumerable<ThingError> errors;

		/// <summary>
		/// Event arguments for internal readout error events.
		/// </summary>
		/// <param name="Done">If the readout is done.</param>
		/// <param name="Errors">Errors.</param>
		/// <param name="State">State object used in original request.</param>
		public InternalReadoutErrorsEventArgs(bool Done, IEnumerable<ThingError> Errors, object State)
			: base(Done, State)
		{
			this.errors = Errors;
		}

		/// <summary>
		/// New errors reported.
		/// </summary>
		public IEnumerable<ThingError> Errors { get { return this.errors; } }
	}
}
