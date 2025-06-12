﻿using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Runtime.Collections;
using Waher.Script;

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
		public override async Task FromXml(XmlElement Input)
		{
			await base.FromXml(Input);

			ChunkedList<ILayoutElement> Children = null;

			foreach (XmlNode Node in Input.ChildNodes)
			{
				if (Node is XmlElement E)
				{
					if (Children is null)
						Children = new ChunkedList<ILayoutElement>();

					Children.Add(await this.Document.CreateElement(E, this));
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
		/// Called before dimensions are measured.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void BeforeMeasureDimensions(DrawingState State)
		{
			this.left0 = this.right0 = this.top0 = this.bottom0 = this.width0 = this.height0 = null;
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		/// <returns>If layout contains relative sizes and dimensions should be recalculated.</returns>
		public override async Task DoMeasureDimensions(DrawingState State)
		{
			await base.DoMeasureDimensions(State);

			if (this.HasChildren && this.MeasureChildrenDimensions)
			{
				foreach (ILayoutElement E in this.children)
				{
					E.BeforeMeasureDimensions(State);

					await E.DoMeasureDimensions(State);

					await E.AfterMeasureDimensions(State);
					this.IncludeElement(E);
				}
			}
		}

		/// <summary>
		/// Called when dimensions have been measured.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override Task AfterMeasureDimensions(DrawingState State)
		{
			float f;

			if (this.left0.HasValue && this.right0.HasValue)
			{
				f = this.right0.Value - this.left0.Value;

				if (!this.width0.HasValue || f > this.width0.Value)
					this.width0 = f;
			}

			if (this.top0.HasValue && this.bottom0.HasValue)
			{
				f = this.bottom0.Value - this.top0.Value;

				if (!this.height0.HasValue || f > this.height0.Value)
					this.height0 = f;
			}

			this.Left = this.Right = this.Width = null;
			this.Top = this.Bottom = this.Height = null;

			if (this.width0.HasValue || this.ExplicitWidth.HasValue)
			{
				this.Width = this.ExplicitWidth ?? this.width0;
				if (this.left0.HasValue)
					this.Left = this.left0;
				else
					this.Right = this.right0;
			}
			else
			{
				this.Right = this.right0;
				this.Left = this.left0;
			}

			if (this.height0.HasValue || this.ExplicitHeight.HasValue)
			{
				this.Height = this.ExplicitHeight ?? this.height0;
				if (this.top0.HasValue)
					this.Top = this.top0;
				else
					this.Bottom = this.bottom0;
			}
			else
			{
				this.Bottom = this.bottom0;
				this.Top = this.top0;
			}

			return Task.CompletedTask;
		}

		private float? left0;
		private float? right0;
		private float? top0;
		private float? bottom0;
		private float? width0;
		private float? height0;

		/// <summary>
		/// Includes element in dimension measurement.
		/// </summary>
		/// <param name="Element">Element to include.</param>
		protected void IncludeElement(ILayoutElement Element)
		{
			float? X = Element.Left;
			float? Y = Element.Top;
			float? V;

			if (X.HasValue && Y.HasValue)
				this.IncludePoint(X.Value, Y.Value);

			V = Element.Width;
			if (!this.width0.HasValue || (V.HasValue && V.Value > this.width0.Value))
				this.width0 = V;

			X = Element.Right;
			Y = Element.Bottom;

			if (X.HasValue && Y.HasValue)
				this.IncludePoint(X.Value, Y.Value);

			V = Element.Height;
			if (!this.height0.HasValue || (V.HasValue && V.Value > this.height0.Value))
				this.height0 = V;
		}

		/// <summary>
		/// If children dimensions are to be measured.
		/// </summary>
		protected virtual bool MeasureChildrenDimensions => true;

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to positions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasurePositions(DrawingState State)
		{
			base.MeasurePositions(State);

			if (this.HasChildren && this.MeasureChildrenPositions)
			{
				foreach (ILayoutElement E in this.children)
					E.MeasurePositions(State);
			}
		}

		/// <summary>
		/// If children positions are to be measured.
		/// </summary>
		protected virtual bool MeasureChildrenPositions => true;

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
			if (!this.left0.HasValue || X < this.left0.Value)
				this.left0 = X;

			if (!this.right0.HasValue || X > this.right0.Value)
				this.right0 = X;

			if (!this.top0.HasValue || Y < this.top0.Value)
				this.top0 = Y;

			if (!this.bottom0.HasValue || Y > this.bottom0.Value)
				this.bottom0 = Y;
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
		protected async Task<CalculatedPoint> IncludePoint(DrawingState State, LengthAttribute XAttribute,
			LengthAttribute YAttribute, StringAttribute RefAttribute, float X, float Y)
		{
			CalculatedPoint Result = await this.CalcPoint(State, XAttribute, YAttribute, RefAttribute, X, Y);
			if (Result.Ok)
				this.IncludePoint(Result.X, Result.Y);
			
			return Result;
		}

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override async Task Draw(DrawingState State)
		{
			if (this.HasChildren)
			{
				foreach (ILayoutElement E in this.children)
				{
					if (E.IsVisible)
						await E.Draw(State);
				}
			}
		}

		/// <summary>
		/// Registers any IDs defined with the encapsulating document.
		/// </summary>
		/// <param name="Session">Session variables.</param>
		public override async Task RegisterIDs(Variables Session)
		{
			await base.RegisterIDs(Session);

			if (this.HasChildren)
			{
				foreach (ILayoutElement E in this.children)
					await E.RegisterIDs(Session);
			}
		}

		/// <summary>
		/// Exports the current state of child nodes of the current element.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportStateChildren(XmlWriter Output)
		{
			base.ExportStateChildren(Output);

			if (this.HasChildren)
			{
				foreach (ILayoutElement Child in this.children)
					Child.ExportState(Output);
			}
		}

	}
}
