using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Mock.Temperature
{
	/// <summary>
	/// Day history record.
	/// </summary>
	public class DayHistoryRecord
	{
		private DateTime periodStart;
		private DateTime periodStop;
		private double minTemperature;
		private double maxTemperature;
		private double averageTemperature;

		/// <summary>
		/// Day history record.
		/// </summary>
		/// <param name="PeriodStart">Period start.</param>
		/// <param name="PeriodStop">Period stop.</param>
		/// <param name="MinTemperature">Minimum temperature.</param>
		/// <param name="MaxTemperature">Maximum temperature.</param>
		/// <param name="AverageTemperature">Average temperature.</param>
		public DayHistoryRecord(DateTime PeriodStart, DateTime PeriodStop, double MinTemperature, double MaxTemperature, double AverageTemperature)
		{
			this.periodStart = PeriodStart;
			this.periodStop = PeriodStop;
			this.minTemperature = MinTemperature;
			this.maxTemperature = MaxTemperature;
			this.averageTemperature = AverageTemperature;
		}

		/// <summary>
		/// Period start.
		/// </summary>
		public DateTime PeriodStart { get { return this.periodStart; } }

		/// <summary>
		/// Period stop.
		/// </summary>
		public DateTime PeriodStop { get { return this.periodStop; } }

		/// <summary>
		/// Minimum temperature.
		/// </summary>
		public double MinTemperature { get { return this.minTemperature; } }

		/// <summary>
		/// Maximum temperature.
		/// </summary>
		public double MaxTemperature { get { return this.maxTemperature; } }

		/// <summary>
		/// Average temperature.
		/// </summary>
		public double AverageTemperature { get { return this.averageTemperature; } }
	}
}
