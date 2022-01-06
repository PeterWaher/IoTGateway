using System;
using System.Xml;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Functions
{
	/// <summary>
	/// Xml(s)
	/// </summary>
	public class Xml : FunctionOneScalarVariable
    {
        /// <summary>
        /// Xml(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Xml(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "xml"; }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(string Argument, Variables Variables)
        {
            XmlDocument Doc = new XmlDocument()
            {
                PreserveWhitespace = true
            };

            Doc.LoadXml(Argument);

			return new ObjectValue(Doc);
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
            object Obj = Argument.AssociatedObjectValue;

            if (Obj is string s)
                return this.EvaluateScalar(s, Variables);
            else if (Obj is XmlDocument)
                return Argument;
            else if (Obj is XmlNode N)
                return this.EvaluateScalar(N.OuterXml, Variables);
            else
			    return base.EvaluateScalar(Argument, Variables);
		}

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
        {
            return Task.FromResult<IElement>(this.EvaluateScalar(Argument, Variables));
        }
    }
}
