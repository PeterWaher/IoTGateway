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
	/// Mid(x,Pos,Len)
	/// </summary>
	public class Mid : FunctionMultiVariate
	{
		/// <summary>
		/// Mid(x,Pos,Len)
		/// </summary>
		/// <param name="X">Argument.</param>
		/// <param name="Pos">Starting position</param>
		/// <param name="Len">Number of elements</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Mid(ScriptNode X, ScriptNode Pos, ScriptNode Len, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { X, Pos, Len }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "mid"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "x", "Pos", "Len" }; }
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			double Pos = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			if (Pos < 0 || Pos > int.MaxValue || Pos != Math.Truncate(Pos))
				throw new ScriptRuntimeException("Expected nonnegative integer in second argument.", this);

			int i = (int)Pos;

			double Len = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
			if (Len < 0 || Len > int.MaxValue || Len != Math.Truncate(Len))
				throw new ScriptRuntimeException("Expected nonnegative integer in third argument.", this);

			int c = (int)Len;

			if (Arguments[0] is StringValue S)
			{
				string s = S.Value;
				int l = s.Length;

				if (i > l)
					return StringValue.Empty;
				else if (i + c >= l)
					return new StringValue(s.Substring(i));
				else
					return new StringValue(s.Substring(i, c));
			}
			else
			{
				ICollection<IElement> ChildElements;

				if (Arguments[0] is IVector Vector)
				{
					int l = Vector.Dimension;

					if (i > l)
						i = l;

					if (i + c > l)
						c = l - i;

					if (i == 0 && c == l)
						return Vector;

					ChildElements = Vector.VectorElements;
				}
				else if (Arguments[0] is ISet Set)
				{
					int? Size = Set.Size;

					if (Size.HasValue)
					{
						int l = Size.Value;

						if (i > l)
							i = l;

						if (i + c > l)
							c = l - i;

						if (i == 0 && c == l)
							return Set;
					}

					ChildElements = Set.ChildElements;
				}
				else
					throw new ScriptRuntimeException("First argument expected to be a string, vector or a set.", this);

				IElement[] Result = new IElement[c];

				if (ChildElements is IElement[] V)
					Array.Copy(V, i, Result, 0, c);
				else if (c > 0)
				{
					int j = 0;

					foreach (IElement Element in ChildElements)
					{
						if (i-- <= 0)
						{
							Result[j++] = Element;
							if (j == c)
								break;
						}
					}
				}

				return Arguments[0].Encapsulate(Result, this);
			}
		}

	}
}