using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.References;
using Waher.Runtime.Collections;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A sequence of points
	/// </summary>
	public abstract class Vertices : Figure
	{
		/// <summary>
		/// Vertex nodes
		/// </summary>
		protected Vertex[] points;

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
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override async Task FromXml(XmlElement Input)
		{
			await base.FromXml(Input);
			this.points = this.GetVertices();
		}

		private Vertex[] GetVertices()
		{
			ChunkedList<Vertex> Result = null;

			if (this.HasChildren)
			{
				foreach (ILayoutElement Child in this.Children)
				{
					if (Child is Vertex P)
					{
						if (Result is null)
							Result = new ChunkedList<Vertex>();

						Result.Add(P);
					}
				}
			}

			return Result?.ToArray();
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Vertices Dest)
				Dest.points = Dest.GetVertices();
		}

	}
}
