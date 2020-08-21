using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Abstract base class for layout containers (area elements containing 
	/// embedded layout elements).
	/// </summary>
	public abstract class LayoutContainer : LayoutArea
	{
		private ILayoutElement[] children;

		/// <summary>
		/// Abstract base class for layout containers (area elements containing 
		/// embedded layout elements).
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public LayoutContainer(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Child elements
		/// </summary>
		public ILayoutElement[] Children
		{
			get => this.children;
			set => this.children = value;
		}

		/// <summary>
		/// If the element has children or not.
		/// </summary>
		public bool HasChildren => !(this.children is null) && this.children.Length > 0;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (this.HasChildren)
			{
				foreach (ILayoutElement E in this.children)
					E.Dispose();
			}
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			List<ILayoutElement> Children = null;

			foreach (XmlNode Node in Input.ChildNodes)
			{
				if (Node is XmlElement E)
				{
					if (Children is null)
						Children = new List<ILayoutElement>();

					Children.Add(this.Document.CreateElement(E, this));
				}
			}

			this.children = Children?.ToArray();
		}

		/// <summary>
		/// Exports child elements to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportChildren(XmlWriter Output)
		{
			base.ExportChildren(Output);

			if (this.HasChildren)
			{
				foreach (ILayoutElement Child in this.children)
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

			if (Destination is LayoutContainer Dest)
			{
				if (this.HasChildren)
				{
					int i, c = this.children.Length;

					ILayoutElement[] Children = new ILayoutElement[c];

					for (i = 0; i < c; i++)
						Children[i] = this.children[i].Copy(Dest);

					Dest.children = Children;
				}
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasureDimensions(DrawingState State)
		{
			base.MeasureDimensions(State);

			if (this.HasChildren)
			{
				foreach (ILayoutElement E in this.children)
				{
					E.MeasureDimensions(State);

					float? X = E.Left;
					float? Y = E.Top;

					if (X.HasValue && Y.HasValue)
						this.IncludePoint(X.Value, Y.Value);

					X = E.Right;
					Y = E.Bottom;

					if (X.HasValue && Y.HasValue)
						this.IncludePoint(X.Value, Y.Value);
				}
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			base.MeasurePositions(State);

			if (this.HasChildren)
			{
				foreach (ILayoutElement E in this.children)
					E.MeasurePositions(State);
			}
		}

		/// <summary>
		/// Includes a point in the area measurement.
		/// </summary>
		/// <param name="X">X-Coordinate of center-point</param>
		/// <param name="Y">Y-Coordinate of center-point</param>
		/// <param name="RX">Radius along X-axis</param>
		/// <param name="RY">Radius along Y-axis</param>
		/// <param name="Angle">Angle, in degrees, clockwise from positive horizontal X-axis.</param>
		protected void IncludePoint(float X, float Y, float RX, float RY, float Angle)
		{
			float Px = (float)(X + RX * Math.Cos(Angle * DegreesToRadians));
			float Py = (float)(Y + RY * Math.Sin(Angle * DegreesToRadians));

			this.IncludePoint(Px, Py);
		}

		/// <summary>
		/// Includes a point in the area measurement.
		/// </summary>
		/// <param name="X">X-Coordinate</param>
		/// <param name="Y">Y-Coordinate</param>
		public void IncludePoint(float X, float Y)
		{
			if (!this.Left.HasValue || X < this.Left.Value)
				this.Left = X;

			if (!this.Right.HasValue || X > this.Right.Value)
				this.Right = X;

			if (!this.Top.HasValue || Y < this.Top.Value)
				this.Top = Y;

			if (!this.Bottom.HasValue || Y > this.Bottom.Value)
				this.Bottom = Y;
		}

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
		protected bool IncludePoint(DrawingState State, LengthAttribute XAttribute,
			LengthAttribute YAttribute, StringAttribute RefAttribute,
			out float X, out float Y)
		{
			bool Result = this.CalcPoint(State, XAttribute, YAttribute, RefAttribute, out X, out Y);

			if (Result)
				this.IncludePoint(X, Y);

			return Result;
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			if (this.HasChildren)
			{
				foreach (ILayoutElement E in this.children)
				{
					if (E.IsVisible)
						E.Draw(State);
				}
			}
		}

	}
}
