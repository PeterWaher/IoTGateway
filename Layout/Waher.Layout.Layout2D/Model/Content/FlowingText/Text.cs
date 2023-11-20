using SkiaSharp;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Represents a segment of text in flowing text.
	/// </summary>
	public class Text : LayoutElement, IFlowingText
	{
		private StringAttribute text;

		/// <summary>
		/// Represents a segment of text in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Text(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Text";

		/// <summary>
		/// Text
		/// </summary>
		public StringAttribute TextAttribute
		{
			get => this.text;
			set => this.text = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.text = new StringAttribute(Input, "text", this.Document);
			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.text?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Text(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Text Dest)
				Dest.text = this.text?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		public async Task MeasureSegments(List<Segment> Segments, DrawingState State)
		{
			EvaluationResult<string> Text = await this.text.TryEvaluate(State.Session);
			if (Text.Ok)
				AddSegments(Segments, Text.Result, State);
		}

		/// <summary>
		/// Reduces text into segments.
		/// </summary>
		/// <param name="Segments">List of segments</param>
		/// <param name="Text">Text to reduce to segments and add to the list of segments.</param>
		/// <param name="State">Drawing state.</param>
		public static void AddSegments(List<Segment> Segments, string Text, DrawingState State)
		{
			StringBuilder sb = new StringBuilder();
			bool Empty = true;

			foreach (char ch in Text)
			{
				if (char.IsWhiteSpace(ch))
				{
					if (!Empty)
					{
						AddSegment(Segments, sb.ToString(), true, State);
						sb.Clear();
						Empty = true;
					}
					else
						AddSegment(Segments, string.Empty, true, State);
				}
				else
				{
					sb.Append(ch);
					Empty = false;
				}
			}

			if (!Empty)
				AddSegment(Segments, sb.ToString(), false, State);
		}

		private static void AddSegment(List<Segment> Segments, string Text, bool SpaceAfter, DrawingState State)
		{
			Segment Segment = new Segment()
			{
				Text = Text,
				Paint = State.Text,
				Font = State.Font,
				SpaceAfter = SpaceAfter,
				DeltaY = 0
			};

			SKRect Bounds = new SKRect();
			State.Text.MeasureText(Text, ref Bounds);
			Segment.Bounds = Bounds;

			Bounds = new SKRect();
			if (Segment.SpaceAfter)
			{
				State.Text.MeasureText("x x", ref Bounds);

				SKRect Bounds2 = new SKRect();
				State.Text.MeasureText("xx", ref Bounds2);

				Bounds.Left -= Bounds2.Left;
				Bounds.Right -= Bounds2.Right;
			}
			Segment.SpaceBounds = Bounds;

			Segments.Add(Segment);
		}

	}
}
