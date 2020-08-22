using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Backgrounds;
using Waher.Layout.Layout2D.Model.Fonts;
using Waher.Layout.Layout2D.Model.Pens;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// Abstract base class for figures
	/// </summary>
	public abstract class Figure : LayoutContainer
	{
		private StringAttribute pen;
		private StringAttribute fill;

		/// <summary>
		/// Abstract base class for figures
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Figure(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Pen
		/// </summary>
		public StringAttribute PenAttribute
		{
			get => this.pen;
			set => this.pen = value;
		}

		/// <summary>
		/// Fill
		/// </summary>
		public StringAttribute FillAttribute
		{
			get => this.fill;
			set => this.fill = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.pen = new StringAttribute(Input, "pen");
			this.fill = new StringAttribute(Input, "fill");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.pen?.Export(Output);
			this.fill?.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Figure Dest)
			{
				Dest.pen = this.pen?.CopyIfNotPreset();
				Dest.fill = this.fill?.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Gets the pen associated with the element. If not found, the default pen
		/// is returned.
		/// </summary>
		/// <param name="State">Current state</param>
		/// <returns>Pen.</returns>
		public SKPaint GetPen(DrawingState State)
		{
			if (!(this.pen is null) && this.pen.TryEvaluate(State.Session, out string PenId))
			{
				if (this.Document.TryGetElement(PenId, out ILayoutElement E) &&
					E is Pen PenDefinition)
				{
					return PenDefinition.Paint;
				}
				else
					return State.DefaultPen;
			}
			else if (!(State.ShapePen is null))
				return State.ShapePen;
			else
				return State.DefaultPen;
		}

		/// <summary>
		/// Tries to get the pen associated with the element, if one is defined.
		/// </summary>
		/// <param name="State">Current state</param>
		/// <param name="Pen">Pen, if defined.</param>
		/// <returns>If a pen was defined, and was found.</returns>
		public bool TryGetPen(DrawingState State, out SKPaint Pen)
		{
			if (!(this.pen is null) && this.pen.TryEvaluate(State.Session, out string PenId))
			{
				if (this.Document.TryGetElement(PenId, out ILayoutElement E) &&
					E is Pen PenDefinition)
				{
					Pen = PenDefinition.Paint;
					return true;
				}
			}
			else if (!(State.ShapePen is null))
			{
				Pen = State.ShapePen;
				return true;
			}

			Pen = null;
			return false;
		}

		/// <summary>
		/// Tries to get the filling of the figure, if one is defined.
		/// </summary>
		/// <param name="State">State object.</param>
		/// <param name="Fill">Filling, if defined.</param>
		/// <returns>If a filling was defined, and was found.</returns>
		public bool TryGetFill(DrawingState State, out SKPaint Fill)
		{
			if (!(this.fill is null) && this.fill.TryEvaluate(State.Session, out string FillId))
			{
				if (this.Document.TryGetElement(FillId, out ILayoutElement E) &&
					E is Background FillDefinition)
				{
					Fill = FillDefinition.Paint;
					return true;
				}
			}
			else if (!(State.ShapeFill is null))
			{
				Fill = State.ShapeFill;
				return true;
			}

			Fill = null;
			return false;
		}

	}
}
