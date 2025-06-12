﻿using SkiaSharp;
using System.Threading.Tasks;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Draws a conic curve to a point, relative to the end of the last segment
	/// </summary>
	public class ConicToRel : ConicTo
	{
		/// <summary>
		/// Draws a conic curve to a point, relative to the end of the last segment
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public ConicToRel(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "ConicToRel";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new ConicToRel(Document, Parent);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public override Task Measure(DrawingState State, PathState PathState)
		{
			if (this.defined)
			{
				PathState.Add(this.xCoordinate, this.yCoordinate);
				PathState.Add(this.xCoordinate2, this.yCoordinate2);
			}
	
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
			{
				this.P0 = Path.LastPoint;
				this.P1 = PathState.Add(this.xCoordinate, this.yCoordinate);
				this.P2 = PathState.Add(this.xCoordinate2, this.yCoordinate2);
				Path.ConicTo(this.P1, this.P2, this.weight);
			}
		
			return Task.CompletedTask;
		}
	}
}
