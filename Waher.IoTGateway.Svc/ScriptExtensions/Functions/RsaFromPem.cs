using System.Security.Cryptography;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.Svc.ScriptExtensions.Functions
{
    /// <summary>
    /// Creates an RSA object from the contents of a PEM file.
    /// </summary>
    public class RsaFromPem : FunctionOneScalarVariable
	{
        /// <summary>
        /// Creates an RSA object from the contents of a PEM file.
        /// </summary>
        /// <param name="PemContents">Contents of PEM file.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public RsaFromPem(ScriptNode PemContents, int Start, int Length, Expression Expression)
            : base(PemContents, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(RsaFromPem);

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "PemContents" };

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
            RSA RSA = RSA.Create();
            RSA.ImportFromPem(Argument);

            return new ObjectValue(RSA);
        }
    }
}
