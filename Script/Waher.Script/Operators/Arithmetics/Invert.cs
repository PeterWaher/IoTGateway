using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Inversion operator.
	/// </summary>
	public class Invert : UnaryOperator, IDifferentiable
	{
		/// <summary>
		/// Inversion operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Invert(ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Operand = this.op.Evaluate(Variables);
			return this.Evaluate(Operand);
		}

		private IElement Evaluate(IElement Element)
		{
			if (Element is IRingElement E)
			{
				E = E.Invert();
				if (E is null)
					throw new ScriptRuntimeException("Operand not invertible.", this);
				else
					return E;
			}
			else if (Element.IsScalar)
				throw new ScriptRuntimeException("Operand not invertible.", this);
			else
			{
				LinkedList<IElement> Elements = new LinkedList<IElement>();

				foreach (IElement E2 in Element.ChildElements)
					Elements.AddLast(this.Evaluate(E2));

				return Element.Encapsulate(Elements, this);
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

			return this.DifferentiationChainRule(VariableName, Variables, this.op,
				new Negate(
					new Invert(
						new Square(this.op, Start, Len, Expression),
						Start, Len, Expression),
					Start, Len, Expression));
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (!(CheckAgainst is EuclidianDomainElement E))
				return PatternMatchResult.NoMatch;

			IElement Inv = E.AssociatedEuclidianDomain.Divide(E.One, E);
			if (Inv is null)
				return PatternMatchResult.NoMatch;

			return this.op.PatternMatch(Inv, AlreadyFound);
		}

	}
}
