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
		public override string FunctionName
		{
			get { return "datetime"; }
		}

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
				return new DateTimeValue(System.DateTime.Parse(Arguments[0].AssociatedObjectValue?.ToString()));

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
	}
}
