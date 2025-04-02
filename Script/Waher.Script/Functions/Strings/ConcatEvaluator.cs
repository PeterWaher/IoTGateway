using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Strings
{
	/// <summary>
	/// Concat(v[,Delimiter]) iterative evaluator
	/// </summary>
	public class ConcatEvaluator : IIterativeEvaluator
	{
		private readonly string delimiter;
		private readonly bool hasDelimiter;
		private StringBuilder sb = null;

		/// <summary>
		/// Concat(v[,Delimiter]) iterative evaluator
		/// </summary>
		/// <param name="Delimiter">Delimiter to use.</param>
		public ConcatEvaluator(string Delimiter)
		{
			this.delimiter = Delimiter;
			this.hasDelimiter = !string.IsNullOrEmpty(this.delimiter);
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.sb = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
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
		public IElement GetAggregatedResult()
		{
			return new StringValue(this.sb?.ToString() ?? string.Empty);
		}
	}
}
