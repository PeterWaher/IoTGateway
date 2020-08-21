using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class of dynamic layout containers (i.e. containers that can
	/// generate child elements dynamically).
	/// </summary>
	public abstract class DynamicContainer : LayoutContainer, IDynamicChildren
	{
		/// <summary>
		/// Abstract base class of dynamic layout containers (i.e. containers that can
		/// generate child elements dynamically).
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public DynamicContainer(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Dynamic array of children
		/// </summary>
		public abstract ILayoutElement[] DynamicChildren
		{
			get;
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			ILayoutElement[] Children = this.DynamicChildren;

			if (!(Children is null))
			{
				foreach (ILayoutElement E in Children)
				{
					if (E.IsVisible)
						E.Draw(State);
				}
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			ILayoutElement[] Children = this.DynamicChildren;

			if (!(Children is null))
			{
				foreach (ILayoutElement E in Children)
				{
					if (E.IsVisible)
						E.MeasurePositions(State);
				}
			}
		}

	}
}
