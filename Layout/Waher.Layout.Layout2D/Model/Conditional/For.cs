﻿using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;

namespace Waher.Layout.Layout2D.Model.Conditional
{
	/// <summary>
	/// Generates layout elements by iterating through a traditional loop
	/// </summary>
	public class For : DynamicContainer
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
		public override ILayoutElement[] DynamicChildren => this.measured;

		/// <summary>
		/// From
		/// </summary>
		public ExpressionAttribute FromAttribute
		{
			get => this.from;
			set => this.from = value;
		}

		/// <summary>
		/// To
		/// </summary>
		public ExpressionAttribute ToAttribute
		{
			get => this.to;
			set => this.to = value;
		}

		/// <summary>
		/// Step
		/// </summary>
		public ExpressionAttribute StepAttribute
		{
			get => this.step;
			set => this.step = value;
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
			this.from = new ExpressionAttribute(Input, "from", this.Document);
			this.to = new ExpressionAttribute(Input, "to", this.Document);
			this.step = new ExpressionAttribute(Input, "step", this.Document);
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

			this.from?.Export(Output);
			this.to?.Export(Output);
			this.step?.Export(Output);
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
				Dest.from = this.from?.CopyIfNotPreset(Destination.Document);
				Dest.to = this.to?.CopyIfNotPreset(Destination.Document);
				Dest.step = this.step?.CopyIfNotPreset(Destination.Document);
				Dest.variable = this.variable?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			if (this.measured is null)
			{
				ChunkedList<ILayoutElement> Measured = new ChunkedList<ILayoutElement>();
				IElement FromValue = await this.from.EvaluateElementAsync(State.Session);
				IElement ToValue = await this.to.EvaluateElementAsync(State.Session);
				IElement Step = await this.step.EvaluateElementAsync(State.Session);

				EvaluationResult<string> Variable = await this.variable.TryEvaluate(State.Session);
				if (Variable.Ok &&
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
						State.Session[Variable.Result] = From;

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

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.from?.ExportState(Output);
			this.to?.ExportState(Output);
			this.step?.ExportState(Output);
			this.variable?.ExportState(Output);
		}

	}
}
