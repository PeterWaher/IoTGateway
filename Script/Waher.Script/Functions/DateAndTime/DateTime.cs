using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
	/// <summary>
	/// Creates a DateTime value.
	/// </summary>
	public class DateTime : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a DateTime value.
		/// </summary>
		/// <param name="String">String representation to be parsed.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTime(ScriptNode String, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { String }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a DateTime value.
		/// </summary>
		/// <param name="Year">Year</param>
		/// <param name="Month">Month</param>
		/// <param name="Day">Day</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTime(ScriptNode Year, ScriptNode Month, ScriptNode Day, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Year, Month, Day }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a DateTime value.
		/// </summary>
		/// <param name="Year">Year</param>
		/// <param name="Month">Month</param>
		/// <param name="Day">Day</param>
		/// <param name="Hour">Hour</param>
		/// <param name="Minute">Minute</param>
		/// <param name="Second">Second</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTime(ScriptNode Year, ScriptNode Month, ScriptNode Day, ScriptNode Hour, ScriptNode Minute, ScriptNode Second,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Year, Month, Day, Hour, Minute, Second }, argumentTypes6Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a DateTime value.
		/// </summary>
		/// <param name="Year">Year</param>
		/// <param name="Month">Month</param>
		/// <param name="Day">Day</param>
		/// <param name="Hour">Hour</param>
		/// <param name="Minute">Minute</param>
		/// <param name="Second">Second</param>
		/// <param name="MSecond">Millisecond</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTime(ScriptNode Year, ScriptNode Month, ScriptNode Day, ScriptNode Hour, ScriptNode Minute, ScriptNode Second,
			ScriptNode MSecond, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Year, Month, Day, Hour, Minute, Second, MSecond }, argumentTypes7Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(DateTime);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "Year", "Month", "Day", "Hour", "Minute", "Second", "MSecond" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int i, c = Arguments.Length;

			if (c == 1)
			{
				object Obj = Arguments[0].AssociatedObjectValue;

				if (Obj is System.DateTimeOffset TPO)
					return new DateTimeValue(TPO.DateTime);
				else if (Obj is long L)
					return new DateTimeValue(FromInteger(L, DateTimeKind.Unspecified));
				else if (Obj is double Dbl)
					return new DateTimeValue(FromInteger((long)Dbl, DateTimeKind.Unspecified));
				else if (!(Obj is null) && TryParse(Obj.ToString(), out System.DateTime TP))
					return new DateTimeValue(TP);
				else
					throw new ScriptRuntimeException("Unable to parse DateTime value.", this);
			}

			double[] d = new double[c];
			DoubleNumber n;

			for (i = 0; i < c; i++)
			{
				n = Arguments[i] as DoubleNumber;
				if (n is null)
					throw new ScriptRuntimeException("Expected number arguments.", this);

				d[i] = n.Value;
			}

			switch (c)
			{
				case 3:
					return new DateTimeValue(new System.DateTime((int)d[0], (int)d[1], (int)d[2]));

				case 6:
					return new DateTimeValue(new System.DateTime((int)d[0], (int)d[1], (int)d[2], (int)d[3], (int)d[4], (int)d[5]));

				case 7:
					return new DateTimeValue(new System.DateTime((int)d[0], (int)d[1], (int)d[2], (int)d[3], (int)d[4], (int)d[5], (int)d[6]));

				default:
					throw new ScriptRuntimeException("Invalid number of parameters.", this);
			}
		}

		/// <summary>
		/// Converts an integer to a <see cref="System.DateTime"/>.
		/// If integer is a 32-bit integer, it is considered a UNIX time,
		/// representing the number of seconds after the <see cref="UnixEpoch"/>.
		/// 64-bit integers are considered <see cref="System.DateTime"/> ticks.
		/// </summary>
		/// <param name="Nr">Integer</param>
		/// <param name="Kind">Kind</param>
		/// <returns>DateTime value.</returns>
		public static System.DateTime FromInteger(long Nr, DateTimeKind Kind)
		{
			if (Nr >= int.MinValue && Nr <= int.MaxValue)
			{
				System.DateTime Result = UnixEpoch.AddSeconds((int)Nr);

				if (Kind == DateTimeKind.Local)
					Result = Result.ToLocalTime();

				return Result;
			}
			else
				return new System.DateTime(Nr, Kind);
		}

		/// <summary>
		/// Unix Date and Time epoch, starting at 1970-01-01T00:00:00Z
		/// </summary>
		public static readonly System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Parses DateTime values from short forms of strings.
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="TP">Parsed DateTime value.</param>
		/// <returns>If successful or not.</returns>
		public static bool TryParse(string s, out System.DateTime TP)
		{
			s = s.Trim();

			DateTimeKind Kind;

			if (s.EndsWith("z", StringComparison.CurrentCultureIgnoreCase))
			{
				Kind = DateTimeKind.Utc;
				s = s.Substring(0, s.Length - 1).TrimEnd();
			}
			else
				Kind = DateTimeKind.Unspecified;

			int c = s.Length;
			int Pos = 0;

			if (!TryParseInt(s, ref Pos, '-', out int Year, out _) || Year <= 0 || Year > 9999)
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, 1, 1, 0, 0, 0, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, '-', out int Month, out _) || Month < 1 || Month > 12)
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, Month, 1, 0, 0, 0, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, 'T', out int Day, out _) || Day < 1 || Day > System.DateTime.DaysInMonth(Year, Month))
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, Month, Day, 0, 0, 0, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, ':', out int Hour, out _) || Hour < 0 || Hour > 23)
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, Month, Day, Hour, 0, 0, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, ':', out int Minute, out _) || Minute < 0 || Minute > 59)
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, Month, Day, Hour, Minute, 0, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, '.', out int Second, out _) || Second < 0 || Second > 59)
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, Month, Day, Hour, Minute, Second, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, '\x0', out int MilliSecond, out int NrDigits) || Pos < c)
			{
				TP = System.DateTime.MinValue;
				return false;
			}

			if (NrDigits > 3)
			{
				while (NrDigits > 4)
				{
					MilliSecond /= 10;
					NrDigits--;
				}

				MilliSecond += 5;
				MilliSecond /= 10;
			}

			TP = new System.DateTime(Year, Month, Day, Hour, Minute, Second, MilliSecond, Kind);
			return true;
		}

		internal static bool TryParseInt(string s, ref int Index, char ExpectedDelimiter, out int i, out int Len)
		{
			int j = s.IndexOf(ExpectedDelimiter, Index);

			if (j < 0)
				j = s.Length;
			else if (j <= Index)
			{
				i = Len = 0;
				return false;
			}

			Len = j - Index;

			if (!int.TryParse(s.Substring(Index, Len), out i))
				return false;

			Index = j + 1;

			return true;
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			object Obj = CheckAgainst.AssociatedObjectValue;

			if (!(Obj is System.DateTime TP))
			{
				if (Obj is System.DateTimeOffset TPO)
					TP = TPO.DateTime;
				else if (Obj is double d)
					TP = FromInteger((long)d, DateTimeKind.Unspecified);
				else
				{
					string s = Obj?.ToString() ?? string.Empty;

					if (!System.DateTime.TryParse(s, out TP))
					{
						if (long.TryParse(s, out long Ticks))
							TP = FromInteger(Ticks, DateTimeKind.Unspecified);
						else
							return PatternMatchResult.NoMatch;
					}
				}
			}

			int c = this.Arguments.Length;
			if (c == 1)
				return this.Arguments[0].PatternMatch(new DateTimeValue(TP), AlreadyFound);

			if (c < 3)
				return PatternMatchResult.NoMatch;

			PatternMatchResult Result = this.Arguments[0].PatternMatch(new DoubleNumber(TP.Year), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[1].PatternMatch(new DoubleNumber(TP.Month), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[2].PatternMatch(new DoubleNumber(TP.Day), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			if (c == 3)
				return TP.TimeOfDay == System.TimeSpan.Zero ? PatternMatchResult.Match : PatternMatchResult.NoMatch;

			if (c < 6)
				return PatternMatchResult.NoMatch;

			Result = this.Arguments[3].PatternMatch(new DoubleNumber(TP.Hour), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[4].PatternMatch(new DoubleNumber(TP.Minute), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[5].PatternMatch(new DoubleNumber(TP.Second), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			if (c == 6)
				return TP.Millisecond == 0 ? PatternMatchResult.Match : PatternMatchResult.NoMatch;

			if (c != 7)
				return PatternMatchResult.NoMatch;

			return this.Arguments[6].PatternMatch(new DoubleNumber(TP.Millisecond), AlreadyFound);
		}

	}
}
