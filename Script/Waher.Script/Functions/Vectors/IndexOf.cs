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
	/// IndexOf(Vector,Item[,From])
	/// IndexOf(Matrix,Item[,FromColumn,FromRow])
	/// </summary>
	public class IndexOf : FunctionMultiVariate
	{
		/// <summary>
		/// IndexOf(Vector,Item)
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Item">Item</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public IndexOf(ScriptNode Vector, ScriptNode Item, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Item }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// IndexOf(Vector,Item,From)
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Item">Item</param>
		/// <param name="From">From which element to start search.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public IndexOf(ScriptNode Vector, ScriptNode Item, ScriptNode From, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Item, From }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// IndexOf(Matrix,Item,FromColumn,FromRow)
		/// </summary>
		/// <param name="Matrix">Matrix.</param>
		/// <param name="Item">Item</param>
		/// <param name="FromColumn">From which column to start search.</param>
		/// <param name="FromRow">From which row to start search.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public IndexOf(ScriptNode Matrix, ScriptNode Item, ScriptNode FromColumn, ScriptNode FromRow,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Matrix, Item, FromColumn, FromRow }, new ArgumentType[] { ArgumentType.Matrix, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(IndexOf);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "Vector", "Item" }; }
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			IElement Vector = Arguments[0];
			IElement Item = Arguments[1];
			IElement From = Arguments.Length > 2 ? Arguments[2] : null;
			IElement From2 = Arguments.Length > 3 ? Arguments[3] : null;
			int FromIndex, FromIndex2;

			if (From is null)
				FromIndex = 0;
			else
				FromIndex = (int)Expression.ToDouble(From.AssociatedObjectValue);

			if (Vector.AssociatedObjectValue is string s1 && Item.AssociatedObjectValue is string s2)
			{
				if (From is null)
					return new DoubleNumber(s1.IndexOf(s2));

				return new DoubleNumber(s1.IndexOf(s2, FromIndex));
			}
			else if (Vector is IMatrix M)
			{
				if (From is null || From2 is null)
				{
					if (M.TryFind(Item, out int Column, out int Row))
						return new DoubleVector(Column, Row);
					else
						return new DoubleVector(-1, -1);
				}
				else
				{
					FromIndex2 = (int)Expression.ToDouble(From2.AssociatedObjectValue);

					if (M.TryFind(Item, FromIndex, FromIndex2, out int Column, out int Row))
						return new DoubleVector(Column, Row);
					else
						return new DoubleVector(-1, -1);
				}
			}
			else
			{
				ICollection<IElement> ChildElements;

				if (Vector is IVector V)
					ChildElements = V.VectorElements;
				else if (Vector is ISet S)
					ChildElements = S.ChildElements;
				else
					throw new ScriptRuntimeException("Expected string arguments, or the first argument to be a vector or a set.", this);

				if (ChildElements is IElement[] V2)
					return new DoubleNumber(Array.IndexOf(V2, Item));

				int i = 0;

				foreach (IElement Element in ChildElements)
				{
					if (i >= FromIndex)
					{
						if (Item.Equals(Element))
							return new DoubleNumber(i);
					}

					i++;
				}

				return new DoubleNumber(-1);
			}
		}

	}
}