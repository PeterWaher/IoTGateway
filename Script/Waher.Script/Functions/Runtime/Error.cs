using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Functions.Runtime
{
    /// <summary>
    /// Throws an exception.
    /// </summary>
    public class Error : FunctionOneVariable
    {
        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Error(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Error);

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases => new string[] { "exception" };

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            string Msg = Argument.ToString();
            throw new Exception(Msg);
        }
    }
}
