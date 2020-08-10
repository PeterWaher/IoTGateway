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
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			ICellLayout Measured = this.GetCellLayout(State);

			if (!(this.Children is null))
			{
				foreach (ILayoutElement Child in this.Children)
				{
					Child.Measure(State);
					if (!Child.IsVisible)
						continue;

					if (Child is IDynamicChildren DynamicChildren)
					{
						foreach (ILayoutElement Child2 in DynamicChildren.DynamicChildren)
							Measured.Add(Child2);
					}
					else
						Measured.Add(Child);
				}
			}

			this.measured = Measured.Align();
			this.Top = 0;
			this.Left = 0;
			this.Right = Measured.TotWidth;
			this.Bottom = Measured.TotHeight;
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
				Canvas.SetMatrix(M);
				Canvas.Translate((float)P.OffsetX, (float)P.OffsetY);
				P.Element.Draw(State);
			}

			Canvas.SetMatrix(M);
		}

	}
}
