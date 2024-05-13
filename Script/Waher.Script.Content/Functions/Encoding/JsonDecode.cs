using System;
using Waher.Content;
using Waher.Script;
using Waher.Script.Abstraction;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Encoding
{
    /// <summary>
    /// JsonDecode(Json)
    /// </summary>
    public class JsonDecode : FunctionOneScalarStringVariable
    {
        /// <summary>
        /// JsonDecode(Json)
        /// </summary>
        /// <param name="Json">Json-encoded data</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public JsonDecode(ScriptNode Json, int Start, int Length, Expression Expression)
            : base(Json, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(JsonDecode);

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
            return Expression.Encapsulate(JSON.Parse(Argument));
        }

    }
}
