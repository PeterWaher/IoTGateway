using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Provides padding for a layout element in a grid.
	/// </summary>
	public class GridPadding : Padding
	{
		private readonly int colSpan;
		private readonly int rowSpan;
		private readonly int x;
		private readonly int y;

		/// <summary>
		/// Provides padding for a layout element in a grid.
		/// </summary>
		/// <param name="Element">Embedded element.</param>
		/// <param name="OffsetX">X-offset</param>
		/// <param name="OffsetY">Y-offset</param>
		/// <param name="X">Cell X-coordinate in grid.</param>
		/// <param name="Y">Cell Y-coordinate in grid.</param>
		/// <param name="ColSpan">Column span</param>
		/// <param name="RowSpan">Row span</param>
		public GridPadding(ILayoutElement Element, float OffsetX, float OffsetY,
			int X, int Y, int ColSpan, int RowSpan)
			: base(Element, OffsetX, OffsetY)
		{
			this.x = X;
			this.y = Y;
			this.colSpan = ColSpan;
			this.rowSpan = RowSpan;
		}

		/// <summary>
		/// Cell X-coordinate in grid.
		/// </summary>
		public int X => this.x;

		/// <summary>
		/// Cell Y-coordinate in grid.
		/// </summary>
		public int Y => this.y;

		/// <summary>
		/// Column span
		/// </summary>
		public int ColSpan => this.colSpan;

		/// <summary>
		/// Row span
		/// </summary>
		public int RowSpan => this.rowSpan;

	}
}
