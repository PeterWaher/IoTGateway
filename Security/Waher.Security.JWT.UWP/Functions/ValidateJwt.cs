using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Security.JWT.Functions
{
	/// <summary>
	/// Validates a Java Web Token (JWT) created by CreateJwt.
	/// </summary>
	public class ValidateJwt : FunctionOneScalarVariable
	{
        /// <summary>
        /// Validates a Java Web Token (JWT) created by CreateJwt.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public ValidateJwt(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "ValidateJwt"; }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "Token" };

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
            JwtToken Token = new JwtToken(Argument);

            if (!CreateJwt.factory.IsValid(Token))
                throw new ScriptRuntimeException("Token not valid.", this);

            return new ObjectValue(Token);
        }
    }
}
