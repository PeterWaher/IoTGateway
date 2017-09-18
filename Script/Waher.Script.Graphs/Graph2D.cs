using System;
using System.Collections;
using System.Collections.Generic;
using SkiaSharp;
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
	/// Delegate for drawing callback methods.
	/// </summary>
	/// <param name="Canvas">Canvas to draw on.</param>
	/// <param name="Points">Points to draw.</param>
	/// <param name="Parameters">Graph-specific parameters.</param>
	/// <param name="PrevPoints">Points of previous graph of same type (if available), null (if not available).</param>
	/// <param name="PrevParameters">Parameters of previous graph of same type (if available), null (if not available).</param>
	/// <param name="DrawingArea">Current drawing area.</param>
	public delegate void DrawCallback(SKCanvas Canvas, SKPoint[] Points, object[] Parameters, SKPoint[] PrevPoints, object[] PrevParameters,
		DrawingArea DrawingArea);

	/// <summary>
	/// Handles two-dimensional graphs.
	/// </summary>
	public class Graph2D : Graph
	{
		private LinkedList<IVector> x = new LinkedList<IVector>();
		private LinkedList<IVector> y = new LinkedList<IVector>();
		private LinkedList<object[]> parameters = new LinkedList<object[]>();
		private LinkedList<DrawCallback> callbacks = new LinkedList<DrawCallback>();
		private IElement minX, maxX;
		private IElement minY, maxY;
		private Type axisTypeX;
		private Type axisTypeY;
		private string labelX = string.Empty;
		private string labelY = string.Empty;
		private bool showXAxis = true;
		private bool showYAxis = true;
		private bool showGrid = true;
		private bool showZeroX = false;
		private bool showZeroY = false;

		/// <summary>
		/// Base class for two-dimensional graphs.
		/// </summary>
		internal Graph2D()
			: base()
		{
		}

		/// <summary>
		/// Base class for two-dimensional graphs.
		/// </summary>
		/// <param name="X">X-axis</param>
		/// <param name="Y">Y-axis</param>
		/// <param name="PlotCallback">Callback method that performs the plotting.</param>
		/// <param name="ShowZeroX">If the y-axis (x=0) should always be shown.</param>
		/// <param name="ShowZeroY">If the x-axis (y=0) should always be shown.</param>
		/// <param name="Node">Node creating the graph.</param>
		/// <param name="Parameters">Graph-specific parameters.</param>
		public Graph2D(IVector X, IVector Y, DrawCallback PlotCallback, bool ShowZeroX, bool ShowZeroY,
			ScriptNode Node, params object[] Parameters)
			: base()
		{
			int i, c = X.Dimension;
			bool HasNull = false;
			IElement ex, ey;

			if (c != Y.Dimension)
				throw new ScriptException("X and Y series must be equally large.");

			if (X is Interval XI)
				X = new DoubleVector(XI.GetArray());

			if (Y is Interval YI)
				Y = new DoubleVector(YI.GetArray());

			for (i = 0; i < c; i++)
			{
				ex = X.GetElement(i);
				ey = Y.GetElement(i);

				if (ex.AssociatedObjectValue == null || ey.AssociatedObjectValue == null)
				{
					HasNull = true;
					break;
				}
			}

			this.showZeroX = ShowZeroX;
			this.showZeroY = ShowZeroY;

			this.minX = Min.CalcMin(X, null);
			this.maxX = Max.CalcMax(X, null);

			this.minY = Min.CalcMin(Y, null);
			this.maxY = Max.CalcMax(Y, null);

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

					if (ex.AssociatedObjectValue == null || ey.AssociatedObjectValue == null)
					{
						if (X2.First != null)
						{
							this.AddSegment(X, Y, X2, Y2, Node, PlotCallback, Parameters);
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
					this.AddSegment(X, Y, X2, Y2, Node, PlotCallback, Parameters);
			}
			else
			{
				this.axisTypeX = X.GetType();
				this.axisTypeY = Y.GetType();

				if (c > 0)
				{
					this.x.AddLast(X);
					this.y.AddLast(Y);
					this.callbacks.AddLast(PlotCallback);
					this.parameters.AddLast(Parameters);
				}
			}

			IElement Zero = null;

			if (ShowZeroX && c > 0 && this.minX.AssociatedSet is IAbelianGroup AG)
			{
				Zero = AG.AdditiveIdentity;

				this.minX = Min.CalcMin(new ObjectVector(this.minX, Zero), null);
				this.maxX = Max.CalcMax(new ObjectVector(this.maxX, Zero), null);
			}

			if (ShowZeroY && c > 0 && this.minY.AssociatedSet is IAbelianGroup AG2)
			{
				Zero = AG2.AdditiveIdentity;

				this.minY = Min.CalcMin(new ObjectVector(this.minY, Zero), null);
				this.maxY = Max.CalcMax(new ObjectVector(this.maxY, Zero), null);
			}
		}

		private void AddSegment(IVector X, IVector Y, ICollection<IElement> X2, ICollection<IElement> Y2, 
			ScriptNode Node, DrawCallback PlotCallback, params object[] Parameters)
		{
			IVector X2V = (IVector)X.Encapsulate(X2, Node);
			IVector Y2V = (IVector)Y.Encapsulate(Y2, Node);

			if (this.axisTypeX == null)
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
			this.callbacks.AddLast(PlotCallback);
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
		/// Label for x-axis.
		/// </summary>
		public string LabelX
		{
			get { return this.labelX; }
		}

		/// <summary>
		/// Label for y-axis.
		/// </summary>
		public string LabelY
		{
			get { return this.labelY; }
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
			if (this.x.First == null)
				return Element;

			Graph2D G = Element as Graph2D;
			if (G == null)
				return null;

			if (G.x.First == null)
				return this;

			Graph2D Result = new Graph2D()
			{
				axisTypeX = this.axisTypeX,
				axisTypeY = this.axisTypeY
			};

			foreach (IVector v in this.x)
				Result.x.AddLast(v);

			foreach (IVector v in this.y)
				Result.y.AddLast(v);

			foreach (DrawCallback Callback in this.callbacks)
				Result.callbacks.AddLast(Callback);

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

			foreach (DrawCallback Callback in G.callbacks)
				Result.callbacks.AddLast(Callback);

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
			Graph2D G = obj as Graph2D;
			if (G == null)
				return false;

			return (
				this.minX.Equals(G.minX) &&
				this.maxX.Equals(G.maxX) &&
				this.minY.Equals(G.minY) &&
				this.maxY.Equals(G.maxY) &&
				this.axisTypeX.Equals(G.axisTypeX) &&
				this.axisTypeY.Equals(G.axisTypeY) &&
				this.labelX.Equals(G.labelX) &&
				this.labelY.Equals(G.labelY) &&
				this.showXAxis.Equals(G.showXAxis) &&
				this.showYAxis.Equals(G.showYAxis) &&
				this.showGrid.Equals(G.showGrid) &&
				this.Equals(this.x.GetEnumerator(), G.x.GetEnumerator()) &&
				this.Equals(this.y.GetEnumerator(), G.y.GetEnumerator()) &&
				this.Equals(this.parameters.GetEnumerator(), G.parameters.GetEnumerator()) &&
				this.Equals(this.callbacks.GetEnumerator(), G.callbacks.GetEnumerator()));
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
			int Result =
				this.minX.GetHashCode() ^
				this.maxX.GetHashCode() ^
				this.minY.GetHashCode() ^
				this.maxY.GetHashCode() ^
				this.axisTypeX.GetHashCode() ^
				this.axisTypeY.GetHashCode() ^
				this.labelX.GetHashCode() ^
				this.labelY.GetHashCode() ^
				this.showXAxis.GetHashCode() ^
				this.showYAxis.GetHashCode() ^
				this.showGrid.GetHashCode();

			foreach (IElement E in this.x)
				Result ^= E.GetHashCode();

			foreach (IElement E in this.y)
				Result ^= E.GetHashCode();

			foreach (object Obj in this.parameters)
				Result ^= Obj.GetHashCode();

			foreach (DrawCallback Callback in this.callbacks)
				Result ^= Callback.GetHashCode();

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
			using (SKSurface Surface = SKSurface.Create(Settings.Width, Settings.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
			{
				SKCanvas Canvas = Surface.Canvas;

				States = new object[0];

				Canvas.Clear(Settings.BackgroundColor);

				int x1, y1, x2, y2, x3, y3, w, h;

				x1 = Settings.MarginLeft;
				x2 = Settings.Width - Settings.MarginRight;
				y1 = Settings.MarginTop;
				y2 = Settings.Height - Settings.MarginBottom;

				IVector YLabels = GetLabels(ref this.minY, ref this.maxY, this.y, Settings.ApproxNrLabelsY, out LabelType YLabelType);
				SKPaint Font = new SKPaint()
				{
					FilterQuality = SKFilterQuality.High,
					HintingLevel = SKPaintHinting.Full,
					SubpixelText = true,
					IsAntialias = true,
					Style = SKPaintStyle.Fill,
					Color = Settings.AxisColor,
					Typeface = SKTypeface.FromFamilyName(Settings.FontName, SKTypefaceStyle.Normal),
					TextSize = (float)Settings.LabelFontSize
				};
				float Size;
				double MaxSize = 0;

				if (this.showYAxis)
				{
					foreach (IElement Label in YLabels.ChildElements)
					{
						Size = Font.MeasureText(LabelString(Label, YLabelType));
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
						Size = Font.MeasureText(LabelString(Label, XLabelType));
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
					Size = Font.MeasureText(s = LabelString(Label, YLabelType));
					f = (float)LabelYY[i++];

					if (this.showGrid)
					{
						if (Label is DoubleNumber && ((DoubleNumber)Label).Value == 0)
							Canvas.DrawLine(x3, f, x2, f, AxisPen);
						else
							Canvas.DrawLine(x3, f, x2, f, GridPen);
					}

					if (this.showYAxis)
					{
						f += (float)Settings.LabelFontSize * 0.5f;
						Canvas.DrawText(s, x3 - Size - Settings.MarginLabel, f, Font);
					}
				}

				double[] LabelXX = DrawingArea.ScaleX(XLabels);
				i = 0;

				foreach (IElement Label in XLabels.ChildElements)
				{
					Size = Font.MeasureText(s = LabelString(Label, XLabelType));
					f = (float)LabelXX[i++];

					if (this.showGrid)
					{
						if (Label is DoubleNumber && ((DoubleNumber)Label).Value == 0)
							Canvas.DrawLine(f, y1, f, y3, AxisPen);
						else
							Canvas.DrawLine(f, y1, f, y3, GridPen);
					}

					if (this.showXAxis)
					{
						f -= Size * 0.5f;
						if (f < x3)
							f = x3;
						else if (f + Size > x3 + w)
							f = x3 + w - Size;

						Canvas.DrawText(s, f, y3 + Settings.MarginLabel + (float)Settings.LabelFontSize, Font);
					}
				}

				IEnumerator<IVector> ex = this.x.GetEnumerator();
				IEnumerator<IVector> ey = this.y.GetEnumerator();
				IEnumerator<object[]> eParameters = this.parameters.GetEnumerator();
				IEnumerator<DrawCallback> eCallbacks = this.callbacks.GetEnumerator();
				SKPoint[] Points;
				SKPoint[] PrevPoints = null;
				object[] PrevParameters = null;
				DrawCallback PrevCallback = null;

				while (ex.MoveNext() && ey.MoveNext() && eParameters.MoveNext() && eCallbacks.MoveNext())
				{
					Points = DrawingArea.Scale(ex.Current, ey.Current);

					if (PrevCallback != null && eCallbacks.Current.Target.GetType() == PrevCallback.Target.GetType())
						eCallbacks.Current(Canvas, Points, eParameters.Current, PrevPoints, PrevParameters, DrawingArea);
					else
						eCallbacks.Current(Canvas, Points, eParameters.Current, null, null, DrawingArea);

					PrevPoints = Points;
					PrevParameters = eParameters.Current;
					PrevCallback = eCallbacks.Current;
				}

				SKImage Result = Surface.Snapshot();

				Font.Dispose();
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

	}
}
