using System.Threading.Tasks;
using System.Xml;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Xml.Functions
{
	/// <summary>
	/// Performs an XML slection using XPATH.
	/// </summary>
	public class SelectXml : FunctionTwoScalarVariables
	{
		/// <summary>
		/// Performs an XML slection using XPATH.
		/// </summary>
		/// <param name="Xml">XML.</param>
		/// <param name="Name">Name of child element.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SelectXml(ScriptNode Xml, ScriptNode Name, int Start, int Length, Expression Expression)
			: base(Xml, Name, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(SelectXml);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "XML", "XPath" };

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
		{
			object Obj = Argument1.AssociatedObjectValue;
			XmlNodeList Result;

			if (Obj is null)
				return ObjectValue.Null;
			else if (Obj is XmlNode N)
				Result = Evaluate(N, Argument2.AssociatedObjectValue?.ToString() ?? string.Empty);
			else
				throw new ScriptRuntimeException("XPath expression only operate on XML.", this);

			int i, c = Result.Count;

			switch (c)
			{
				case 0:
					return ObjectValue.Null;

				case 1:
					return ToElement(Result[0]);

				default:
					IElement[] Items = new IElement[c];

					for (i = 0; i < c; i++)
						Items[i] = ToElement(Result[i]);

					return Operators.Vectors.VectorDefinition.Encapsulate(Items, false, this);
			}
		}

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument1, IElement Argument2, Variables Variables)
		{
			return Task.FromResult(this.EvaluateScalar(Argument1, Argument2, Variables));
		}

		internal static XmlNodeList Evaluate(XmlNode N, string XPath)
		{
			XmlNamespaceManager NamespaceManager;
			XmlNodeList Result;
			XmlElement Root;

			if (N is XmlDocument Doc)
			{
				NamespaceManager = new XmlNamespaceManager(Doc.NameTable);
				Root = Doc.DocumentElement;
			}
			else if (N is XmlElement E && !(N.OwnerDocument is null))
			{
				NamespaceManager = new XmlNamespaceManager(N.OwnerDocument.NameTable);
				Root = E;
			}
			else
			{
				NamespaceManager = null;
				Root = null;
			}

			if (NamespaceManager is null)
				Result = N.SelectNodes(XPath);
			else
			{
				if (XPath.Contains("default:"))
				{
					string Namespace = null;

					if (string.IsNullOrEmpty(Root.Prefix) && !string.IsNullOrEmpty(Root.NamespaceURI))
						Namespace = Root.NamespaceURI;
					else
					{
						ChunkedList<XmlElement> ToProcess = new ChunkedList<XmlElement>();

						foreach (XmlNode N2 in Root.ChildNodes)
						{
							if (N2 is XmlElement E)
								ToProcess.Add(E);
						}

						while (ToProcess.HasFirstItem)
						{
							Root = ToProcess.RemoveFirst();

							if (string.IsNullOrEmpty(Root.Prefix) && !string.IsNullOrEmpty(Root.NamespaceURI))
							{
								Namespace = Root.NamespaceURI;
								break;
							}

							foreach (XmlNode N2 in Root.ChildNodes)
							{
								if (N2 is XmlElement E)
									ToProcess.Add(E);
							}
						}
					}

					if (!string.IsNullOrEmpty(Namespace) && !NamespaceManager.HasNamespace(Namespace))
						NamespaceManager.AddNamespace("default", Namespace);
				}

				Result = N.SelectNodes(XPath, NamespaceManager);
			}

			return Result;
		}

		/// <summary>
		/// Encapsulates an XML Node for use in script.
		/// </summary>
		/// <param name="Node">XML Node</param>
		/// <returns>Script element.</returns>
		public static IElement ToElement(XmlNode Node)
		{
			if (Node is XmlText Text)
				return ToElement(Text.Value);
			else if (Node is XmlCDataSection CData)
				return ToElement(CData.Value);
			else if (Node is XmlAttribute Attr)
				return ToElement(Attr.Value);
			else
			{
				if (Node.HasChildNodes && Node.FirstChild == Node.LastChild && Node.FirstChild is XmlText Text2)
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
