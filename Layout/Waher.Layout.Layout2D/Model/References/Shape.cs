using System;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Figures.SegmentNodes;

namespace Waher.Layout.Layout2D.Model.References
{
	/// <summary>
	/// Defines a shape for use elsewhere in the layout.
	/// </summary>
	public class Shape : LayoutContainer, ISegment
	{
		/// <summary>
		/// Defines a shape for use elsewhere in the layout.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Shape(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Shape";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Shape(Document, Parent);
		}

		/// <summary>
		/// Draws the shape
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void DrawShape(DrawingState State)
		{
			base.Draw(State);
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			// Don't draw a shape definition.
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public void Measure(DrawingState State, PathState PathState)
		{
			ILayoutElement[] Children = this.Children;

			if (!(Children is null))
			{
				foreach (ILayoutElement E in Children)
				{
					if (E.IsVisible && E is ISegment Segment)
						Segment.Measure(State, PathState);
				}
			}
		}

		/// <summary>
		/// Draws the segments of the path.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		/// <param name="Path">Path being generated.</param>
		public void Draw(DrawingState State, PathState PathState, SKPath Path)
		{
			ILayoutElement[] Children = this.Children;

			if (!(Children is null))
			{
				foreach (ILayoutElement E in Children)
				{
					if (E.IsVisible && E is ISegment Segment)
						Segment.Draw(State, PathState, Path);
				}
			}
		}

		/// <summary>
		/// Draws shape as a tail
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="DirectedElement">Directed element.</param>
		/// <param name="DefaultPen">Default pen, if any, null otherwise</param>
		/// <param name="DefaultFill">Default fill, if any, null otherwise.</param>
		public void DrawTail(DrawingState State, IDirectedElement DirectedElement,
			SKPaint DefaultPen, SKPaint DefaultFill)
		{
			if (DirectedElement.TryGetStart(out float X, out float Y, out float Direction))
				this.Draw(State, DefaultPen, DefaultFill, X, Y, Direction);
		}

		/// <summary>
		/// Draws shape as a head
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="DirectedElement">Directed element.</param>
		/// <param name="DefaultPen">Default pen, if any, null otherwise</param>
		/// <param name="DefaultFill">Default fill, if any, null otherwise.</param>
		public void DrawHead(DrawingState State, IDirectedElement DirectedElement,
			SKPaint DefaultPen, SKPaint DefaultFill)
		{
			if (DirectedElement.TryGetEnd(out float X, out float Y, out float Direction))
				this.Draw(State, DefaultPen, DefaultFill, X, Y, Direction);
		}

		/// <summary>
		/// Draws the shape at a given position &amp; direction.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="DefaultPen">Default pen, if any, null otherwise</param>
		/// <param name="DefaultFill">Default fill, if any, null otherwise.</param>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		/// <param name="Direction"></param>
		public void Draw(DrawingState State, SKPaint DefaultPen, SKPaint DefaultFill, 
			float X, float Y, float Direction)
		{
			ILayoutElement[] Children = this.Children;
			if (!(Children is null))
			{
				SKPaint PenBak = null;
				SKPaint FillBak = null;
				SKPath Path = null;
				PathState PathState = null;
				bool HasPenBak = false;
				bool HasFillBak = false;

				if (!(DefaultPen is null))
				{
					PenBak = State.ShapePen;
					State.ShapePen = DefaultPen;
					HasPenBak = true;
				}

				if (!(DefaultFill is null))
				{
					FillBak = State.ShapeFill;
					State.ShapeFill = DefaultFill;
					HasFillBak = true;
				}

				try
				{
					foreach (ILayoutElement Element in Children)
					{
						if (Element is ISegment Segment)
						{
							if (Path is null)
							{
								Path = new SKPath();
								PathState = new PathState(null, Path, false, false);
							}

							PathState.Set0(X, Y);
							PathState.TurnTowards(Direction);
							Path.MoveTo(X, Y);

							Segment.Draw(State, PathState, Path);
						}
						else
						{
							if (!(Path is null))
							{
								PathState.FlushSpline();

								if (!(DefaultFill is null))
									State.Canvas.DrawPath(Path, DefaultFill);

								if (!(DefaultPen is null))
									State.Canvas.DrawPath(Path, DefaultPen);

								Path.Dispose();
								Path = null;
								PathState = null;
							}

							Element.Draw(State);
						}
					}

					if (!(Path is null))
					{
						PathState.FlushSpline();

						if (!(DefaultFill is null))
							State.Canvas.DrawPath(Path, DefaultFill);

						if (!(DefaultPen is null))
							State.Canvas.DrawPath(Path, DefaultPen);
					}
				}
				finally
				{
					if (HasPenBak)
						State.ShapePen = PenBak;

					if (HasFillBak)
						State.ShapeFill = FillBak;

					Path?.Dispose();
				}
			}
		}

	}
}
