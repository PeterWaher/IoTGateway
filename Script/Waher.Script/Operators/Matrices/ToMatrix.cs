using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;

namespace Waher.Script.Operators.Matrices
{
    /// <summary>
    /// To-Matrix operator.
    /// </summary>
    public class ToMatrix : NullCheckUnaryOperator
    {
		/// <summary>
		/// To-Matrix operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ToMatrix(ScriptNode Operand, bool NullCheck, int Start, int Length, Expression Expression)
            : base(Operand, NullCheck, Start, Length, Expression)
        {
        }

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.ConvertToMatrix(this.op.Evaluate(Variables));
		}

		private IElement ConvertToMatrix(IElement E)
		{ 
			if (this.nullCheck && E.AssociatedObjectValue is null)
				return E;

			if (E is IMatrix)
                return E;

            if (E is IVector V)
                return MatrixDefinition.Encapsulate(V.VectorElements, 1, V.Dimension, this);

            if (E is ISet S)
			{
				ICollection<IElement> Elements = S.ChildElements;
                return MatrixDefinition.Encapsulate(Elements, 1, Elements.Count, this);
            }

            return MatrixDefinition.Encapsulate(new IElement[] { E }, 1, 1, this);
        }

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			return this.op.PatternMatch(this.ConvertToMatrix(CheckAgainst), AlreadyFound);
		}
	}
}
