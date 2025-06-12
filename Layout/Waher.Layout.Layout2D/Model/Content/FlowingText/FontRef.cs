﻿using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Fonts;
using Waher.Runtime.Collections;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Represents text using a specific font in flowing text.
	/// </summary>
	public class FontRef : EmbeddedText
	{
		private StringAttribute font;

		/// <summary>
		/// Represents text using a specific font in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public FontRef(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "FontRef";

		/// <summary>
		/// Font
		/// </summary>
		public StringAttribute FontAttribute
		{
			get => this.font;
			set => this.font = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.font = new StringAttribute(Input, "font", this.Document);
			
			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.font?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new FontRef(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is FontRef Dest)
				Dest.font = this.font?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		public override async Task MeasureSegments(ChunkedList<Segment> Segments, DrawingState State)
		{
			EvaluationResult<string> FontId = await this.font.TryEvaluate(State.Session);

			if (FontId.Ok && 
				this.Document.TryGetElement(FontId.Result, out ILayoutElement E) && 
				E is Font Font)
			{
				SKFont Bak = State.Font;
				SKPaint Bak2 = State.Text;

				State.Font = Font.FontDef;
				State.Text = Font.Text;

				await base.MeasureSegments(Segments, State);

				State.Font = Bak;
				State.Text = Bak2;
			}
			else
				await base.MeasureSegments(Segments, State);
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.font?.ExportState(Output);
		}
	}
}
