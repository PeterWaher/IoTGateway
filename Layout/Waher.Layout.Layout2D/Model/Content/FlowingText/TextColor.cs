using SkiaSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Represents a text color segment in flowing text.
	/// </summary>
	public class TextColor : EmbeddedText
	{
		private ColorAttribute color;

		/// <summary>
		/// Represents a text color segment in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public TextColor(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "TextColor";

		/// <summary>
		/// Color
		/// </summary>
		public ColorAttribute ColorAttribute
		{
			get => this.color;
			set => this.color = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.color = new ColorAttribute(Input, "color", this.Document);
			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.color?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new TextColor(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is TextColor Dest)
				Dest.color = this.color?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		public override async Task MeasureSegments(List<Segment> Segments, DrawingState State)
		{
			EvaluationResult<SKColor> Color = await this.color.TryEvaluate(State.Session);

			if (Color.Ok)
			{
				SKPaint Bak = State.Text;
			
				State.Text = State.Text.Clone();
				State.Text.Color = Color.Result;

				await base.MeasureSegments(Segments, State);

				State.Text = Bak;
			}
			else
				await base.MeasureSegments(Segments, State);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.color?.ExportState(Output);
		}
	}
}
