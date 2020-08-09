using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Lays out elements flexibly, first horizontally, then vertically.
	/// </summary>
	public class FlexibleHorizontalCells : ICellLayout
	{
		private readonly List<Padding> currentRow = new List<Padding>();
		private readonly List<Tuple<double, double, Padding[]>> rows = new List<Tuple<double, double, Padding[]>>();
		private readonly Variables session;
		private readonly double limitWidth;
		private readonly HorizontalDirection horizontalDirection;
		private readonly VerticalDirection verticalDirection;
		private readonly HorizontalAlignment horizontalAlignment;
		private double maxWidth = 0;
		private double maxHeight = 0;
		private double x = 0;
		private double y = 0;

		/// <summary>
		/// Lays out elements flexibly, first horizontally, then vertically.
		/// </summary>
		/// <param name="Session">Current session</param>
		/// <param name="WidthLimit">Width limit of area</param>
		/// <param name="HorizontalDirection">Horizontal direction</param>
		/// <param name="VerticalDirection">Vertical direction</param>
		/// <param name="HorizontalAlignment">Horizontal alignment of cells in layout.</param>
		public FlexibleHorizontalCells(Variables Session, double WidthLimit,
			HorizontalDirection HorizontalDirection, VerticalDirection VerticalDirection,
			HorizontalAlignment HorizontalAlignment)
		{
			this.session = Session;
			this.limitWidth = WidthLimit;
			this.horizontalDirection = HorizontalDirection;
			this.verticalDirection = VerticalDirection;
			this.horizontalAlignment = HorizontalAlignment;
		}

		/// <summary>
		/// Adds a cell to the layout.
		/// </summary>
		/// <param name="Element">Cell element</param>
		public void Add(ILayoutElement Element)
		{
			double Width = Element.Right - Element.Left;
			double Height = Element.Bottom - Element.Top;

			if (this.x + Width > this.limitWidth)
				this.Flush();

			this.currentRow.Add(new Padding(Element, -Element.Left, -Element.Top));
			this.x += Width;

			if (Height > this.maxHeight)
				this.maxHeight = Height;
		}

		private void Flush()
		{
			if (this.currentRow.Count > 0)
			{
				this.rows.Add(new Tuple<double, double, Padding[]>(this.x, this.maxHeight, this.currentRow.ToArray()));
				this.currentRow.Clear();
				this.y += this.maxHeight;

				if (this.x > this.maxWidth)
					this.maxWidth = x;
			}

			this.maxHeight = 0;
			this.x = 0;
		}

		/// <summary>
		/// Total width of layout
		/// </summary>
		public double TotWidth => this.maxWidth;

		/// <summary>
		/// Total height of layout
		/// </summary>
		public double TotHeight => this.y;

		/// <summary>
		/// Aligns cells and returns an array of padded cells.
		/// </summary>
		/// <returns>Array of padded cells.</returns>
		public Padding[] Align()
		{
			List<Padding> Result = new List<Padding>();
			double X;
			double Y = this.verticalDirection == VerticalDirection.TopDown ? 0 : this.y;
			double Diff;
			this.Flush();

			foreach (Tuple<double, double, Padding[]> Row in this.rows)
			{
				Diff = this.maxWidth - Row.Item1;
				switch (this.horizontalAlignment)
				{
					case HorizontalAlignment.Left:
					default:
						if (this.horizontalDirection == HorizontalDirection.LeftRight)
							X = 0;
						else
							X = this.maxWidth - Diff;
						break;

					case HorizontalAlignment.Center:
						if (this.horizontalDirection == HorizontalDirection.LeftRight)
							X = Diff * 0.5;
						else
							X = this.maxWidth - Diff * 0.5;
						break;

					case HorizontalAlignment.Right:
						if (this.horizontalDirection == HorizontalDirection.LeftRight)
							X = Diff;
						else
							X = this.maxWidth;
						break;
				}

				if (this.verticalDirection == VerticalDirection.BottomUp)
					Y -= Row.Item2;

				foreach (Padding P in Row.Item3)
				{
					double Width = P.Element.Right - P.Element.Left;
					double Height = P.Element.Bottom - P.Element.Top;

					if (this.horizontalDirection == HorizontalDirection.RightLeft)
						X -= Width;

					P.OffsetX += X;
					P.OffsetY += Y;
					P.AlignedMeasuredCell(null, Row.Item2, this.session);
					Result.Add(P);

					if (this.horizontalDirection == HorizontalDirection.LeftRight)
						X += Width;
				}

				if (this.verticalDirection == VerticalDirection.TopDown)
					Y += Row.Item2;
			}

			return Result.ToArray();
		}
	}
}
