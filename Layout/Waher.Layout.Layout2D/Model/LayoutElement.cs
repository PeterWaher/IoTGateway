using System;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class for layout elements.
	/// </summary>
	public abstract class LayoutElement : ILayoutElement
	{
		private StringAttribute id;
		private BooleanAttribute visible;
		private readonly Layout2DDocument document;
		private readonly ILayoutElement parent;

		/// <summary>
		/// Abstract base class for layout elements.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public LayoutElement(Layout2DDocument Document, ILayoutElement Parent)
		{
			this.document = Document;
			this.parent = Parent;
		}

		/// <summary>
		/// Layout document.
		/// </summary>
		public Layout2DDocument Document => this.document;

		/// <summary>
		/// Parent element.
		/// </summary>
		public ILayoutElement Parent => this.parent;

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public abstract string LocalName
		{
			get;
		}

		/// <summary>
		/// Left coordinate of bounding box, after measurement.
		/// </summary>
		public double Left
		{
			get;
			set;
		}

		/// <summary>
		/// Right coordinate of bounding box, after measurement.
		/// </summary>
		public double Right
		{
			get;
			set;
		}

		/// <summary>
		/// Top coordinate of bounding box, after measurement.
		/// </summary>
		public double Top
		{
			get;
			set;
		}

		/// <summary>
		/// Bottom coordinate of bounding box, after measurement.
		/// </summary>
		public double Bottom
		{
			get;
			set;
		}

		/// <summary>
		/// Namespace of type of element.
		/// </summary>
		public virtual string Namespace => Layout2DDocument.Namespace;

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public abstract ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent);

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public virtual void FromXml(XmlElement Input)
		{
			this.id = new StringAttribute(Input, "id");
			this.visible = new BooleanAttribute(Input, "visible");
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public void ToXml(XmlWriter Output)
		{
			Output.WriteStartElement(this.LocalName, this.Namespace);
			this.ExportAttributes(Output);
			this.ExportChildren(Output);
			Output.WriteEndElement();
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public virtual void ExportAttributes(XmlWriter Output)
		{
			this.id.Export(Output);
			this.visible.Export(Output);
		}

		/// <summary>
		/// Exports child elements to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public virtual void ExportChildren(XmlWriter Output)
		{
		}

		/// <summary>
		/// Creates a copy of the layout element.
		/// </summary>
		/// <param name="Parent">Parent of the new element.</param>
		/// <returns></returns>
		public ILayoutElement Copy(ILayoutElement Parent)
		{
			ILayoutElement Result = this.Create(this.document, Parent);
			this.CopyContents(Result);
			return Result;
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public virtual void CopyContents(ILayoutElement Destination)
		{
			if (Destination is LayoutElement Dest)
			{
				Dest.id = this.id.CopyIfNotPreset();
				Dest.visible = this.visible.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public abstract void Measure(DrawingState State);

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public abstract void Draw(DrawingState State);

	}
}
