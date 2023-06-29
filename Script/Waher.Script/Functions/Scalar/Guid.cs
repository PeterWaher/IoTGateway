using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
    /// <summary>
    /// Guid(x)
    /// </summary>
    public class Guid : FunctionOneScalarVariable
    {
        /// <summary>
        /// Guid(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Guid(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Guid);

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
            if (System.Guid.TryParse(Argument, out System.Guid Guid))
                return new ObjectValue(Guid);
            else
                throw new ScriptRuntimeException("Invalid GUID.", this);
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

            if (Obj is Guid)
                return Argument;
            else if (Obj is string s)
                return this.EvaluateScalar(s, Variables);
            else if (Obj is byte[] Bin)
            {
                if (Bin.Length == 16)
                    return new ObjectValue(new System.Guid(Bin));
                else
                    throw new ScriptRuntimeException("Invalid number of bytes.", this);
			}
			else
                throw new ScriptRuntimeException("Invalid GUID.", this);
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
        {
            return Task.FromResult<IElement>(this.EvaluateScalar(Argument, Variables));
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

            if (!(Obj is Guid))
            {
                if (Obj is string s)
                {
                    if (System.Guid.TryParse(s, out System.Guid Guid))
                        CheckAgainst = new ObjectValue(Guid);
                    else
                        return PatternMatchResult.NoMatch;
                }
                else if (Obj is byte[] Bin)
                {
                    if (Bin.Length == 16)
                        CheckAgainst = new ObjectValue(new System.Guid(Bin));
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
