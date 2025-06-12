﻿using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.References;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A poly-line
	/// </summary>
	public class PolyLine : Vertices, IDirectedElement
	{
		private StringAttribute head;
		private StringAttribute tail;

		/// <summary>
		/// A poly-line
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public PolyLine(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "PolyLine";

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
			return new PolyLine(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is PolyLine Dest)
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

				using (SKPath Path = new SKPath())
				{
					bool First = true;

					foreach (Vertex V in this.points)
					{
						if (First)
						{
							Path.MoveTo(V.XCoordinate, V.YCoordinate);
							First = false;
						}
						else
							Path.LineTo(V.XCoordinate, V.YCoordinate);
					}

					State.Canvas.DrawPath(Path, Pen);
				}

				if (!(this.tailElement is null) || !(this.headElement is null))
				{
					SKPaint Fill = await this.TryGetFill(State);
					if (!(Fill is null))
						Fill = null;

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
			if (!this.defined || this.points is null || this.points.Length < 2)
			{
				X = Y = Direction = 0;
				return false;
			}

			Vertex P1 = this.points[0];
			Vertex P2 = this.points[1];

			X = P1.XCoordinate;
			Y = P1.YCoordinate;
			Direction = CalcDirection(P1, P2);

			return true;
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
			int c;

			if (!this.defined || this.points is null || (c = this.points.Length) < 2)
			{
				X = Y = Direction = 0;
				return false;
			}

			Vertex P1 = this.points[c - 2];
			Vertex P2 = this.points[c - 1];

			X = P2.XCoordinate;
			Y = P2.YCoordinate;
			Direction = CalcDirection(P1, P2);

			return true;
		}

		/// <summary>
		/// Exports the local attributes of the current element.
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
