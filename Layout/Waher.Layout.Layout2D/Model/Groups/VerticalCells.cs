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
		private double y = 0;
		private double maxWidth = 0;

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
			this.measured.Add(new Padding(Element, -Element.Left, this.y - Element.Top));
			this.y += Element.Height;

			double Width = Element.Width;
			if (Width > this.maxWidth)
				this.maxWidth = Width;
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
			foreach (Padding P in this.measured)
				P.AlignedMeasuredCell(this.maxWidth, null, this.session);

			return this.measured.ToArray();
		}
	}
}
