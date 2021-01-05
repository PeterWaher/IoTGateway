using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using SkiaSharp;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Vectors;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Sets;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Graphs
{
	/// <summary>
	/// Handles two-dimensional graphs.
	/// </summary>
	public class Graph2D : Graph
	{
		private readonly LinkedList<IVector> x = new LinkedList<IVector>();
		private readonly LinkedList<IVector> y = new LinkedList<IVector>();
		private readonly LinkedList<object[]> parameters = new LinkedList<object[]>();
		private readonly LinkedList<IPainter2D> painters = new LinkedList<IPainter2D>();
		private IElement minX, maxX;
		private IElement minY, maxY;
		private Type axisTypeX;
		private Type axisTypeY;
		private string title = string.Empty;
		private string labelX = string.Empty;
		private string labelY = string.Empty;
		private bool showXAxis = true;
		private bool showYAxis = true;
		private bool showGrid = true;
		//private readonly bool showZeroX = false;
		//private readonly bool showZeroY = false;

		/// <summary>
		/// Base class for two-dimensional graphs.
		/// </summary>
		public Graph2D()
			: base()
		{
		}

		/// <summary>
		/// Base class for two-dimensional graphs.
		/// </summary>
		/// <param name="X">X-axis</param>
		/// <param name="Y">Y-axis</param>
		/// <param name="Painter">Painter of graph.</param>
		/// <param name="ShowZeroX">If the y-axis (x=0) should always be shown.</param>
		/// <param name="ShowZeroY">If the x-axis (y=0) should always be shown.</param>
		/// <param name="Node">Node creating the graph.</param>
		/// <param name="Parameters">Graph-specific parameters.</param>
		public Graph2D(IVector X, IVector Y, IPainter2D Painter, bool ShowZeroX, bool ShowZeroY,
			ScriptNode Node, params object[] Parameters)
			: base()
		{
			if (X is Interval XI)
				X = new DoubleVector(XI.GetArray());

			if (Y is Interval YI)
				Y = new DoubleVector(YI.GetArray());

			int i, c = X.Dimension;
			bool HasNull = false;
			IElement ex, ey;
			IElement Zero;

			if (c != Y.Dimension)
				throw new ScriptException("X and Y series must be equally large.");

			for (i = 0; i < c; i++)
			{
				ex = X.GetElement(i);
				ey = Y.GetElement(i);

				if (ex.AssociatedObjectValue is null || ey.AssociatedObjectValue is null)
				{
					HasNull = true;
					break;
				}
			}

			//this.showZeroX = ShowZeroX;
			//this.showZeroY = ShowZeroY;

			this.minX = Min.CalcMin(X, Node);
			this.maxX = Max.CalcMax(X, Node);

			if (ShowZeroX && c > 0 && this.minX.AssociatedSet is IAbelianGroup AG)
			{
				Zero = AG.AdditiveIdentity;

				this.minX = Min.CalcMin(new ObjectVector(this.minX, Zero), null);
				this.maxX = Max.CalcMax(new ObjectVector(this.maxX, Zero), null);
			}

			this.minY = Min.CalcMin(Y, Node);
			this.maxY = Max.CalcMax(Y, Node);

			if (ShowZeroY && c > 0 && this.minY.AssociatedSet is IAbelianGroup AG2)
			{
				Zero = AG2.AdditiveIdentity;

				this.minY = Min.CalcMin(new ObjectVector(this.minY, Zero), null);
				this.maxY = Max.CalcMax(new ObjectVector(this.maxY, Zero), null);
			}

			if (HasNull)
			{
				LinkedList<IElement> X2 = new LinkedList<IElement>();
				LinkedList<IElement> Y2 = new LinkedList<IElement>();

				this.axisTypeX = null;
				this.axisTypeY = null;

				for (i = 0; i < c; i++)
				{
					ex = X.GetElement(i);
					ey = Y.GetElement(i);

					if (ex.AssociatedObjectValue is null || ey.AssociatedObjectValue is null)
					{
						if (X2.First != null)
						{
							this.AddSegment(X, Y, X2, Y2, Node, Painter, Parameters);
							X2 = new LinkedList<IElement>();
							Y2 = new LinkedList<IElement>();
						}
					}
					else
					{
						X2.AddLast(ex);
						Y2.AddLast(ey);
					}
				}

				if (X2.First != null)
					this.AddSegment(X, Y, X2, Y2, Node, Painter, Parameters);
			}
			else
			{
				this.axisTypeX = X.GetType();
				this.axisTypeY = Y.GetType();

				if (c > 0)
				{
					this.x.AddLast(X);
					this.y.AddLast(Y);
					this.painters.AddLast(Painter);
					this.parameters.AddLast(Parameters);
				}
			}
		}

		private void AddSegment(IVector X, IVector Y, ICollection<IElement> X2, ICollection<IElement> Y2,
			ScriptNode Node, IPainter2D Painter, params object[] Parameters)
		{
			IVector X2V = (IVector)X.Encapsulate(X2, Node);
			IVector Y2V = (IVector)Y.Encapsulate(Y2, Node);

			if (this.axisTypeX is null)
			{
				this.axisTypeX = X2V.GetType();
				this.axisTypeY = Y2V.GetType();
			}
			else
			{
				if (X2V.GetType() != this.axisTypeX || Y2V.GetType() != this.axisTypeY)
					throw new ScriptException("Incompatible types of series.");
			}

			this.x.AddLast(X2V);
			this.y.AddLast(Y2V);
			this.painters.AddLast(Painter);
			this.parameters.AddLast(Parameters);
		}

		/// <summary>
		/// X-axis series.
		/// </summary>
		public LinkedList<IVector> X
		{
			get { return this.x; }
		}

		/// <summary>
		/// Y-axis series.
		/// </summary>
		public LinkedList<IVector> Y
		{
			get { return this.y; }
		}

		/// <summary>
		/// Parameters.
		/// </summary>
		public LinkedList<object[]> Parameters
		{
			get { return this.parameters; }
		}

		/// <summary>
		/// Smallest X-value.
		/// </summary>
		public IElement MinX
		{
			get { return this.minX; }
		}

		/// <summary>
		/// Largest X-value.
		/// </summary>
		public IElement MaxX
		{
			get { return this.maxX; }
		}

		/// <summary>
		/// Smallest Y-value.
		/// </summary>
		public IElement MinY
		{
			get { return this.minY; }
		}

		/// <summary>
		/// Largest Y-value.
		/// </summary>
		public IElement MaxY
		{
			get { return this.maxY; }
		}

		/// <summary>
		/// Title for graph.
		/// </summary>
		public string Title
		{
			get { return this.title; }
			set { this.title = value; }
		}

		/// <summary>
		/// Label for x-axis.
		/// </summary>
		public string LabelX
		{
			get { return this.labelX; }
			set { this.labelX = value; }
		}

		/// <summary>
		/// Label for y-axis.
		/// </summary>
		public string LabelY
		{
			get { return this.labelY; }
			set { this.labelY = value; }
		}

		/// <summary>
		/// If the X-axis is to be displayed.
		/// </summary>
		public bool ShowXAxis
		{
			get { return this.showXAxis; }
			set { this.showXAxis = value; }
		}

		/// <summary>
		/// If the Y-axis is to be displayed.
		/// </summary>
		public bool ShowYAxis
		{
			get { return this.showYAxis; }
			set { this.showYAxis = value; }
		}

		/// <summary>
		/// If the grid is to be displayed.
		/// </summary>
		public bool ShowGrid
		{
			get { return this.showGrid; }
			set { this.showGrid = value; }
		}

		/// <summary>
		/// Tries to add an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ISemiGroupElement AddLeft(ISemiGroupElement Element)
		{
			return Element.AddRight(this);
		}

		/// <summary>
		/// Tries to add an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ISemiGroupElement AddRight(ISemiGroupElement Element)
		{
			if (this.x.First is null)
				return Element;

			if (!(Element is Graph2D G))
				return null;

			if (G.x.First is null)
				return this;

			Graph2D Result = new Graph2D()
			{
				axisTypeX = this.axisTypeX,
				axisTypeY = this.axisTypeY,
				title = this.title,
				labelX = this.labelX,
				labelY = this.labelY,
				SameScale = this.SameScale
			};

			foreach (IVector v in this.x)
				Result.x.AddLast(v);

			foreach (IVector v in this.y)
				Result.y.AddLast(v);

			foreach (IPainter2D Painter in this.painters)
				Result.painters.AddLast(Painter);

			foreach (object[] P in this.parameters)
				Result.parameters.AddLast(P);

			foreach (IVector v in G.x)
			{
				if (v.GetType() != this.axisTypeX)
					throw new ScriptException("Incompatible types of series.");

				Result.x.AddLast(v);
			}

			foreach (IVector v in G.y)
			{
				if (v.GetType() != this.axisTypeY)
					throw new ScriptException("Incompatible types of series.");

				Result.y.AddLast(v);
			}

			foreach (IPainter2D Painter in G.painters)
				Result.painters.AddLast(Painter);

			foreach (object[] P in G.parameters)
				Result.parameters.AddLast(P);

			Result.minX = Min.CalcMin((IVector)VectorDefinition.Encapsulate(new IElement[] { this.minX, G.minX }, false, null), null);
			Result.maxX = Max.CalcMax((IVector)VectorDefinition.Encapsulate(new IElement[] { this.maxX, G.maxX }, false, null), null);
			Result.minY = Min.CalcMin((IVector)VectorDefinition.Encapsulate(new IElement[] { this.minY, G.minY }, false, null), null);
			Result.maxY = Max.CalcMax((IVector)VectorDefinition.Encapsulate(new IElement[] { this.maxY, G.maxY }, false, null), null);

			Result.showXAxis |= G.showXAxis;
			Result.showYAxis |= G.showYAxis;

			return Result;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is Graph2D G))
				return false;

			return (
				this.minX.Equals(G.minX) &&
				this.maxX.Equals(G.maxX) &&
				this.minY.Equals(G.minY) &&
				this.maxY.Equals(G.maxY) &&
				this.axisTypeX.Equals(G.axisTypeX) &&
				this.axisTypeY.Equals(G.axisTypeY) &&
				this.title.Equals(G.title) &&
				this.labelX.Equals(G.labelX) &&
				this.labelY.Equals(G.labelY) &&
				this.showXAxis.Equals(G.showXAxis) &&
				this.showYAxis.Equals(G.showYAxis) &&
				this.showGrid.Equals(G.showGrid) &&
				this.Equals(this.x.GetEnumerator(), G.x.GetEnumerator()) &&
				this.Equals(this.y.GetEnumerator(), G.y.GetEnumerator()) &&
				this.Equals(this.parameters.GetEnumerator(), G.parameters.GetEnumerator()) &&
				this.Equals(this.painters.GetEnumerator(), G.painters.GetEnumerator()));
		}

		private bool Equals(IEnumerator e1, IEnumerator e2)
		{
			bool b1 = e1.MoveNext();
			bool b2 = e2.MoveNext();

			while (b1 && b2)
			{
				if (!e1.Current.Equals(e2.Current))
					return false;

				b1 = e1.MoveNext();
				b2 = e2.MoveNext();
			}

			return !(b1 || b2);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = this.minX.GetHashCode();
			Result ^= Result << 5 ^ this.maxX.GetHashCode();
			Result ^= Result << 5 ^ this.minY.GetHashCode();
			Result ^= Result << 5 ^ this.maxY.GetHashCode();
			Result ^= Result << 5 ^ this.axisTypeX.GetHashCode();
			Result ^= Result << 5 ^ this.axisTypeY.GetHashCode();
			Result ^= Result << 5 ^ this.title.GetHashCode();
			Result ^= Result << 5 ^ this.labelX.GetHashCode();
			Result ^= Result << 5 ^ this.labelY.GetHashCode();
			Result ^= Result << 5 ^ this.showXAxis.GetHashCode();
			Result ^= Result << 5 ^ this.showYAxis.GetHashCode();
			Result ^= Result << 5 ^ this.showGrid.GetHashCode();

			foreach (IElement E in this.x)
				Result ^= Result << 5 ^ E.GetHashCode();

			foreach (IElement E in this.y)
				Result ^= Result << 5 ^ E.GetHashCode();

			foreach (object Obj in this.parameters)
				Result ^= Result << 5 ^ Obj.GetHashCode();

			foreach (IPainter2D Painter in this.painters)
				Result ^= Result << 5 ^ Painter.GetHashCode();

			return Result;
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <param name="States">State object(s) that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public override SKImage CreateBitmap(GraphSettings Settings, out object[] States)
		{
			using (SKSurface Surface = SKSurface.Create(new SKImageInfo(Settings.Width, Settings.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul)))
			{
				SKCanvas Canvas = Surface.Canvas;

				States = new object[0];

				Canvas.Clear(Settings.BackgroundColor);

				int x1, y1, x2, y2, x3, y3, w, h;

				x1 = Settings.MarginLeft;
				x2 = Settings.Width - Settings.MarginRight;
				y1 = Settings.MarginTop;
				y2 = Settings.Height - Settings.MarginBottom;

				if (!string.IsNullOrEmpty(this.labelY))
					x1 += (int)(Settings.LabelFontSize * 2 + 0.5);

				if (!string.IsNullOrEmpty(this.labelX))
					y2 -= (int)(Settings.LabelFontSize * 2 + 0.5);

				if (!string.IsNullOrEmpty(this.title))
					y1 += (int)(Settings.LabelFontSize * 2 + 0.5);

				IVector YLabels = GetLabels(ref this.minY, ref this.maxY, this.y, Settings.ApproxNrLabelsY, out LabelType YLabelType);
				SKPaint Font = new SKPaint()
				{
					FilterQuality = SKFilterQuality.High,
					HintingLevel = SKPaintHinting.Full,
					SubpixelText = true,
					IsAntialias = true,
					Style = SKPaintStyle.Fill,
					Color = Settings.AxisColor,
					Typeface = SKTypeface.FromFamilyName(Settings.FontName, SKFontStyle.Normal),
					TextSize = (float)Settings.LabelFontSize
				};
				SKRect Bounds = new SKRect();
				float Size;
				double MaxSize = 0;

				if (this.showYAxis)
				{
					foreach (IElement Label in YLabels.ChildElements)
					{
						Font.MeasureText(LabelString(Label, YLabelType), ref Bounds);
						Size = Bounds.Width;
						if (Size > MaxSize)
							MaxSize = Size;
					}
				}

				x3 = (int)Math.Ceiling(x1 + MaxSize) + Settings.MarginLabel;

				IVector XLabels = GetLabels(ref this.minX, ref this.maxX, this.x, Settings.ApproxNrLabelsX, out LabelType XLabelType);
				MaxSize = 0;

				if (this.showXAxis)
				{
					foreach (IElement Label in XLabels.ChildElements)
					{
						Font.MeasureText(LabelString(Label, XLabelType), ref Bounds);
						Size = Bounds.Height;
						if (Size > MaxSize)
							MaxSize = Size;
					}
				}

				y3 = (int)Math.Floor(y2 - MaxSize) - Settings.MarginLabel;
				w = x2 - x3;
				h = y3 - y1;

				SKPaint AxisBrush = new SKPaint()
				{
					FilterQuality = SKFilterQuality.High,
					IsAntialias = true,
					Style = SKPaintStyle.Fill,
					Color = Settings.AxisColor
				};
				SKPaint GridBrush = new SKPaint()
				{
					FilterQuality = SKFilterQuality.High,
					IsAntialias = true,
					Style = SKPaintStyle.Fill,
					Color = Settings.GridColor
				};
				SKPaint AxisPen = new SKPaint()
				{
					FilterQuality = SKFilterQuality.High,
					IsAntialias = true,
					Style = SKPaintStyle.Stroke,
					Color = Settings.AxisColor,
					StrokeWidth = Settings.AxisWidth
				};
				SKPaint GridPen = new SKPaint()
				{
					FilterQuality = SKFilterQuality.High,
					IsAntialias = true,
					Style = SKPaintStyle.Stroke,
					Color = Settings.GridColor,
					StrokeWidth = Settings.GridWidth
				};

				if (this.SameScale &&
					this.minX is DoubleNumber MinX &&
					this.maxX is DoubleNumber MaxX &&
					this.minY is DoubleNumber MinY &&
					this.maxY is DoubleNumber MaxY)
				{
					double DX = MaxX.Value - MinX.Value;
					double DY = MaxY.Value - MinY.Value;
					double SX = w / (DX == 0 ? 1 : DX);
					double SY = h / (DY == 0 ? 1 : DY);

					if (SX < SY)
					{
						int h2 = (int)(h * SX / SY + 0.5);
						y3 -= (h - h2) / 2;
						h = h2;
					}
					else if (SY < SX)
					{
						int w2 = (int)(w * SY / SX + 0.5);
						x3 += (w - w2) / 2;
						w = w2;
					}
				}

				double OrigoX;
				double OrigoY;

				if (this.minX.AssociatedSet is IAbelianGroup AgX)
					OrigoX = Scale(new ObjectVector(AgX.AdditiveIdentity), this.minX, this.maxX, x3, w)[0];
				else
					OrigoX = 0;

				if (this.minY.AssociatedSet is IAbelianGroup AgY)
					OrigoY = Scale(new ObjectVector(AgY.AdditiveIdentity), this.minY, this.maxY, y3, -h)[0];
				else
					OrigoY = 0;

				DrawingArea DrawingArea = new DrawingArea(this.minX, this.maxX, this.minY, this.maxY, x3, y3, w, -h, (float)OrigoX, (float)OrigoY);
				double[] LabelYY = DrawingArea.ScaleY(YLabels);
				int i = 0;
				float f;
				string s;

				foreach (IElement Label in YLabels.ChildElements)
				{
					Font.MeasureText(s = LabelString(Label, YLabelType), ref Bounds);
					f = (float)LabelYY[i++];

					if (this.showGrid)
					{
						if (Label is DoubleNumber Lbl && Lbl.Value == 0)
							Canvas.DrawLine(x3, f, x2, f, AxisPen);
						else
							Canvas.DrawLine(x3, f, x2, f, GridPen);
					}

					if (this.showYAxis)
					{
						f += Bounds.Height * 0.5f;
						Canvas.DrawText(s, x3 - Bounds.Width - Settings.MarginLabel, f, Font);
					}
				}

				double[] LabelXX = DrawingArea.ScaleX(XLabels);
				i = 0;

				foreach (IElement Label in XLabels.ChildElements)
				{
					Font.MeasureText(s = LabelString(Label, XLabelType), ref Bounds);
					f = (float)LabelXX[i++];

					if (this.showGrid)
					{
						if (Label is DoubleNumber DLbl && DLbl.Value == 0)
							Canvas.DrawLine(f, y1, f, y3, AxisPen);
						else
							Canvas.DrawLine(f, y1, f, y3, GridPen);
					}

					if (this.showXAxis)
					{
						Size = Bounds.Width;
						f -= Size * 0.5f;
						if (f < x3)
							f = x3;
						else if (f + Size > x3 + w)
							f = x3 + w - Size;

						Canvas.DrawText(s, f, y3 + Settings.MarginLabel + (float)Settings.LabelFontSize, Font);
					}
				}

				Font.Dispose();
				Font = null;

				Font = new SKPaint()
				{
					FilterQuality = SKFilterQuality.High,
					HintingLevel = SKPaintHinting.Full,
					SubpixelText = true,
					IsAntialias = true,
					Style = SKPaintStyle.Fill,
					Color = Settings.AxisColor,
					Typeface = SKTypeface.FromFamilyName(Settings.FontName, SKFontStyle.Bold),
					TextSize = (float)(Settings.LabelFontSize * 1.5)
				};

				if (!string.IsNullOrEmpty(this.title))
				{
					Font.MeasureText(this.title, ref Bounds);
					Size = Bounds.Width;

					f = x3 + (x2 - x3 - Size) * 0.5f;

					if (f < x3)
						f = x3;
					else if (f + Size > x3 + w)
						f = x3 + w - Size;

					Canvas.DrawText(this.title, f, (float)(Settings.MarginTop + 0.1 * Settings.LabelFontSize - Bounds.Top), Font);
				}

				if (!string.IsNullOrEmpty(this.labelX))
				{
					Font.MeasureText(this.labelX, ref Bounds);
					Size = Bounds.Width;

					f = x3 + (x2 - x3 - Size) * 0.5f;

					if (f < x3)
						f = x3;
					else if (f + Size > x3 + w)
						f = x3 + w - Size;

					Canvas.DrawText(this.labelX, f, (float)(Settings.Height - Settings.MarginBottom - 0.1 * Settings.LabelFontSize - Bounds.Height - Bounds.Top), Font);
				}

				if (!string.IsNullOrEmpty(this.labelY))
				{
					Font.MeasureText(this.labelY, ref Bounds);
					Size = Bounds.Width;

					f = y3 - (y3 - y1 - Size) * 0.5f;

					if (f - Size < y1)
						f = y1 + Size;
					else if (f > y3 + h)
						f = y3 + h;

					Canvas.Translate((float)(Settings.MarginLeft + 0.1 * Settings.LabelFontSize - Bounds.Top), f);
					Canvas.RotateDegrees(-90);
					Canvas.DrawText(this.labelY, 0, 0, Font);
					Canvas.ResetMatrix();
				}

				IEnumerator<IVector> ex = this.x.GetEnumerator();
				IEnumerator<IVector> ey = this.y.GetEnumerator();
				IEnumerator<object[]> eParameters = this.parameters.GetEnumerator();
				IEnumerator<IPainter2D> ePainters = this.painters.GetEnumerator();
				SKPoint[] Points;
				SKPoint[] PrevPoints = null;
				object[] PrevParameters = null;
				IPainter2D PrevPainter = null;

				while (ex.MoveNext() && ey.MoveNext() && eParameters.MoveNext() && ePainters.MoveNext())
				{
					Points = DrawingArea.Scale(ex.Current, ey.Current);

					if (PrevPainter != null && ePainters.Current.GetType() == PrevPainter.GetType())
						ePainters.Current.DrawGraph(Canvas, Points, eParameters.Current, PrevPoints, PrevParameters, DrawingArea);
					else
						ePainters.Current.DrawGraph(Canvas, Points, eParameters.Current, null, null, DrawingArea);

					PrevPoints = Points;
					PrevParameters = eParameters.Current;
					PrevPainter = ePainters.Current;
				}

				SKImage Result = Surface.Snapshot();

				Font?.Dispose();

				AxisBrush.Dispose();
				GridBrush.Dispose();
				GridPen.Dispose();
				AxisPen.Dispose();

				States = new object[] { DrawingArea };

				return Result;
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
			DrawingArea DrawingArea = (DrawingArea)States[0];

			IElement X2 = DrawingArea.DescaleX(X);
			IElement Y2 = DrawingArea.DescaleY(Y);

			return "[" + X2.ToString() + "," + Y2.ToString() + "]";
		}

		/// <summary>
		/// Exports graph specifics to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportGraph(XmlWriter Output)
		{
			Output.WriteStartElement("Graph2D");
			Output.WriteAttributeString("title", this.title);
			Output.WriteAttributeString("labelX", this.labelX);
			Output.WriteAttributeString("labelY", this.labelY);
			Output.WriteAttributeString("axisTypeX", this.axisTypeX.FullName);
			Output.WriteAttributeString("axisTypeY", this.axisTypeY.FullName);
			Output.WriteAttributeString("minX", Expression.ToString(this.minX));
			Output.WriteAttributeString("maxX", Expression.ToString(this.maxX));
			Output.WriteAttributeString("minY", Expression.ToString(this.minY));
			Output.WriteAttributeString("maxY", Expression.ToString(this.maxY));
			Output.WriteAttributeString("showXAxis", this.showXAxis ? "true" : "false");
			Output.WriteAttributeString("showYAxis", this.showYAxis ? "true" : "false");
			Output.WriteAttributeString("showGrid", this.showGrid ? "true" : "false");

			foreach (IVector v in this.x)
				Output.WriteElementString("X", Expression.ToString(v));

			foreach (IVector v in this.y)
				Output.WriteElementString("Y", Expression.ToString(v));

			foreach (object[] v in this.parameters)
				Output.WriteElementString("Parameters", Expression.ToString(new ObjectVector(v)));

			foreach (IPainter2D Painter in this.painters)
				Output.WriteElementString("Painter", Painter.GetType().FullName);

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
					case "title":
						this.title = Attr.Value;
						break;

					case "labelX":
						this.labelX = Attr.Value;
						break;

					case "labelY":
						this.labelY = Attr.Value;
						break;

					case "axisTypeX":
						this.axisTypeX = Types.GetType(Attr.Value);
						break;

					case "axisTypeY":
						this.axisTypeY = Types.GetType(Attr.Value);
						break;

					case "minX":
						this.minX = this.Parse(Attr.Value, Variables);
						break;

					case "maxX":
						this.maxX = this.Parse(Attr.Value, Variables);
						break;

					case "minY":
						this.minY = this.Parse(Attr.Value, Variables);
						break;

					case "maxY":
						this.maxY = this.Parse(Attr.Value, Variables);
						break;

					case "showXAxis":
						this.showXAxis = Attr.Value == "true";
						break;

					case "showYAxis":
						this.showYAxis = Attr.Value == "true";
						break;

					case "showGrid":
						this.showGrid = Attr.Value == "true";
						break;
				}
			}

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "X":
							this.x.AddLast((IVector)this.Parse(E.InnerText, Variables));
							break;

						case "Y":
							this.y.AddLast((IVector)this.Parse(E.InnerText, Variables));
							break;

						case "Parameters":
							IVector v = (IVector)this.Parse(E.InnerText, Variables);
							this.parameters.AddLast(this.ToObjectArray(v));
							break;

						case "Painter":
							this.painters.AddLast((IPainter2D)Activator.CreateInstance(Types.GetType(E.InnerText)));
							break;
					}
				}
			}
		}

		/// <summary>
		/// If graph uses default color
		/// </summary>
		public override bool UsesDefaultColor
		{
			get
			{
				IEnumerator<object[]> eParameters = this.parameters.GetEnumerator();
				IEnumerator<IPainter2D> ePainter = this.painters.GetEnumerator();

				while (eParameters.MoveNext() && ePainter.MoveNext())
				{
					if (!ePainter.Current.UsesDefaultColor(eParameters.Current))
						return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Tries to set the default color.
		/// </summary>
		/// <param name="Color">Default color.</param>
		/// <returns>If possible to set.</returns>
		public override bool TrySetDefaultColor(SKColor Color)
		{
			if (!this.UsesDefaultColor)
				return false;

			IEnumerator<object[]> eParameters = this.parameters.GetEnumerator();
			IEnumerator<IPainter2D> ePainter = this.painters.GetEnumerator();
			bool Result = true;

			while (eParameters.MoveNext() && ePainter.MoveNext())
			{
				if (!ePainter.Current.TrySetDefaultColor(Color, eParameters.Current))
					Result = false;
			}

			return Result;
		}

	}
}
