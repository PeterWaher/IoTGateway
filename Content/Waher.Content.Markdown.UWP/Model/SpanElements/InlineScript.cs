using System;
using System.Collections.Generic;
#if !WINDOWS_UWP
using System.Drawing;
using System.Drawing.Imaging;
#endif
using System.IO;
using System.Text;
using System.Xml;
using Waher.Script;
using Waher.Script.Exceptions;
#if !WINDOWS_UWP
using Waher.Script.Graphs;
#endif
using Waher.Script.Objects;

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

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		public override void GenerateHTML(StringBuilder Output)
		{
            object Result = this.expression.Evaluate(this.variables);
            if (Result == null)
                return;

			string s;

#if !WINDOWS_UWP
			Graph G = Result as Graph;
			Image Img;

			if (G != null)
			{
				GraphSettings GraphSettings = new GraphSettings();
				Variable v;
				object Obj;
				double d;

				if (this.variables.TryGetVariable("GraphWidth", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
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

				Bitmap Bmp = G.CreateBitmap(GraphSettings);
				MemoryStream ms = new MemoryStream();
				Bmp.Save(ms, ImageFormat.Png);
				byte[] Data = ms.GetBuffer();
				s = System.Convert.ToBase64String(Data, 0, (int)ms.Position, Base64FormattingOptions.None);
				s = "<img border=\"2\" width=\"" + GraphSettings.Width.ToString() + "\" height=\"" + GraphSettings.Height.ToString() +
					"\" src=\"data:image/png;base64," + s + "\" />";

				if (this.aloneInParagraph)
					s = "<figure>" + s + "</figure>";
			}
			else if ((Img = Result as Image) != null)
			{
				string ContentType;
				byte[] Data = InternetContent.Encode(Img, Encoding.UTF8, out ContentType);

				s = System.Convert.ToBase64String(Data, 0, Data.Length, Base64FormattingOptions.None);
				s = "<img border=\"2\" width=\"" + Img.Width.ToString() + "\" height=\"" + Img.Height.ToString() +
					"\" src=\"data:" + ContentType + ";base64," + s + "\" />";

				if (this.aloneInParagraph)
					s = "<figure>" + s + "</figure>";
			}
			else
#endif
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
            object Result = this.expression.Evaluate(this.variables);
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
			object Result = this.expression.Evaluate(this.variables);
			if (Result == null)
				return;

#if !WINDOWS_UWP
			Graph G = Result as Graph;
			Image Img;
			string s;

			if (G != null)
			{
				GraphSettings GraphSettings = new GraphSettings();
				Variable v;
				object Obj;
				double d;

				if (this.variables.TryGetVariable("GraphWidth", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
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

				using (Bitmap Bmp = G.CreateBitmap(GraphSettings))
				{
					MemoryStream ms = new MemoryStream();
					Bmp.Save(ms, ImageFormat.Png);
					byte[] Data = ms.GetBuffer();
					s = "data:image/png;base64," + System.Convert.ToBase64String(Data, 0, (int)ms.Position, Base64FormattingOptions.None);

					// TODO: WPF does not support data URI scheme. Change to local temporary file.

					Output.WriteStartElement("Image");
					Output.WriteAttributeString("Source", s);
					Output.WriteAttributeString("Width", Bmp.Width.ToString());
					Output.WriteAttributeString("Height", Bmp.Height.ToString());
					Output.WriteEndElement();
				}
			}
			else if ((Img = Result as Image) != null)
			{
				string ContentType;
				byte[] Data = InternetContent.Encode(Img, Encoding.UTF8, out ContentType);

				s = "data:" + ContentType + ";base64," + System.Convert.ToBase64String(Data, 0, Data.Length, Base64FormattingOptions.None);

				// TODO: WPF does not support data URI scheme. Change to local temporary file.

				Output.WriteStartElement("Image");
				Output.WriteAttributeString("Source", s);
				Output.WriteAttributeString("Width", Img.Width.ToString());
				Output.WriteAttributeString("Height", Img.Height.ToString());
				Output.WriteEndElement();
			}
			else
#endif
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

	}
}
