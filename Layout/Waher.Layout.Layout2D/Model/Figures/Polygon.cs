using System;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.References;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A polygon
	/// </summary>
	public class Polygon : Vertices
	{
		/// <summary>
		/// A polygon
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Polygon(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Polygon";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Polygon(Document, Parent);
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			if (this.defined)
			{
				using (SKPath Path = new SKPath())
				{
					Vertex First = null;

					foreach (Vertex V in this.points)
					{
						if (First is null)
						{
							Path.MoveTo(V.XCoordinate, V.YCoordinate);
							First = V;
						}
						else
							Path.LineTo(V.XCoordinate, V.YCoordinate);
					}

					Path.Close();

					if (this.TryGetFill(State, out SKPaint Fill))
						State.Canvas.DrawPath(Path, Fill);

					if (this.TryGetPen(State, out SKPaint Pen))
						State.Canvas.DrawPath(Path, Pen);
				}
			}
		
			base.Draw(State);
		}
	}
}
