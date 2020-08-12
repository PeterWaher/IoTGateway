using System;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.References;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A poly-line
	/// </summary>
	public class PolyLine : Vertices
	{
		/// <summary>
		/// A poly-line
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public PolyLine(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "PolyLine";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new PolyLine(Document, Parent);
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			base.Draw(State);

			if (this.defined)
			{
				using (SKPath Path = new SKPath())
				{
					bool First = true;

					foreach (Vertex V in this.points)
					{
						if (First)
						{
							Path.MoveTo(V.XCoordinate, V.YCoordinate);
							First = false;
						}
						else
							Path.LineTo(V.XCoordinate, V.YCoordinate);
					}

					State.Canvas.DrawPath(Path, this.GetPen(State));
				}
			}
		}
	}
}
