using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Count(v)
	/// </summary>
	public class Count : FunctionMultiVariate, IIterativeEvaluator
	{
		/// <summary>
		/// Count(v)
		/// </summary>
		/// <param name="Vector">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Count(ScriptNode Vector, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector }, argumentTypes1Normal, Start, Length, Expression)
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
		public override string FunctionName => nameof(Count);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Vector" };

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			ICollection<IElement> ChildElements;
			IElement Item0;
			int Count;

			if (Arguments[0] is IVector v)
				ChildElements = v.VectorElements;
			else if (Arguments[0] is ISet S)
				ChildElements = S.ChildElements;
			else if (Arguments[0].AssociatedObjectValue is System.Collections.ICollection Collection)
			{
				if (Arguments.Length == 1)
					return new DoubleNumber(Collection.Count);
				else
				{
					Item0 = Arguments[1];
					Count = 0;

					foreach (object Item in Collection)
					{
						if (Item0.Equals(Expression.Encapsulate(Item)))
							Count++;
					}

					return new DoubleNumber(Count);
				}
			}
			else
				ChildElements = new IElement[] { Arguments[0] };

			if (Arguments.Length == 1)
				return new DoubleNumber(ChildElements.Count);

			Item0 = Arguments[1];
			Count = 0;

			foreach (IElement Item in ChildElements)
			{
				if (Item.Equals(Item0))
					Count++;
			}

			return new DoubleNumber(Count);
		}

		#region IIterativeEvaluator

		private long count = 0;

		/// <summary>
		/// If the evaluator can perform the computation iteratively.
		/// </summary>
		public bool CanEvaluateIteratively => this.Arguments.Length == 1;

		/// <summary>
		/// Creates a new instance of the iterative evaluator.
		/// </summary>
		/// <returns>Reference to new instance.</returns>
		public IIterativeEvaluator CreateNewEvaluator()
		{
			return new Count(this.Arguments[0], this.Start, this.Length, this.Expression);
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.count = 0;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			this.count++;
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			return new DoubleNumber(this.count);
		}

		#endregion

	}
}