using System;
using System.Collections.Generic;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Lays ut cells horizontally
	/// </summary>
	public class VerticalCells : ICellLayout
	{
		private readonly List<Padding> measured = new List<Padding>();
		private readonly Variables session;
		private float y = 0;
		private float maxWidth = 0;

		/// <summary>
		/// Lays ut cells horizontally
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
		public void Add(ILayoutElement Element)
		{
			this.measured.Add(new Padding(Element, 0, this.y));
			this.y += Element.Height ?? 0;

			float Width = Element.Width ?? 0;
			if (Width > this.maxWidth)
				this.maxWidth = Width;
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
		/// Aligns cells and returns an array of padded cells.
		/// </summary>
		/// <returns>Array of padded cells.</returns>
		public Padding[] Align()
		{
			foreach (Padding P in this.measured)
				P.AlignedMeasuredCell(this.maxWidth, null, this.session);

			return this.measured.ToArray();
		}
	}
}
