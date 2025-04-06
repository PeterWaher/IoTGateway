using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Arithmetics;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Variance(v) iterative evaluator
	/// </summary>
	public class VarianceEvaluator : IIterativeEvaluator
	{
		private ChunkedList<double> doubleValues = new ChunkedList<double>();
		private ChunkedList<IElement> elementValues = null;
		private readonly Variance node;
		private IElement sum = null;
		private double doubleSum = 0;
		private long count = 0;
		private bool isDouble = true;

		/// <summary>
		/// Variance(v) iterative evaluator
		/// </summary>
		public VarianceEvaluator(Variance Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Restarts the evaluator.
		/// </summary>
		public void RestartEvaluator()
		{
			this.sum = null;
			this.count = 0;
			this.doubleSum = 0;
			this.isDouble = true;

			if (this.doubleValues is null)
				this.doubleValues = new ChunkedList<double>();
			else
				this.doubleValues.Clear();

			this.elementValues = null;
		}

		/// <summary>
		/// Aggregates one new element.
		/// </summary>
		/// <param name="Element">Element.</param>
		public void AggregateElement(IElement Element)
		{
			if (this.isDouble && Element is DoubleNumber D)
			{
				this.doubleSum += D.Value;
				this.doubleValues.Add(D.Value);
			}
			else if (this.sum is null)
			{
				this.isDouble = false;
				this.elementValues = new ChunkedList<IElement>();

				if (this.doubleValues.Count > 0)
				{
					ChunkNode<double> Loop = this.doubleValues.FirstChunk;
					int i, c;

					while (!(Loop is null))
					{
						for (i = Loop.Start, c = Loop.Pos; i < c; i++)
							this.elementValues.Add(new DoubleNumber(Loop.Elements[i]));

						Loop = Loop.Next;
					}
				}

				if (this.sum is null)
					this.sum = Element;
				else
					this.sum = Add.EvaluateAddition(new DoubleNumber(this.doubleSum), Element, this.node);

				this.elementValues.Add(Element);
			}
			else
			{
				this.sum = Add.EvaluateAddition(this.sum, Element, this.node);
				this.elementValues.Add(Element);
			}

			this.count++;
		}

		/// <summary>
		/// Gets the aggregated result.
		/// </summary>
		public IElement GetAggregatedResult()
		{
			if (this.count == 0)
				return ObjectValue.Null;
			else if (this.isDouble)
			{
				double Average = this.doubleSum / this.count;
				double Result = 0;
				double Term;

				ChunkNode<double> Loop = this.doubleValues.FirstChunk;
				int i, c;

				while (!(Loop is null))
				{
					for (i = Loop.Start, c = Loop.Pos; i < c; i++)
					{
						Term = Loop[i] - Average;
						Result += Term * Term;
					}

					Loop = Loop.Next;
				}

				return new DoubleNumber(Result / this.count);
			}
			else
			{
				IElement Average = Divide.EvaluateDivision(this.sum, new DoubleNumber(this.count), this.node);
				IElement Result = null;
				IElement Term;

				ChunkNode<IElement> Loop = this.elementValues.FirstChunk;
				int i, c;

				while (!(Loop is null))
				{
					for (i = Loop.Start, c = Loop.Pos; i < c; i++)
					{
						Term = Subtract.EvaluateSubtraction(Loop[i], Average, this.node);
						Term = Multiply.EvaluateMultiplication(Term, Term, this.node);

						if (Result is null)
							Result = Term;
						else
							Result = Add.EvaluateAddition(Result, Term, this.node);
					}

					Loop = Loop.Next;
				}

				if (Result is null)
					return ObjectValue.Null;
				else
					return Divide.EvaluateDivision(Result, new DoubleNumber(this.count), this.node);
			}
		}
	}
}
