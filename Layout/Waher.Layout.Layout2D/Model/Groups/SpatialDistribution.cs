using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Abstract base class of elements that do spatial distribution of children.
	/// </summary>
	public abstract class SpatialDistribution : LayoutContainer
	{
		/// <summary>
		/// Measured children.
		/// </summary>
		protected Padding[] measured;

		/// <summary>
		/// Abstract base class of elements that do spatial distribution of children.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public SpatialDistribution(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Gets a cell layout object that will be responsible for laying out cells.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Cell layout object.</returns>
		public abstract ICellLayout GetCellLayout(DrawingState State);

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasureDimensions(DrawingState State)
		{
			base.MeasureDimensions(State);

			this.cellLayout = this.GetCellLayout(State);

			if (this.HasChildren)
			{
				foreach (ILayoutElement Child in this.Children)
				{
					if (!Child.IsVisible)
						continue;

					if (Child is IDynamicChildren DynamicChildren)
					{
						foreach (ILayoutElement Child2 in DynamicChildren.DynamicChildren)
							this.cellLayout.Add(Child2);
					}
					else
						this.cellLayout.Add(Child);
				}
			}

			this.Left = 0;
			this.Top = 0;
			this.Right = null;
			this.Bottom = null;
			this.Width = this.cellLayout.TotWidth;
			this.Height = this.cellLayout.TotHeight;
		}

		private ICellLayout cellLayout = null;

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			// Don't call base method. Cell Layout calls MeasurePositions on children.
			
			this.cellLayout?.MeasurePositions(State);
			this.measured = this.cellLayout?.Align();
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			SKCanvas Canvas = State.Canvas;
			SKMatrix M = Canvas.TotalMatrix;

			foreach (Padding P in this.measured)
			{
				Canvas.Translate(P.OffsetX, P.OffsetY);
				P.Element.Draw(State);
				Canvas.SetMatrix(M);
			}
		}

	}
}
