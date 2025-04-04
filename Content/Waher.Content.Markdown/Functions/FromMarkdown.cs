using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Xml;
using Waher.Runtime.Collections;
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
	public class FromMarkdown : FunctionOneScalarStringVariable
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
		public override string FunctionName => nameof(FromMarkdown);

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return this.EvaluateScalarAsync(Argument, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(string Argument, Variables Variables)
		{
			return Evaluate(Argument, this);
		}

		/// <summary>
		/// Converts a Markdown string to a script element.
		/// </summary>
		/// <param name="Argument">Argument</param>
		/// <param name="Node">Optional script node.</param>
		/// <returns>Script element.</returns>
		public static async Task<IElement> Evaluate(string Argument, ScriptNode Node)
		{
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Argument);
			IElement Result = null;
			ChunkedList<IElement> Results = null;
			IElement Item = null;
			ChunkNode<MarkdownElement> Loop = Doc.Elements.FirstChunk;
			int i, c;

			while (!(Loop is null))
			{
				for (i = Loop.Start, c = Loop.Pos; i < c; i++)
				{
					Item = await Evaluate(Loop[i]);

					if (Result is null)
						Result = Item;
					else
					{
						if (Results is null)
						{
							Results = new ChunkedList<IElement>
							{
								Result
							};
						}

						Results.Add(Item);
					}
				}

				Loop = Loop.Next;
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
		public static async Task<IElement> Evaluate(MarkdownElement Element)
		{
			if (Element is Model.BlockElements.Table Table)
			{
				if (Table.Headers.Length == 1)
				{
					MarkdownElement[] Headers = Table.Headers[0];
					int i, Columns = Headers.Length;
					string[] Headers2 = new string[Columns];
					int Rows = Table.Rows.Length;
					ChunkedList<IElement> Elements = new ChunkedList<IElement>();
					MarkdownElement E;
					int j, d;

					for (i = 0; i < Columns; i++)
						Headers2[i] = (await Evaluate(Headers[i])).AssociatedObjectValue?.ToString() ?? string.Empty;

					foreach (MarkdownElement[] Row in Table.Rows)
					{
						for (j = 0, d = Row.Length; j < d; j++)
						{
							E = Row[j];

							if (E is null)
								Elements.Add(ObjectValue.Null);
							else
								Elements.Add(await Evaluate(E));
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
						return await Model.CodeContent.GraphContent.GetGraph(CodeBlock.Rows);

					case "xml":
						StringBuilder sb = new StringBuilder();

						foreach (string Row in CodeBlock.Rows)
							sb.AppendLine(Row);

						XmlDocument Doc = new XmlDocument();
						Doc.LoadXml(sb.ToString());

						return new ObjectValue(Doc);

					default:
						ICodeContentHtmlRenderer Renderer = CodeBlock.CodeContentHandler<ICodeContentHtmlRenderer>();

						if (Renderer is IImageCodeContent ImageCodeContent)
						{
							PixelInformation Pixels = await ImageCodeContent.GenerateImage(CodeBlock.Rows, Language, Element.Document);
							return new GraphBitmap(Element.Document.Settings.Variables, Pixels);
						}
						break;
				}
			}
			else if (Element is null)
				return ObjectValue.Null;

			using (TextRenderer Renderer2 = new TextRenderer())
			{
				await Element.Render(Renderer2);
				string s = Renderer2.ToString().Trim();

				if (CommonTypes.TryParse(s, out double d))
					return new DoubleNumber(d);
				else if (CommonTypes.TryParse(s, out bool b))
					return new BooleanValue(b);
				else if (Measurement.TryParse(s, out Measurement M))
					return M;
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
}
