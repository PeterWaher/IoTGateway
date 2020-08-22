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
		/// Reference
		/// </summary>
		public StringAttribute ReferenceAttribute
		{
			get => this._ref;
			set => this._ref = value;
		}

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

			this._ref?.Export(Output);
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
				Dest._ref = this._ref?.CopyIfNotPreset();
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool MeasureDimensions(DrawingState State)
		{
			bool Relative = base.MeasureDimensions(State);

			if (this.defined &&
				!(this._ref is null) &&
				this._ref.TryEvaluate(State.Session, out string RefId) &&
				this.Document.TryGetElement(RefId, out this.reference))
			{
				this.reference = this.reference.Copy(this);
				if (this.reference.MeasureDimensions(State))
					Relative = true;

				this.Width = this.reference.Width;
				this.Height = this.reference.Height;
			}
			else
				this.defined = false;

			return Relative;
		}

		private ILayoutElement reference;

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			if (this.defined)
			{
				this.reference.MeasurePositions(State);
				this.Left = this.reference.Left;
				this.Top = this.reference.Top;
			}
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			if (this.defined)
				this.reference.DrawShape(State);
		
			base.Draw(State);
		}
	}
}
