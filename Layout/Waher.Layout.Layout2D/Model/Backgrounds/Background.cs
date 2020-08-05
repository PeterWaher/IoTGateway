using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Backgrounds
{
	/// <summary>
	/// Abstract base class for backgrounds.
	/// </summary>
	public abstract class Background : LayoutElement
	{
		/// <summary>
		/// Abstract base class for backgrounds.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Background(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}
	}
}
