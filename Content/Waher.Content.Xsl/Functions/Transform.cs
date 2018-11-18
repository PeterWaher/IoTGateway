using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Content.Xsl.Functions
{
	/// <summary>
	/// Transform(XML,XSLT)
	/// </summary>
	public class Transform : FunctionTwoScalarVariables
    {
		/// <summary>
		/// Transform(XML,XSLT)
		/// </summary>
		/// <param name="Xml">XML</param>
		/// <param name="Xslt">XSLT</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Transform(ScriptNode Xml, ScriptNode Xslt, int Start, int Length, Expression Expression)
            : base(Xml, Xslt, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "transform"; }
        }

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
		{
			string s;

			if (Argument1.AssociatedObjectValue is XmlDocument Xml)
				s = Xml.OuterXml;
			else if (Argument1 is StringValue S)
				s = S.Value;
			else
				s = Expression.ToString(Argument1.AssociatedObjectValue);

			if (!(Argument2.AssociatedObjectValue is XslCompiledTransform Xslt))
				throw new ScriptRuntimeException("Second parameter must be an XSL Transform (XSLT) object.", this);

			return this.DoTransform(s, Xslt);
		}

		private IElement DoTransform(string Xml, XslCompiledTransform Xslt)
		{
			return new StringValue(XSL.Transform(Xml, Xslt));
		}

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
		{
			using (StringReader s = new StringReader(Argument2))
			{
				using (XmlReader r = XmlReader.Create(s))
				{
					XslCompiledTransform Xslt = new XslCompiledTransform();
					Xslt.Load(r);

					return this.DoTransform(Argument1, Xslt);
				}
			}
		}

	}
}
