﻿using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Lays ut cells vertically
	/// </summary>
	public class VerticalCells : ICellLayout
	{
		private readonly ChunkedList<Padding> measured = new ChunkedList<Padding>();
		private readonly Variables session;
		private float y = 0;
		private float maxWidth = 0;

		/// <summary>
		/// Lays ut cells vertically
		/// </summary>
		/// <param name="Session">Current session</param>
		public VerticalCells(Variables Session)
		{
			this.session = Session;
		}

		/// <summary>
		/// Adds a cell to the layout.
		/// </summary>
		/// <param name="Element">Cell element</param>
		public Task Add(ILayoutElement Element)
		{
			this.measured.Add(new Padding(Element, 0, this.y));
			this.y += Element.Height ?? 0;

			float Width = Element.Width ?? 0;
			if (Width > this.maxWidth)
				this.maxWidth = Width;
	
			return Task.CompletedTask;
		}

		/// <summary>
		/// Flushes any waiting elements int he layout pipeline.
		/// </summary>
		public void Flush()
		{
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public void MeasurePositions(DrawingState State)
		{
			ILayoutElement Element;

			foreach (Padding P in this.measured)
			{
				Element = P.Element;

				Element.MeasurePositions(State);
				P.OffsetX -= Element.Left ?? 0;
				P.OffsetY -= Element.Top ?? 0;
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
			foreach (Padding P in this.measured)
				P.Distribute(this.maxWidth, null, this.session, SetPosition);
		}

		/// <summary>
		/// Aligns cells and returns an array of padded cells.
		/// </summary>
		/// <returns>Array of padded cells.</returns>
		public Padding[] Align()
		{
			return this.measured.ToArray();
		}
	}
}
