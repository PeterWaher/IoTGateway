﻿using System.Threading.Tasks;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// Ordering child elements horizontally.
	/// </summary>
	public class Horizontal : SpatialDistribution
	{
		/// <summary>
		/// Ordering child elements horizontally.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Horizontal(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Horizontal";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Horizontal(Document, Parent);
		}

		/// <summary>
		/// Gets a cell layout object that will be responsible for laying out cells.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>Cell layout object.</returns>
		public override Task<ICellLayout> GetCellLayout(DrawingState State)
		{
			return Task.FromResult<ICellLayout>(new HorizontalCells(State.Session));
		}

	}
}
