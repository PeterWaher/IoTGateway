using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Draws a line to a point that lies a certain distance forward of the last point,
	/// in the current direction of movement.
	/// </summary>
	public class Forward : Distance, ISegment
	{
		/// <summary>
		/// Draws a line to a point that lies a certain distance forward of the last point,
		/// in the current direction of movement.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Forward(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Forward";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Forward(Document, Parent);
		}
	}
}
