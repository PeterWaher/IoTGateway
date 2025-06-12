using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Runtime.Collections;

namespace Waher.Layout.Layout2D.Model.Content.FlowingText
{
	/// <summary>
	/// Abstract base class of embedded text elements in flowing text.
	/// </summary>
	public abstract class EmbeddedText : LayoutElement, IFlowingText
	{
		private IFlowingText[] text;

		/// <summary>
		/// Abstract base class of embedded text elements in flowing text.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public EmbeddedText(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!(this.text is null))
			{
				foreach (ILayoutElement E in this.text)
					E.Dispose();
			}
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override async Task FromXml(XmlElement Input)
		{
			await base.FromXml(Input);

			ChunkedList<IFlowingText> Children = new ChunkedList<IFlowingText>();

			foreach (XmlNode Node in Input.ChildNodes)
			{
				if (Node is XmlElement E)
				{
					ILayoutElement Child = await this.Document.CreateElement(E, this);

					if (Child is IFlowingText Text)
						Children.Add(Text);
					else
						throw new LayoutSyntaxException("Not flowing text: " + E.NamespaceURI + "#" + E.LocalName);
				}
			}

			this.text = Children.ToArray();
		}

		/// <summary>
		/// Exports child elements to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportChildren(XmlWriter Output)
		{
			base.ExportChildren(Output);

			if (!(this.text is null))
			{
				foreach (ILayoutElement Child in this.text)
					Child.ToXml(Output);
			}
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is EmbeddedText Dest)
			{
				if (!(this.text is null))
				{
					int i, c = this.text.Length;

					IFlowingText[] Children = new IFlowingText[c];

					for (i = 0; i < c; i++)
						Children[i] = this.text[i].Copy(Dest) as IFlowingText;

					Dest.text = Children;
				}
			}
		}

		/// <summary>
		/// Measures text segments to a list of segments.
		/// </summary>
		/// <param name="Segments">List of segments.</param>
		/// <param name="State">Current drawing state.</param>
		public virtual async Task MeasureSegments(ChunkedList<Segment> Segments, DrawingState State)
		{
			if (!(this.text is null))
			{
				foreach (IFlowingText Text in this.text)
				{
					await Text.MeasureDimensions(State);
					if (Text.IsVisible)
						await Text.MeasureSegments(Segments, State);
				}
			}
		}

		/// <summary>
		/// Exports the current state of child nodes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateChildren(XmlWriter Output)
		{
			if (!(this.text is null))
			{
				foreach (ILayoutElement Child in this.text)
					Child.ExportState(Output);
			}
		}

	}
}
