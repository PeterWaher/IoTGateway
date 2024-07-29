using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Functions.Vectors
{
    /// <summary>
    /// PushFirst(v)
    /// </summary>
    public class PushFirst : FunctionMultiVariate
    {
        private readonly VariableReference reference;

        /// <summary>
        /// PushFirst(v)
        /// </summary>
        /// <param name="Element">Element to push.</param>
        /// <param name="Vector">Vector to receive element.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public PushFirst(ScriptNode Element, ScriptNode Vector, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Element, Vector }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Vector }, 
            Start, Length, Expression)
        {
            this.reference = Vector as VariableReference;
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(PushFirst);

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "x", "v" };

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Arguments">Function arguments.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            if (!(Arguments[1] is IVector V))
                throw new ScriptRuntimeException("Expected second argument to be a vector.", this);

            LinkedList<IElement> Elements = new LinkedList<IElement>();

            Elements.AddLast(Arguments[0]);

            foreach (IElement E in V.VectorElements)
                Elements.AddLast(E);

            IElement Result = V.Encapsulate(Elements, this);

            if (!(this.reference is null))
                Variables[this.reference.VariableName] = Result;

            return Result;
        }

    }
}
