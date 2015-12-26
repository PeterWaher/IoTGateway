using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Base class for internal readout event arguments.
	/// </summary>
	public abstract class InternalReadoutEventArgs
	{
		private bool done;
		private object state;

		/// <summary>
		/// Base class for internal readout event arguments.
		/// </summary>
		/// <param name="Done">If the readout is done.</param>
		/// <param name="State">State object used in original request.</param>
		public InternalReadoutEventArgs(bool Done, object State)
		{
			this.done = Done;
			this.state = State;
		}

		/// <summary>
		/// If the readout is done.
		/// </summary>
		public bool Done { get { return this.done; } }

		/// <summary>
		/// State object used in original request.
		/// </summary>
		public object State { get { return this.state; } }
	}
}
