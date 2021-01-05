using System;
using System.Collections.Generic;
using System.Resources;
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
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool DoMeasureDimensions(DrawingState State)
		{
			bool Relative = base.DoMeasureDimensions(State);

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

				this.cellLayout.Flush();
			}

			return Relative;
		}

		/// <summary>
		/// Called when dimensions have been measured.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="Relative">If layout contains relative sizes and dimensions should be recalculated.</param>
		public override void AfterMeasureDimensions(DrawingState State, ref bool Relative)
		{
			base.AfterMeasureDimensions(State, ref Relative);

			this.cellLayout?.Distribute(false);

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
			base.MeasurePositions(State);
			
			this.cellLayout?.MeasurePositions(State);
			this.cellLayout?.Distribute(true);
			this.measured = this.cellLayout?.Align();
		}

		/// <summary>
		/// If children positions are to be measured.
		/// </summary>
		protected override bool MeasureChildrenPositions => false;  // Cell Layout calls MeasurePositions on children.

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
