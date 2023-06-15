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
	/// Contains(Vector,Item)
	/// </summary>
	public class Contains : FunctionMultiVariate
	{
		/// <summary>
		/// Contains(Vector,Item)
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Item">Item</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Contains(ScriptNode Vector, ScriptNode Item, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Item }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Contains);

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

			if (Vector.AssociatedObjectValue is string s1 && Item.AssociatedObjectValue is string s2)
				return s1.Contains(s2) ? BooleanValue.True : BooleanValue.False;
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
					return Array.IndexOf(V2, Item) >= 0 ? BooleanValue.True : BooleanValue.False;

				foreach (IElement Element in ChildElements)
				{
					if (Item.Equals(Element))
						return BooleanValue.True;
				}

				return BooleanValue.False;
			}
		}

	}
}