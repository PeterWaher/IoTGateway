using System;
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
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.width = new LengthAttribute(Input, "width");
			this.cap = new EnumAttribute<SKStrokeCap>(Input, "cap");
			this.join = new EnumAttribute<SKStrokeJoin>(Input, "join");
			this.miter = new LengthAttribute(Input, "miter");
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
				Dest.width = this.width?.CopyIfNotPreset();
				Dest.cap = this.cap?.CopyIfNotPreset();
				Dest.join = this.join?.CopyIfNotPreset();
				Dest.miter = this.miter?.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override bool DoMeasureDimensions(DrawingState State)
		{
			bool Relative = base.DoMeasureDimensions(State);
			float a;

			if (!(this.width is null) && this.width.TryEvaluate(State.Session, out Length Width))
			{
				a = this.penWidth ?? 0;
				State.CalcDrawingSize(Width, ref a, true, ref Relative);
				this.penWidth = a;
			}
			else
				this.penWidth = null;

			if (!(this.cap is null) && this.cap.TryEvaluate(State.Session, out SKStrokeCap Cap))
				this.penCap = Cap;
			else
				this.penCap = null;

			if (!(this.join is null) && this.join.TryEvaluate(State.Session, out SKStrokeJoin Join))
				this.penJoin = Join;
			else
				this.penJoin = null;

			if (!(this.miter is null) && this.miter.TryEvaluate(State.Session, out Width))
			{
				a = this.penMiter ?? 0;
				State.CalcDrawingSize(Width, ref a, true, ref Relative);
				this.penMiter = a;
			}
			else
				this.penMiter = null;

			return Relative;
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
