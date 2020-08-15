using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Transforms
{
	/// <summary>
	/// Base abstract class for linear transforms.
	/// </summary>
	public abstract class LinearTrasformation : LayoutContainer
	{
		/// <summary>
		/// Base abstract class for linear transforms.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public LinearTrasformation(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}
	}
}
