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
		/// <summary>
		/// Pi / 180
		/// </summary>
		public const float DegreesToRadians = (float)(Math.PI / 180);

		private StringAttribute id;
		private BooleanAttribute visible;
		private readonly Layout2DDocument document;
		private readonly ILayoutElement parent;
		private bool isVisible = true;

		/// <summary>
		/// If element is well-defined.
		/// </summary>
		protected bool defined;

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
		public float Left
		{
			get;
			set;
		}

		/// <summary>
		/// Right coordinate of bounding box, after measurement.
		/// </summary>
		public float Right
		{
			get;
			set;
		}

		/// <summary>
		/// Top coordinate of bounding box, after measurement.
		/// </summary>
		public float Top
		{
			get;
			set;
		}

		/// <summary>
		/// Bottom coordinate of bounding box, after measurement.
		/// </summary>
		public float Bottom
		{
			get;
			set;
		}

		/// <summary>
		/// Width of element
		/// </summary>
		public float Width
		{
			get => this.Right - this.Left;
			set => this.Right = this.Left + value;
		}

		/// <summary>
		/// Height of element
		/// </summary>
		public float Height
		{
			get => this.Bottom - this.Top;
			set => this.Bottom = this.Top + value;
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
		public virtual void Measure(DrawingState State)
		{
			if (this.id.TryEvaluate(State.Session, out string Id) && !string.IsNullOrEmpty(Id))
				State.AddElementId(Id, this);

			if (this.visible.TryEvaluate(State.Session, out bool b))
				this.isVisible = b;

			this.defined = true;
		}

		/// <summary>
		/// If the element is visible or not.
		/// </summary>
		public bool IsVisible => this.isVisible;

		/// <summary>
		/// Includes a point in the area measurement.
		/// </summary>
		/// <param name="State">Current drawing state</param>
		/// <param name="XAttribute">X-Coordinate attribute</param>
		/// <param name="YAttribute">Y-Coordinate attribute</param>
		/// <param name="RefAttribute">Reference attribute</param>
		/// <param name="X">Resulting X-coordinate.</param>
		/// <param name="Y">Resulting Y-coordinate.</param>
		/// <returns>If point is well-defined.</returns>
		protected bool CalcPoint(DrawingState State, LengthAttribute XAttribute,
			LengthAttribute YAttribute, StringAttribute RefAttribute,
			out float X, out float Y)
		{
			if (XAttribute.TryEvaluate(State.Session, out Length X1) &&
				YAttribute.TryEvaluate(State.Session, out Length Y1))
			{
				X = State.GetDrawingSize(X1, this, true);
				Y = State.GetDrawingSize(Y1, this, false);

				return true;
			}
			else if (!(RefAttribute is null) &&
				RefAttribute.TryEvaluate(State.Session, out string RefId) &&
				State.TryGetElement(RefId, out ILayoutElement Element))
			{
				X = Element.Left;
				Y = Element.Top;

				return true;
			}
			else
			{
				X = Y = 0;
				return false;
			}
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public virtual void Draw(DrawingState State)
		{
		}

	}
}
