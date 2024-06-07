using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.References;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A line
	/// </summary>
	public class Line : FigurePoint2, IDirectedElement
	{
		private StringAttribute head;
		private StringAttribute tail;

		/// <summary>
		/// A line
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Line(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Line";

		/// <summary>
		/// Head
		/// </summary>
		public StringAttribute HeadAttribute
		{
			get => this.head;
			set => this.head = value;
		}

		/// <summary>
		/// Tail
		/// </summary>
		public StringAttribute TailAttribute
		{
			get => this.tail;
			set => this.tail = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.head = new StringAttribute(Input, "head", this.Document);
			this.tail = new StringAttribute(Input, "tail", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.head?.Export(Output);
			this.tail?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Line(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Line Dest)
			{
				Dest.head = this.head?.CopyIfNotPreset(Destination.Document);
				Dest.tail = this.tail?.CopyIfNotPreset(Destination.Document);
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

			EvaluationResult<string> RefId = await this.head.TryEvaluate(State.Session);
			if (RefId.Ok &&
				this.Document.TryGetElement(RefId.Result, out ILayoutElement Element) &&
				Element is Shape Shape)
			{
				this.headElement = Shape;
			}

			RefId = await this.tail.TryEvaluate(State.Session);
			if (RefId.Ok &&
				this.Document.TryGetElement(RefId.Result, out Element) &&
				Element is Shape Shape2)
			{
				this.tailElement = Shape2;
			}
		}

		private Shape headElement;
		private Shape tailElement;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			if (this.defined)
			{
				SKPaint Pen = await this.GetPen(State);

				State.Canvas.DrawLine(this.xCoordinate, this.yCoordinate,
					this.xCoordinate2, this.yCoordinate2, Pen);

				if (!(this.tailElement is null) || !(this.headElement is null))
				{
					SKPaint Fill = await this.TryGetFill(State);

					this.tailElement?.DrawTail(State, this, Pen, Fill);
					this.headElement?.DrawHead(State, this, Pen, Fill);
				}
			}

			await base.Draw(State);
		}

		/// <summary>
		/// Tries to get start position and initial direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Initial direction.</param>
		/// <returns>If a start position was found.</returns>
		public bool TryGetStart(out float X, out float Y, out float Direction)
		{
			X = this.xCoordinate;
			Y = this.yCoordinate;
			Direction = CalcDirection(this.xCoordinate, this.yCoordinate, this.xCoordinate2, this.yCoordinate2);

			return this.defined;
		}

		/// <summary>
		/// Tries to get end position and terminating direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Terminating direction.</param>
		/// <returns>If a terminating position was found.</returns>
		public bool TryGetEnd(out float X, out float Y, out float Direction)
		{
			X = this.xCoordinate2;
			Y = this.yCoordinate2;
			Direction = CalcDirection(this.xCoordinate, this.yCoordinate, this.xCoordinate2, this.yCoordinate2);

			return this.defined;
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateAttributes(XmlWriter Output)
		{
			base.ExportStateAttributes(Output);

			this.head?.ExportState(Output);
			this.tail?.ExportState(Output);
		}

	}
}
