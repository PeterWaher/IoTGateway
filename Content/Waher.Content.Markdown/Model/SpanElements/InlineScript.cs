using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Content.Emoji;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.Multimedia;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Objects.Matrices;

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Inline source code.
	/// </summary>
	public class InlineScript : MarkdownElement, IContextVariables
	{
		private readonly Expression expression;
		private readonly Variables variables;
		private readonly int startPosition;
		private readonly int endPosition;
		private readonly bool aloneInParagraph;

		/// <summary>
		/// Inline source code.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Expression">Expression.</param>
		/// <param name="Variables">Collection of variables to use when executing the script.</param>
		/// <param name="AloneInParagraph">If construct stands alone in a paragraph.</param>
		/// <param name="StartPosition">Starting position of script.</param>
		/// <param name="EndPosition">Ending position of script.</param>
		public InlineScript(MarkdownDocument Document, Expression Expression, Variables Variables, bool AloneInParagraph,
			int StartPosition, int EndPosition)
			: base(Document)
		{
			this.expression = Expression;
			this.variables = Variables;
			this.aloneInParagraph = AloneInParagraph;
			this.startPosition = StartPosition;
			this.endPosition = EndPosition;
		}

		/// <summary>
		/// Expression
		/// </summary>
		public Expression Expresion => this.expression;

		/// <summary>
		/// If the element is alone in a paragraph.
		/// </summary>
		public bool AloneInParagraph => this.aloneInParagraph;

		/// <summary>
		/// Starting position of script in markdown document.
		/// </summary>
		public int StartPosition => this.startPosition;

		/// <summary>
		/// Ending position of script in markdown document.
		/// </summary>
		public int EndPosition => this.endPosition;

		private async Task<object> EvaluateExpression()
		{
			IContextVariables Bak = this.variables.ContextVariables;
			try
			{
				this.variables.ContextVariables = this;
				return await this.expression.EvaluateAsync(this.variables);
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				this.Document.CheckException(ex);

				return ex;
			}
			finally
			{
				this.variables.ContextVariables = Bak;
			}
		}

		/// <summary>
		/// Generates Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		public override Task GenerateMarkdown(StringBuilder Output)
		{
			Output.Append("{");
			Output.Append(this.expression.Script);
			Output.Append("}");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override async Task GenerateHTML(StringBuilder Output)
		{
			object Result = await this.EvaluateExpression();
			if (Result is null)
				return;

			await GenerateHTML(Result, Output, this.aloneInParagraph, this.variables);
		}

		/// <summary>
		/// Generates HTML from Script output.
		/// </summary>
		/// <param name="Result">Script output.</param>
		/// <param name="Output">HTML output.</param>
		/// <param name="AloneInParagraph">If the script output is to be presented alone in a paragraph.</param>
		/// <param name="Variables">Current variables.</param>
		public static async Task GenerateHTML(object Result, StringBuilder Output, bool AloneInParagraph, Variables Variables)
		{
			if (Result is XmlDocument Xml)
				Result = await MarkdownDocument.TransformXml(Xml, Variables);

			if (Result is Graph G)
			{
				PixelInformation Pixels = G.CreatePixels(Variables, out GraphSettings GraphSettings);
				byte[] Bin = Pixels.EncodeAsPng();

				if (AloneInParagraph)
					Output.Append("<figure>");

				Output.Append("<img border=\"2\" width=\"");
				Output.Append(GraphSettings.Width.ToString());
				Output.Append("\" height=\"");
				Output.Append(GraphSettings.Height.ToString());
				Output.Append("\" src=\"data:image/png;base64,");
				Output.Append(Convert.ToBase64String(Bin, 0, Bin.Length));
				Output.Append("\" />");

				if (AloneInParagraph)
					Output.Append("</figure>");
			}
			else if (Result is PixelInformation Pixels)
			{
				byte[] Bin = Pixels.EncodeAsPng();

				if (AloneInParagraph)
					Output.Append("<figure>");

				Output.Append("<img border=\"2\" width=\"");
				Output.Append(Pixels.Width.ToString());
				Output.Append("\" height=\"");
				Output.Append(Pixels.Height.ToString());
				Output.Append("\" src=\"data:image/png;base64,");
				Output.Append(Convert.ToBase64String(Bin, 0, Bin.Length));
				Output.Append("\" />");

				if (AloneInParagraph)
					Output.Append("</figure>");
			}
			else if (Result is SKImage Img)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();

					if (AloneInParagraph)
						Output.Append("<figure>");

					Output.Append("<img border=\"2\" width=\"");
					Output.Append(Img.Width.ToString());
					Output.Append("\" height=\"");
					Output.Append(Img.Height.ToString());
					Output.Append("\" src=\"data:image/png;base64,");
					Output.Append(Convert.ToBase64String(Bin, 0, Bin.Length));
					Output.Append("\" />");

					if (AloneInParagraph)
						Output.Append("</figure>");
				}
			}
			else if (Result is Exception ex)
			{
				ex = Log.UnnestException(ex);

				if (ex is AggregateException ex2)
				{
					foreach (Exception ex3 in ex2.InnerExceptions)
					{
						Output.Append("<p><font class=\"error\">");
						Output.Append(XML.HtmlValueEncode(ex3.Message));
						Output.AppendLine("</font></p>");
					}
				}
				else
				{
					if (AloneInParagraph)
						Output.Append("<p>");

					Output.Append("<font class=\"error\">");
					Output.Append(XML.HtmlValueEncode(ex.Message));
					Output.Append("</font>");

					if (AloneInParagraph)
						Output.Append("</p>");
				}
			}
			else if (Result is ObjectMatrix M && !(M.ColumnNames is null))
			{
				Output.Append("<table><thead><tr>");

				foreach (string s2 in M.ColumnNames)
				{
					Output.Append("<th>");
					Output.Append(FormatText(XML.HtmlValueEncode(s2)));
					Output.Append("</th>");
				}

				Output.Append("</tr></thead><tbody>");

				int x, y;

				for (y = 0; y < M.Rows; y++)
				{
					Output.Append("<tr>");

					for (x = 0; x < M.Columns; x++)
					{
						Output.Append("<td>");

						object Item = M.GetElement(x, y).AssociatedObjectValue;
						if (!(Item is null))
						{
							if (Item is string s2)
								Output.Append(FormatText(XML.HtmlValueEncode(s2)));
							else if (Item is MarkdownElement Element)
								await Element.GenerateHTML(Output);
							else
								Output.Append(FormatText(XML.HtmlValueEncode(Expression.ToString(Item))));
						}

						Output.Append("</td>");
					}

					Output.Append("</tr>");
				}

				Output.Append("</tbody></table>");
			}
			else if (Result is Array A)
			{
				foreach (object Item in A)
					await GenerateHTML(Item, Output, false, Variables);
			}
			else
			{
				if (AloneInParagraph)
					Output.Append("<p>");

				Output.Append(XML.HtmlValueEncode(Result?.ToString() ?? string.Empty));

				if (AloneInParagraph)
					Output.Append("</p>");
			}

			if (AloneInParagraph)
				Output.AppendLine();
		}

		private static string FormatText(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\n", "<br/>").Replace("\r", "<br/>").
				Replace("\t", "&nbsp;&nbsp;&nbsp;").Replace(" ", "&nbsp;");
		}

		/// <summary>
		/// Generates plain text for the markdown element.
		/// </summary>
		/// <param name="Output">Plain text will be output here.</param>
		public override async Task GeneratePlainText(StringBuilder Output)
		{
			object Result = await this.EvaluateExpression();
			if (Result is null)
				return;

			await GeneratePlainText(Result, Output, this.aloneInParagraph);
		}

		/// <summary>
		/// Generates plain text from Script output.
		/// </summary>
		/// <param name="Result">Script output.</param>
		/// <param name="Output">HTML output.</param>
		/// <param name="AloneInParagraph">If the script output is to be presented alone in a paragraph.</param>
		public static Task GeneratePlainText(object Result, StringBuilder Output, bool AloneInParagraph)
		{
			Output.Append(Result.ToString());

			if (AloneInParagraph)
			{
				Output.AppendLine();
				Output.AppendLine();
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override async Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment)
		{
			object Result = await this.EvaluateExpression();
			if (Result is null)
				return;

			await GenerateXAML(Result, Output, TextAlignment, this.aloneInParagraph, this.variables, this.Document.Settings.XamlSettings);
		}

		/// <summary>
		/// Generates WPF XAML from Script output.
		/// </summary>
		/// <param name="Result">Script output.</param>
		/// <param name="Output">HTML output.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="AloneInParagraph">If the script output is to be presented alone in a paragraph.</param>
		/// <param name="Variables">Current variables.</param>
		/// <param name="XamlSettings">XAML Settings</param>
		public static async Task GenerateXAML(object Result, XmlWriter Output, TextAlignment TextAlignment, bool AloneInParagraph,
			Variables Variables, XamlSettings XamlSettings)
		{
			string s;

			if (Result is Graph G)
			{
				PixelInformation Pixels = G.CreatePixels(Variables);
				byte[] Bin = Pixels.EncodeAsPng();

				s = "data:image/png;base64," + Convert.ToBase64String(Bin, 0, Bin.Length);

				await ImageContent.OutputWpf(Output, new ImageSource()
				{
					Url = s,
					Width = Pixels.Width,
					Height = Pixels.Height
				}, string.Empty);
			}
			else if (Result is SKImage Img)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();

					s = "data:image/png;base64," + Convert.ToBase64String(Bin, 0, Bin.Length);

					await ImageContent.OutputWpf(Output, new ImageSource()
					{
						Url = s,
						Width = Img.Width,
						Height = Img.Height
					}, string.Empty);
				}
			}
			else if (Result is Exception ex)
			{
				ex = Log.UnnestException(ex);

				if (ex is AggregateException ex2)
				{
					foreach (Exception ex3 in ex2.InnerExceptions)
					{
						Output.WriteStartElement("TextBlock");
						Output.WriteAttributeString("TextWrapping", "Wrap");
						Output.WriteAttributeString("Margin", XamlSettings.ParagraphMargins);

						if (TextAlignment != TextAlignment.Left)
							Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

						Output.WriteAttributeString("Foreground", "Red");
						Output.WriteValue(ex3.Message);
						Output.WriteEndElement();
					}
				}
				else
				{
					if (AloneInParagraph)
					{
						Output.WriteStartElement("TextBlock");
						Output.WriteAttributeString("TextWrapping", "Wrap");
						Output.WriteAttributeString("Margin", XamlSettings.ParagraphMargins);
						if (TextAlignment != TextAlignment.Left)
							Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());
					}
					else
						Output.WriteStartElement("Run");

					Output.WriteAttributeString("Foreground", "Red");
					Output.WriteValue(ex.Message);
					Output.WriteEndElement();
				}
			}
			else
			{
				if (AloneInParagraph)
				{
					Output.WriteStartElement("TextBlock");
					Output.WriteAttributeString("TextWrapping", "Wrap");
					Output.WriteAttributeString("Margin", XamlSettings.ParagraphMargins);
					if (TextAlignment != TextAlignment.Left)
						Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());
				}

				Output.WriteValue(Result.ToString());

				if (AloneInParagraph)
					Output.WriteEndElement();
			}
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		public override async Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State)
		{
			object Result = await this.EvaluateExpression();
			if (Result is null)
				return;

			await GenerateXamarinForms(Result, Output, State, this.aloneInParagraph, this.variables, this.Document.Settings.XamlSettings);
		}

		/// <summary>
		/// Generates WPF XAML from Script output.
		/// </summary>
		/// <param name="Result">Script output.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		/// <param name="Output">HTML output.</param>
		/// <param name="AloneInParagraph">If the script output is to be presented alone in a paragraph.</param>
		/// <param name="Variables">Current variables.</param>
		/// <param name="XamlSettings">XAML Settings</param>
		public static async Task GenerateXamarinForms(object Result, XmlWriter Output, XamarinRenderingState State, bool AloneInParagraph,
			Variables Variables, XamlSettings XamlSettings)
		{
			string s;
			
			if (State.InLabel)
			{
				s = Result?.ToString();
				if (!string.IsNullOrEmpty(s))
					Paragraph.GenerateXamarinFormsSpan(Output, Result?.ToString() ?? string.Empty, State);

				return;
			}

			if (Result is Graph G)
			{
				PixelInformation Pixels = G.CreatePixels(Variables);
				byte[] Bin = Pixels.EncodeAsPng();

				s = "data:image/png;base64," + Convert.ToBase64String(Bin, 0, Bin.Length);

				await ImageContent.OutputXamarinForms(Output, new ImageSource()
				{
					Url = s,
					Width = Pixels.Width,
					Height = Pixels.Height
				});
			}
			else if (Result is SKImage Img)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();

					s = "data:image/png;base64," + Convert.ToBase64String(Bin, 0, Bin.Length);

					await ImageContent.OutputXamarinForms(Output, new ImageSource()
					{
						Url = s,
						Width = Img.Width,
						Height = Img.Height
					});
				}
			}
			else if (Result is Exception ex)
			{
				ex = Log.UnnestException(ex);

				if (ex is AggregateException ex2)
				{
					foreach (Exception ex3 in ex2.InnerExceptions)
					{
						Paragraph.GenerateXamarinFormsContentView(Output, State.TextAlignment, XamlSettings);
						Output.WriteStartElement("Label");
						Output.WriteAttributeString("LineBreakMode", "WordWrap");
						Output.WriteAttributeString("TextColor", "Red");
						Output.WriteValue(ex3.Message);
						Output.WriteEndElement();
						Output.WriteEndElement();
					}
				}
				else
				{
					if (AloneInParagraph)
						Paragraph.GenerateXamarinFormsContentView(Output, State.TextAlignment, XamlSettings);

					Output.WriteStartElement("Label");
					Output.WriteAttributeString("LineBreakMode", "WordWrap");
					Output.WriteAttributeString("TextColor", "Red");
					Output.WriteValue(ex.Message);
					Output.WriteEndElement();

					if (AloneInParagraph)
						Output.WriteEndElement();
				}
			}
			else
			{
				if (AloneInParagraph)
					Paragraph.GenerateXamarinFormsContentView(Output, State.TextAlignment, XamlSettings);

				Output.WriteStartElement("Label");
				Output.WriteAttributeString("LineBreakMode", "WordWrap");
				Output.WriteValue(Result.ToString());
				Output.WriteEndElement();

				if (AloneInParagraph)
					Output.WriteEndElement();
			}
		}

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		public override async Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State)
		{
			object Result = await this.EvaluateExpression();
			if (Result is null)
				return;

			if (this.aloneInParagraph)
				Output.WriteStartElement("paragraph");

			Output.WriteElementString("text", Result.ToString());

			if (this.aloneInParagraph)
				Output.WriteEndElement();
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement => true;

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		internal override bool OutsideParagraph => true;

		/// <summary>
		/// Baseline alignment
		/// </summary>
		internal override string BaselineAlignment => "Baseline";

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public override void Export(XmlWriter Output)
		{
			Output.WriteStartElement("Script");
			Output.WriteAttributeString("expression", this.expression.Script);
			Output.WriteAttributeString("aloneInParagraph", CommonTypes.Encode(this.aloneInParagraph));
			Output.WriteEndElement();
		}

		/// <summary>
		/// Tries to get a variable object, given its name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Variable">Variable, if found, or null otherwise.</param>
		/// <returns>If a variable with the corresponding name was found.</returns>
		public bool TryGetVariable(string Name, out Variable Variable)
		{
			switch (Name)
			{
				case "StartPosition":
					Variable = new Variable(Name, this.startPosition);
					return true;

				case "EndPosition":
					Variable = new Variable(Name, this.endPosition);
					return true;

				default:
					Variable = null;
					return false;
			}
		}

		/// <summary>
		/// If the collection contains a variable with a given name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <returns>If a variable with that name exists.</returns>
		public bool ContainsVariable(string Name)
		{
			return Name == "StartPosition" || Name == "EndPosition";
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is InlineScript x &&
				this.expression.Script == x.expression.Script &&
				this.aloneInParagraph == x.aloneInParagraph &&
				base.Equals(obj);
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = base.GetHashCode();
			int h2 = this.expression?.Script?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;
			h2 = this.aloneInParagraph.GetHashCode();

			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
