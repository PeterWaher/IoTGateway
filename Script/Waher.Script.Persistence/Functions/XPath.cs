using System;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.Functions
{
	/// <summary>
	/// Node repesenting an XPath expression
	/// </summary>
	public class XPath : FunctionOneScalarVariable
	{
		private readonly string xpath;
		private bool extractValue;

		/// <summary>
		/// Node repesenting an XPath expression
		/// </summary>
		/// <param name="XPath">XPath expression.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XPath(ScriptNode XPath, int Start, int Length, Expression Expression)
			: base(XPath, Start, Length, Expression)
		{
			this.xpath = null;
			this.extractValue = false;
		}

		/// <summary>
		/// Node repesenting an XPath expression
		/// </summary>
		/// <param name="XPath">XPath expression.</param>
		/// <param name="ExtractValue">If the goal of the XPATH is to extract a value.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XPath(string XPath, bool ExtractValue, int Start, int Length, Expression Expression)
			: base(new ConstantElement(new StringValue(XPath, false), Start, Length, Expression), Start, Length, Expression)
		{
			this.xpath = XPath;
			this.extractValue = ExtractValue;
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "xpath";

		/// <summary>
		/// XPATH expression
		/// </summary>
		public string XPathExpression => this.xpath;

		/// <summary>
		/// If the value should be extracted.
		/// </summary>
		public bool ExtractValue
		{
			get => this.extractValue;
			internal set => this.extractValue = value;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (string.IsNullOrEmpty(this.xpath))
				return base.Evaluate(Variables);
			else
				return this.EvaluateScalar(this.xpath, Variables);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			if (!Variables.TryGetVariable("this", out Variable v))
				throw new ScriptRuntimeException("'this' variable not defined.", this);

			object Obj = v.ValueObject;
			XmlNodeList Result;

			if (Obj is null)
				return ObjectValue.Null;
			else if (Obj is XmlNode N)
				Result = N.SelectNodes(Argument);
			else
				throw new ScriptRuntimeException("XPath expression only operate on XML.", this);

			int i, c = Result.Count;

			switch (c)
			{
				case 0:
					return ObjectValue.Null;

				case 1:
					return ToElement(Result[0], this.extractValue);

				default:
					IElement[] Items = new IElement[c];

					for (i = 0; i < c; i++)
						Items[i] = ToElement(Result[i], this.extractValue);

					return Operators.Vectors.VectorDefinition.Encapsulate(Items, false, this);
			}
		}

		/// <summary>
		/// Encapsulates an XML Node for use in script.
		/// </summary>
		/// <param name="Node">XML Node</param>
		/// <param name="ExtractValue">If the goal of the XPATH is to extract a value.</param>
		/// <returns>Script element.</returns>
		public static IElement ToElement(XmlNode Node, bool ExtractValue)
		{
			if (Node is XmlText Text)
				return new StringValue(Text.Value);
			else if (Node is XmlCDataSection CData)
				return new StringValue(CData.Value);
			else if (Node is XmlAttribute Attr)
				return ToElement(Attr.Value);
			else
			{
				if (ExtractValue && Node.HasChildNodes && Node.FirstChild == Node.LastChild && Node.FirstChild is XmlText Text2)
					return ToElement(Text2.Value);
				else
					return new ObjectValue(Node);
			}
		}

		private static IElement ToElement(string s)
		{
			if (Expression.TryParse(s, out double d))
				return new DoubleNumber(d);
			else
				return new StringValue(s);
		}

	}
}
