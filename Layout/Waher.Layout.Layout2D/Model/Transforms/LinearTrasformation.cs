using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Transforms
{
	/// <summary>
	/// Base abstract class for linear transforms.
	/// </summary>
	public abstract class LinearTrasformation : LayoutContainer
	{
		/// <summary>
		/// Base abstract class for linear transforms.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public LinearTrasformation(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool MeasureDimensions(DrawingState State)
		{
			this.Width = this.Height = this.Left = this.Top = this.Right = this.Bottom = null;

			return base.MeasureDimensions(State);
		}

	}
}
