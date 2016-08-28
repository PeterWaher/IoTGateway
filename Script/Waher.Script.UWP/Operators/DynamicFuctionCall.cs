using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Dynamic function call operator
	/// </summary>
	public class DynamicFunctionCall : UnaryScalarOperator 
	{
		private ScriptNode[] arguments;

		/// <summary>
		/// Dynamic function call operator
		/// </summary>
		/// <param name="Function">Function</param>
		/// <param name="Arguments">Arguments</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public DynamicFunctionCall(ScriptNode Function, ScriptNode[] Arguments, int Start, int Length, Expression Expression)
			: base(Function, Start, Length, Expression)
		{
			this.arguments = Arguments;
		}

		/// <summary>
		/// Arguments
		/// </summary>
		public ScriptNode[] Arguments
		{
			get { return this.arguments; }
		}

        /// <summary>
        /// Evaluates the operator on scalar operands.
        /// </summary>
        /// <param name="Operand">Operand.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result</returns>
        public override IElement EvaluateScalar(IElement Operand, Variables Variables)
        {
            LambdaDefinition Lambda = Operand as LambdaDefinition;
            if (Lambda == null)
                throw new ScriptRuntimeException("Expected a lambda expression.", this);

            int c = this.arguments.Length;
            if (c != Lambda.NrArguments)
                throw new ScriptRuntimeException("Expected " + Lambda.NrArguments.ToString() + " arguments.", this);

            IElement[] Arguments = new IElement[c];
            int i;

            for (i = 0; i < c; i++)
                Arguments[i] = this.arguments[i].Evaluate(Variables);

            return Lambda.Evaluate(Arguments, Variables);
        }
    }
}
