using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Figures.VertexNodes;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A sequence of points
	/// </summary>
	public abstract class Vertices : Figure
	{
		private Vertex[] points;

		/// <summary>
		/// A sequence of points
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Vertices(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!(this.points is null))
			{
				foreach (Vertex E in this.points)
					E.Dispose();
			}
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			List<Vertex> Points = null;

			foreach (ILayoutElement Child in this.Children)
			{
				if (Child is Vertex P)
				{
					if (Points is null)
						Points = new List<Vertex>();

					Points.Add(P);
				}
				else
					throw new LayoutSyntaxException("Not a vertex type: " + Child.Namespace + "#" + Child.LocalName);
			}

			this.points = Points?.ToArray();
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Vertices Dest)
			{
				if (!(this.points is null))
				{
					int i, c = this.points.Length;

					Vertex[] Children = new Vertex[c];

					for (i = 0; i < c; i++)
						Children[i] = this.points[i].Copy(Dest) as Vertex;

					Dest.points = Children;
				}
			}
		}

	}
}
