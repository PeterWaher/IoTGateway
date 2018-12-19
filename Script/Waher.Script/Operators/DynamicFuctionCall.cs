using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Dynamic function call operator
	/// </summary>
	public class DynamicFunctionCall : UnaryScalarOperator 
	{
		private readonly ScriptNode[] arguments;

		/// <summary>
		/// Dynamic function call operator
		/// </summary>
		/// <param name="Function">Function</param>
		/// <param name="Arguments">Arguments</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
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
			if (!(Operand.AssociatedObjectValue is ILambdaExpression Lambda))
				throw new ScriptRuntimeException("Expected a lambda expression.", this);

			int c = this.arguments.Length;
            if (c != Lambda.NrArguments)
                throw new ScriptRuntimeException("Expected " + Lambda.NrArguments.ToString() + " arguments.", this);

            IElement[] Arg = new IElement[c];
			ScriptNode Node;
            int i;

            for (i = 0; i < c; i++)
			{
				Node = this.arguments[i];
				if (Node is null)
					Arg[i] = ObjectValue.Null;
				else
					Arg[i] = Node.Evaluate(Variables);
			}

            return Lambda.Evaluate(Arg, Variables);
        }
    }
}
