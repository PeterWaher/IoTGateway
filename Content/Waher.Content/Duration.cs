using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Waher.Content
{
	/// <summary>
	/// Represents a duration value, as defined by the xsd:duration data type:
	/// http://www.w3.org/TR/xmlschema-2/#duration
	/// </summary>
	public class Duration
	{
		private static Regex parser = new Regex(@"(?'Negation'-)?P((?'Years'\d+)Y)?((?'Months'\d+)M)?((?'Days'\d+)D)?(T((?'Hours'\d+)H)?((?'Minutes'\d+)M)?((?'Seconds'\d+([.]\d*)?)S)?)?", RegexOptions.Singleline | RegexOptions.Compiled);

		private int years;
		private int months;
		private int days;
		private int hours;
		private int minutes;
		private double seconds;
		private bool negation;

		/// <summary>
		/// Represents a duration value, as defined by the xsd:duration data type:
		/// http://www.w3.org/TR/xmlschema-2/#duration
		/// </summary>
		/// <param name="Negation">If the duration is negative (true) or positive (false).</param>
		/// <param name="Years">Number of years.</param>
		/// <param name="Months">Number of months.</param>
		/// <param name="Days">Number of days.</param>
		/// <param name="Hours">Number of hours.</param>
		/// <param name="Minutes">Number of minutes.</param>
		/// <param name="Seconds">Number of seconds.</param>
		public Duration(bool Negation, int Years, int Months, int Days, int Hours, int Minutes, double Seconds)
		{
			this.negation = Negation;
			this.years = Years;
			this.months = Months;
			this.days = Days;
			this.hours = Hours;
			this.minutes = Minutes;
			this.seconds = Seconds;
		}

		/// <summary>
		/// Tries to parse a duration value.
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="Result">Duration, if successful.</param>
		/// <returns>If the string could be parsed.</returns>
		public static bool TryParse(string s, out Duration Result)
		{
			Match M = parser.Match(s);
			if (!M.Success || M.Index > 0 || M.Length != s.Length)
			{
				Result = null;
				return false;
			}

			GroupCollection Groups = M.Groups;

			Result = new Duration(
				!string.IsNullOrEmpty(Groups["Negation"].Value),
				int.Parse(Groups["Years"].Value),
				int.Parse(Groups["Months"].Value),
				int.Parse(Groups["Days"].Value),
				int.Parse(Groups["Hours"].Value),
				int.Parse(Groups["Minutes"].Value),
				double.Parse(Groups["Seconds"].Value));

			return true;
		}

		/// <summary>
		/// Number of years.
		/// </summary>
		public int Years { get { return this.years; } }

		/// <summary>
		/// Number of months.
		/// </summary>
		public int Months { get { return this.months; } }

		/// <summary>
		/// Number of days.
		/// </summary>
		public int Days { get { return this.days; } }

		/// <summary>
		/// Number of hours.
		/// </summary>
		public int Hours { get { return this.hours; } }

		/// <summary>
		/// Number of minutes.
		/// </summary>
		public int Minutes { get { return this.minutes; } }

		/// <summary>
		/// Number of seconds.
		/// </summary>
		public double Seconds { get { return this.seconds; } }

		/// <summary>
		/// If the duration is negative (true) or positive (false).
		/// </summary>
		public bool Negation { get { return this.negation; } }

		/// <summary>
		/// Adds a duration to a <see cref="DateTime"/> value.
		/// </summary>
		/// <param name="Timepoint">DateTime value.</param>
		/// <param name="Offset">Offset.</param>
		/// <returns><paramref name="Timepoint"/>+<paramref name="Offset"/>.</returns>
		public static DateTime operator +(DateTime Timepoint, Duration Offset)
		{
			if (Offset.negation)
			{
				if (Offset.years != 0)
					Timepoint = Timepoint.AddYears(-Offset.years);

				if (Offset.months != 0)
					Timepoint = Timepoint.AddMonths(-Offset.months);

				if (Offset.days != 0)
					Timepoint = Timepoint.AddDays(-Offset.days);

				if (Offset.hours != 0)
					Timepoint = Timepoint.AddHours(-Offset.hours);

				if (Offset.minutes != 0)
					Timepoint = Timepoint.AddMinutes(-Offset.minutes);

				if (Offset.seconds != 0)
					Timepoint = Timepoint.AddSeconds(-Offset.seconds);
			}
			else
			{
				if (Offset.years != 0)
					Timepoint = Timepoint.AddYears(Offset.years);

				if (Offset.months != 0)
					Timepoint = Timepoint.AddMonths(Offset.months);

				if (Offset.days != 0)
					Timepoint = Timepoint.AddDays(Offset.days);

				if (Offset.hours != 0)
					Timepoint = Timepoint.AddHours(Offset.hours);

				if (Offset.minutes != 0)
					Timepoint = Timepoint.AddMinutes(Offset.minutes);

				if (Offset.seconds != 0)
					Timepoint = Timepoint.AddSeconds(Offset.seconds);
			}

			return Timepoint;
		}

		/// <summary>
		/// Subtracts a duration from a <see cref="DateTime"/> value.
		/// </summary>
		/// <param name="Timepoint">DateTime value.</param>
		/// <param name="Offset">Offset.</param>
		/// <returns><paramref name="Timepoint"/>-<paramref name="Offset"/>.</returns>
		public static DateTime operator -(DateTime Timepoint, Duration Offset)
		{
			if (Offset.negation)
			{
				if (Offset.years != 0)
					Timepoint = Timepoint.AddYears(Offset.years);

				if (Offset.months != 0)
					Timepoint = Timepoint.AddMonths(Offset.months);

				if (Offset.days != 0)
					Timepoint = Timepoint.AddDays(Offset.days);

				if (Offset.hours != 0)
					Timepoint = Timepoint.AddHours(Offset.hours);

				if (Offset.minutes != 0)
					Timepoint = Timepoint.AddMinutes(Offset.minutes);

				if (Offset.seconds != 0)
					Timepoint = Timepoint.AddSeconds(Offset.seconds);
			}
			else
			{
				if (Offset.years != 0)
					Timepoint = Timepoint.AddYears(-Offset.years);

				if (Offset.months != 0)
					Timepoint = Timepoint.AddMonths(-Offset.months);

				if (Offset.days != 0)
					Timepoint = Timepoint.AddDays(-Offset.days);

				if (Offset.hours != 0)
					Timepoint = Timepoint.AddHours(-Offset.hours);

				if (Offset.minutes != 0)
					Timepoint = Timepoint.AddMinutes(-Offset.minutes);

				if (Offset.seconds != 0)
					Timepoint = Timepoint.AddSeconds(-Offset.seconds);
			}

			return Timepoint;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append('P');

			if (this.negation)
				sb.Append('-');

			if (this.years != 0)
			{
				sb.Append(this.years.ToString());
				sb.Append('Y');
			}

			if (this.months != 0)
			{
				sb.Append(this.months.ToString());
				sb.Append('M');
			}

			if (this.days != 0)
			{
				sb.Append(this.days.ToString());
				sb.Append('D');
			}

			if (this.hours != 0 || this.minutes != 0 || this.seconds != 0)
			{
				sb.Append('T');

				if (this.hours != 0)
				{
					sb.Append(this.hours.ToString());
					sb.Append('H');
				}

				if (this.minutes != 0)
				{
					sb.Append(this.minutes.ToString());
					sb.Append('M');
				}

				if (this.seconds != 0)
				{
					sb.Append(CommonTypes.Encode(this.seconds));
					sb.Append('S');
				}
			}

			string Result = sb.ToString();

			if (Result.Length <= 2)
				Result += "T0S";

			return Result;
		}

	}
}
