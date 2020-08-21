using System;
using System.Text;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.References;

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
		private float? left;
		private float? right;
		private float? top;
		private float? bottom;
		private float? width;
		private float? height;
		private float? minWidth;
		private float? minHeight;
		private float? maxWidth;
		private float? maxHeight;

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
		public float? Left
		{
			get
			{
				if (this.left.HasValue)
					return this.left;
				else if (this.right.HasValue && this.width.HasValue)
					return this.right.Value - this.width.Value;
				else
					return null;
			}

			set
			{
				this.left = value;
				if (this.left.HasValue)
				{
					if (this.width.HasValue)
						this.right = null;
					else if (this.minWidth.HasValue || this.maxWidth.HasValue)
					{
						float? W = this.Width;
						if (W.HasValue)
						{
							if (this.minWidth.HasValue && W.Value < this.minWidth.Value)
								this.right = this.left.Value + this.minWidth.Value;
							else if (this.maxWidth.HasValue && W.Value > this.maxWidth.Value)
								this.right = this.left.Value + this.maxWidth.Value;
						}
					}
				}
			}
		}

		/// <summary>
		/// Right coordinate of bounding box, after measurement.
		/// </summary>
		public float? Right
		{
			get
			{
				if (this.right.HasValue)
					return this.right;
				else if (this.left.HasValue && this.width.HasValue)
					return this.left.Value + this.width.Value;
				else
					return null;
			}

			set
			{
				this.right = value;
				if (this.right.HasValue)
				{
					if (this.width.HasValue)
						this.left = null;
					else if (this.minWidth.HasValue || this.maxWidth.HasValue)
					{
						float? W = this.Width;
						if (W.HasValue)
						{
							if (this.minWidth.HasValue && W.Value < this.minWidth.Value)
								this.left = this.right.Value - this.minWidth.Value;
							else if (this.maxWidth.HasValue && W.Value > this.maxWidth.Value)
								this.left = this.right.Value - this.maxWidth.Value;
						}
					}
				}
			}
		}

		/// <summary>
		/// Top coordinate of bounding box, after measurement.
		/// </summary>
		public float? Top
		{
			get
			{
				if (this.top.HasValue)
					return this.top;
				else if (this.bottom.HasValue && this.height.HasValue)
					return this.bottom.Value - this.height.Value;
				else
					return null;
			}

			set
			{
				this.top = value;
				if (this.top.HasValue)
				{
					if (this.height.HasValue)
						this.bottom = null;
					else if (this.minHeight.HasValue || this.maxHeight.HasValue)
					{
						float? H = this.Height;
						if (H.HasValue)
						{
							if (this.minHeight.HasValue && H.Value < this.minHeight.Value)
								this.bottom = this.top.Value + this.minHeight.Value;
							else if (this.maxHeight.HasValue && H.Value > this.maxHeight.Value)
								this.bottom = this.top.Value + this.maxHeight.Value;
						}
					}
				}
			}
		}

		/// <summary>
		/// Bottom coordinate of bounding box, after measurement.
		/// </summary>
		public float? Bottom
		{
			get
			{
				if (this.bottom.HasValue)
					return this.bottom;
				else if (this.top.HasValue && this.height.HasValue)
					return this.top.Value + this.height.Value;
				else
					return null;
			}

			set
			{
				this.bottom = value;
				if (this.bottom.HasValue)
				{
					if (this.height.HasValue)
						this.top = null;
					else if (this.minHeight.HasValue || this.maxHeight.HasValue)
					{
						float? H = this.Height;
						if (H.HasValue)
						{
							if (this.minHeight.HasValue && H.Value < this.minHeight.Value)
								this.top = this.bottom.Value - this.minHeight.Value;
							else if (this.maxHeight.HasValue && H.Value > this.maxHeight.Value)
								this.top = this.bottom.Value - this.maxHeight.Value;
						}
					}
				}
			}
		}

		/// <summary>
		/// Width of element
		/// </summary>
		public float? Width
		{
			get
			{
				if (this.width.HasValue)
					return this.width;
				else if (this.left.HasValue && this.right.HasValue)
					return this.right.Value - this.left.Value;
				else
					return null;
			}

			set
			{
				if (value.HasValue)
				{
					if (this.minWidth.HasValue && value.Value < this.minWidth.Value)
						value = this.minWidth;
					else if (this.maxWidth.HasValue && value.Value > this.maxWidth.Value)
						value = this.maxWidth;

					if (this.left.HasValue)
						this.right = null;
				}

				this.width = value;
			}
		}

		/// <summary>
		/// Height of element
		/// </summary>
		public float? Height
		{
			get
			{
				if (this.height.HasValue)
					return this.height;
				else if (this.top.HasValue && this.bottom.HasValue)
					return this.bottom.Value - this.top.Value;
				else
					return null;
			}

			set
			{
				if (value.HasValue)
				{
					if (this.minHeight.HasValue && value.Value < this.minHeight.Value)
						value = this.minHeight;
					else if (this.maxHeight.HasValue && value.Value > this.maxHeight.Value)
						value = this.maxHeight;

					if (this.top.HasValue)
						this.bottom = null;
				}

				this.height = value;
			}
		}

		/// <summary>
		/// Minimum width.
		/// </summary>
		public float? MinWidth
		{
			get => this.minWidth;
			set
			{
				this.minWidth = value;
				if (value.HasValue)
				{
					float? W = this.Width;
					if (W.HasValue && W.Value < this.minWidth.Value)
						this.Width = this.minWidth;
				}
			}
		}

		/// <summary>
		/// Maximum width.
		/// </summary>
		public float? MaxWidth
		{
			get => this.maxWidth;
			set
			{
				this.maxWidth = value;
				if (value.HasValue)
				{
					float? W = this.Width;
					if (W.HasValue && W.Value > this.maxWidth.Value)
						this.Width = this.maxWidth;
				}
			}
		}

		/// <summary>
		/// Minimum height.
		/// </summary>
		public float? MinHeight
		{
			get => this.minHeight;
			set
			{
				this.minHeight = value;
				if (value.HasValue)
				{
					float? H = this.Height;
					if (H.HasValue && H.Value < this.minHeight.Value)
						this.Height = this.minHeight;
				}
			}
		}

		/// <summary>
		/// Maximum height.
		/// </summary>
		public float? MaxHeight
		{
			get => this.maxHeight;
			set
			{
				this.maxHeight = value;
				if (value.HasValue)
				{
					float? H = this.Height;
					if (H.HasValue && H.Value > this.maxHeight.Value)
						this.Height = this.maxHeight;
				}
			}
		}

		/// <summary>
		/// Bounding rectangle.
		/// </summary>
		public SKRect? BoundingRect
		{
			get
			{
				float? X = this.Left;
				float? Y = this.Top;
				float? R = this.Right;
				float? B = this.Bottom;

				if (X.HasValue && Y.HasValue && R.HasValue && B.HasValue)
					return new SKRect(X.Value, Y.Value, R.Value, B.Value);
				else
					return null;
			}
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
		/// ID of element
		/// </summary>
		public StringAttribute IdAttribute
		{
			get => this.id;
			set => this.id = value;
		}

		/// <summary>
		/// Visibility attribute
		/// </summary>
		public BooleanAttribute VisibleAttribute
		{
			get => this.visible;
			set => this.visible = value;
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
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public virtual void MeasureDimensions(DrawingState State)
		{
			if (this.visible.TryEvaluate(State.Session, out bool b))
				this.isVisible = b;

			this.defined = true;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public virtual void MeasurePositions(DrawingState State)
		{
		}

		/// <summary>
		/// Transforms the measured bounding box.
		/// </summary>
		/// <param name="M">Transformation matrix.</param>
		protected void TransformBoundingBox(SKMatrix M)
		{
			float? L = this.Left;
			float? R = this.Right;
			float? T = this.Top;
			float? B = this.Bottom;

			if (L.HasValue && R.HasValue && T.HasValue && B.HasValue)
			{
				SKRect Rect = new SKRect(L.Value, T.Value, R.Value, B.Value);

				Rect = M.MapRect(Rect);

				this.width = null;
				this.height = null;
				this.left = Rect.Left;
				this.top = Rect.Top;
				this.right = Rect.Right;
				this.bottom = Rect.Bottom;
			}
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
				this.Document.TryGetElement(RefId, out ILayoutElement Element))
			{
				X = Element.Left ?? 0;
				Y = Element.Top ?? 0;

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

		/// <summary>
		/// Draw the shape represented by the layout element.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public virtual void DrawShape(DrawingState State)
		{
			this.Draw(State);
		}

		/// <summary>
		/// Calculates the direction of the pen when drawing from (x1,y1) to (x2,y2).
		/// </summary>
		/// <param name="x1">First X-coordinate</param>
		/// <param name="y1">First Y-coordinate</param>
		/// <param name="x2">Second X-coordinate</param>
		/// <param name="y2">Second Y-coordinate</param>
		/// <returns>Direction angle</returns>
		public static float CalcDirection(float x1, float y1, float x2, float y2)
		{
			return CalcDirection(x2 - x1, y2 - y1);
		}

		/// <summary>
		/// Calculates the direction of the pen when drawing in the direction of (dx,dy)
		/// </summary>
		/// <param name="dx">Delta-X</param>
		/// <param name="dy">Delta-Y</param>
		/// <returns>Direction angle</returns>
		public static float CalcDirection(float dx, float dy)
		{
			if (dx == 0 && dy == 0)
				return 0;
			else
				return (float)Math.Atan2(dy, dx);
		}

		/// <summary>
		/// Calculates the direction of the pen when drawing from P1 to P2.
		/// </summary>
		/// <param name="P1">First point</param>
		/// <param name="P2">Second point</param>
		/// <returns>Direction angle</returns>
		public static float CalcDirection(SKPoint P1, SKPoint P2)
		{
			return CalcDirection(P2.X - P1.X, P2.Y - P1.Y);
		}

		/// <summary>
		/// Calculates the direction of the pen when drawing from P1 to P2.
		/// </summary>
		/// <param name="P1">First vertex</param>
		/// <param name="P2">Second vertex</param>
		/// <returns>Direction angle</returns>
		public static float CalcDirection(Vertex P1, Vertex P2)
		{
			return CalcDirection(P2.XCoordinate - P1.XCoordinate, P2.YCoordinate - P1.YCoordinate);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append('(');

			if (this.left.HasValue)
				sb.Append(this.left.Value.ToString());

			sb.Append(", ");

			if (this.top.HasValue)
				sb.Append(this.right.Value.ToString());

			sb.Append(") - (");

			if (this.right.HasValue)
				sb.Append(this.right.Value.ToString());

			sb.Append(", ");

			if (this.bottom.HasValue)
				sb.Append(this.bottom.Value.ToString());

			sb.Append(") (");

			if (this.width.HasValue)
				sb.Append(this.width.Value.ToString());

			sb.Append(" x ");

			if (this.height.HasValue)
				sb.Append(this.height.Value.ToString());

			sb.Append("): ");
			sb.Append(this.LocalName);

			return sb.ToString();
		}
	}
}
