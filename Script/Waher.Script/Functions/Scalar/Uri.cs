using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
    /// <summary>
    /// Uri(x)
    /// </summary>
    public class Uri : FunctionOneScalarVariable
    {
        /// <summary>
        /// Uri(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Uri(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Uri);

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
            if (System.Uri.TryCreate(Argument, UriKind.RelativeOrAbsolute, out System.Uri Uri))
                return new ObjectValue(Uri);
            else
                throw new ScriptRuntimeException("Invalid URI.", this);
        }

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			object Obj = Argument.AssociatedObjectValue;

			if (Obj is System.Uri)
				return Argument;
			else if (Obj is string s)
				return this.EvaluateScalar(s, Variables);
			else
				throw new ScriptRuntimeException("Invalid URI.", this);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
		{
			return Task.FromResult(this.EvaluateScalar(Argument, Variables));
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
        {
            object Obj = CheckAgainst.AssociatedObjectValue;

            if (!(Obj is System.Uri))
            {
                if (Obj is string s)
                {
                    if (System.Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out System.Uri Uri))
                        CheckAgainst = new ObjectValue(Uri);
                    else
                        return PatternMatchResult.NoMatch;
                }
                else
                    return PatternMatchResult.NoMatch;
			}
            
            return this.Argument.PatternMatch(CheckAgainst, AlreadyFound);
        }
    }
}
