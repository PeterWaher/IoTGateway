using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Conditional
{
	/// <summary>
	/// Generates layout elements while a condition is true.
	/// </summary>
	public class While : DynamicContainer
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
		public override ILayoutElement[] DynamicChildren => this.measured;

		/// <summary>
		/// Expression
		/// </summary>
		public ExpressionAttribute ExpressionAttribute
		{
			get => this.expression;
			set => this.expression = value;
		}

		/// <summary>
		/// Test After attribute
		/// </summary>
		public BooleanAttribute TestAfterAttribute
		{
			get => this.testAfter;
			set => this.testAfter = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.expression = new ExpressionAttribute(Input, "expression", this.Document);
			this.testAfter = new BooleanAttribute(Input, "testAfter", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.expression?.Export(Output);
			this.testAfter?.Export(Output);
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
				Dest.expression = this.expression?.CopyIfNotPreset(Destination.Document);
				Dest.testAfter = this.testAfter?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			if (this.measured is null)
			{
				List<ILayoutElement> Measured = new List<ILayoutElement>();

				EvaluationResult<bool> TestAfter = await this.testAfter.TryEvaluate(State.Session);
				if (TestAfter.Ok && TestAfter.Result)
				{
					do
					{
						if (this.HasChildren)
						{
							foreach (ILayoutElement Child in this.Children)
							{
								ILayoutElement Copy = Child.Copy(this);
								Measured.Add(Copy);

								await Copy.MeasureDimensions(State);

								this.IncludeElement(Copy);
							}
						}
					}
					while (await this.expression.EvaluateAsync(State.Session) is bool b && b);
				}
				else
				{
					while (await this.expression.EvaluateAsync(State.Session) is bool b && b)
					{
						if (this.HasChildren)
						{
							foreach (ILayoutElement Child in this.Children)
							{
								ILayoutElement Copy = Child.Copy(this);
								Measured.Add(Copy);

								await Copy.DoMeasureDimensions(State);

								this.IncludeElement(Copy);
							}
						}
					}
				}

				this.measured = Measured.ToArray();
			}
			else
			{
				foreach (ILayoutElement E in this.measured)
				{
					await E.MeasureDimensions(State);
					this.IncludeElement(E);
				}
			}
		}

		/// <summary>
		/// If children dimensions are to be measured.
		/// </summary>
		protected override bool MeasureChildrenDimensions => false;

	}
}
