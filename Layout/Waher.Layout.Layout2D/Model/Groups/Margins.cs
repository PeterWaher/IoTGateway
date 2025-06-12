using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Groups
{
	/// <summary>
	/// A translation transform
	/// </summary>
	public class Margins : LayoutContainer
	{
		private LengthAttribute left;
		private LengthAttribute right;
		private LengthAttribute top;
		private LengthAttribute bottom;

		/// <summary>
		/// A translation transform
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Margins(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Margins";

		/// <summary>
		/// Left
		/// </summary>
		public LengthAttribute LeftAttribute
		{
			get => this.left;
			set => this.left = value;
		}

		/// <summary>
		/// Right
		/// </summary>
		public LengthAttribute RightAttribute
		{
			get => this.right;
			set => this.right = value;
		}

		/// <summary>
		/// Top
		/// </summary>
		public LengthAttribute TopAttribute
		{
			get => this.top;
			set => this.top = value;
		}

		/// <summary>
		/// Bottom
		/// </summary>
		public LengthAttribute BottomAttribute
		{
			get => this.bottom;
			set => this.bottom = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.left = new LengthAttribute(Input, "left", this.Document);
			this.right = new LengthAttribute(Input, "right", this.Document);
			this.top = new LengthAttribute(Input, "top", this.Document);
			this.bottom = new LengthAttribute(Input, "bottom", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.left?.Export(Output);
			this.right?.Export(Output);
			this.top?.Export(Output);
			this.bottom?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Margins(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Margins Dest)
			{
				Dest.left = this.left?.CopyIfNotPreset(Destination.Document);
				Dest.right = this.right?.CopyIfNotPreset(Destination.Document);
				Dest.top = this.top?.CopyIfNotPreset(Destination.Document);
				Dest.bottom = this.bottom?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			EvaluationResult<Length> L = await this.left.TryEvaluate(State.Session);
			if (L.Ok)
				State.CalcDrawingSize(L.Result, ref this.leftMargin, true, this);
			else
				this.leftMargin = 0;

			L = await this.right.TryEvaluate(State.Session);
			if (L.Ok)
				State.CalcDrawingSize(L.Result, ref this.rightMargin, true, this);
			else
				this.rightMargin = 0;

			L = await this.top.TryEvaluate(State.Session);
			if (L.Ok)
				State.CalcDrawingSize(L.Result, ref this.topMargin, true, this);
			else
				this.topMargin = 0;

			L = await this.bottom.TryEvaluate(State.Session);
			if (L.Ok)
				State.CalcDrawingSize(L.Result, ref this.bottomMargin, true, this);
			else
				this.bottomMargin = 0;

			float InnerWidth = this.ExplicitWidth ?? this.Parent?.InnerWidth ?? State.AreaWidth;
			float InnerHeight = this.ExplicitHeight ?? this.Parent?.InnerHeight ?? State.AreaHeight;

			SKSize Prev = State.SetAreaSize(new SKSize(
				InnerWidth - this.leftMargin - this.rightMargin,
				InnerHeight - this.topMargin - this.bottomMargin));

			await base.DoMeasureDimensions(State);

			State.SetAreaSize(Prev);
		}

		/// <summary>
		/// Called when dimensions have been measured.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task AfterMeasureDimensions(DrawingState State)
		{
			await base.AfterMeasureDimensions(State);

			this.innerWidth = this.Width;
			this.Width = this.innerWidth + this.leftMargin + this.rightMargin;

			this.innerHeight = this.Height;
			this.Height = this.innerHeight + this.topMargin + this.bottomMargin;

		}

		/// <summary>
		/// Inner Width of element
		/// </summary>
		public override float? InnerWidth => this.innerWidth;

		/// <summary>
		/// Inner Height of element
		/// </summary>
		public override float? InnerHeight => this.innerHeight;

		/// <summary>
		/// Potential Width of element
		/// </summary>
		public override float? PotentialWidth
		{
			get
			{
				float? f = base.PotentialWidth;

				if (f.HasValue)
					return f.Value - this.leftMargin - this.rightMargin;
				else
					return null;
			}
		}

		/// <summary>
		/// Potential Height of element
		/// </summary>
		public override float? PotentialHeight
		{
			get
			{
				float? f = base.PotentialHeight;

				if (f.HasValue)
					return f.Value - this.topMargin - this.bottomMargin;
				else
					return null;
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			float InnerWidth = this.ExplicitWidth ?? this.Parent?.InnerWidth ?? State.AreaWidth;
			float InnerHeight = this.ExplicitHeight ?? this.Parent?.InnerHeight ?? State.AreaHeight;

			SKSize Prev = State.SetAreaSize(new SKSize(
				InnerWidth - this.leftMargin - this.rightMargin,
				InnerHeight - this.topMargin - this.bottomMargin));

			base.MeasurePositions(State);

			State.SetAreaSize(Prev);
		}

		private float? innerWidth;
		private float? innerHeight;
		private float leftMargin;
		private float rightMargin;
		private float topMargin;
		private float bottomMargin;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			SKMatrix M = State.Canvas.TotalMatrix;
			State.Canvas.Translate(this.leftMargin, this.topMargin);

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

			this.left?.ExportState(Output);
			this.right?.ExportState(Output);
			this.top?.ExportState(Output);
			this.bottom?.ExportState(Output);
		}
	}
}
