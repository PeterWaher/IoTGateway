using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using SkiaSharp;
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
	public class InlineScript : MarkdownElement
	{
		private readonly Expression expression;
		private readonly Variables variables;
		private readonly bool aloneInParagraph;

		/// <summary>
		/// Inline source code.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		/// <param name="Expression">Expression.</param>
		/// <param name="Variables">Collection of variables to use when executing the script.</param>
		/// <param name="AloneInParagraph">If construct stands alone in a paragraph.</param>
		public InlineScript(MarkdownDocument Document, Expression Expression, Variables Variables, bool AloneInParagraph)
			: base(Document)
		{
			this.expression = Expression;
			this.variables = Variables;
			this.aloneInParagraph = AloneInParagraph;
		}

		/// <summary>
		/// Expression
		/// </summary>
		public Expression Expresion
		{
			get { return this.expression; }
		}

		/// <summary>
		/// If the element is alone in a paragraph.
		/// </summary>
		public bool AloneInParagraph
		{
			get { return this.aloneInParagraph; }
		}

		private object EvaluateExpression()
		{
			try
			{
				return this.expression.Evaluate(this.variables);
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				this.Document.CheckException(ex);

				return ex;
			}
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
			object Result = this.EvaluateExpression();
			if (Result == null)
				return;

			GenerateHTML(Result, Output, this.aloneInParagraph, this.variables);
		}

		/// <summary>
		/// Generates HTML from Script output.
		/// </summary>
		/// <param name="Result">Script output.</param>
		/// <param name="Output">HTML output.</param>
		/// <param name="AloneInParagraph">If the script output is to be presented alone in a paragraph.</param>
		/// <param name="Variables">Current variables.</param>
		public static void GenerateHTML(object Result, StringBuilder Output, bool AloneInParagraph, Variables Variables)
		{
			if (Result is Graph G)
			{
				using (SKImage Bmp = G.CreateBitmap(Variables, out GraphSettings GraphSettings))
				{
					SKData Data = Bmp.Encode(SKEncodedImageFormat.Png, 100);
					byte[] Bin = Data.ToArray();

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

					Data.Dispose();
				}
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
						Output.Append("<p><font style=\"color:red\">");
						Output.Append(XML.HtmlValueEncode(ex3.Message));
						Output.AppendLine("</font></p>");
					}
				}
				else
				{
					if (AloneInParagraph)
						Output.Append("<p>");

					Output.Append("<font style=\"color:red\">");
					Output.Append(XML.HtmlValueEncode(ex.Message));
					Output.Append("</font>");

					if (AloneInParagraph)
						Output.Append("</p>");
				}
			}
			else if (Result is ObjectMatrix M && M.ColumnNames != null)
			{
				Output.Append("<table><thead>");

				foreach (string s2 in M.ColumnNames)
				{
					Output.Append("<th>");
					Output.Append(FormatText(XML.HtmlValueEncode(s2)));
					Output.Append("</th>");
				}

				Output.Append("</thead><tbody>");

				int x, y;

				for (y = 0; y < M.Rows; y++)
				{
					Output.Append("<tr>");

					for (x = 0; x < M.Columns; x++)
					{
						Output.Append("<td>");

						object Item = M.GetElement(x, y).AssociatedObjectValue;
						if (Item != null)
						{
							if (Item is string s2)
								Output.Append(FormatText(XML.HtmlValueEncode(s2)));
							else
								Output.Append(FormatText(XML.HtmlValueEncode(Expression.ToString(Item))));
						}

						Output.Append("</td>");
					}

					Output.Append("</tr>");
				}

				Output.Append("</tbody></table>");
			}
			else
			{
				if (AloneInParagraph)
					Output.Append("<p>");

				Output.Append(XML.HtmlValueEncode(Result.ToString()));

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
		public override void GeneratePlainText(StringBuilder Output)
		{
			object Result = this.EvaluateExpression();
			if (Result == null)
				return;

			Output.Append(Result.ToString());

			if (this.aloneInParagraph)
			{
				Output.AppendLine();
				Output.AppendLine();
			}
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment)
		{
			object Result = this.EvaluateExpression();
			if (Result == null)
				return;

			SKImage Img;
			string s;

			if (Result is Graph G)
			{
				using (SKImage Bmp = G.CreateBitmap(this.variables))
				{
					SKData Data = Bmp.Encode(SKEncodedImageFormat.Png, 100);
					byte[] Bin = Data.ToArray();

					s = "data:image/png;base64," + System.Convert.ToBase64String(Bin, 0, Bin.Length);

					// TODO: WPF does not support data URI scheme. Change to local temporary file.

					Output.WriteStartElement("Image");
					Output.WriteAttributeString("Source", s);
					Output.WriteAttributeString("Width", Bmp.Width.ToString());
					Output.WriteAttributeString("Height", Bmp.Height.ToString());
					Output.WriteEndElement();

					Data.Dispose();
				}
			}
			else if ((Img = Result as SKImage) != null)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();

					s = "data:image/png;base64," + System.Convert.ToBase64String(Bin, 0, Bin.Length);

					// TODO: WPF does not support data URI scheme. Change to local temporary file.

					Output.WriteStartElement("Image");
					Output.WriteAttributeString("Source", s);
					Output.WriteAttributeString("Width", Img.Width.ToString());
					Output.WriteAttributeString("Height", Img.Height.ToString());
					Output.WriteEndElement();
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
						Output.WriteAttributeString("Margin", Settings.ParagraphMargins);

						if (TextAlignment != TextAlignment.Left)
							Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());

						Output.WriteAttributeString("Foreground", "Red");
						Output.WriteValue(ex.Message);
						Output.WriteEndElement();
					}
				}
				else
				{
					if (this.aloneInParagraph)
					{
						Output.WriteStartElement("TextBlock");
						Output.WriteAttributeString("TextWrapping", "Wrap");
						Output.WriteAttributeString("Margin", Settings.ParagraphMargins);
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
				if (this.aloneInParagraph)
				{
					Output.WriteStartElement("TextBlock");
					Output.WriteAttributeString("TextWrapping", "Wrap");
					Output.WriteAttributeString("Margin", Settings.ParagraphMargins);
					if (TextAlignment != TextAlignment.Left)
						Output.WriteAttributeString("TextAlignment", TextAlignment.ToString());
				}

				Output.WriteValue(Result.ToString());

				if (this.aloneInParagraph)
					Output.WriteEndElement();
			}
		}

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		internal override bool InlineSpanElement
		{
			get { return true; }
		}

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		internal override bool OutsideParagraph
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Baseline alignment
		/// </summary>
		internal override string BaselineAlignment
		{
			get
			{
				return "Baseline";
			}
		}

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

	}
}
