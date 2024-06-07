using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.Backgrounds;
using Waher.Layout.Layout2D.Model.Pens;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// Abstract base class for figures
	/// </summary>
	public abstract class Figure : LayoutContainer
	{
		private StringAttribute pen;
		private StringAttribute fill;

		/// <summary>
		/// Abstract base class for figures
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Figure(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Pen
		/// </summary>
		public StringAttribute PenAttribute
		{
			get => this.pen;
			set => this.pen = value;
		}

		/// <summary>
		/// Fill
		/// </summary>
		public StringAttribute FillAttribute
		{
			get => this.fill;
			set => this.fill = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.pen = new StringAttribute(Input, "pen", this.Document);
			this.fill = new StringAttribute(Input, "fill", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.pen?.Export(Output);
			this.fill?.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Figure Dest)
			{
				Dest.pen = this.pen?.CopyIfNotPreset(Destination.Document);
				Dest.fill = this.fill?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Gets the pen associated with the element. If not found, the default pen is returned.
		/// </summary>
		/// <param name="State">Current state</param>
		/// <returns>Pen.</returns>
		public async Task<SKPaint> GetPen(DrawingState State)
		{
			EvaluationResult<string> PenId = await this.pen.TryEvaluate(State.Session);
			if (PenId.Ok)
			{
				if (this.Document.TryGetElement(PenId.Result, out ILayoutElement E) && E is Pen PenDefinition)
					return PenDefinition.Paint;
				else
					return State.DefaultPen;
			}
			else if (!(State.ShapePen is null))
				return State.ShapePen;
			else
				return State.DefaultPen;
		}

		/// <summary>
		/// Tries to get the pen associated with the element, if one is defined.
		/// </summary>
		/// <param name="State">Current state</param>
		/// <returns>Pen, if defined, null otherwise.</returns>
		public async Task<SKPaint> TryGetPen(DrawingState State)
		{
			EvaluationResult<string> PenId = await this.pen.TryEvaluate(State.Session);
			if (PenId.Ok)
			{
				if (this.Document.TryGetElement(PenId.Result, out ILayoutElement E) && E is Pen PenDefinition)
					return PenDefinition.Paint;
			}
			else if (!(State.ShapePen is null))
				return State.ShapePen;

			return null;
		}

		/// <summary>
		/// Tries to get the filling of the figure, if one is defined.
		/// </summary>
		/// <param name="State">State object.</param>
		/// <returns>Filling, if defined, null otherwise.</returns>
		public async Task<SKPaint> TryGetFill(DrawingState State)
		{
			EvaluationResult<string> FillId = await this.fill.TryEvaluate(State.Session);
			if (FillId.Ok)
			{
				if (this.Document.TryGetElement(FillId.Result, out ILayoutElement E) && E is Background FillDefinition)
					return FillDefinition.Paint;
			}
			else if (!(State.ShapeFill is null))
				return State.ShapeFill;

			return null;
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.pen?.ExportState(Output);
			this.fill?.ExportState(Output);
		}

	}
}
