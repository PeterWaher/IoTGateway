﻿using SkiaSharp;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Figures.SegmentNodes
{
	/// <summary>
	/// Replays the segments of another path.
	/// </summary>
	public class Replay : LayoutElement, ISegment
	{
		private StringAttribute @ref;

		/// <summary>
		/// Replays the segments of another path.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Replay(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Replay";

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
			return new Replay(Document, Parent);
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Replay Dest)
				Dest.@ref = this.@ref?.CopyIfNotPreset(Destination.Document);
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		public virtual async Task Measure(DrawingState State, PathState PathState)
		{
			if (this.defined)
			{
				EvaluationResult<string> RefId = await this.@ref.TryEvaluate(State.Session);

				if (RefId.Ok &&
					this.Document.TryGetElement(RefId.Result, out ILayoutElement Element) &&
					Element is ISegment Segment)
				{
					this.reference = Segment;
					await this.reference.Measure(State, PathState);
				}
				else
				{
					this.reference = null;
					this.defined = false;
				}
			}
			else
				this.reference = null;
		}

		private ISegment reference;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <param name="PathState">Current path state.</param>
		/// <param name="Path">Path being generated.</param>
		public virtual Task Draw(DrawingState State, PathState PathState, SKPath Path)
		{
			if (this.defined)
				this.reference.Draw(State, PathState, Path);
		
			return Task.CompletedTask;
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
