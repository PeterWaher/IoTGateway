using System;
using System.Collections.Generic;
using System.Text;
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
		public override string FunctionName
		{
			get { return "timespan"; }
		}

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
				return new ObjectValue(System.TimeSpan.Parse(Arguments[0].AssociatedObjectValue?.ToString()));

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
	}
}
