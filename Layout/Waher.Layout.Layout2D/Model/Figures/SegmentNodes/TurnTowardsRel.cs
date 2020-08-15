using System;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Turns the current direction towards a point, relative to the current point.
	/// </summary>
	public class TurnTowardsRel : Point, ISegment
	{
		/// <summary>
		/// Turns the current direction towards a point, relative to the current point.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public TurnTowardsRel(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "TurnTowardsRel";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new TurnTowardsRel(Document, Parent);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public void Measure(DrawingState State, PathState PathState)
		{
			if (this.defined)
				PathState.TurnTowardsRel(this.xCoordinate, this.yCoordinate);
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		/// <param name="Path">Path being generated.</param>
		public void Draw(DrawingState State, PathState PathState, SKPath Path)
		{
			if (this.defined)
				PathState.TurnTowardsRel(this.xCoordinate, this.yCoordinate);
		}
	}
}
