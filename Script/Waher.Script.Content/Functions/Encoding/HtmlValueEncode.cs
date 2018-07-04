using System;
using Waher.Content.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Encoding
{
	/// <summary>
	/// HtmlValueEncode(s)
	/// </summary>
	public class HtmlValueEncode : FunctionOneScalarVariable
    {
        /// <summary>
        /// HtmlValueEncode(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public HtmlValueEncode(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "htmlvalueencode"; }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
			return new StringValue(XML.HtmlValueEncode(Argument));
        }
    }
}
