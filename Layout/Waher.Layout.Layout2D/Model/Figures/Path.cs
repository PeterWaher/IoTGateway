using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Figures.SegmentNodes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A path
	/// </summary>
	public class Path : Figure
	{
		private ISegment[] segments;

		/// <summary>
		/// A path
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Path(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Path";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);
			this.segments = this.GetSegments();
		}

		private ISegment[] GetSegments()
		{
			List<ISegment> Segments = null;

			foreach (ILayoutElement Child in this.Children)
			{
				if (Child is ISegment Segment)
				{
					if (Segments is null)
						Segments = new List<ISegment>();

					Segments.Add(Segment);
				}
				else
					throw new LayoutSyntaxException("Not a segment type: " + Child.Namespace + "#" + Child.LocalName);
			}

			return Segments?.ToArray();
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Path(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Path Dest)
				Dest.segments = Dest.GetSegments();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (!(this.segments is null))
			{
				PathState PathState = new PathState(this);

				foreach (ISegment Segment in this.segments)
					Segment.Measure(State, PathState);
			}
		}

	}
}
