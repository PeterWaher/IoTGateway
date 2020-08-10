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

		/// <summary>
		/// Provides padding for a layout element in a grid.
		/// </summary>
		/// <param name="Element">Embedded element.</param>
		/// <param name="OffsetX">X-offset</param>
		/// <param name="OffsetY">Y-offset</param>
		/// <param name="ColSpan">Column span</param>
		/// <param name="RowSpan">Row span</param>
		public GridPadding(ILayoutElement Element, float OffsetX, float OffsetY,
			int ColSpan, int RowSpan)
			: base(Element, OffsetX, OffsetY)
		{
			this.colSpan = ColSpan;
			this.rowSpan = RowSpan;
		}

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
