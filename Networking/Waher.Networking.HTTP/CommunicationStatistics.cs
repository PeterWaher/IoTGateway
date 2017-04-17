using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Contains communication statistics.
	/// </summary>
	public class CommunicationStatistics
	{
		private Dictionary<string, long> nrCallsPerMethod;
		private Dictionary<string, long> nrCallsPerUserAgent;
		private Dictionary<string, long> nrCallsPerFrom;
		private Dictionary<string, long> nrCallsPerResource;
		private DateTime lastStat;
		private DateTime currentStat;
		private long nrBytesRx;
		private long nrBytesTx;
		private long nrCalls;

		/// <summary>
		/// Number of method calls, per method.
		/// </summary>
		public Dictionary<string, long> NrCallsPerMethod
		{
			get { return this.nrCallsPerMethod; }
			internal set { this.nrCallsPerMethod = value; }
		}

		/// <summary>
		/// Number of method calls, per User Agent header value.
		/// </summary>
		public Dictionary<string, long> NrCallsPerUserAgent
		{
			get { return this.nrCallsPerUserAgent; }
			internal set { this.nrCallsPerUserAgent = value; }
		}

		/// <summary>
		/// Number of method calls, per From header value.
		/// </summary>
		public Dictionary<string, long> NrCallsPerFrom
		{
			get { return this.nrCallsPerFrom; }
			internal set { this.nrCallsPerFrom = value; }
		}

		/// <summary>
		/// Number of method calls, per resource.
		/// </summary>
		public Dictionary<string, long> NrCallsPerResource
		{
			get { return this.nrCallsPerResource; }
			internal set { this.nrCallsPerResource = value; }
		}

		/// <summary>
		/// Timestamp of last statistics. If <see cref="DateTime.MinValue"/>, no statistics has been retrieved since restart of server.
		/// </summary>
		public DateTime LastStat
		{
			get { return this.lastStat; }
			internal set { this.lastStat = value; }
		}

		/// <summary>
		/// Timestamp of current statistics.
		/// </summary>
		public DateTime CurrentStat
		{
			get { return this.currentStat; }
			internal set { this.currentStat = value; }
		}

		/// <summary>
		/// Number of bytes received.
		/// </summary>
		public long NrBytesRx
		{
			get { return this.nrBytesRx; }
			internal set { this.nrBytesRx = value; }
		}

		/// <summary>
		/// Number of bytes transmitted.
		/// </summary>
		public long NrBytesTx
		{
			get { return this.nrBytesTx; }
			internal set { this.nrBytesTx = value; }
		}

		/// <summary>
		/// Number of method calls.
		/// </summary>
		public long NrCalls
		{
			get { return this.nrCalls; }
			internal set { this.nrCalls = value; }
		}

	}
}
