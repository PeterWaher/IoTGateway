using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
    /// <summary>
    /// Represents a constant element value.
    /// </summary>
    public class ConstantElement : ScriptLeafNode, IDifferentiable
    {
        private readonly IElement constant;

        /// <summary>
        /// Represents a constant element value.
        /// </summary>
		/// <param name="Constant">Constant.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public ConstantElement(IElement Constant, int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
            this.constant = Constant;
        }

        /// <summary>
        /// Constant value.
        /// </summary>
        public IElement Constant
        {
            get { return this.constant; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            return this.constant;
        }

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			return this.constant.Equals(CheckAgainst) ? PatternMatchResult.Match : PatternMatchResult.NoMatch;
        }

		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			return new ConstantElement(DoubleNumber.ZeroElement, this.Start, this.Length, this.Expression);
		}

		/// <summary>
		/// Default variable name, if any, null otherwise.
		/// </summary>
		public string DefaultVariableName => null;

		/// <summary>
		/// <see cref="object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is ConstantElement O &&
				this.constant.Equals(O.constant) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.constant.GetHashCode();
			return Result;
		}
	}
}
