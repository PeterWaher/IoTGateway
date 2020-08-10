using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Basic interface for cell layout objects.
	/// </summary>
	public interface ICellLayout
	{
		/// <summary>
		/// Adds a cell to the layout.
		/// </summary>
		/// <param name="Element">Cell element</param>
		void Add(ILayoutElement Element);

		/// <summary>
		/// Aligns cells and returns an array of padded cells.
		/// </summary>
		/// <returns>Array of padded cells.</returns>
		Padding[] Align();

		/// <summary>
		/// Total width of layout
		/// </summary>
		float TotWidth
		{
			get;
		}

		/// <summary>
		/// Total height of layout
		/// </summary>
		float TotHeight
		{
			get;
		}
	}
}
