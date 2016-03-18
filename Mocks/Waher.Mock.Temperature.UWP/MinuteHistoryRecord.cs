using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Mock.Temperature
{
	/// <summary>
	/// Minute history record.
	/// </summary>
	public class MinuteHistoryRecord
	{
		private DateTime timestamp;
		private double temperature;

		/// <summary>
		/// Minute history record.
		/// </summary>
		/// <param name="Timestamp">Timestamp</param>
		/// <param name="Temperature">Temperature</param>
		public MinuteHistoryRecord(DateTime Timestamp, double Temperature)
		{
			this.temperature = Temperature;
			this.timestamp = Timestamp;
		}

		/// <summary>
		/// Timestamp.
		/// </summary>
		public DateTime Timestamp { get { return this.timestamp; } }

		/// <summary>
		/// Temperature.
		/// </summary>
		public double Temperature { get { return this.temperature; } }
	}
}
