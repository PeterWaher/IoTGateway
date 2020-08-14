using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.References;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// An arrow
	/// </summary>
	public class Arrow : FigurePoint2
	{
		private StringAttribute head;
		private StringAttribute tail;

		/// <summary>
		/// An arrow
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Arrow(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Arrow";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.head = new StringAttribute(Input, "head");
			this.tail = new StringAttribute(Input, "tail");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.head.Export(Output);
			this.tail.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Arrow(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Arrow Dest)
			{
				Dest.head = this.head.CopyIfNotPreset();
				Dest.tail = this.tail.CopyIfNotPreset();
			}
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
				State.Canvas.DrawLine(this.xCoordinate, this.yCoordinate,
					this.xCoordinate2, this.yCoordinate2, this.GetPen(State));

				if (this.head.TryEvaluate(State.Session, out string RefId) &&
					State.TryGetElement(RefId, out ILayoutElement Element))
				{
					if (Element is Shape Shape)
						Shape.DrawShape(State);
					else
						Element.Draw(State);
				}

				if (this.tail.TryEvaluate(State.Session, out RefId) &&
					State.TryGetElement(RefId, out Element))
				{
					if (Element is Shape Shape)
						Shape.DrawShape(State);
					else
						Element.Draw(State);
				}
			}

		}
	}
}
