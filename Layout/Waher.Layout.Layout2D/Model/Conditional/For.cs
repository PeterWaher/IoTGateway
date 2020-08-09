using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;

namespace Waher.Layout.Layout2D.Model.Conditional
{
	/// <summary>
	/// Generates layout elements by iterating through a traditional loop
	/// </summary>
	public class For : LayoutContainer, IDynamicChildren
	{
		private ExpressionAttribute from;
		private ExpressionAttribute to;
		private ExpressionAttribute step;
		private StringAttribute variable;
		private ILayoutElement[] measured;

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

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is For Dest)
			{
				Dest.from = this.from.CopyIfNotPreset();
				Dest.to = this.to.CopyIfNotPreset();
				Dest.step = this.step.CopyIfNotPreset();
				Dest.variable = this.variable.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			List<ILayoutElement> Measured = new List<ILayoutElement>();
			IElement FromValue = this.from.EvaluateElement(State.Session);
			IElement ToValue = this.to.EvaluateElement(State.Session);
			IElement Step = this.step.EvaluateElement(State.Session);
			
			if (this.variable.TryEvaluate(State.Session, out string VariableName) &&
				FromValue is ICommutativeRingWithIdentityElement From &&
				ToValue is ICommutativeRingWithIdentityElement To &&
				From.AssociatedSet is IOrderedSet S)
			{
				int Direction = S.Compare(From, To);
				bool DoLoop = true;

				if (Step is null)
				{
					if (Direction <= 0)
						Step = From.One;
					else
						Step = From.One.Negate();
				}
				else
				{
					if (Direction < 0)
					{
						if (S.Compare(Step, From.Zero) <= 0)
							DoLoop = false;
					}
					else if (Direction > 0)
					{
						if (S.Compare(Step, From.Zero) >= 0)
							DoLoop = false;
					}
				}

				while (DoLoop)
				{
					State.Session[VariableName] = From;

					foreach (ILayoutElement Child in this.Children)
					{
						ILayoutElement Copy = Child.Copy(this);
						Measured.Add(Copy);
						Copy.Measure(State);
					}

					if (Direction == 0)
						break;
					else
					{
						From = Script.Operators.Arithmetics.Add.EvaluateAddition(From, Step, null) as ICommutativeRingWithIdentityElement;
						if (From is null)
							break;

						if (Direction > 0)
							DoLoop = S.Compare(From, To) >= 0;
						else
							DoLoop = S.Compare(From, To) <= 0;
					}
				}
			}

			this.measured = Measured.ToArray();
		}

	}
}
