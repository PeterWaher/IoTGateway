using System;
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
			: base(new ScriptNode[] { Vector }, new ArgumentType[] { ArgumentType.Vector }, Start, Length, Expression)
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
			: base(new ScriptNode[] { Vector, Item }, new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar }, 
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
			if (!(Arguments[0] is IVector v))
				throw new ScriptRuntimeException("First argument expected to be a vector.", this);

			if (Arguments.Length == 1)
				return new DoubleNumber(v.Dimension);

			IElement Item0 = Arguments[1];
			int Count = 0;

			foreach (IElement Item in v.VectorElements)
			{
				if (Item.Equals(Item0))
					Count++;
			}

			return new DoubleNumber(Count);
		}

	}
}