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

				if (Obj is long L)
					return new DateTimeValue(new System.DateTime(L));
				else if (Obj is double Dbl)
					return new DateTimeValue(new System.DateTime((long)Dbl));
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

			if (!TryParseInt(s, ref Pos, '-', out int Year) || Year <= 0 || Year > 9999)
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, 1, 1, 0, 0, 0, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, '-', out int Month) || Month < 1 || Month > 12)
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, Month, 1, 0, 0, 0, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, 'T', out int Day) || Day < 1 || Day > System.DateTime.DaysInMonth(Year, Month))
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, Month, Day, 0, 0, 0, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, ':', out int Hour) || Hour < 0 || Hour > 23)
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, Month, Day, Hour, 0, 0, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, ':', out int Minute) || Minute < 0 || Minute > 59)
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, Month, Day, Hour, Minute, 0, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, '.', out int Second) || Second < 0 || Second > 59)
			{
				TP = System.DateTime.MinValue;
				return false;
			}
			else if (Pos >= c)
			{
				TP = new System.DateTime(Year, Month, Day, Hour, Minute, Second, Kind);
				return true;
			}

			if (!TryParseInt(s, ref Pos, '\x0', out int MilliSecond) || MilliSecond < 0 || MilliSecond > 999 || Pos < c)
			{
				TP = System.DateTime.MinValue;
				return false;
			}

			TP = new System.DateTime(Year, Month, Day, Hour, Minute, Second, MilliSecond, Kind);
			return true;
		}

		internal static bool TryParseInt(string s, ref int Index, char ExpectedDelimiter, out int i)
		{
			int j = s.IndexOf(ExpectedDelimiter, Index);

			if (j < 0)
				j = s.Length;
			else if (j == 0)
			{
				i = 0;
				return false;
			}

			if (!int.TryParse(s.Substring(Index, j - Index), out i))
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
			System.DateTime TP;

			if (CheckAgainst is DateTimeValue DTV)
				TP = DTV.Value;
			else
			{
				if (CheckAgainst is DoubleNumber D)
					TP = new System.DateTime((long)D.Value);
				else
				{
					string s = CheckAgainst.AssociatedObjectValue?.ToString() ?? string.Empty;

					if (!System.DateTime.TryParse(s, out TP))
					{
						if (long.TryParse(s, out long Ticks))
							TP = new System.DateTime(Ticks);
						else
							return PatternMatchResult.NoMatch;
					}
				}
			}

			int c = Arguments.Length;
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
