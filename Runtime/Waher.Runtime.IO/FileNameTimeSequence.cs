using System;
using System.IO;

namespace Waher.Runtime.IO
{
	/// <summary>
	/// Class that generates a sequence of file names, based on the system time.
	/// </summary>
	public class FileNameTimeSequence
	{
		/// <summary>
		/// %YEAR%
		/// </summary>
		public const string DefaultYearPlaceholder = "%YEAR%";

		/// <summary>
		/// %MONTH%
		/// </summary>
		public const string DefaultMonthPlaceholder = "%MONTH%";

		/// <summary>
		/// %DAY%
		/// </summary>
		public const string DefaultDayPlaceholder = "%DAY%";

		/// <summary>
		/// %HOUR%
		/// </summary>
		public const string DefaultHourPlaceholder = "%HOUR%";

		/// <summary>
		/// %MINUTE%
		/// </summary>
		public const string DefaultMinutePlaceholder = "%MINUTE%";

		/// <summary>
		/// %SECOND%
		/// </summary>
		public const string DefaultSecondPlaceholder = "%SECOND%";

		private static readonly object systemSynchObject = new object();

		private readonly bool utc;
		private readonly string fileNamePattern;
		private readonly string placeholderYear;
		private readonly string placeholderMonth;
		private readonly string placeholderDay;
		private readonly string placeholderHour;
		private readonly string placeholderMinute;
		private readonly string placeholderSecond;
		private readonly bool usesYearPlaceholder;
		private readonly bool usesMonthPlaceholder;
		private readonly bool usesDayPlaceholder;
		private readonly bool usesHourPlaceholder;
		private readonly bool usesMinutePlaceholder;
		private readonly bool usesSecondPlaceholder;
		private DateTime last = DateTime.MinValue;
		private bool hasLast = false;


		/// <summary>
		/// Class that generates a sequence of file names, based on the system time.
		/// </summary>
		///	<param name="FileNamePattern">File Name Pattern</param>
		public FileNameTimeSequence(string FileNamePattern)
			: this(FileNamePattern, true)
		{
		}

		/// <summary>
		/// Class that generates a sequence of file names, based on the system time.
		/// </summary>
		///	<param name="FileNamePattern">File Name Pattern</param>
		/// <param name="Utc">If UTC time (true) or local time (false) should be used.</param>
		public FileNameTimeSequence(string FileNamePattern, bool Utc)
			: this(FileNamePattern, Utc, DefaultYearPlaceholder, DefaultMonthPlaceholder,
				  DefaultDayPlaceholder, DefaultHourPlaceholder, DefaultMinutePlaceholder,
				  DefaultSecondPlaceholder)
		{
		}

		/// <summary>
		/// Class that generates a sequence of file names, based on the system time.
		/// </summary>
		///	<param name="FileNamePattern">File Name Pattern</param>
		/// <param name="YearPlaceholder">Placeholder string that will be replaced by the current year.</param>
		/// <param name="MonthPlaceholder">Placeholder string that will be replaced by the current month.</param>
		/// <param name="DayPlaceholder">Placeholder string that will be replaced by the current day.</param>
		/// <param name="HourPlaceholder">Placeholder string that will be replaced by the current hour.</param>
		/// <param name="MinutePlaceholder">Placeholder string that will be replaced by the current minute.</param>
		/// <param name="SecondPlaceholder">Placeholder string that will be replaced by the current second.</param>
		public FileNameTimeSequence(string FileNamePattern,
			string YearPlaceholder, string MonthPlaceholder, string DayPlaceholder,
			string HourPlaceholder, string MinutePlaceholder, string SecondPlaceholder)
			: this(FileNamePattern, true, YearPlaceholder, MonthPlaceholder, DayPlaceholder,
				HourPlaceholder, MinutePlaceholder, SecondPlaceholder)
		{
		}

		/// <summary>
		/// Class that generates a sequence of file names, based on the system time.
		/// </summary>
		///	<param name="FileNamePattern">File Name Pattern</param>
		/// <param name="Utc">If UTC time (true) or local time (false) should be used.</param>
		/// <param name="YearPlaceholder">Placeholder string that will be replaced by the current year.</param>
		/// <param name="MonthPlaceholder">Placeholder string that will be replaced by the current month.</param>
		/// <param name="DayPlaceholder">Placeholder string that will be replaced by the current day.</param>
		/// <param name="HourPlaceholder">Placeholder string that will be replaced by the current hour.</param>
		/// <param name="MinutePlaceholder">Placeholder string that will be replaced by the current minute.</param>
		/// <param name="SecondPlaceholder">Placeholder string that will be replaced by the current second.</param>
		public FileNameTimeSequence(string FileNamePattern, bool Utc,
			string YearPlaceholder, string MonthPlaceholder, string DayPlaceholder,
			string HourPlaceholder, string MinutePlaceholder, string SecondPlaceholder)
		{
			this.fileNamePattern = FileNamePattern;
			this.utc = Utc;

			this.placeholderYear = YearPlaceholder;
			this.placeholderMonth = MonthPlaceholder;
			this.placeholderDay = DayPlaceholder;
			this.placeholderHour = HourPlaceholder;
			this.placeholderMinute = MinutePlaceholder;
			this.placeholderSecond = SecondPlaceholder;

			this.usesYearPlaceholder = !string.IsNullOrEmpty(this.placeholderYear) && this.fileNamePattern.Contains(this.placeholderYear);
			this.usesMonthPlaceholder = !string.IsNullOrEmpty(this.placeholderMonth) && this.fileNamePattern.Contains(this.placeholderMonth);
			this.usesDayPlaceholder = !string.IsNullOrEmpty(this.placeholderDay) && this.fileNamePattern.Contains(this.placeholderDay);
			this.usesHourPlaceholder = !string.IsNullOrEmpty(this.placeholderHour) && this.fileNamePattern.Contains(this.placeholderHour);
			this.usesMinutePlaceholder = !string.IsNullOrEmpty(this.placeholderMinute) && this.fileNamePattern.Contains(this.placeholderMinute);
			this.usesSecondPlaceholder = !string.IsNullOrEmpty(this.placeholderSecond) && this.fileNamePattern.Contains(this.placeholderSecond);
		}

