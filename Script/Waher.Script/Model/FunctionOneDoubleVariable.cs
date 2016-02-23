using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for funcions of one double variable.
    /// </summary>
    public abstract class FunctionOneDoubleVariable : FunctionOneScalarVariable 
	{
        /// <summary>
        /// Base class for funcions of one double variable.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public FunctionOneDoubleVariable(ScriptNode Argument, int Start, int Length)
			: base(Argument, Start, Length)
		{
		}

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(IElement Argument, Variables Variables)
        {
            DoubleNumber D = Argument as DoubleNumber;
            if (D != null)
                return this.EvaluateScalar(D.Value, Variables);
            else
                throw new ScriptRuntimeException("Expected a double-valued argument.", this);
        }

        /// <summary>
        /// Evaluates the function on a double argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public abstract IElement EvaluateScalar(double Argument, Variables Variables);

    }
}
