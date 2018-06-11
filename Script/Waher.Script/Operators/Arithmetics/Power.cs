using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Analytic;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Power operator.
	/// </summary>
	public class Power : BinaryOperator, IDifferentiable
	{
		/// <summary>
		/// Power operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Power(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Left = this.left.Evaluate(Variables);
			IElement Right = this.right.Evaluate(Variables);

			return EvaluatePower(Left, Right, this);
		}

		/// <summary>
		/// Calculates Left ^ Right.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluatePower(IElement Left, IElement Right, ScriptNode Node)
		{
			DoubleNumber DR = Right as DoubleNumber;

			if (Left is DoubleNumber DL && DR != null)
				return new DoubleNumber(Math.Pow(DL.Value, DR.Value));

			if (Left is IRingElement LE && DR != null)
			{
				double d = DR.Value;
				if (d >= long.MinValue && d <= long.MaxValue && Math.Truncate(d) == d)
				{
					long n = (long)d;

					if (n < 0)
					{
						LE = LE.Invert();
						if (LE == null)
							throw new ScriptRuntimeException("Base element not invertible.", Node);

						n = -n;
					}
					else if (n == 0)
					{
						if (!(LE is ICommutativeRingWithIdentityElement LE2))
							throw new ScriptRuntimeException("Base element ring does not have unity.", Node);

						return LE2.One;
					}

					IRingElement Result = null;

					while (n > 0)
					{
						if ((n & 1) == 1)
						{
							if (Result == null)
								Result = LE;
							else
								Result = (IRingElement)Multiply.EvaluateMultiplication(Result, LE, Node);
						}

						n >>= 1;
						if (n > 0)
							LE = (IRingElement)Multiply.EvaluateMultiplication(LE, LE, Node);
					}

					return Result;
				}
				else
					throw new ScriptRuntimeException("Exponent too large.", Node);
			}

			if (Left.IsScalar)
			{
				if (Right.IsScalar)
					throw new ScriptRuntimeException("Power operation could not be computed.", Node);
				else
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Elements.AddLast(EvaluatePower(Left, RightChild, Node));

					return Right.Encapsulate(Elements, Node);
				}
			}
			else
			{
				if (Right.IsScalar)
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Elements.AddLast(EvaluatePower(LeftChild, Right, Node));

					return Left.Encapsulate(Elements, Node);
				}
				else
				{
					ICollection<IElement> LeftChildren = Left.ChildElements;
					ICollection<IElement> RightChildren = Right.ChildElements;

					if (LeftChildren.Count == RightChildren.Count)
					{
						LinkedList<IElement> Elements = new LinkedList<IElement>();
						IEnumerator<IElement> eLeft = LeftChildren.GetEnumerator();
						IEnumerator<IElement> eRight = RightChildren.GetEnumerator();

						try
						{
							while (eLeft.MoveNext() && eRight.MoveNext())
								Elements.AddLast(EvaluatePower(eLeft.Current, eRight.Current, Node));
						}
						finally
						{
							eLeft.Dispose();
							eRight.Dispose();
						}

						return Left.Encapsulate(Elements, Node);
					}
					else
					{
						LinkedList<IElement> LeftResult = new LinkedList<IElement>();

						foreach (IElement LeftChild in LeftChildren)
						{
							LinkedList<IElement> RightResult = new LinkedList<IElement>();

							foreach (IElement RightChild in RightChildren)
								RightResult.AddLast(EvaluatePower(LeftChild, RightChild, Node));

							LeftResult.AddLast(Right.Encapsulate(RightResult, Node));
						}

						return Left.Encapsulate(LeftResult, Node);
					}
				}
			}
		}

		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			int Start = this.Start;
			int Len = this.Length;
			Expression Expression = this.Expression;

			if (this.left is ConstantElement ConstantBase)
			{
				return this.DifferentiationChainRule(VariableName, Variables, this.right,
					new Multiply(
						this,
						new Ln(ConstantBase, Start, Len, Expression),
						Start, Len, Expression));
			}
			else if (this.left is IDifferentiable Left)
			{
				if (this.right is ConstantElement ConstantExp)
				{
					if (ConstantExp.Constant.Equals(DoubleNumber.OneElement))
						return Left.Differentiate(VariableName, Variables);
					else if (ConstantExp.Constant.Equals(DoubleNumber.TwoElement))
					{
						return this.DifferentiationChainRule(VariableName, Variables, this.left,
							new Multiply(
								ConstantExp,
								this.left,
								Start, Len, Expression));
					}
					else
					{
						return this.DifferentiationChainRule(VariableName, Variables, this.left,
							new Multiply(
								ConstantExp,
								new Power(
									this.left,
									new ConstantElement(Subtract.EvaluateSubtraction(ConstantExp.Constant, DoubleNumber.OneElement, this), Start, Len, Expression),
									Start, Len, Expression),
								Start, Len, Expression));
					}
				}
				else if (this.right is IDifferentiable Right)
				{
					return new Multiply(
						this,
						new Add(
							new Multiply(
								Left.Differentiate(VariableName, Variables),
								new Divide(this.right, this.left, Start, Len, Expression),
								Start, Len, Expression),
							new Multiply(
								Right.Differentiate(VariableName, Variables),
								new Ln(this.left, Start, Len, Expression),
								Start, Len, Expression),
							Start, Len, Expression),
						Start, Len, Expression);
				}
				else
					throw new ScriptRuntimeException("Operands not differentiable.", this);
			}
			else
				throw new ScriptRuntimeException("Operands not differentiable.", this);
		}

	}
}
