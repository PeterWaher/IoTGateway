using System;
using System.Text;
using System.Xml;
using Waher.Content.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Functions
{
	/// <summary>
	/// PrettyXml(s)
	/// </summary>
	public class PrettyXml : FunctionOneScalarVariable
	{
		/// <summary>
		/// PrettyXml(x)
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
			get { return "prettyxml"; }
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

			StringBuilder sb = new StringBuilder();

			using (XmlWriter w = XmlWriter.Create(sb, XML.WriterSettings(true, true)))
			{
				Doc.WriteTo(w);
			}

			return new StringValue(sb.ToString());
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

			if (Obj is null)
				return Argument;
			else if (Obj is string s)
				return this.EvaluateScalar(s, Variables);
			else if (Obj is XmlNode N)
			{
				StringBuilder sb = new StringBuilder();

				using (XmlWriter w = XmlWriter.Create(sb, XML.WriterSettings(true, true)))
				{
					N.WriteTo(w);
				}

				return new StringValue(sb.ToString());
			}
			else
				return base.EvaluateScalar(Argument, Variables);
		}

		/// <summary>
		/// Converts XML to an indented string representation.
		/// </summary>
		/// <param name="Xml">XML to convert.</param>
		/// <returns>Prettified string representation of XML.</returns>
		public static string ToString(XmlNode Xml)
		{
			StringBuilder sb = new StringBuilder();

			using (XmlWriter w = XmlWriter.Create(sb, XML.WriterSettings(true, true)))
			{
				Xml?.WriteTo(w);
			}

			return sb.ToString();
		}
	}
}
