using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
	/// <summary>
	/// Creates a TimeSpan value.
	/// </summary>
	public class TimeSpan : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a TimeSpan value.
		/// </summary>
		/// <param name="String">String representation to be parsed.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public TimeSpan(ScriptNode String, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { String }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a TimeSpan value.
		/// </summary>
		/// <param name="Hours">Hours</param>
		/// <param name="Minutes">Minutes</param>
		/// <param name="Seconds">Seconds</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public TimeSpan(ScriptNode Hours, ScriptNode Minutes, ScriptNode Seconds, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Hours, Minutes, Seconds }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a TimeSpan value.
		/// </summary>
		/// <param name="Days">Days</param>
		/// <param name="Hours">Hours</param>
		/// <param name="Minutes">Minutes</param>
		/// <param name="Seconds">Seconds</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public TimeSpan(ScriptNode Days, ScriptNode Hours, ScriptNode Minutes, ScriptNode Seconds, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Days, Hours, Minutes, Seconds }, argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a TimeSpan value.
		/// </summary>
		/// <param name="Days">Days</param>
		/// <param name="Hours">Hours</param>
		/// <param name="Minutes">Minutes</param>
		/// <param name="Seconds">Seconds</param>
		/// <param name="MSeconds">Milliseconds</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public TimeSpan(ScriptNode Days, ScriptNode Hours, ScriptNode Minutes, ScriptNode Seconds, ScriptNode MSeconds, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Days, Hours, Minutes, Seconds, MSeconds }, argumentTypes5Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(TimeSpan);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "Hour", "Minute", "Second" }; }
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
				if (!(Obj is null) && TryParse(Obj.ToString(), out System.TimeSpan TS))
					return new ObjectValue(TS);
				else
					throw new ScriptRuntimeException("Unable to parse TimeSpan value.", this);
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
					return new ObjectValue(new System.TimeSpan((int)d[0], (int)d[1], (int)d[2]));

				case 4:
					return new ObjectValue(new System.TimeSpan((int)d[0], (int)d[1], (int)d[2], (int)d[3]));

				case 5:
					return new ObjectValue(new System.TimeSpan((int)d[0], (int)d[1], (int)d[2], (int)d[3], (int)d[4]));

				default:
					throw new ScriptRuntimeException("Invalid number of parameters.", this);
			}
		}

		/// <summary>
		/// Parses TimeSpan values from short forms of strings.
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="TS">Parsed TimeSpan value.</param>
		/// <returns>If successful or not.</returns>
		public static bool TryParse(string s, out System.TimeSpan TS)
		{
			s = s.Trim();

			int c = s.Length;
			int Pos = 0;

			if (!DateTime.TryParseInt(s, ref Pos, '.', out int Days) || Days < System.TimeSpan.MinValue.Days || Days > System.TimeSpan.MaxValue.Days)
				Days = 0;
			else if (Pos >= c)
			{
				TS = new System.TimeSpan(Days, 0, 0, 0);
				return true;
			}

			if (!DateTime.TryParseInt(s, ref Pos, ':', out int Hour) || Hour < 0 || Hour > 23)
			{
				TS = System.TimeSpan.Zero;
				return false;
			}
			else if (Pos >= c)
			{
				TS = new System.TimeSpan(Days, Hour, 0, 0);
				return true;
			}

			if (!DateTime.TryParseInt(s, ref Pos, ':', out int Minute) || Minute < 0 || Minute > 59)
			{
				TS = System.TimeSpan.Zero;
				return false;
			}
			else if (Pos >= c)
			{
				TS = new System.TimeSpan(Days, Hour, Minute, 0);
				return true;
			}

			if (!DateTime.TryParseInt(s, ref Pos, '.', out int Second) || Second < 0 || Second > 59)
			{
				TS = System.TimeSpan.Zero;
				return false;
			}
			else if (Pos >= c)
			{
				TS = new System.TimeSpan(Days, Hour, Minute, Second);
				return true;
			}

			if (!DateTime.TryParseInt(s, ref Pos, '\x0', out int MilliSecond) || MilliSecond < 0 || MilliSecond > 999 || Pos < c)
			{
				TS = System.TimeSpan.Zero;
				return false;
			}

			TS = new System.TimeSpan(Days, Hour, Minute, Second, MilliSecond);
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

			if (!(Obj is System.TimeSpan TS))
			{
				if (Obj is double d)
					TS = new System.TimeSpan((long)d);
				else
				{
					string s = CheckAgainst.AssociatedObjectValue?.ToString() ?? string.Empty;

					if (!System.TimeSpan.TryParse(s, out TS))
					{
						if (long.TryParse(s, out long Ticks))
							TS = new System.TimeSpan(Ticks);
						else
							return PatternMatchResult.NoMatch;
					}
				}
			}

			switch (Arguments.Length)
			{
				case 1:
					return this.Arguments[0].PatternMatch(new ObjectValue(TS), AlreadyFound);

				case 3:
					PatternMatchResult Result = this.Arguments[0].PatternMatch(new DoubleNumber(TS.Hours), AlreadyFound);
					if (Result != PatternMatchResult.Match)
						return Result;

					Result = this.Arguments[1].PatternMatch(new DoubleNumber(TS.Minutes), AlreadyFound);
					if (Result != PatternMatchResult.Match)
						return Result;

					return this.Arguments[2].PatternMatch(new DoubleNumber(TS.Seconds), AlreadyFound);

				case 4:
					Result = this.Arguments[0].PatternMatch(new DoubleNumber(TS.Days), AlreadyFound);
					if (Result != PatternMatchResult.Match)
						return Result;

					Result = this.Arguments[1].PatternMatch(new DoubleNumber(TS.Hours), AlreadyFound);
					if (Result != PatternMatchResult.Match)
						return Result;

					Result = this.Arguments[2].PatternMatch(new DoubleNumber(TS.Minutes), AlreadyFound);
					if (Result != PatternMatchResult.Match)
						return Result;

					return this.Arguments[3].PatternMatch(new DoubleNumber(TS.Seconds), AlreadyFound);

				case 5:
					Result = this.Arguments[0].PatternMatch(new DoubleNumber(TS.Days), AlreadyFound);
					if (Result != PatternMatchResult.Match)
						return Result;

					Result = this.Arguments[1].PatternMatch(new DoubleNumber(TS.Hours), AlreadyFound);
					if (Result != PatternMatchResult.Match)
						return Result;

					Result = this.Arguments[2].PatternMatch(new DoubleNumber(TS.Minutes), AlreadyFound);
					if (Result != PatternMatchResult.Match)
						return Result;

					Result = this.Arguments[3].PatternMatch(new DoubleNumber(TS.Seconds), AlreadyFound);
					if (Result != PatternMatchResult.Match)
						return Result;

					return this.Arguments[4].PatternMatch(new DoubleNumber(TS.Milliseconds), AlreadyFound);

				default:
					return PatternMatchResult.NoMatch;
			}
		}

	}
}
