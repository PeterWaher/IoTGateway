using System.Threading.Tasks;
using Waher.Content;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Encoding
{
    /// <summary>
    /// JsonEncode(Data)
    /// </summary>
    public class JsonEncode : FunctionOneVariable
    {
        /// <summary>
        /// JsonEncode(Data)
        /// </summary>
        /// <param name="Data">Binary data</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public JsonEncode(ScriptNode Data, int Start, int Length, Expression Expression)
            : base(Data, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(JsonEncode);

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            return new StringValue(JSON.Encode(Argument.AssociatedObjectValue, false));
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override Task<IElement> EvaluateAsync(IElement Argument, Variables Variables)
        {
            return Task.FromResult(this.Evaluate(Argument, Variables));
        }

    }
}
