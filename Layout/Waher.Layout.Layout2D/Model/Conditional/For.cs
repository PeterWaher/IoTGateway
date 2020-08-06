using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Conditional
{
	/// <summary>
	/// Generates layout elements by iterating through a traditional loop
	/// </summary>
	public class For : LayoutContainer
	{
		private ExpressionAttribute from;
		private ExpressionAttribute to;
		private ExpressionAttribute step;
		private StringAttribute variable;

		/// <summary>
		/// Generates layout elements by iterating through a traditional loop
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public For(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "For";

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.from = new ExpressionAttribute(Input, "from");
			this.to = new ExpressionAttribute(Input, "to");
			this.step = new ExpressionAttribute(Input, "step");
			this.variable = new StringAttribute(Input, "variable");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.from.Export(Output);
			this.to.Export(Output);
			this.step.Export(Output);
			this.variable.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new For(Document, Parent);
		}
	}
}
