using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Backgrounds
{
	/// <summary>
	/// Abstract base class for backgrounds.
	/// </summary>
	public abstract class Background : LayoutElement
	{
		/// <summary>
		/// Current background
		/// </summary>
		protected SKPaint paint;

		/// <summary>
		/// Abstract base class for backgrounds.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Background(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.paint?.Dispose();
			this.paint = null;
		}

		/// <summary>
		/// Current pen
		/// </summary>
		public SKPaint Paint => this.paint;
	}
}
