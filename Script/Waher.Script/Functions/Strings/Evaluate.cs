using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Strings
{
    /// <summary>
    /// Evaluate(s)
    /// </summary>
    public class Evaluate : FunctionOneScalarStringVariable
    {
        /// <summary>
        /// Evaluate(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Evaluate(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Evaluate);

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases => new string[] { "eval" };

        /// <summary>
        /// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
        /// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
        /// </summary>
        public override bool IsAsynchronous => true;

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
            return this.EvaluateScalarAsync(Argument, Variables).Result;
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override async Task<IElement> EvaluateScalarAsync(string Argument, Variables Variables)
        {
            Expression Exp = new Expression(Argument, this.Expression.Source);

            try
            {
                return await Exp.Root.EvaluateAsync(Variables);
            }
            catch (ScriptReturnValueException ex)
            {
                return ex.ReturnValue;
                //IElement ReturnValue = ex.ReturnValue;
				//ScriptReturnValueException.Reuse(ex);
                //return ReturnValue;
			}
			catch (ScriptBreakLoopException ex)
			{
				return ex.LoopValue ?? ObjectValue.Null;
				//ScriptBreakLoopException.Reuse(ex);
			}
			catch (ScriptContinueLoopException ex)
			{
				return ex.LoopValue ?? ObjectValue.Null;
				//ScriptContinueLoopException.Reuse(ex);
			}
		}
	}
}
