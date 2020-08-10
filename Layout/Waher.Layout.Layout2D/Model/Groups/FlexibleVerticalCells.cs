using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Lays out elements flexibly, first vertically, then horizontally.
	/// </summary>
	public class FlexibleVerticalCells : ICellLayout
	{
		private readonly List<Padding> currentColumn = new List<Padding>();
		private readonly List<Tuple<double, double, Padding[]>> columns = new List<Tuple<double, double, Padding[]>>();
		private readonly Variables session;
		private readonly double limitHeight;
		private readonly HorizontalDirection horizontalDirection;
		private readonly VerticalDirection verticalDirection;
		private readonly VerticalAlignment verticalAlignment;
		private double maxWidth = 0;
		private double maxHeight = 0;
		private double x = 0;
		private double y = 0;

		/// <summary>
		/// Lays out elements flexibly, first vertically, then horizontally.
		/// </summary>
		/// <param name="Session">Current session</param>
		/// <param name="WidthLimit">Width limit of area</param>
		/// <param name="HorizontalDirection">Horizontal direction</param>
		/// <param name="VerticalDirection">Vertical direction</param>
		/// <param name="VerticalAlignment">Vertical alignment of cells in layout.</param>
		public FlexibleVerticalCells(Variables Session, double WidthLimit,
			HorizontalDirection HorizontalDirection, VerticalDirection VerticalDirection,
			VerticalAlignment VerticalAlignment)
		{
			this.session = Session;
			this.limitHeight = WidthLimit;
			this.horizontalDirection = HorizontalDirection;
			this.verticalDirection = VerticalDirection;
			this.verticalAlignment = VerticalAlignment;
		}

		/// <summary>
		/// Adds a cell to the layout.
		/// </summary>
		/// <param name="Element">Cell element</param>
		public void Add(ILayoutElement Element)
		{
			double Width = Element.Width;
			double Height = Element.Height;

			if (this.y + Height > this.limitHeight)
				this.Flush();

			this.currentColumn.Add(new Padding(Element, -Element.Left, -Element.Top));
			this.y += Height;

			if (Width > this.maxWidth)
				this.maxWidth = Width;
		}

		private void Flush()
		{
			if (this.currentColumn.Count > 0)
			{
				this.columns.Add(new Tuple<double, double, Padding[]>(this.maxWidth, this.y, this.currentColumn.ToArray()));
				this.currentColumn.Clear();
				this.x += this.maxWidth;

				if (this.y > this.maxHeight)
					this.maxHeight = y;
			}

			this.maxWidth = 0;
			this.y = 0;
		}

		/// <summary>
		/// Total width of layout
		/// </summary>
		public double TotWidth => this.x;

		/// <summary>
		/// Total height of layout
		/// </summary>
		public double TotHeight => this.maxHeight;

		/// <summary>
		/// Aligns cells and returns an array of padded cells.
		/// </summary>
		/// <returns>Array of padded cells.</returns>
		public Padding[] Align()
		{
			List<Padding> Result = new List<Padding>();
			double X = this.horizontalDirection == HorizontalDirection.LeftRight ? 0 : this.x;
			double Y;
			double Diff;
			this.Flush();

			foreach (Tuple<double, double, Padding[]> Column in this.columns)
			{
				Diff = this.maxHeight - Column.Item2;
				switch (this.verticalAlignment)
				{
					case VerticalAlignment.Top:
					default:
						if (this.verticalDirection == VerticalDirection.TopDown)
							Y = 0;
						else
							Y = this.maxHeight - Diff;
						break;

					case VerticalAlignment.Center:
						if (this.verticalDirection == VerticalDirection.TopDown)
							Y = Diff * 0.5;
						else
							Y = this.maxHeight - Diff * 0.5;
						break;

					case VerticalAlignment.Bottom:
						if (this.verticalDirection == VerticalDirection.TopDown)
							Y = Diff;
						else
							Y = this.maxHeight;
						break;
				}

				if (this.horizontalDirection == HorizontalDirection.RightLeft)
					X -= Column.Item1;

				foreach (Padding P in Column.Item3)
				{
					double Height = P.Element.Height;

					if (this.verticalDirection == VerticalDirection.BottomUp)
						Y -= Height;

					P.OffsetX += X;
					P.OffsetY += Y;
					P.AlignedMeasuredCell(Column.Item1, null, this.session);
					Result.Add(P);

					if (this.verticalDirection == VerticalDirection.TopDown)
						Y += Height;
				}

				if (this.horizontalDirection == HorizontalDirection.LeftRight)
					X += Column.Item1;
			}

			return Result.ToArray();
		}
	}
}
