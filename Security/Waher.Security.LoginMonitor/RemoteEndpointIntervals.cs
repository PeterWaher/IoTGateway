namespace Waher.Security.LoginMonitor
{
    /// <summary>
    /// A set of intervals specific for a given endpoint.
    /// </summary>
    public class RemoteEndpointIntervals
    {
        private readonly string endpoint;
		private readonly LoginInterval[] intervals;
		private readonly int nrIntervals;

		/// <summary>
		/// A set of intervals specific for a given endpoint.
		/// </summary>
		/// <param name="Endpoint">Endpoint</param>
		/// <param name="LoginIntervals">Number of login attempts possible during given time period. Numbers must be positive, and
		/// interval ascending. If continually failing past accepted intervals, remote endpoint will be registered as malicious.</param>
		public RemoteEndpointIntervals(string Endpoint, params LoginInterval[] LoginIntervals)
		{
			this.endpoint = Endpoint;
			this.intervals = LoginIntervals;
			this.nrIntervals = LoginIntervals.Length;
		}

		/// <summary>
		/// Associated remote endpoint.
		/// </summary>
		public string Endpoint => this.endpoint;

		/// <summary>
		/// Number of login attempts possible during given time period. Numbers must be positive, and
		/// interval ascending. If continually failing past accepted intervals, remote endpoint will be registered as malicious.
		/// </summary>
		public LoginInterval[] Intervals => this.intervals;

		/// <summary>
		/// Number of intervals.
		/// </summary>
		public int NrIntervals => this.nrIntervals;
	}
}
