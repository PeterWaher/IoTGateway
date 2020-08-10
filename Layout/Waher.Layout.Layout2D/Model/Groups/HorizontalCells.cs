using System;
using System.Collections.Generic;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Lays ut cells horizontally
	/// </summary>
	public class HorizontalCells : ICellLayout
	{
		private readonly List<Padding> measured = new List<Padding>();
		private readonly Variables session;
		private double x = 0;
		private double maxHeight = 0;

		/// <summary>
		/// Lays ut cells horizontally
		/// </summary>
		/// <param name="Session">Current session.</param>
		public HorizontalCells(Variables Session)
		{
			this.session = Session;
		}

		/// <summary>
		/// Adds a cell to the layout.
		/// </summary>
		/// <param name="Element">Cell element</param>
		public void Add(ILayoutElement Element)
		{
			this.measured.Add(new Padding(Element, this.x - Element.Left, -Element.Top));
			this.x += Element.Width;

			double Height = Element.Height;
			if (Height > this.maxHeight)
				this.maxHeight = Height;
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
			foreach (Padding P in this.measured)
				P.AlignedMeasuredCell(null, this.maxHeight, this.session);

			return this.measured.ToArray();
		}
	}
}
