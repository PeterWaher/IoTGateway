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
		private bool firstPoint = true;

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
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!(this.children is null))
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

			if (!(this.children is null))
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
				if (!(this.children is null))
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
		/// Measures layout entities and defines unassigned properties.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Measure(DrawingState State)
		{
			base.Measure(State);

			this.firstPoint = true;
			this.Left = this.Right = this.Top = this.Bottom = 0;
			
			if (!(this.children is null))
			{
				foreach (ILayoutElement E in this.children)
				{
					E.Measure(State);

					this.IncludePoint(E.Left, E.Top);
					this.IncludePoint(E.Right, E.Bottom);
				}
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
			if (this.firstPoint)
			{
				this.firstPoint = false;

				this.Left = this.Right = X;
				this.Top = this.Bottom = Y;
			}
			else
			{
				if (X < this.Left)
					this.Left = X;
				else if (X > this.Right)
					this.Right = X;

				if (Y < this.Top)
					this.Top = Y;
				else if (Y > this.Bottom)
					this.Bottom = Y;
			}
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
			if (!(this.children is null))
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
