using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
	/// <summary>
	/// Creates a DateTimeOffset value.
	/// </summary>
	public class DateTimeOffset : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a DateTimeOffset value.
		/// </summary>
		/// <param name="String">String representation to be parsed.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTimeOffset(ScriptNode String, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { String }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a DateTimeOffset value.
		/// </summary>
		/// <param name="Year">Year</param>
		/// <param name="Month">Month</param>
		/// <param name="Day">Day</param>
		/// <param name="TimeZone">Time-zone</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTimeOffset(ScriptNode Year, ScriptNode Month, ScriptNode Day, ScriptNode TimeZone, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Year, Month, Day, TimeZone }, argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a DateTimeOffset value.
		/// </summary>
		/// <param name="Year">Year</param>
		/// <param name="Month">Month</param>
		/// <param name="Day">Day</param>
		/// <param name="Hour">Hour</param>
		/// <param name="Minute">Minute</param>
		/// <param name="Second">Second</param>
		/// <param name="TimeZone">Time-zone</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTimeOffset(ScriptNode Year, ScriptNode Month, ScriptNode Day, ScriptNode Hour, ScriptNode Minute, ScriptNode Second,
			ScriptNode TimeZone, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Year, Month, Day, Hour, Minute, Second, TimeZone }, argumentTypes7Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a DateTimeOffset value.
		/// </summary>
		/// <param name="Year">Year</param>
		/// <param name="Month">Month</param>
		/// <param name="Day">Day</param>
		/// <param name="Hour">Hour</param>
		/// <param name="Minute">Minute</param>
		/// <param name="Second">Second</param>
		/// <param name="MSecond">Millisecond</param>
		/// <param name="TimeZone">Time-zone</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DateTimeOffset(ScriptNode Year, ScriptNode Month, ScriptNode Day, ScriptNode Hour, ScriptNode Minute, ScriptNode Second,
			ScriptNode MSecond, ScriptNode TimeZone, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Year, Month, Day, Hour, Minute, Second, MSecond, TimeZone }, argumentTypes8Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(DateTimeOffset);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "Year", "Month", "Day", "Hour", "Minute", "Second", "MSecond", "TZ" }; }
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
			object Obj;

			if (c == 1)
			{
				Obj = Arguments[0].AssociatedObjectValue;
				string s = Obj?.ToString() ?? string.Empty;

				if (TryParse(s, out System.DateTimeOffset TP))
					return new ObjectValue(TP);
				else
					throw new ScriptRuntimeException("Unable to parse DateTimeOffset value: " + s, this);
			}

			Obj = Arguments[c - 1].AssociatedObjectValue;

			if (!(Obj is System.TimeSpan TimeZone))
			{
				string s = Obj?.ToString() ?? string.Empty;

				if (!TimeSpan.TryParse(s, out TimeZone))
					throw new ScriptRuntimeException("Unable to parse Time-Zone value: " + s, this);
			}

			c--;

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
					return new ObjectValue(new System.DateTimeOffset((int)d[0], (int)d[1], (int)d[2], 0, 0, 0, TimeZone));

				case 6:
					return new ObjectValue(new System.DateTimeOffset((int)d[0], (int)d[1], (int)d[2], (int)d[3], (int)d[4], (int)d[5], TimeZone));

				case 7:
					return new ObjectValue(new System.DateTimeOffset((int)d[0], (int)d[1], (int)d[2], (int)d[3], (int)d[4], (int)d[5], (int)d[6], TimeZone));

				default:
					throw new ScriptRuntimeException("Invalid number of parameters.", this);
			}
		}

		/// <summary>
		/// Parses DateTimeOffset values from short forms of strings.
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="TP">Parsed DateTimeOffset value.</param>
		/// <returns>If successful or not.</returns>
		public static bool TryParse(string s, out System.DateTimeOffset TP)
		{
			s = s.Trim();

			System.TimeSpan TimeZone;

			if (s.EndsWith("z", StringComparison.CurrentCultureIgnoreCase))
			{
				TimeZone = System.TimeSpan.Zero;
				s = s.Substring(0, s.Length - 1).TrimEnd();
			}
			else
			{
				int i = s.LastIndexOfAny(PlusMinus);
				if (i < 0)
				{
					TP = default;
					return false;
				}

				if (!TimeSpan.TryParse(s.Substring(i + 1), out TimeZone))
				{
					TP = default;
					return false;
				}

				if (s[i] == '-')
					TimeZone = -TimeZone;

				s = s.Substring(0, i).TrimEnd();
			}

			if (!DateTime.TryParse(s, out System.DateTime TP0))
			{
				TP = default;
				return false;
			}

			TP = new System.DateTimeOffset(TP0, TimeZone);
			return true;
		}

		internal static readonly char[] PlusMinus = new char[] { '+', '-' };

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			object Obj = CheckAgainst.AssociatedObjectValue;

			if (!(Obj is System.DateTimeOffset TPO))
			{
				if (Obj is System.DateTime TP)
				{
					switch (TP.Kind)
					{
						case DateTimeKind.Utc:
						default:
							TPO = new System.DateTimeOffset(TP, System.TimeSpan.Zero);
							break;

						case DateTimeKind.Local:
							TPO = new System.DateTimeOffset(TP, TimeZoneInfo.Local.GetUtcOffset(System.DateTime.Now));
							break;
					}
				}
				else if (Obj is double d)
				{
					TP = DateTime.FromInteger((long)d, DateTimeKind.Unspecified);
					TPO = new System.DateTimeOffset(TP, System.TimeSpan.Zero);
				}
				else
				{
					string s = Obj?.ToString() ?? string.Empty;

					if (!TryParse(s, out TPO))
						return PatternMatchResult.NoMatch;
				}
			}

			int c = this.Arguments.Length;
			if (c == 1)
				return this.Arguments[0].PatternMatch(new ObjectValue(TPO), AlreadyFound);

			if (c < 4)
				return PatternMatchResult.NoMatch;

			PatternMatchResult Result = this.Arguments[c - 1].PatternMatch(new ObjectValue(TPO.Offset), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			c--;

			Result = this.Arguments[0].PatternMatch(new DoubleNumber(TPO.Year), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[1].PatternMatch(new DoubleNumber(TPO.Month), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[2].PatternMatch(new DoubleNumber(TPO.Day), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			if (c < 6)
				return PatternMatchResult.NoMatch;

			Result = this.Arguments[3].PatternMatch(new DoubleNumber(TPO.Hour), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[4].PatternMatch(new DoubleNumber(TPO.Minute), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			Result = this.Arguments[5].PatternMatch(new DoubleNumber(TPO.Second), AlreadyFound);
			if (Result != PatternMatchResult.Match)
				return Result;

			if (c == 6)
				return TPO.Millisecond == 0 ? PatternMatchResult.Match : PatternMatchResult.NoMatch;

			if (c != 7)
				return PatternMatchResult.NoMatch;

			return this.Arguments[6].PatternMatch(new DoubleNumber(TPO.Millisecond), AlreadyFound);
		}

	}
}
