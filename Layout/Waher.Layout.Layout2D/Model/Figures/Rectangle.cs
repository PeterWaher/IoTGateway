﻿using SkiaSharp;
using System.Threading.Tasks;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A rectangle
	/// </summary>
	public class Rectangle : FigurePoint2
	{
		/// <summary>
		/// A rectangle
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Rectangle(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Rectangle";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Rectangle(Document, Parent);
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			if (this.defined)
			{
				SKPaint Fill = await this.TryGetFill(State);
				if (!(Fill is null))
				{
					State.Canvas.DrawRect(this.xCoordinate, this.yCoordinate,
						this.xCoordinate2 - this.xCoordinate, 
						this.yCoordinate2 - this.yCoordinate, Fill);
				}

				SKPaint Pen = await this.TryGetPen(State);
				if (!(Pen is null))
				{
					State.Canvas.DrawRect(this.xCoordinate, this.yCoordinate,
						this.xCoordinate2 - this.xCoordinate, 
						this.yCoordinate2 - this.yCoordinate, Pen);
				}
			}
		
			await base.Draw(State);
		}
	}
}
