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
		/// Flushes any waiting elements int he layout pipeline.
		/// </summary>
		void Flush();

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		void MeasurePositions(DrawingState State);

		/// <summary>
		/// Distributes cells in layout.
		/// </summary>
		/// <param name="SetPosition">If position of inner content is to be set..</param>
		void Distribute(bool SetPosition);

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
