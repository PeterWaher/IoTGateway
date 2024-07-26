using System;
using System.Text;
using System.Text.RegularExpressions;
using Waher.Script.Functions.DateAndTime;

namespace Waher.Content
{
	/// <summary>
	/// Represents a duration value, as defined by the xsd:duration data type:
	/// http://www.w3.org/TR/xmlschema-2/#duration
	/// </summary>
	public struct Duration : IComparable<Duration>, ISeconds, IMinutes, IHours, IDays, IMonths, IYears
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
		/// Represents a duration value, as defined by the xsd:duration data type:
		/// http://www.w3.org/TR/xmlschema-2/#duration
		/// </summary>
		/// <param name="s">String representation of duration.</param>
		public Duration(string s)
		{
			if (!TryParse(s, out Duration Result))
				throw new ArgumentException("Invalid duration", nameof(s));

			this.years = Result.years;
			this.months = Result.months;
			this.days = Result.days;
			this.hours = Result.hours;
			this.minutes = Result.minutes;
			this.seconds = Result.seconds;
			this.negation = Result.negation;
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
				Result = default;
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
			get => this.years;
			set => this.years = value;
		}

		/// <summary>
		/// Number of months.
		/// </summary>
		public int Months
		{
			get => this.months;
			set => this.months = value;
		}

		/// <summary>
		/// Number of days.
		/// </summary>
		public int Days
		{
			get => this.days;
			set => this.days = value;
		}

		/// <summary>
		/// Number of hours.
		/// </summary>
		public int Hours
		{
			get => this.hours;
			set => this.hours = value;
		}

		/// <summary>
		/// Number of minutes.
		/// </summary>
		public int Minutes
		{
			get => this.minutes;
			set => this.minutes = value;
		}

		/// <summary>
		/// Number of seconds.
		/// </summary>
		public double Seconds
		{
			get => this.seconds;
			set => this.seconds = value;
		}

		/// <summary>
		/// If the duration is negative (true) or positive (false).
		/// </summary>
		public bool Negation
		{
			get => this.negation;
			set => this.negation = value;
		}

		/// <summary>
		/// Adds a duration to a <see cref="System.DateTime"/> value.
		/// </summary>
		/// <param name="Timepoint">System.DateTime value.</param>
		/// <param name="Offset">Offset.</param>
		/// <returns><paramref name="Timepoint"/>+<paramref name="Offset"/>.</returns>
		public static System.DateTime operator +(System.DateTime Timepoint, Duration Offset)
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
		/// Subtracts a duration from a <see cref="System.DateTime"/> value.
		/// </summary>
		/// <param name="Timepoint">System.DateTime value.</param>
		/// <param name="Offset">Offset.</param>
		/// <returns><paramref name="Timepoint"/>-<paramref name="Offset"/>.</returns>
		public static System.DateTime operator -(System.DateTime Timepoint, Duration Offset)
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
			System.DateTime DT1 = JSON.UnixEpoch + D1;
			System.DateTime DT2 = JSON.UnixEpoch + D2;

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
			System.DateTime DT1 = JSON.UnixEpoch + D1;
			System.DateTime DT2 = JSON.UnixEpoch + D2;

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
			System.DateTime DT1 = JSON.UnixEpoch + D1;
			System.DateTime DT2 = JSON.UnixEpoch + D2;

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
			System.DateTime DT1 = JSON.UnixEpoch + D1;
			System.DateTime DT2 = JSON.UnixEpoch + D2;

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

			System.DateTime DT1 = JSON.UnixEpoch + D1;
			System.DateTime DT2 = JSON.UnixEpoch + D2;

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

			System.DateTime DT1 = JSON.UnixEpoch + D1;
			System.DateTime DT2 = JSON.UnixEpoch + D2;

