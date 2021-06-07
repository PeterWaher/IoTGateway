using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Markdown.Model;
using Waher.Content.Xml;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Operators.Vectors;

namespace Waher.Content.Markdown.Functions
{
	/// <summary>
	/// Converts markdown to an element.
	/// </summary>
	public class FromMarkdown : FunctionOneScalarVariable
	{
		/// <summary>
		/// Converts markdown to an element.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public FromMarkdown(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "FromMarkdown";

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return Evaluate(Argument, this);
		}

		/// <summary>
		/// Converts a Markdown string to a script element.
		/// </summary>
		/// <param name="Argument">Argument</param>
		/// <param name="Node">Optional script node.</param>
		/// <returns>Script element.</returns>
		public static IElement Evaluate(string Argument, ScriptNode Node)
		{
			MarkdownDocument Doc = new MarkdownDocument(Argument);
			IElement Result = null;
			LinkedList<IElement> Results = null;
			IElement Item = null;

			foreach (MarkdownElement E in Doc.Elements)
			{
				Item = Evaluate(E);

				if (Result is null)
					Result = Item;
				else
				{
					if (Results is null)
					{
						Results = new LinkedList<IElement>();
						Results.AddLast(Result);
					}

					Results.AddLast(Item);
				}
			}

			if (Results is null)
				return Item ?? ObjectValue.Null;
			else
				return VectorDefinition.Encapsulate(Results, false, Node);
		}

		/// <summary>
		/// Converts a Markdown element to a script element.
		/// </summary>
		/// <param name="Element">Markdown element.</param>
		/// <returns>Script element.</returns>
		public static IElement Evaluate(MarkdownElement Element)
		{
			StringBuilder sb;

			if (Element is Model.BlockElements.Table Table)
			{
				if (Table.Headers.Length == 1)
				{
					MarkdownElement[] Headers = Table.Headers[0];
					int i, Columns = Headers.Length;
					string[] Headers2 = new string[Columns];
					int Rows = Table.Rows.Length;
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					for (i = 0; i < Columns; i++)
						Headers2[i] = Evaluate(Headers[i]).AssociatedObjectValue?.ToString() ?? string.Empty;

					foreach (MarkdownElement[] Row in Table.Rows)
					{
						foreach (MarkdownElement E in Row)
						{
							if (E is null)
								Elements.AddLast(ObjectValue.Null);
							else
								Elements.AddLast(Evaluate(E));
						}
					}

					ObjectMatrix M = new ObjectMatrix(Rows, Columns, Elements)
					{
						ColumnNames = Headers2
					};

					return M;
				}
			}
			else if (Element is Model.BlockElements.CodeBlock CodeBlock)
			{
				string Language = CodeBlock.Language;
				int i = Language.IndexOf(':');

				if (i > 0)
					Language = Language.Substring(0, i);

				switch (Language.ToLower())
				{
					case "graph":
						return Model.CodeContent.GraphContent.GetGraph(CodeBlock.Rows);

					case "xml":
						sb = new StringBuilder();

						foreach (string Row in CodeBlock.Rows)
							sb.AppendLine(Row);

						XmlDocument Doc = new XmlDocument();
						Doc.LoadXml(sb.ToString());

						return new ObjectValue(Doc);

					default:
						if (CodeBlock.Handler is IImageCodeContent ImageCodeContent)
						{
							PixelInformation Pixels = ImageCodeContent.GenerateImage(CodeBlock.Rows, Language, Element.Document);
							return new GraphBitmap(Pixels);
						}
						break;
				}
			}

			sb = new StringBuilder();
			Element.GeneratePlainText(sb);
			string s = sb.ToString().Trim();

			if (CommonTypes.TryParse(s, out double d))
				return new DoubleNumber(d);
			else if (CommonTypes.TryParse(s, out bool b))
				return new BooleanValue(b);
			else if (PhysicalQuantity.TryParse(s, out PhysicalQuantity Q))
				return Q;
			else if (XML.TryParse(s, out DateTime TP))
				return new DateTimeValue(TP);
			else if (Element is Model.SpanElements.InlineText)
				return new StringValue(s);
			else
				return new ObjectValue(Element);
		}
	}
}
