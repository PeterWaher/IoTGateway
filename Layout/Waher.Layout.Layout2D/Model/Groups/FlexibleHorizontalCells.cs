using System;
using System.Collections.Generic;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Lays out elements flexibly, first horizontally, then vertically.
	/// </summary>
	public class FlexibleHorizontalCells : ICellLayout
	{
		private readonly List<Padding> currentRow = new List<Padding>();
		private readonly List<Tuple<float, float, Padding[]>> rows = new List<Tuple<float, float, Padding[]>>();
		private readonly Variables session;
		private readonly float limitWidth;
		private readonly HorizontalDirection horizontalDirection;
		private readonly VerticalDirection verticalDirection;
		private readonly HorizontalAlignment horizontalAlignment;
		private float maxWidth = 0;
		private float maxHeight = 0;
		private float x = 0;
		private float y = 0;

		/// <summary>
		/// Lays out elements flexibly, first horizontally, then vertically.
		/// </summary>
		/// <param name="Session">Current session</param>
		/// <param name="WidthLimit">Width limit of area</param>
		/// <param name="HorizontalDirection">Horizontal direction</param>
		/// <param name="VerticalDirection">Vertical direction</param>
		/// <param name="HorizontalAlignment">Horizontal alignment of cells in layout.</param>
		public FlexibleHorizontalCells(Variables Session, float WidthLimit,
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
			float Width = Element.Width ?? 0;
			float Height = Element.Height ?? 0;

			if (this.x + Width > this.limitWidth)
				this.Flush();

			this.currentRow.Add(new Padding(Element, 0, 0));
			this.x += Width;

			if (Height > this.maxHeight)
				this.maxHeight = Height;
		}

		/// <summary>
		/// Flushes any waiting elements int he layout pipeline.
		/// </summary>
		public void Flush()
		{
			if (this.currentRow.Count > 0)
			{
				this.rows.Add(new Tuple<float, float, Padding[]>(this.x, this.maxHeight, this.currentRow.ToArray()));
				this.currentRow.Clear();
				this.y += this.maxHeight;

				if (this.x > this.maxWidth)
					this.maxWidth = x;
			}

			this.maxHeight = 0;
			this.x = 0;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public void MeasurePositions(DrawingState State)
		{
			ILayoutElement Element;

			foreach (Tuple<float, float, Padding[]> Row in this.rows)
			{
				foreach (Padding P in Row.Item3)
				{
					Element = P.Element;

					Element.MeasurePositions(State);
					P.OffsetX -= Element.Left ?? 0;
					P.OffsetY -= Element.Top ?? 0;
				}
			}
		}

		/// <summary>
		/// Total width of layout
		/// </summary>
		public float TotWidth => this.maxWidth;

		/// <summary>
		/// Total height of layout
		/// </summary>
		public float TotHeight => this.y;

		/// <summary>
		/// Distributes cells in layout.
		/// </summary>
		/// <param name="SetPosition">If position of inner content is to be set..</param>
		public void Distribute(bool SetPosition)
		{
			foreach (Tuple<float, float, Padding[]> Row in this.rows)
			{
				foreach (Padding P in Row.Item3)
					P.Distribute(null, Row.Item2, this.session, SetPosition);
			}
		}

		/// <summary>
		/// Aligns cells and returns an array of padded cells.
		/// </summary>
		/// <returns>Array of padded cells.</returns>
		public Padding[] Align()
		{
			List<Padding> Result = new List<Padding>();
			float X;
			float Y = this.verticalDirection == VerticalDirection.TopDown ? 0 : this.y;
			float Diff;

			foreach (Tuple<float, float, Padding[]> Row in this.rows)
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
							X = Diff / 2;
						else
							X = this.maxWidth - Diff / 2;
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
					float Width = P.Element.Width ?? 0;

					if (this.horizontalDirection == HorizontalDirection.RightLeft)
						X -= Width;

					P.OffsetX += X;
					P.OffsetY += Y;
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
