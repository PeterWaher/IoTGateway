﻿using SkiaSharp;
using System.Threading.Tasks;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Turns the current direction right.
	/// </summary>
	public class TurnRight : Angle, ISegment
	{
		/// <summary>
		/// Turns the current direction right.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public TurnRight(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "TurnRight";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new TurnRight(Document, Parent);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public Task Measure(DrawingState State, PathState PathState)
		{
			if (this.defined)
				PathState.TurnRight(this.angle);
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		/// <param name="Path">Path being generated.</param>
		public Task Draw(DrawingState State, PathState PathState, SKPath Path)
		{
			if (this.defined)
				PathState.TurnRight(this.angle);
		
			return Task.CompletedTask;
		}
	}
}
