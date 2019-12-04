using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Left(x,N)
	/// </summary>
	public class Left : FunctionMultiVariate
	{
		/// <summary>
		/// Left(x,N)
		/// </summary>
		/// <param name="X">Argument.</param>
		/// <param name="N">Number of elements</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Left(ScriptNode X, ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, N }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "left"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "x", "N" }; }
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			double N = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			if (N < 0 || N > int.MaxValue || N != Math.Truncate(N))
				throw new ScriptRuntimeException("Expected nonnegative integer in second argument.", this);

			int c = (int)N;

			if (Arguments[0] is StringValue S)
			{
				string s = S.Value;

				if (c > s.Length)
					return S;
				else
					return new StringValue(s.Substring(0, c));
			}
			else
			{
				ICollection<IElement> ChildElements;

				if (Arguments[0] is IVector Vector)
				{
					if (Vector.Dimension <= c)
						return Vector;

					ChildElements = Vector.VectorElements;
				}
				else if (Arguments[0] is ISet Set)
				{
					int? Size = Set.Size;

					if (Size.HasValue && Size.Value <= c)
						return Set;

					ChildElements = Set.ChildElements;
				}
				else
					throw new ScriptRuntimeException("First argument expected to be a string, vector or a set.", this);

				IElement[] Result = new IElement[c];

				if (ChildElements is IElement[] V)
					Array.Copy(V, 0, Result, 0, c);
				else if (c > 0)
				{
					int i = 0;

					foreach (IElement Element in ChildElements)
					{
						Result[i++] = Element;
						if (i == c)
							break;
					}
				}

				return Arguments[0].Encapsulate(Result, this);
			}
		}

	}
}