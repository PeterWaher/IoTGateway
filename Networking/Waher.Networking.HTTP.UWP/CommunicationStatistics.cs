using System;
using System.Collections.Generic;
using Waher.Events.Statistics;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Contains communication statistics.
	/// </summary>
	public class CommunicationStatistics
	{
		private Dictionary<string, Statistic> callsPerMethod;
		private Dictionary<string, Statistic> callsPerUserAgent;
		private Dictionary<string, Statistic> callsPerFrom;
		private Dictionary<string, Statistic> callsPerResource;
		private DateTime lastStat;
		private DateTime currentStat;
		private long nrBytesRx;
		private long nrBytesTx;
		private long nrCalls;

		/// <summary>
		/// Calls per method.
		/// </summary>
		public Dictionary<string, Statistic> CallsPerMethod
		{
			get { return this.callsPerMethod; }
			internal set { this.callsPerMethod = value; }
		}

		/// <summary>
		/// Calls per User Agent header value.
		/// </summary>
		public Dictionary<string, Statistic> CallsPerUserAgent
		{
			get { return this.callsPerUserAgent; }
			internal set { this.callsPerUserAgent = value; }
		}

		/// <summary>
		/// Calls per From header value.
		/// </summary>
		public Dictionary<string, Statistic> CallsPerFrom
		{
			get { return this.callsPerFrom; }
			internal set { this.callsPerFrom = value; }
		}

		/// <summary>
		/// Calls per resource.
		/// </summary>
		public Dictionary<string, Statistic> CallsPerResource
		{
			get { return this.callsPerResource; }
			internal set { this.callsPerResource = value; }
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
