using System;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Pens
{
	/// <summary>
	/// Abstract base class for pens.
	/// </summary>
	public abstract class Pen : LayoutElement
	{
		private LengthAttribute width;
		private EnumAttribute<SKStrokeCap> cap;
		private EnumAttribute<SKStrokeJoin> join;
		private LengthAttribute miter;

		/// <summary>
		/// Current pen
		/// </summary>
		protected SKPaint paint;

		/// <summary>
		/// Abstract base class for pens.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Pen(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.paint?.Dispose();
			this.paint = null;
		}

		/// <summary>
		/// Current pen
		/// </summary>
		public SKPaint Paint => this.paint;

		/// <summary>
		/// Width
		/// </summary>
		public LengthAttribute WidthAttribute
		{
			get => this.width;
			set => this.width = value;
		}

		/// <summary>
		/// Cap
		/// </summary>
		public EnumAttribute<SKStrokeCap> CapAttribute
		{
			get => this.cap;
			set => this.cap = value;
		}

		/// <summary>
		/// Join
		/// </summary>
		public EnumAttribute<SKStrokeJoin> JoinAttribute
		{
			get => this.join;
			set => this.join = value;
		}

		/// <summary>
		/// Miter
		/// </summary>
		public LengthAttribute MiterAttribute
		{
			get => this.miter;
			set => this.miter = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.width = new LengthAttribute(Input, "width", this.Document);
			this.cap = new EnumAttribute<SKStrokeCap>(Input, "cap", this.Document);
			this.join = new EnumAttribute<SKStrokeJoin>(Input, "join", this.Document);
			this.miter = new LengthAttribute(Input, "miter", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.width?.Export(Output);
			this.cap?.Export(Output);
			this.join?.Export(Output);
			this.miter?.Export(Output);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Pen Dest)
			{
				Dest.width = this.width?.CopyIfNotPreset(Destination.Document);
				Dest.cap = this.cap?.CopyIfNotPreset(Destination.Document);
				Dest.join = this.join?.CopyIfNotPreset(Destination.Document);
				Dest.miter = this.miter?.CopyIfNotPreset(Destination.Document);
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
			float a;

			EvaluationResult<Length> Length = await this.width.TryEvaluate(State.Session);
			if (Length.Ok)
			{
				a = this.penWidth ?? 0;
				State.CalcDrawingSize(Length.Result, ref a, true);
				this.penWidth = a;
			}
			else
				this.penWidth = null;

			EvaluationResult<SKStrokeCap> Cap = await this.cap.TryEvaluate(State.Session);
			if (Cap.Ok)
				this.penCap = Cap.Result;
			else
				this.penCap = null;

			EvaluationResult<SKStrokeJoin> Join = await this.join.TryEvaluate(State.Session);
			if (Join.Ok)
				this.penJoin = Join.Result;
			else
				this.penJoin = null;

			Length = await this.miter.TryEvaluate(State.Session);
			if (Length.Ok)
			{
				a = this.penMiter ?? 0;
				State.CalcDrawingSize(Length.Result, ref a, true);
				this.penMiter = a;
			}
			else
				this.penMiter = null;
		}

		/// <summary>
		/// Measured pen width.
		/// </summary>
		protected float? penWidth;

		/// <summary>
		/// Measured pen stroke cap.
		/// </summary>
		protected SKStrokeCap? penCap;

		/// <summary>
		/// Measured pen stroke join.
		/// </summary>
		protected SKStrokeJoin? penJoin;

		/// <summary>
		/// Measured pen miter.
		/// </summary>
		protected float? penMiter;

	}
}
