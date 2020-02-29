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
	/// Count(v)
	/// </summary>
	public class Count : FunctionMultiVariate
	{
		/// <summary>
		/// Count(v)
		/// </summary>
		/// <param name="Vector">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Count(ScriptNode Vector, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector }, new ArgumentType[] { ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Count(v,item)
		/// </summary>
		/// <param name="Vector">Argument.</param>
		/// <param name="Item">Item</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Count(ScriptNode Vector, ScriptNode Item, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Item }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar }, 
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "count"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "Vector" }; }
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			ICollection<IElement> ChildElements;

			if (Arguments[0] is IVector v)
				ChildElements = v.VectorElements;
			else if (Arguments[0] is ISet S)
				ChildElements = S.ChildElements;
			else
				ChildElements = new IElement[] { Arguments[0] };

			if (Arguments.Length == 1)
				return new DoubleNumber(ChildElements.Count);

			IElement Item0 = Arguments[1];
			int Count = 0;

			foreach (IElement Item in ChildElements)
			{
				if (Item.Equals(Item0))
					Count++;
			}

			return new DoubleNumber(Count);
		}

	}
}