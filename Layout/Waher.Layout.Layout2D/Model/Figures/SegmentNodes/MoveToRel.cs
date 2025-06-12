﻿using SkiaSharp;
using System.Threading.Tasks;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Moves to a point, relative to the end of the last segment
	/// </summary>
	public class MoveToRel : MoveTo
	{
		/// <summary>
		/// Moves to a point, relative to the end of the last segment
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public MoveToRel(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "MoveToRel";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new MoveToRel(Document, Parent);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public override Task Measure(DrawingState State, PathState PathState)
		{
			if (this.defined)
				PathState.Add0(this.xCoordinate, this.yCoordinate);
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		/// <param name="Path">Path being generated.</param>
		public override Task Draw(DrawingState State, PathState PathState, SKPath Path)
		{
			if (this.defined)
				Path.MoveTo(PathState.Add0(this.xCoordinate, this.yCoordinate));
	
			return Task.CompletedTask;
		}
	}
}
