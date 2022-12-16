namespace Waher.Networking.XMPP.Sensor
{
	/// <summary>
	/// Base class for internal readout event arguments.
	/// </summary>
	public abstract class InternalReadoutEventArgs
	{
		private readonly bool done;
		private readonly object state;

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
		public bool Done => this.done;

		/// <summary>
		/// State object used in original request.
		/// </summary>
		public object State => this.state;
	}
}
