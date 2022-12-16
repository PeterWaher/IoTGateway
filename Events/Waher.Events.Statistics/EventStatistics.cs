using System;
using System.Collections.Generic;

namespace Waher.Events.Statistics
{
	/// <summary>
	/// Contains event statistics.
	/// </summary>
	public class EventStatistics
	{
		private Dictionary<string, Statistic> perActor = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perEventId = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perFacility = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perModule = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perLevel = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perType = new Dictionary<string, Statistic>();
		private Dictionary<string, Statistic> perStackTrace = new Dictionary<string, Statistic>();
		private DateTime lastStat;
		private DateTime currentStat;

		/// <summary>
		/// Number of events, per actor.
		/// </summary>
		public Dictionary<string, Statistic> PerActor
		{
			get => this.perActor;
			internal set => this.perActor = value;
		}

		/// <summary>
		/// Number of events, per actor.
		/// </summary>
		public Dictionary<string, Statistic> PerEventId
		{
			get => this.perEventId;
			internal set => this.perEventId = value;
		}

		/// <summary>
		/// Number of events, per actor.
		/// </summary>
		public Dictionary<string, Statistic> PerFacility
		{
			get => this.perFacility;
			internal set => this.perFacility = value;
		}

		/// <summary>
		/// Number of events, per actor.
		/// </summary>
		public Dictionary<string, Statistic> PerModule
		{
			get => this.perModule;
			internal set => this.perModule = value;
		}

		/// <summary>
		/// Number of events, per actor.
		/// </summary>
		public Dictionary<string, Statistic> PerLevel
		{
			get => this.perLevel;
			internal set => this.perLevel = value;
		}

		/// <summary>
		/// Number of events, per actor.
		/// </summary>
		public Dictionary<string, Statistic> PerType
		{
			get => this.perType;
			internal set => this.perType = value;
		}

		/// <summary>
		/// Number of events, per stack trace (Only of type Critical, Alert and Emergency).
		/// </summary>
		public Dictionary<string, Statistic> PerStackTrace
		{
			get => this.perStackTrace;
			internal set => this.perStackTrace = value;
		}

		/// <summary>
		/// Timestamp of last statistics. If <see cref="DateTime.MinValue"/>, no statistics has been retrieved since restart of server.
		/// </summary>
		public DateTime LastStat
		{
			get => this.lastStat;
			internal set => this.lastStat = value;
		}

		/// <summary>
		/// Timestamp of current statistics.
		/// </summary>
		public DateTime CurrentStat
		{
			get => this.currentStat;
			internal set => this.currentStat = value;
		}
	}
}
