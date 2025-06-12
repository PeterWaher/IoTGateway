﻿using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Transforms
{
	/// <summary>
	/// A scale transform
	/// </summary>
	public class Scale : PivotTrasformation
	{
		private FloatAttribute scaleX;
		private FloatAttribute scaleY;

		/// <summary>
		/// A scale transform
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Scale(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Scale";

		/// <summary>
		/// Scale X
		/// </summary>
		public FloatAttribute ScaleXAttribute
		{
			get => this.scaleX;
			set => this.scaleX = value;
		}

		/// <summary>
		/// Scale Y
		/// </summary>
		public FloatAttribute ScaleYAttribute
		{
			get => this.scaleY;
			set => this.scaleY = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override Task FromXml(XmlElement Input)
		{
			this.scaleX = new FloatAttribute(Input, "scaleX", this.Document);
			this.scaleY = new FloatAttribute(Input, "scaleY", this.Document);

			return base.FromXml(Input);
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.scaleX?.Export(Output);
			this.scaleY?.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Scale(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Scale Dest)
			{
				Dest.scaleX = this.scaleX?.CopyIfNotPreset(Destination.Document);
				Dest.scaleY = this.scaleY?.CopyIfNotPreset(Destination.Document);
			}
		}

		/// <summary>
		/// Called when dimensions have been measured.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task AfterMeasureDimensions(DrawingState State)
		{
			await base.AfterMeasureDimensions(State);

			this.sx = await this.scaleX.Evaluate(State.Session, 1);
			this.sy = await this.scaleY.Evaluate(State.Session, 1);

			SKMatrix M = SKMatrix.CreateScale(this.sx, this.sy, this.xCoordinate, this.yCoordinate);
			this.TransformBoundingBox(M);
		}

		private float sx;
		private float sy;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			SKMatrix M = State.Canvas.TotalMatrix;
			State.Canvas.Scale(this.sx, this.sy, this.xCoordinate, this.yCoordinate);

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

			this.scaleX?.ExportState(Output);
			this.scaleY?.ExportState(Output);
		}
	}
}
