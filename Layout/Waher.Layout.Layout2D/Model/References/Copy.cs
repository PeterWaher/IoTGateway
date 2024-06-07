using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.References
{
	/// <summary>
	/// Copies the layout from a reference
	/// </summary>
	public class Copy : LayoutElement
	{
		private StringAttribute @ref;

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
			get => this.@ref;
			set => this.@ref = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.@ref = new StringAttribute(Input, "ref", this.Document);
			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.@ref?.Export(Output);
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
				Dest.@ref = this.@ref?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			if (this.defined)
			{
				EvaluationResult<string> RefId = await this.@ref.TryEvaluate(State.Session);
				if (RefId.Ok && this.Document.TryGetElement(RefId.Result, out this.reference))
				{
					this.reference = this.reference.Copy(this);

					await this.reference.MeasureDimensions(State);

					this.Width = this.reference.Width;
					this.ExplicitWidth = this.reference.ExplicitWidth;
					this.Height = this.reference.Height;
					this.ExplicitHeight = this.reference.ExplicitHeight;
				}
				else
					this.defined = false;
			}
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
		public override async Task Draw(DrawingState State)
		{
			if (this.defined)
				await this.reference.DrawShape(State);
		
			await base.Draw(State);
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.@ref?.ExportState(Output);
		}
	}
}
