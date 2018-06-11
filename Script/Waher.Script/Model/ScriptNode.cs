using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Objects;
using Waher.Script.Operators.Arithmetics;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for all nodes in a parsed script tree.
	/// </summary>
	public abstract class ScriptNode
	{
		private readonly Expression expression;
		private int start;
		private readonly int length;

		/// <summary>
		/// Base class for all nodes in a parsed script tree.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ScriptNode(int Start, int Length, Expression Expression)
		{
			this.start = Start;
			this.length = Length;
			this.expression = Expression;
		}

		/// <summary>
		/// Start position in script expression.
		/// </summary>
		public int Start
		{
			get { return this.start; }
			internal set { this.start = value; }
		}

		/// <summary>
		/// Length of expression covered by node.
		/// </summary>
		public int Length
		{
			get { return this.length; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public abstract IElement Evaluate(Variables Variables);

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		public virtual void PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			throw new ScriptRuntimeException("Pattern mismatch.", this);
		}

		/// <summary>
		/// Expression of which the node is a part.
		/// </summary>
		public Expression Expression
		{
			get { return this.expression; }
		}

		/// <summary>
		/// Sub-expression defining the node.
		/// </summary>
		public string SubExpression
		{
			get { return this.expression.Script.Substring(this.start, this.length); }
		}

		/// <summary>
		/// Implements the differentiation chain rule, by differentiating the argument and multiplying it to the differentiation of the main node.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <param name="Argument">Inner argument</param>
		/// <param name="Differentiation">Differentiation of main node.</param>
		/// <returns><paramref name="Differentiation"/>*D(<paramref name="Argument"/>)</returns>
		protected ScriptNode DifferentiationChainRule(string VariableName, Variables Variables, ScriptNode Argument, ScriptNode Differentiation)
		{
			if (Argument is IDifferentiable Differentiable)
			{
				ScriptNode ChainFactor = Differentiable.Differentiate(VariableName, Variables);

				if (ChainFactor is ConstantElement ConstantElement &&
					ConstantElement.Constant is DoubleNumber DoubleNumber)
				{
					if (DoubleNumber.Value == 0)
						return ConstantElement;
					else if (DoubleNumber.Value == 1)
						return Differentiation;
				}

				int Start = this.Start;
				int Len = this.Length;
				Expression Exp = this.Expression;

				if (Differentiation is Invert Invert)
				{
					if (Invert.Operand is Negate Negate)
						return new Negate(new Divide(ChainFactor, Negate.Operand, Start, Len, Expression), Start, Len, Expression);
					else
						return new Divide(ChainFactor, Invert.Operand, Start, Len, Expression);
				}
				else if (Differentiation is Negate Negate)
				{
					if (Negate.Operand is Invert Invert2)
						return new Negate(new Divide(ChainFactor, Invert2.Operand, Start, Len, Expression), Start, Len, Expression);
					else
						return new Negate(new Multiply(Negate.Operand, ChainFactor, Start, Len, Expression), Start, Len, Expression);
				}
				else
					return new Multiply(Differentiation, ChainFactor, Start, Len, Expression);
			}
			else
				throw new ScriptRuntimeException("Argument not differentiable.", this);

		}
	}
}
