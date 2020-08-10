using System;
using System.Collections.Generic;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Lays ut cells one on-top of another
	/// </summary>
	public class OverlayCells : ICellLayout
	{
		private readonly List<Padding> measured = new List<Padding>();
		private readonly Variables session;
		private float maxWidth = 0;
		private float maxHeight = 0;

		/// <summary>
		/// Lays ut cells one on-top of another
		/// </summary>
		/// <param name="Session">Current session.</param>
		public OverlayCells(Variables Session)
		{
			this.session = Session;
		}

		/// <summary>
		/// Adds a cell to the layout.
		/// </summary>
		/// <param name="Element">Cell element</param>
		public void Add(ILayoutElement Element)
		{
			this.measured.Add(new Padding(Element, -Element.Left, -Element.Top));

			float Width = Element.Width;
			if (Width > this.maxWidth)
				this.maxWidth = Width;

			float Height = Element.Height;
			if (Height > this.maxHeight)
				this.maxHeight = Height;
		}

		/// <summary>
		/// Total width of layout
		/// </summary>
		public float TotWidth => this.maxWidth;

		/// <summary>
		/// Total height of layout
		/// </summary>
		public float TotHeight => this.maxHeight;

		/// <summary>
		/// Aligns cells and returns an array of padded cells.
		/// </summary>
		/// <returns>Array of padded cells.</returns>
		public Padding[] Align()
		{
			foreach (Padding P in this.measured)
				P.AlignedMeasuredCell(this.maxWidth, this.maxHeight, this.session);

			return this.measured.ToArray();
		}
	}
}
