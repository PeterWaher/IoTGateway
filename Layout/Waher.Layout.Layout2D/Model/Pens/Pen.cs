using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Pens
{
	/// <summary>
	/// Abstract base class for pens.
	/// </summary>
	public abstract class Pen : LayoutElement
	{
		private LengthAttribute width;

		/// <summary>
		/// Current pen
		/// </summary>
		protected SKPaint paint;

		/// <summary>
		/// Abstract base class for pens.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Pen(Layout2DDocument Document, ILayoutElement Parent)
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

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.width = new LengthAttribute(Input, "width");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.width.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Pen Dest)
				Dest.width = this.width.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (this.width.TryEvaluate(State.Session, out Length Width))
				this.penWidth = State.GetDrawingSize(Width, this, true);
			else
				this.penWidth = null;
		}

		/// <summary>
		/// Measured pen width.
		/// </summary>
		protected float? penWidth;

	}
}
