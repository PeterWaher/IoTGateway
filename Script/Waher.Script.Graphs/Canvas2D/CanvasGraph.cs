using System;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Graphs.Canvas2D.Operations;

namespace Waher.Script.Graphs.Canvas2D
{
	/// <summary>
	/// Canvas graph. Permits custom drawing from scrupt.
	/// </summary>
	public class CanvasGraph : Graph
	{
		private readonly LinkedList<CanvasOperation> operations = new LinkedList<CanvasOperation>();
		private SKColor? defaultColor;
		private SKColor? bgColor;
		private int width;
		private int height;

		/// <summary>
		/// Canvas graph. Permits custom drawing from scrupt.
		/// </summary>
		public CanvasGraph()
			: base()
		{
		}

		/// <summary>
		/// Canvas graph. Permits custom drawing from scrupt.
		/// </summary>
		/// <param name="Width">Width, in puxels.</param>
		/// <param name="Height">Height, in puxels.</param>
		/// <param name="BackgroundColor">Background color.</param>
		/// <param name="DefaultColor">Default color.</param>
		public CanvasGraph(int Width, int Height, SKColor? DefaultColor, SKColor? BackgroundColor)
			: base()
		{
			this.width = Width;
			this.height = Height;
			this.bgColor = BackgroundColor;
			this.defaultColor = DefaultColor;
		}

		/// <summary>
		/// Background color.
		/// </summary>
		public SKColor? BgColor => this.bgColor;

		/// <summary>
		/// Default color.
		/// </summary>
		public new SKColor? DefaultColor => this.defaultColor;

		/// <summary>
		/// Width, in puxels.
		/// </summary>
		public int Width => this.width;

		/// <summary>
		/// Height, in puxels.
		/// </summary>
		public int Height => this.height;

		/// <summary>
		/// If graph uses default color
		/// </summary>
		public override bool UsesDefaultColor => this.defaultColor != SKColors.Empty;

		/// <summary>
		/// Tries to set the default color.
		/// </summary>
		/// <param name="Color">Default color.</param>
		/// <returns>If possible to set.</returns>
		public override bool TrySetDefaultColor(SKColor Color)
		{
			this.defaultColor = Color;
			return true;
		}

		/// <summary>
		/// Tries to add an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ISemiGroupElement AddLeft(ISemiGroupElement Element)
		{
			if (Element is CanvasGraph G)
			{
				CanvasGraph Result = new CanvasGraph(G.width, G.height, G.bgColor, G.defaultColor);

				foreach (CanvasOperation Op in G.operations)
					Result.operations.AddLast(Op);

				foreach (CanvasOperation Op in this.operations)
					Result.operations.AddLast(Op);

				return Result;
			}
			else
				return null;
		}

		/// <summary>
		/// Tries to add an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ISemiGroupElement AddRight(ISemiGroupElement Element)
		{
			if (Element is CanvasGraph G)
			{
				CanvasGraph Result = new CanvasGraph(this.width, this.height, this.bgColor, this.defaultColor);

				foreach (CanvasOperation Op in this.operations)
					Result.operations.AddLast(Op);

				foreach (CanvasOperation Op in G.operations)
					Result.operations.AddLast(Op);

				return Result;
			}
			else
				return null;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is CanvasGraph G) ||
				this.width != G.width ||
				this.height != G.height ||
				this.bgColor != G.bgColor ||
				this.defaultColor != G.defaultColor)
			{
				return false;
			}

			LinkedList<CanvasOperation>.Enumerator e1 = this.operations.GetEnumerator();
			LinkedList<CanvasOperation>.Enumerator e2 = G.operations.GetEnumerator();
			bool b1, b2;

			while (true)
			{
				b1 = e1.MoveNext();
				b2 = e2.MoveNext();

				if (b1 ^ b2)
					return false;

				if (!b1)
					return true;

				if (!e1.Current.Equals(e2.Current))
					return false;
			}
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.width.GetHashCode();
			Result ^= Result << 5 ^ this.height.GetHashCode();
			Result ^= Result << 5 ^ this.bgColor.GetHashCode();
			Result ^= Result << 5 ^ this.defaultColor.GetHashCode();

			foreach (CanvasOperation Op in this.operations)
				Result ^= Result << 5 ^ Op.GetHashCode();

			return Result;
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <param name="States">State objects that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public override PixelInformation CreatePixels(GraphSettings Settings, out object[] States)
		{
			using (SKSurface Surface = SKSurface.Create(new SKImageInfo(this.width, this.height, SKImageInfo.PlatformColorType, SKAlphaType.Premul)))
			{
				SKCanvas Canvas = Surface.Canvas;
				CanvasState State = new CanvasState()
				{
					FgColor = this.defaultColor ?? Graph.DefaultColor,
					BgColor = this.bgColor ?? Settings.BackgroundColor
				};

				Canvas.Clear(State.BgColor);

				foreach (CanvasOperation Op in this.operations)
					Op.Draw(Canvas, State);

				using (SKImage Result = Surface.Snapshot())
				{
					States = new object[0];
					return PixelInformation.FromImage(Result);
				}
			}
		}

