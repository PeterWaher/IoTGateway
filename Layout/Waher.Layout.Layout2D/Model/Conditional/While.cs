using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Conditional
{
	/// <summary>
	/// Generates layout elements while a condition is true.
	/// </summary>
	public class While : LayoutContainer, IDynamicChildren
	{
		private ExpressionAttribute expression;
		private BooleanAttribute testAfter;
		private ILayoutElement[] measured;

		/// <summary>
		/// Generates layout elements while a condition is true.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public While(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "While";

		/// <summary>
		/// Dynamic array of children
		/// </summary>
		public ILayoutElement[] DynamicChildren => this.measured;

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.expression = new ExpressionAttribute(Input, "expression");
			this.testAfter = new BooleanAttribute(Input, "testAfter");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.expression.Export(Output);
			this.testAfter.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new While(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is While Dest)
			{
				Dest.expression = this.expression.CopyIfNotPreset();
				Dest.testAfter = this.testAfter.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			List<ILayoutElement> Measured = new List<ILayoutElement>();

			if (!this.testAfter.TryEvaluate(State.Session, out bool TestAfter))
				TestAfter = false;

			if (TestAfter)
			{
				do
				{
					foreach (ILayoutElement Child in this.Children)
					{
						ILayoutElement Copy = Child.Copy(this);
						Measured.Add(Copy);
						Copy.Measure(State);
					}
				}
				while (this.expression.Evaluate(State.Session) is bool b && b);
			}
			else
			{
				while (this.expression.Evaluate(State.Session) is bool b && b)
				{
					foreach (ILayoutElement Child in this.Children)
					{
						ILayoutElement Copy = Child.Copy(this);
						Measured.Add(Copy);
						Copy.Measure(State);
					}
				}
			}

			this.measured = Measured.ToArray();
		}

	}
}
