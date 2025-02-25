using System.Net;
using Waher.Runtime.IO;

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
		private readonly IPAddress ipAddress;
		private readonly IpCidr ipRange;
		private readonly bool isIpRange;
		private readonly bool isIpAddress;

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

			this.isIpAddress = IPAddress.TryParse(Endpoint, out ipAddress);
			this.isIpRange = !this.isIpAddress && IpCidr.TryParse(Endpoint, out this.ipRange);
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

		/// <summary>
		/// If the endpoint represents an IP Address.
		/// </summary>
		public bool IsIpAddress => this.isIpAddress;

		/// <summary>
		/// IP Address, if <see cref="IsIpAddress"/> is true.
		/// </summary>
		public IPAddress IpAddress => this.ipAddress;

		/// <summary>
		/// If the endpoint represents an IP range.
		/// </summary>
		public bool IsIpRange => this.isIpRange;

		/// <summary>
		/// IP Range, if <see cref="isIpRange"/> is true.
		/// </summary>
		public IpCidr IpRange => this.ipRange;
	}
}
