using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Strings
{
	/// <summary>
	/// Concat(v[,Delimiter]) iterative evaluator
	/// </summary>
	public class ConcatEvaluator : UnaryIterativeEvaluator
	{
		private readonly string delimiter;
		private readonly bool hasDelimiter;
		private StringBuilder sb = null;

		/// <summary>
		/// Concat(v[,Delimiter]) iterative evaluator
		/// </summary>
		/// <param name="Node">Node being iteratively evaluated.</param>
		public ConcatEvaluator(Concat Node)
			: base(Node.Arguments[0])
		{
			if (Node.Arguments.Length == 2 && Node.Arguments[1] is ConstantElement C)
			{
				this.delimiter = C.Constant.AssociatedObjectValue?.ToString();
				this.hasDelimiter = !string.IsNullOrEmpty(this.delimiter);
			}
			else
			{
				this.delimiter = null;
				this.hasDelimiter = false;
			}
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public override void RestartEvaluator()
		{
			this.sb = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public override void AggregateElement(IElement Element)
		{
			if (this.sb is null)
				this.sb = new StringBuilder();
			else if (this.hasDelimiter)
				this.sb.Append(this.delimiter);

			this.sb.Append(Element.AssociatedObjectValue?.ToString());
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public override IElement GetAggregatedResult()
		{
			return new StringValue(this.sb?.ToString() ?? string.Empty);
		}
	}
}