			return DT1 != DT2;
		}

		/// <summary>
		/// Adds two durations.
		/// </summary>
		/// <param name="D1">Duration 1</param>
		/// <param name="D2">Duration 2</param>
		/// <returns>If <paramref name="D1"/>+<paramref name="D2"/>.</returns>
		public static Duration operator +(Duration D1, Duration D2)
		{
			if (D1.negation ^ D2.negation)
			{
				return new Duration(
					D1.negation,
					D1.years - D2.years,
					D1.months - D2.months,
					D1.days - D2.days,
					D1.hours - D2.hours,
					D1.minutes - D2.minutes,
					D1.seconds - D2.seconds);
			}
			else
			{
				return new Duration(
					D1.negation,
					D1.years + D2.years,
					D1.months + D2.months,
					D1.days + D2.days,
					D1.hours + D2.hours,
					D1.minutes + D2.minutes,
					D1.seconds + D2.seconds);
			}
		}

		/// <summary>
		/// Adds two durations.
		/// </summary>
		/// <param name="D1">Duration 1</param>
		/// <param name="D2">Duration 2</param>
		/// <returns>If <paramref name="D1"/>+<paramref name="D2"/>.</returns>
		public static Duration operator -(Duration D1, Duration D2)
		{
			if (D1.negation ^ D2.negation)
			{
				return new Duration(
					D1.negation,
					D1.years + D2.years,
					D1.months + D2.months,
					D1.days + D2.days,
					D1.hours + D2.hours,
					D1.minutes + D2.minutes,
					D1.seconds + D2.seconds);
			}
			else
			{
				return new Duration(
					D1.negation,
					D1.years - D2.years,
					D1.months - D2.months,
					D1.days - D2.days,
					D1.hours - D2.hours,
					D1.minutes - D2.minutes,
					D1.seconds - D2.seconds);
			}
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is Duration D))
				return false;

			return this == D;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return (reference + this).GetHashCode();
		}

		private static readonly System.DateTime reference = new System.DateTime(2000, 1, 1);

		/// <inheritdoc/>
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
			return new Duration(Minutes < 0, 0, 0, 0, 0, Math.Abs(Minutes), 0);
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

		/// <summary>
		/// Converts a TimeSpan to a Duration. (This will loose milliseconds.)
		/// </summary>
		/// <param name="TS">TimeSpan value.</param>
		/// <returns>Duration value</returns>
		public static Duration FromTimeSpan(System.TimeSpan TS)
		{
			bool Sign = TS < System.TimeSpan.Zero;
			if (Sign)
				TS = -TS;

			return new Duration(Sign, 0, 0, TS.Days, TS.Hours, TS.Minutes, TS.Seconds);
		}

		/// <summary>
		/// Compares the current instance with another object of the same type and returns
		/// an integer that indicates whether the current instance precedes, follows, or
		/// occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="other">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared. The
		/// return value has these meanings: Value Meaning Less than zero This instance precedes
		/// other in the sort order. Zero This instance occurs in the same position in the
		/// sort order as other. Greater than zero This instance follows other in the sort
		/// order.</returns>
		public int CompareTo(Duration other)
		{
			System.DateTime TP1 = JSON.UnixEpoch + this;
			System.DateTime TP2 = JSON.UnixEpoch + other;

			return TP1.CompareTo(TP2);
		}

		/// <summary>
		/// Negates the duration.
		/// </summary>
		/// <returns>Negated duration.</returns>
		public Duration Negate()
		{
			return new Duration(
				!this.negation,
				this.years,
				this.months,
				this.days,
				this.hours,
				this.minutes,
				this.seconds);
		}

		/// <summary>
		/// Calculates the duration between two dates.
		/// </summary>
		/// <param name="From">Starting point.</param>
		/// <param name="To">Ending point.</param>
		/// <returns>Duration between.</returns>
		public static Duration GetDurationBetween(System.DateTime From, System.DateTime To)
		{
			System.DateTime Temp;

			if (From == To)
				return Zero;

			int Years, Months, Days, Hours, Minutes;
			int DMonths = 0;
			int DDays = 0;
			int DHours = 0;
			int DMinutes = 0;
			int DSeconds = 0;
			int DMilliSeconds = 0;
			double Seconds;

			if (From > To)
			{
				Years = To.Year - From.Year;
				if (Years < 0)
				{
					Temp = From.AddYears(Years);
					if (Temp < To)
					{
						From = From.AddYears(++Years);
						DMonths = -12;
					}
					else
						From = Temp;
				}

				if (From.Month != To.Month && (From.Day > 28 || To.Day > 28))
				{
					Months = 0;
					Days = 1 - From.Day;
					Temp = From.AddDays(Days);
					if (Temp < To)
						From = From.AddDays(++Days);
					else
					{
						From = Temp;

						while (From.Month != To.Month)
						{
							int i = -DaysInMonth(From.AddMonths(-1));
							Temp = From.AddDays(i);
							if (Temp < To)
								break;
							else
							{
								Days += i;
								From = Temp;
							}
						}
					}

					if (From.Month != To.Month)
					{
						int Days2 = To.Day - 1 - DaysInMonth(To);
						if (Days2 < 0)
						{
							Temp = From.AddDays(Days2);
							if (Temp < To)
							{
								From = From.AddDays(++Days2);
								DHours = -24;
							}
							else
								From = Temp;
						}

						Days += Days2;
					}
				}
				else
				{
					Months = To.Month - From.Month + DMonths;
					if (Months < 0)
					{
						Temp = From.AddMonths(Months);
						if (Temp < To)
						{
							From = From.AddMonths(++Months);
							DDays = -DaysInMonth(From.AddMonths(-1));
						}
						else
							From = Temp;
					}

					Days = To.Day - From.Day + DDays;
					if (Days < 0)
					{
						Temp = From.AddDays(Days);
						if (Temp < To)
						{
							From = From.AddDays(++Days);
							DHours = -24;
						}
						else
							From = Temp;
					}
				}

				Hours = To.Hour - From.Hour + DHours;
				if (Hours < 0)
				{
					Temp = From.AddHours(Hours);
					if (Temp < To)
					{
						From = From.AddHours(++Hours);
						DMinutes = -60;
					}
					else
						From = Temp;
				}

				Minutes = To.Minute - From.Minute + DMinutes;
				if (Minutes < 0)
				{
					Temp = From.AddMinutes(Minutes);
					if (Temp < To)
					{
						From = From.AddMinutes(++Minutes);
						DSeconds = -60;
					}
					else
						From = Temp;
				}

				Seconds = To.Second - From.Second + DSeconds;
				if (Seconds < 0)
				{
					Temp = From.AddSeconds(Seconds);
					if (Temp < To)
					{
						From = From.AddSeconds(++Seconds);
						DMilliSeconds = -1000;
					}
					else
						From = Temp;
				}

				int Milliseconds = To.Millisecond - From.Millisecond + DMilliSeconds;

				Seconds += Milliseconds * 0.001;

				return new Duration(true, -Years, -Months, -Days, -Hours, -Minutes, -Seconds);
			}
			else
			{
				Years = To.Year - From.Year;
				if (Years > 0)
				{
					Temp = From.AddYears(Years);
					if (Temp > To)
					{
						From = From.AddYears(--Years);
						DMonths = 12;
					}
					else
						From = Temp;
				}

				if (From.Month != To.Month && (From.Day > 28 || To.Day > 28))
				{
					Months = 0;
					Days = DaysInMonth(From) + 1 - From.Day;
					Temp = From.AddDays(Days);
					if (Temp > To)
					{
						From = From.AddDays(--Days);
						DHours = 24;
					}
					else
					{
						From = Temp;

						while (From.Month != To.Month)
						{
							int i = DaysInMonth(From);
							Temp = From.AddDays(i);
							if (Temp > To)
							{
								From = From.AddDays(--i);
								DHours = 24;
								Days += i;
								break;
							}
							else
							{
								Days += i;
								From = Temp;
							}
						}
					}

					if (DHours == 0)
					{
						int Days2 = To.Day - From.Day;
						if (Days2 > 0)
						{
							Temp = From.AddDays(Days2);
							if (Temp > To)
							{
								From = From.AddDays(--Days2);
								DHours = 24;
							}
							else
								From = Temp;
						}

						Days += Days2;
					}
				}
				else
				{
					Months = To.Month - From.Month + DMonths;
					if (Months > 0)
					{
						Temp = From.AddMonths(Months);
						if (Temp > To)
						{
							From = From.AddMonths(--Months);
							DDays = DaysInMonth(From);
						}
						else
							From = Temp;
					}

					Days = To.Day - From.Day + DDays;
					if (Days > 0)
					{
						Temp = From.AddDays(Days);
						if (Temp > To)
						{
							From = From.AddDays(--Days);
							DHours = 24;
						}
						else
							From = Temp;
					}
				}

				Hours = To.Hour - From.Hour + DHours;
				if (Hours > 0)
				{
					Temp = From.AddHours(Hours);
					if (Temp > To)
					{
						From = From.AddHours(--Hours);
						DMinutes = 60;
					}
					else
						From = Temp;
				}

				Minutes = To.Minute - From.Minute + DMinutes;
				if (Minutes > 0)
				{
					Temp = From.AddMinutes(Minutes);
					if (Temp > To)
					{
						From = From.AddMinutes(--Minutes);
						DSeconds = 60;
					}
					else
						From = Temp;
				}

				Seconds = To.Second - From.Second + DSeconds;
				if (Seconds > 0)
				{
					Temp = From.AddSeconds(Seconds);
					if (Temp > To)
					{
						From = From.AddSeconds(--Seconds);
						DMilliSeconds = 1000;
					}
					else
						From = Temp;
				}

				int Milliseconds = To.Millisecond - From.Millisecond + DMilliSeconds;

				Seconds += Milliseconds * 0.001;
		
				return new Duration(false, Years, Months, Days, Hours, Minutes, Seconds);
			}
		}

		private static int DaysInMonth(System.DateTime TP)
		{
			return System.DateTime.DaysInMonth(TP.Year, TP.Month);
		}
	}
}
