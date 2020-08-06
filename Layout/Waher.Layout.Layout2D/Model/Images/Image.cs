using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Images
{
	/// <summary>
	/// Abstract base class for images.
	/// </summary>
	public abstract class Image : Point
	{
		/// <summary>
		/// Abstract base class for images.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Image(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}
	}
}
