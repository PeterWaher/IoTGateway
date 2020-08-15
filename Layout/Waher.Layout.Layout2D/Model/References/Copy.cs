using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Figures;

namespace Waher.Layout.Layout2D.Model.References
{
	/// <summary>
	/// Copies the layout from a reference
	/// </summary>
	public class Copy : LayoutElement
	{
		private StringAttribute _ref;

		/// <summary>
		/// Copies the layout from a reference
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Copy(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Copy";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this._ref = new StringAttribute(Input, "ref");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this._ref.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Copy(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Copy Dest)
				Dest._ref = this._ref.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			if (this.defined &&
				(!this._ref.TryEvaluate(State.Session, out string RefId) ||
				!State.TryGetElement(RefId, out this.reference)))
			{
				this.defined = false;
			}
		}

		private ILayoutElement reference;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			base.Draw(State);

			if (this.defined)
			{
				if (this.reference is Shape Shape)
					Shape.DrawShape(State);
				else
					this.reference.Draw(State);
			}
		}
	}
}
