﻿using System;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Lays out elements flexibly, first vertically, then horizontally.
	/// </summary>
	public class FlexibleVerticalCells : ICellLayout
	{
		private readonly ChunkedList<Padding> currentColumn = new ChunkedList<Padding>();
		private readonly ChunkedList<Tuple<float, float, Padding[]>> columns = new ChunkedList<Tuple<float, float, Padding[]>>();
		private readonly Variables session;
		private readonly float limitHeight;
		private readonly HorizontalDirection horizontalDirection;
		private readonly VerticalDirection verticalDirection;
		private readonly VerticalAlignment verticalAlignment;
		private float maxWidth = 0;
		private float maxHeight = 0;
		private float x = 0;
		private float y = 0;

		/// <summary>
		/// Lays out elements flexibly, first vertically, then horizontally.
		/// </summary>
		/// <param name="Session">Current session</param>
		/// <param name="WidthLimit">Width limit of area</param>
		/// <param name="HorizontalDirection">Horizontal direction</param>
		/// <param name="VerticalDirection">Vertical direction</param>
		/// <param name="VerticalAlignment">Vertical alignment of cells in layout.</param>
		public FlexibleVerticalCells(Variables Session, float WidthLimit,
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
		public Task Add(ILayoutElement Element)
		{
			float Width = Element.Width ?? 0;
			float Height = Element.Height ?? 0;

			if (this.y + Height > this.limitHeight)
				this.Flush();

			this.currentColumn.Add(new Padding(Element, 0, 0));
			this.y += Height;

			if (Width > this.maxWidth)
				this.maxWidth = Width;
		
			return Task.CompletedTask;
		}

		/// <summary>
		/// Flushes any waiting elements int he layout pipeline.
		/// </summary>
		public void Flush()
		{
			if (this.currentColumn.Count > 0)
			{
				this.columns.Add(new Tuple<float, float, Padding[]>(this.maxWidth, this.y, this.currentColumn.ToArray()));
				this.currentColumn.Clear();
				this.x += this.maxWidth;

				if (this.y > this.maxHeight)
					this.maxHeight = this.y;
			}

			this.maxWidth = 0;
			this.y = 0;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public void MeasurePositions(DrawingState State)
		{
			ILayoutElement Element;

			foreach (Tuple<float, float, Padding[]> Row in this.columns)
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
		public float TotWidth => this.x;

		/// <summary>
		/// Total height of layout
		/// </summary>
		public float TotHeight => this.maxHeight;

		/// <summary>
		/// Distributes cells in layout.
		/// </summary>
		/// <param name="SetPosition">If position of inner content is to be set..</param>
		public void Distribute(bool SetPosition)
		{
			foreach (Tuple<float, float, Padding[]> Column in this.columns)
			{
				foreach (Padding P in Column.Item3)
					P.Distribute(Column.Item1, null, this.session, SetPosition);
			}
		}

		/// <summary>
		/// Aligns cells and returns an array of padded cells.
		/// </summary>
		/// <returns>Array of padded cells.</returns>
		public Padding[] Align()
		{
			ChunkedList<Padding> Result = new ChunkedList<Padding>();
			float X = this.horizontalDirection == HorizontalDirection.LeftRight ? 0 : this.x;
			float Y;
			float Diff;

			foreach (Tuple<float, float, Padding[]> Column in this.columns)
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
							Y = Diff / 2;
						else
							Y = this.maxHeight - Diff / 2;
						break;

					case VerticalAlignment.Bottom:
					case VerticalAlignment.BaseLine:
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
					float Height = P.Element.Height ?? 0;

					if (this.verticalDirection == VerticalDirection.BottomUp)
						Y -= Height;

					P.OffsetX += X;
					P.OffsetY += Y;
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
