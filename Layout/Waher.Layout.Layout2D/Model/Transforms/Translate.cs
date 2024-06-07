using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Transforms
{
	/// <summary>
	/// A translation transform
	/// </summary>
	public class Translate : LinearTrasformation
	{
		private LengthAttribute translateX;
		private LengthAttribute translateY;

		/// <summary>
		/// A translation transform
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Translate(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Translate";

		/// <summary>
		/// Translate X
		/// </summary>
		public LengthAttribute TranslateXAttribute
		{
			get => this.translateX;
			set => this.translateX = value;
		}

		/// <summary>
		/// Translate Y
		/// </summary>
		public LengthAttribute TranslateYAttribute
		{
			get => this.translateY;
			set => this.translateY = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.translateX = new LengthAttribute(Input, "translateX", this.Document);
			this.translateY = new LengthAttribute(Input, "translateY", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.translateX?.Export(Output);
			this.translateY?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Translate(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Translate Dest)
			{
				Dest.translateX = this.translateX?.CopyIfNotPreset(Destination.Document);
				Dest.translateY = this.translateY?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Called when dimensions have been measured.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task AfterMeasureDimensions(DrawingState State)
		{
			await base.AfterMeasureDimensions(State);

			EvaluationResult<Length> L = await this.translateX.TryEvaluate(State.Session);
			if (L.Ok)
				State.CalcDrawingSize(L.Result, ref this.dx, true, this.Parent);
			else
				this.dx = 0;

			L = await this.translateY.TryEvaluate(State.Session);
			if (L.Ok)
				State.CalcDrawingSize(L.Result, ref this.dy, false, this.Parent);
			else
				this.dy = 0;

			SKMatrix M = SKMatrix.CreateTranslation(this.dx, this.dy);
			this.TransformBoundingBox(M);
		}

		private float dx;
		private float dy;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			SKMatrix M = State.Canvas.TotalMatrix;
			State.Canvas.Translate(this.dx, this.dy);

			await base.Draw(State);

			State.Canvas.SetMatrix(M);
		}

		/// <summary>
		/// Exports the local attributes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.translateX?.ExportState(Output);
			this.translateY?.ExportState(Output);
		}
	}
}
