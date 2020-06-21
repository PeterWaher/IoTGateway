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
		private static readonly Regex parser = new Regex(@"(?'Negation'-)?P((?'Years'\d+)Y)?((?'Months'\d+)M)?((?'Days'\d+)D)?(T((?'Hours'\d+)H)?((?'Minutes'\d+)M)?((?'Seconds'\d+([.]\d*)?)S)?)?", RegexOptions.Singleline | RegexOptions.Compiled);

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
		public Duration()
		{
			this.years = 0;
			this.months = 0;
			this.days = 0;
			this.hours = 0;
			this.minutes = 0;
			this.seconds = 0;
			this.negation = false;
		}

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
		/// Parses a duration from its string representation.
		/// </summary>
		/// <param name="s">String representation of duration.</param>
		/// <returns>Duration</returns>
		/// <exception cref="ArgumentException">If <paramref name="s"/> does not represent a valid duration.</exception>
		public static Duration Parse(string s)
		{
			if (!TryParse(s, out Duration Result))
				throw new ArgumentException("Invalid duration", nameof(s));

			return Result;
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
				IntOrEmpty(Groups["Years"].Value),
				IntOrEmpty(Groups["Months"].Value),
				IntOrEmpty(Groups["Days"].Value),
				IntOrEmpty(Groups["Hours"].Value),
				IntOrEmpty(Groups["Minutes"].Value),
				DoubleOrEmpty(Groups["Seconds"].Value));

			return true;
		}

		private static int IntOrEmpty(string s)
		{
			if (string.IsNullOrEmpty(s))
				return 0;
			else
				return int.Parse(s);
		}

		private static double DoubleOrEmpty(string s)
		{
			if (string.IsNullOrEmpty(s))
				return 0;
			else if (CommonTypes.TryParse(s, out double Result))
				return Result;
			else
				throw new ArgumentException("Invalid double number.", nameof(s));
		}

		/// <summary>
		/// Number of years.
		/// </summary>
		public int Years
		{
			get { return this.years; }
			set { this.years = value; }
		}

		/// <summary>
		/// Number of months.
		/// </summary>
		public int Months
		{
			get { return this.months; }
			set { this.months = value; }
		}

		/// <summary>
		/// Number of days.
		/// </summary>
		public int Days
		{
			get { return this.days; }
			set { this.days = value; }
		}

		/// <summary>
		/// Number of hours.
		/// </summary>
		public int Hours
		{
			get { return this.hours; }
			set { this.hours = value; }
		}

		/// <summary>
		/// Number of minutes.
		/// </summary>
		public int Minutes
		{
			get { return this.minutes; }
			set { this.minutes = value; }
		}

		/// <summary>
		/// Number of seconds.
		/// </summary>
		public double Seconds
		{
			get { return this.seconds; }
			set { this.seconds = value; }
		}

		/// <summary>
		/// If the duration is negative (true) or positive (false).
		/// </summary>
		public bool Negation
		{
			get { return this.negation; }
			set { this.negation = value; }
		}

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
		/// Checks if duration <paramref name="D1"/> is less than duration <paramref name="D2"/>.
		/// </summary>
		/// <param name="D1">Duration 1</param>
		/// <param name="D2">Duration 2</param>
		/// <returns>If <paramref name="D1"/>&lt;<paramref name="D2"/>.</returns>
		public static bool operator <(Duration D1, Duration D2)
		{
			DateTime Now = DateTime.Today;
			DateTime DT1 = Now + D1;
			DateTime DT2 = Now + D2;

			return DT1 < DT2;
		}

		/// <summary>
		/// Checks if duration <paramref name="D1"/> is less than or equal to duration <paramref name="D2"/>.
		/// </summary>
		/// <param name="D1">Duration 1</param>
		/// <param name="D2">Duration 2</param>
		/// <returns>If <paramref name="D1"/>&lt;=<paramref name="D2"/>.</returns>
		public static bool operator <=(Duration D1, Duration D2)
		{
			DateTime Now = DateTime.Today;
			DateTime DT1 = Now + D1;
			DateTime DT2 = Now + D2;

			return DT1 <= DT2;
		}

		/// <summary>
		/// Checks if duration <paramref name="D1"/> is greater than duration <paramref name="D2"/>.
		/// </summary>
		/// <param name="D1">Duration 1</param>
		/// <param name="D2">Duration 2</param>
		/// <returns>If <paramref name="D1"/>&gt;<paramref name="D2"/>.</returns>
		public static bool operator >(Duration D1, Duration D2)
		{
			DateTime Now = DateTime.Today;
			DateTime DT1 = Now + D1;
			DateTime DT2 = Now + D2;

			return DT1 > DT2;
		}

		/// <summary>
		/// Checks if duration <paramref name="D1"/> is greater than or equal to duration <paramref name="D2"/>.
		/// </summary>
		/// <param name="D1">Duration 1</param>
		/// <param name="D2">Duration 2</param>
		/// <returns>If <paramref name="D1"/>&gt;=<paramref name="D2"/>.</returns>
		public static bool operator >=(Duration D1, Duration D2)
		{
			DateTime Now = DateTime.Today;
			DateTime DT1 = Now + D1;
			DateTime DT2 = Now + D2;

			return DT1 >= DT2;
		}

		/// <summary>
		/// Checks if duration <paramref name="D1"/> is equal to duration <paramref name="D2"/>.
		/// </summary>
		/// <param name="D1">Duration 1</param>
		/// <param name="D2">Duration 2</param>
		/// <returns>If <paramref name="D1"/>==<paramref name="D2"/>.</returns>
		public static bool operator ==(Duration D1, Duration D2)
		{
			if (((object)D1) is null ^ ((object)D2) is null)
				return false;

			if ((object)D1 is null)
				return true;

			DateTime Now = DateTime.Today;
			DateTime DT1 = Now + D1;
			DateTime DT2 = Now + D2;

			return DT1 == DT2;
		}

		/// <summary>
		/// Checks if duration <paramref name="D1"/> is not equal to duration <paramref name="D2"/>.
		/// </summary>
		/// <param name="D1">Duration 1</param>
		/// <param name="D2">Duration 2</param>
		/// <returns>If <paramref name="D1"/>!=<paramref name="D2"/>.</returns>
		public static bool operator !=(Duration D1, Duration D2)
		{
			if (((object)D1) is null ^ ((object)D2) is null)
				return true;

			if ((object)D1 is null)
				return false;

			DateTime Now = DateTime.Today;
			DateTime DT1 = Now + D1;
			DateTime DT2 = Now + D2;

			return DT1 != DT2;
		}

		/// <summary>
		/// Checks if the duration is equal to another object.
		/// </summary>
		/// <param name="obj">Object to compare against.</param>
		/// <returns>If they are equal.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Duration D))
				return false;

			return this == D;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			return (reference + this).GetHashCode();
		}

		private static readonly DateTime reference = new DateTime(2000, 1, 1);

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (this.negation)
				sb.Append('-');

			sb.Append('P');

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

		/// <summary>
		/// Zero value
		/// </summary>
		public static readonly Duration Zero = new Duration(false, 0, 0, 0, 0, 0, 0);

		/// <summary>
		/// Creates a <see cref="Duration"/> object from a given number of years.
		/// </summary>
		/// <param name="Years">Number of years.</param>
		/// <returns><see cref="Duration"/> object.</returns>
		public static Duration FromYears(int Years)
		{
			return new Duration(Years < 0, Math.Abs(Years), 0, 0, 0, 0, 0);
		}

		/// <summary>
		/// Creates a <see cref="Duration"/> object from a given number of months.
		/// </summary>
		/// <param name="Months">Number of months.</param>
		/// <returns><see cref="Duration"/> object.</returns>
		public static Duration FromMonths(int Months)
		{
			return new Duration(Months < 0, 0, Math.Abs(Months), 0, 0, 0, 0);
		}

		/// <summary>
		/// Creates a <see cref="Duration"/> object from a given number of days.
		/// </summary>
		/// <param name="Days">Number of days.</param>
		/// <returns><see cref="Duration"/> object.</returns>
		public static Duration FromDays(int Days)
		{
			return new Duration(Days < 0, 0, 0, Math.Abs(Days), 0, 0, 0);
		}

		/// <summary>
		/// Creates a <see cref="Duration"/> object from a given number of hours.
		/// </summary>
		/// <param name="Hours">Number of hours.</param>
		/// <returns><see cref="Duration"/> object.</returns>
		public static Duration FromHours(int Hours)
		{
			return new Duration(Hours < 0, 0, 0, 0, Math.Abs(Hours), 0, 0);
		}

		/// <summary>
		/// Creates a <see cref="Duration"/> object from a given number of minutes.
		/// </summary>
		/// <param name="Minutes">Number of minutes.</param>
		/// <returns><see cref="Duration"/> object.</returns>
		public static Duration FromMinutes(int Minutes)
		{
			return new Duration(Minutes < 0, 0, 0, 0, 0,Math.Abs(Minutes), 0);
		}

		/// <summary>
		/// Creates a <see cref="Duration"/> object from a given number of seconds.
		/// </summary>
		/// <param name="Seconds">Number of seconds.</param>
		/// <returns><see cref="Duration"/> object.</returns>
		public static Duration FromSeconds(int Seconds)
		{
			return new Duration(Seconds < 0, 0, 0, 0, 0, 0, Math.Abs(Seconds));
		}
	}
}
