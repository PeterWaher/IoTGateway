using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Backgrounds;
using Waher.Layout.Layout2D.Model.Figures.SegmentNodes;
using Waher.Layout.Layout2D.Model.References;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A path
	/// </summary>
	public class Path : Figure, ISegment, IDirectedElement
	{
		private ISegment[] segments;
		private StringAttribute head;
		private StringAttribute tail;
		private StringAttribute shapeFill;

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

			this.head = new StringAttribute(Input, "head");
			this.tail = new StringAttribute(Input, "tail");
			this.shapeFill = new StringAttribute(Input, "shapeFill");

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
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.head.Export(Output);
			this.tail.Export(Output);
			this.shapeFill.Export(Output);
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
			{
				Dest.head = this.head.CopyIfNotPreset();
				Dest.tail = this.tail.CopyIfNotPreset();
				Dest.shapeFill = this.shapeFill.CopyIfNotPreset();

				Dest.segments = Dest.GetSegments();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			PathState PathState = new PathState(this, null, false, false);

			if (this.head.TryEvaluate(State.Session, out string RefId) &&
				State.TryGetElement(RefId, out ILayoutElement Element) &&
				Element is Shape Shape)
			{
				this.headElement = Shape;
			}

			if (this.tail.TryEvaluate(State.Session, out RefId) &&
				State.TryGetElement(RefId, out Element) &&
				Element is Shape Shape2)
			{
				this.tailElement = Shape2;
			}

			if (this.shapeFill.TryEvaluate(State.Session, out RefId) &&
				State.TryGetElement(RefId, out Element) &&
				Element is Background Background)
			{
				this.shapeFiller = Background;
			}

			if (!(this.segments is null))
			{
				foreach (ISegment Segment in this.segments)
					Segment.Measure(State, PathState);
			}
		}

		private Shape headElement;
		private Shape tailElement;
		private Background shapeFiller;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			base.Draw(State);

			if (!this.TryGetFill(State, out SKPaint Fill))
				Fill = null;

			if (!this.TryGetPen(State, out SKPaint Pen))
				Pen = null;

			bool CalcStart = !(this.tailElement is null);
			bool CalcEnd = !(this.headElement is null);

			if (!(this.segments is null))
			{
				using (SKPath Path = new SKPath())
				{
					PathState PathState = new PathState(this, Path, CalcStart, CalcEnd);

					this.Draw(State, PathState, Path);
					
					PathState.FlushSpline();

					if (!(Fill is null))
						State.Canvas.DrawPath(Path, Fill);

					if (!(Pen is null))
						State.Canvas.DrawPath(Path, Pen);
				}
			}

			if (CalcStart || CalcEnd)
			{
				if (!(this.shapeFiller is null))
					Fill = this.shapeFiller.Paint;

				this.tailElement?.DrawTail(State, this, Pen, Fill);
				this.headElement?.DrawHead(State, this, Pen, Fill);
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public void Measure(DrawingState State, PathState PathState)
		{
			if (!(this.segments is null))
			{
				foreach (ISegment Segment in this.segments)
					Segment.Measure(State, PathState);
			}

			this.hasStart = false;
			this.hasEnd = false;
		}

		/// <summary>
		/// Draws the segments of the path.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		/// <param name="Path">Path being generated.</param>
		public void Draw(DrawingState State, PathState PathState, SKPath Path)
		{
			if (!(this.segments is null))
			{
				bool CalcStart = PathState.CalcStart;

				foreach (ISegment Segment in this.segments)
				{
					if (Segment.IsVisible)
					{
						Segment.Draw(State, PathState, Path);

						if (CalcStart &&
							Segment is IDirectedElement DirectedElement &&
							DirectedElement.TryGetStart(out this.startX, out this.startY, out this.startDirection))
						{
							CalcStart = false;
							this.hasStart = true;
						}
					}
				}

				if (PathState.CalcEnd)
				{
					int i = this.segments.Length;

					while (i-- > 0)
					{
						if (this.segments[i] is IDirectedElement DirectedElement &&
							DirectedElement.TryGetEnd(out this.endX, out this.endY, out this.endDirection))
						{
							this.hasEnd = true;
							break;
						}
					}
				}
			}
		}

		private float startX;
		private float startY;
		private float startDirection;
		private bool hasStart;
		private float endX;
		private float endY;
		private float endDirection;
		private bool hasEnd;

		/// <summary>
		/// Tries to get start position and initial direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Initial direction.</param>
		/// <returns>If a start position was found.</returns>
		public bool TryGetStart(out float X, out float Y, out float Direction)
		{
			if (this.hasStart)
			{
				X = this.startX;
				Y = this.startY;
				Direction = this.startDirection;

				return true;
			}
			else
			{
				X = Y = Direction = 0;
				return false;
			}
		}

		/// <summary>
		/// Tries to get end position and terminating direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Terminating direction.</param>
		/// <returns>If a terminating position was found.</returns>
		public bool TryGetEnd(out float X, out float Y, out float Direction)
		{
			if (this.hasEnd)
			{
				X = this.endX;
				Y = this.endY;
				Direction = this.endDirection;

				return true;
			}
			else
			{
				X = Y = Direction = 0;
				return false;
			}
		}

	}
}
