using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Layout.Layout2D.Model.Figures.VertexNodes
{
	/// <summary>
	/// Represents a vertex.
	/// </summary>
	public abstract class Vertex : Point
	{
		/// <summary>
		/// Represents a vertex.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Vertex(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}
	}
}