		/// <summary>
		/// If UTC timestamps are used.
		/// </summary>
		public bool Utc => this.utc;

		/// <summary>
		/// File name pattern.
		/// </summary>
		public string FileNamePattern => this.fileNamePattern;

		/// <summary>
		/// Placeholder for the current year.
		/// </summary>
		public string PlaceholderYear => this.placeholderYear;

		/// <summary>
		/// Placeholder for the current month.
		/// </summary>
		public string PlaceholderMonth => this.placeholderMonth;

		/// <summary>
		/// Placeholder for the current day.
		/// </summary>
		public string PlaceholderDay => this.placeholderDay;

		/// <summary>
		/// Placeholder for the current hour.
		/// </summary>
		public string PlaceholderHour => this.placeholderHour;

		/// <summary>
		/// Placeholder for the current minute.
		/// </summary>
		public string PlaceholderMinute => this.placeholderMinute;

		/// <summary>
		/// Placeholder for the current second.
		/// </summary>
		public string PlaceholderSecond => this.placeholderSecond;

		/// <summary>
		/// If the placeholder for the current year is used.
		/// </summary>
		public bool UsesYearPlaceholder => this.usesYearPlaceholder;

		/// <summary>
		/// If the placeholder for the current month is used.
		/// </summary>
		public bool UsesMonthPlaceholder => this.usesMonthPlaceholder;

		/// <summary>
		/// If the placeholder for the current day is used.
		/// </summary>
		public bool UsesDayPlaceholder => this.usesDayPlaceholder;

		/// <summary>
		/// If the placeholder for the current hour is used.
		/// </summary>
		public bool UsesHourPlaceholder => this.usesHourPlaceholder;

		/// <summary>
		/// If the placeholder for the current minute is used.
		/// </summary>
		public bool UsesMinutePlaceholder => this.usesMinutePlaceholder;

		/// <summary>
		/// If the placeholder for the current second is used.
		/// </summary>
		public bool UsesSecondPlaceholder => this.usesSecondPlaceholder;

		/// <summary>
		/// Last timestamp that generated a new file name.
		/// </summary>
		public DateTime Last => this.last;

		/// <summary>
		/// Returns a unique file name, if a file with the name already exists.
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns>If possible to find a unique file name.</returns>
		public static bool MakeUnique(ref string FileName)
		{
			if (File.Exists(FileName))
			{
				int i = FileName.LastIndexOf('.');
				if (i < 0)
					i = FileName.Length;

				string FileName2;
				string Suffix;
				int j = 1;

				do
				{
					j++;
					Suffix = " (" + j.ToString() + ")";

					FileName2 = FileName.Insert(i, Suffix);
				}
				while (File.Exists(FileName2) && j < 65536);

				if (j == 65536)
					return false;

				FileName = FileName2;
			}

			return true;
		}

		/// <summary>
		/// Tries to get a new file name.
		/// </summary>
		/// <param name="FileName">Resulting new file name, or null if no new file name
		/// is generated.</param>
		/// <returns>If a new file name was created.</returns>
		public bool TryGetNewFileName(out string FileName)
		{
			DateTime TP = this.utc ? DateTime.UtcNow : DateTime.Now;

			if (!this.hasLast ||
				(this.usesSecondPlaceholder && this.last.Second != TP.Second) ||
				(this.usesMinutePlaceholder && this.last.Minute != TP.Minute) ||
				(this.usesHourPlaceholder && this.last.Hour != TP.Hour) ||
				(this.usesDayPlaceholder && this.last.Day != TP.Day) ||
				(this.usesMonthPlaceholder && this.last.Month != TP.Month) ||
				(this.usesYearPlaceholder && this.last.Year != TP.Year))
			{
				this.last = TP;
				this.hasLast = true;

				FileName = this.fileNamePattern;

				if (this.usesSecondPlaceholder)
					FileName = FileName.Replace(this.placeholderSecond, TP.Second.ToString("D2"));

				if (this.usesMinutePlaceholder)
					FileName = FileName.Replace(this.placeholderMinute, TP.Minute.ToString("D2"));

				if (this.usesHourPlaceholder)
					FileName = FileName.Replace(this.placeholderHour, TP.Hour.ToString("D2"));

				if (this.usesDayPlaceholder)
					FileName = FileName.Replace(this.placeholderDay, TP.Day.ToString("D2"));

				if (this.usesMonthPlaceholder)
					FileName = FileName.Replace(this.placeholderMonth, TP.Month.ToString("D2"));

				if (this.usesYearPlaceholder)
					FileName = FileName.Replace(this.placeholderYear, TP.Year.ToString("D4"));

				lock (systemSynchObject)
				{
					if (!MakeUnique(ref FileName))
						return false;

					StreamWriter fs = File.CreateText(FileName);    // Creates a zero-byte file, to reserve the name,
					fs.Close();
					fs.Dispose();
				}

				return true;
			}
			else
			{
				FileName = null;
				return false;
			}
		}
	}
}
