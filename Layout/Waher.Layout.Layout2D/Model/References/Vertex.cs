using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Figures;

namespace Waher.Layout.Layout2D.Model.References
{
	/// <summary>
	/// Defines a vertex in the graf
	/// </summary>
	public class Vertex : Point
	{
		/// <summary>
		/// Defines a vertex in the graf
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Vertex(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Vertex";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Vertex(Document, Parent);
		}

		/// <summary>
		/// Measured X-coordinate
		/// </summary>
		public float XCoordinate => this.xCoordinate;

		/// <summary>
		/// Measured Y-coordinate
		/// </summary>
		public float YCoordinate => this.yCoordinate;
	}
}