		/// <summary>
		/// Exports graph specifics to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportGraph(XmlWriter Output)
		{
			Output.WriteStartElement("CanvasGraph");
			Output.WriteAttributeString("width", this.width.ToString());
			Output.WriteAttributeString("height", this.height.ToString());

			if (this.bgColor.HasValue)
				Output.WriteAttributeString("bgColor", Expression.ToString(this.bgColor.Value));

			if (this.defaultColor.HasValue)
				Output.WriteAttributeString("defaultColor", Expression.ToString(this.defaultColor.Value));

			foreach (CanvasOperation Op in this.operations)
			{
				Output.WriteStartElement(Op.GetType().Name);
				Op.ExportGraph(Output);
				Output.WriteEndElement();
			}

			Output.WriteEndElement();
		}

		/// <summary>
		/// Imports graph specifics from XML.
		/// </summary>
		/// <param name="Xml">XML input.</param>
		public override void ImportGraph(XmlElement Xml)
		{
			Variables Variables = new Variables();

			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "width":
						this.width = int.Parse(Attr.Value);
						break;

					case "height":
						this.height = int.Parse(Attr.Value);
						break;

					case "bgColor":
						this.bgColor = Graph.ToColor(Parse(Attr.Value, Variables).AssociatedObjectValue);
						break;

					case "defaultColor":
						this.defaultColor = Graph.ToColor(Parse(Attr.Value, Variables).AssociatedObjectValue);
						break;
				}
			}

			string OperationNamespace = typeof(CanvasOperation).Namespace + ".";

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					Type T = Types.GetType(OperationNamespace + E.LocalName);
					if (T is null)
						continue;

					if (!(Activator.CreateInstance(T) is CanvasOperation Op))
						continue;

					Op.ImportGraph(E, Variables);

					this.operations.AddLast(Op);
				}
			}
		}

		/// <summary>
		/// Gets script corresponding to a point in a generated bitmap representation of the graph.
		/// </summary>
		/// <param name="X">X-Coordinate.</param>
		/// <param name="Y">Y-Coordinate.</param>
		/// <param name="States">State objects for the generated bitmap.</param>
		/// <returns>Script.</returns>
		public override string GetBitmapClickScript(double X, double Y, object[] States)
		{
			return "[" + Expression.ToString(X) + "," + Expression.ToString(Y) + "]";
		}

		/// <summary>
		/// Draws a line to a specific coordinate
		/// </summary>
		public void LineTo(double x, double y)
		{
			this.operations.AddLast(new LineTo((float)x, (float)y));
		}

		/// <summary>
		/// Draws a line to a specific coordinate
		/// </summary>
		public void MoveTo(double x, double y)
		{
			this.operations.AddLast(new MoveTo((float)x, (float)y));
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="x1">X-coordinate of first point.</param>
		/// <param name="y1">Y-coordinate of first point.</param>
		/// <param name="x2">X-coordinate of second point.</param>
		/// <param name="y2">Y-coordinate of second point.</param>
		public void Line(double x1, double y1, double x2, double y2)
		{
			this.operations.AddLast(new Line((float)x1, (float)y1, (float)x2, (float)y2));
		}

		/// <summary>
		/// Sets the current pen width.
		/// </summary>
		/// <param name="Width">New pen width</param>
		public void PenWidth(double Width)
		{
			this.operations.AddLast(new PenWidth((float)Width));
		}

		/// <summary>
		/// Sets the current pen color.
		/// </summary>
		/// <param name="Color">New pen color</param>
		public void Color(SKColor Color)
		{
			this.operations.AddLast(new PenColor(Color));
		}

		/// <summary>
		/// Sets the current pen color.
		/// </summary>
		/// <param name="Color">New pen color</param>
		public void Color(object Color)
		{
			this.operations.AddLast(new PenColor(Graph.ToColor(Color)));
		}

		/// <summary>
		/// Draws a rectangle defined by two opposing corner points.
		/// </summary>
		/// <param name="x1">X-coordinate of first point.</param>
		/// <param name="y1">Y-coordinate of first point.</param>
		/// <param name="x2">X-coordinate of second point.</param>
		/// <param name="y2">Y-coordinate of second point.</param>
		public void Rectangle(double x1, double y1, double x2, double y2)
		{
			this.operations.AddLast(new Rectangle((float)x1, (float)y1, (float)x2, (float)y2));
		}

		/// <summary>
		/// Fills a rectangle defined by two opposing corner points.
		/// </summary>
		/// <param name="x1">X-coordinate of first point.</param>
		/// <param name="y1">Y-coordinate of first point.</param>
		/// <param name="x2">X-coordinate of second point.</param>
		/// <param name="y2">Y-coordinate of second point.</param>
		public void FillRectangle(double x1, double y1, double x2, double y2)
		{
			this.operations.AddLast(new FillRectangle((float)x1, (float)y1, (float)x2, (float)y2));
		}


	}
}
