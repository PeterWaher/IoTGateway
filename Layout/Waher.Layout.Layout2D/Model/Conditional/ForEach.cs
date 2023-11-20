using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Conditional
{
	/// <summary>
	/// Generates layout elements by looping through a set or vector of values.
	/// </summary>
	public class ForEach : DynamicContainer
	{
		private ExpressionAttribute expression;
		private StringAttribute variable;
		private ILayoutElement[] measured;

		/// <summary>
		/// Generates layout elements by looping through a set or vector of values.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public ForEach(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "ForEach";

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
		/// Variable
		/// </summary>
		public StringAttribute VariableAttribute
		{
			get => this.variable;
			set => this.variable = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.expression = new ExpressionAttribute(Input, "expression", this.Document);
			this.variable = new StringAttribute(Input, "variable", this.Document);

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
			this.variable?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new ForEach(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is ForEach Dest)
			{
				Dest.expression = this.expression?.CopyIfNotPreset(Destination.Document);
				Dest.variable = this.variable?.CopyIfNotPreset(Destination.Document);
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
				object Result = await this.expression.EvaluateAsync(State.Session);

				if (Result is IEnumerable Set && this.HasChildren)
				{
					EvaluationResult<string> Variable = await this.variable.TryEvaluate(State.Session);
					if (Variable.Ok)
					{
						IEnumerator e = Set.GetEnumerator();

						while (e.MoveNext())
						{
							State.Session[Variable.Result] = e.Current;

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
