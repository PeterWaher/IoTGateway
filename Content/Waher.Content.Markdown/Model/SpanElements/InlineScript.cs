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

namespace Waher.Content.Markdown.Model.SpanElements
{
	/// <summary>
	/// Inline source code.
	/// </summary>
	public class InlineScript : MarkdownElement
	{
		private Expression expression;
		private Variables variables;
		private bool aloneInParagraph;

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

			SKImage Img;
			string s;

			if (Result is Graph G)
			{
				GraphSettings GraphSettings = new GraphSettings();
				object Obj;
				double d;

				if (this.variables.TryGetVariable("GraphWidth", out Variable v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
				{
					GraphSettings.Width = (int)Math.Round(d);
					GraphSettings.MarginLeft = (int)Math.Round(15 * d / 640);
					GraphSettings.MarginRight = GraphSettings.MarginLeft;
				}
				else if (!this.variables.ContainsVariable("GraphWidth"))
					this.variables["GraphWidth"] = (double)GraphSettings.Width;


				if (this.variables.TryGetVariable("GraphHeight", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
				{
					GraphSettings.Height = (int)Math.Round(d);
					GraphSettings.MarginTop = (int)Math.Round(15 * d / 480);
					GraphSettings.MarginBottom = GraphSettings.MarginTop;
					GraphSettings.LabelFontSize = 12 * d / 480;
				}
				else if (!this.variables.ContainsVariable("GraphHeight"))
					this.variables["GraphHeight"] = (double)GraphSettings.Height;

				using (SKImage Bmp = G.CreateBitmap(GraphSettings))
				{
					SKData Data = Bmp.Encode(SKEncodedImageFormat.Png, 100);
					byte[] Bin = Data.ToArray();

					s = System.Convert.ToBase64String(Bin, 0, Bin.Length);
					s = "<img border=\"2\" width=\"" + GraphSettings.Width.ToString() + "\" height=\"" + GraphSettings.Height.ToString() +
						"\" src=\"data:image/png;base64," + s + "\" />";

					if (this.aloneInParagraph)
						s = "<figure>" + s + "</figure>";

					Data.Dispose();
				}
			}
			else if ((Img = Result as SKImage) != null)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();

					s = System.Convert.ToBase64String(Bin, 0, Bin.Length);
					s = "<img border=\"2\" width=\"" + Img.Width.ToString() + "\" height=\"" + Img.Height.ToString() +
						"\" src=\"data:image/png;base64," + s + "\" />";

					if (this.aloneInParagraph)
						s = "<figure>" + s + "</figure>";
				}
			}
			else if (Result is Exception ex)
			{
				ex = Log.UnnestException(ex);

				if (ex is AggregateException ex2)
				{
					StringBuilder sb = new StringBuilder();

					foreach (Exception ex3 in ex2.InnerExceptions)
					{
						sb.Append("<p><font style=\"color:red\">");
						sb.Append(XML.HtmlValueEncode(ex3.Message));
						sb.AppendLine("</font></p>");
					}

					s = sb.ToString();
				}
				else
				{
					s = "<font style=\"color:red\">" + XML.HtmlValueEncode(ex.Message) + "</font>";

					if (this.aloneInParagraph)
						s = "<p>" + s + "</p>";
				}
			}
			else
			{
				s = XML.HtmlValueEncode(Result.ToString());

				if (this.aloneInParagraph)
					s = "<p>" + s + "</p>";
			}

			Output.Append(s);

			if (this.aloneInParagraph)
				Output.AppendLine();
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
				GraphSettings GraphSettings = new GraphSettings();
				object Obj;
				double d;

				if (this.variables.TryGetVariable("GraphWidth", out Variable v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
				{
					GraphSettings.Width = (int)Math.Round(d);
					GraphSettings.MarginLeft = (int)Math.Round(15 * d / 640);
					GraphSettings.MarginRight = GraphSettings.MarginLeft;
				}
				else if (!this.variables.ContainsVariable("GraphWidth"))
				{
					this.variables["GraphWidth"] = (double)Settings.DefaultGraphWidth;
					GraphSettings.MarginLeft = (int)Math.Round(15.0 * Settings.DefaultGraphWidth / 640);
					GraphSettings.MarginRight = GraphSettings.MarginLeft;
				}

				if (this.variables.TryGetVariable("GraphHeight", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
				{
					GraphSettings.Height = (int)Math.Round(d);
					GraphSettings.MarginTop = (int)Math.Round(15 * d / 480);
					GraphSettings.MarginBottom = GraphSettings.MarginTop;
					GraphSettings.LabelFontSize = 12 * d / 480;
				}
				else if (!this.variables.ContainsVariable("GraphHeight"))
				{
					this.variables["GraphHeight"] = (double)Settings.DefaultGraphHeight;
					GraphSettings.MarginTop = (int)Math.Round(15.0 * Settings.DefaultGraphHeight / 480);
					GraphSettings.MarginBottom = GraphSettings.MarginTop;
				}

				using (SKImage Bmp = G.CreateBitmap(GraphSettings))
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
