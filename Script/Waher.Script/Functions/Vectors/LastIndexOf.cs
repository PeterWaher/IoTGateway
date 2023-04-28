using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// LastIndexOf(Vector,Item[,From])
	/// </summary>
	public class LastIndexOf : FunctionMultiVariate
	{
		/// <summary>
		/// LastIndexOf(Vector,Item)
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Item">Item</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LastIndexOf(ScriptNode Vector, ScriptNode Item, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Item }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// LastIndexOf(Vector,Item,From)
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Item">Item</param>
		/// <param name="From">From which element to start search.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LastIndexOf(ScriptNode Vector, ScriptNode Item, ScriptNode From, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Item, From }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(LastIndexOf);

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
			int FromIndex;

			if (From is null)
				FromIndex = int.MaxValue;
			else
				FromIndex = (int)Expression.ToDouble(From.AssociatedObjectValue);

			if (Vector is StringValue S1 && Item is StringValue S2)
			{
				if (From is null)
					return new DoubleNumber(S1.Value.LastIndexOf(S2.Value));

				return new DoubleNumber(S1.Value.LastIndexOf(S2.Value, FromIndex));
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
					return new DoubleNumber(Array.LastIndexOf(V2, Item));

				int i = 0;
				int Result = -1;

				foreach (IElement Element in ChildElements)
				{
					if (Item.Equals(Element))
						Result = i;

					if (++i > FromIndex)
						break;
				}

				return new DoubleNumber(Result);
			}
		}

	}
}