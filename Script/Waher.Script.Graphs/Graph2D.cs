using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Vectors;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Graphs
{
	public delegate void DrawCallback(Graphics Canvas, PointF[] Points, object[] Parameters);

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
		public Graph2D(IVector X, IVector Y, DrawCallback PlotCallback, params object[] Parameters)
			: base()
		{
			if (X.Dimension != Y.Dimension)
				throw new ScriptException("X and Y series must be equally large.");

			this.axisTypeX = X.GetType();
			this.axisTypeY = Y.GetType();

			this.x.AddLast(X);
			this.y.AddLast(Y);
			this.callbacks.AddLast(PlotCallback);
			this.parameters.AddLast(Parameters);

			this.minX = Min.CalcMin(X, null);
			this.maxX = Max.CalcMax(X, null);

			this.minY = Min.CalcMin(Y, null);
			this.maxY = Max.CalcMax(Y, null);
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
			Graph2D G = Element as Graph2D;
			if (G == null)
				return null;

			Graph2D Result = new Graph2D();

			Result.axisTypeX = this.axisTypeX;
			Result.axisTypeY = this.axisTypeY;

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
			Result.maxX = Min.CalcMin((IVector)VectorDefinition.Encapsulate(new IElement[] { this.maxX, G.maxX }, false, null), null);
			Result.minY = Min.CalcMin((IVector)VectorDefinition.Encapsulate(new IElement[] { this.minY, G.minY }, false, null), null);
			Result.maxY = Min.CalcMin((IVector)VectorDefinition.Encapsulate(new IElement[] { this.maxY, G.maxY }, false, null), null);

			return Result;
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
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
				this.labelY.GetHashCode();

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
		/// <returns>Bitmap</returns>
		public override Bitmap CreateBitmap(GraphSettings Settings)
		{
			Bitmap Bmp = new Bitmap(Settings.Width, Settings.Height);

			using (Graphics Canvas = Graphics.FromImage(Bmp))
			{
				Canvas.Clear(Settings.BackgroundColor);

				Canvas.CompositingMode = CompositingMode.SourceOver;
				Canvas.CompositingQuality = CompositingQuality.HighQuality;
				Canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
				Canvas.SmoothingMode = SmoothingMode.HighQuality;
				Canvas.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

				int x1, y1, x2, y2, x3, y3, w, h;

				x1 = Settings.MarginLeft;
				x2 = Settings.Width - Settings.MarginRight;
				y1 = Settings.MarginTop;
				y2 = Settings.Height - Settings.MarginBottom;

				IVector YLabels = GetLabels(this.minY, this.maxY, this.y, Settings.ApproxNrLabelsY);
				Font Font = new Font(Settings.FontName, (float)Settings.LabelFontSize);
				SizeF Size;
				double MaxSize = 0;

				foreach (IElement Label in YLabels.ChildElements)
				{
					Size = Canvas.MeasureString(Label.ToString(), Font);
					if (Size.Width > MaxSize)
						MaxSize = Size.Width;
				}

				x3 = (int)Math.Ceiling(x1 + MaxSize) + Settings.MarginLabel;

				IVector XLabels = GetLabels(this.minX, this.maxX, this.x, Settings.ApproxNrLabelsX);
				MaxSize = 0;

				foreach (IElement Label in XLabels.ChildElements)
				{
					Size = Canvas.MeasureString(Label.ToString(), Font);
					if (Size.Height > MaxSize)
						MaxSize = Size.Height;
				}

				y3 = (int)Math.Floor(y2 - MaxSize) - Settings.MarginLabel;
				w = x2 - x3;
				h = y3 - y1;

				Brush AxisBrush = new SolidBrush(Settings.AxisColor);
				Brush GridBrush = new SolidBrush(Settings.GridColor);
				Pen AxisPen = new Pen(AxisBrush, Settings.AxisWidth);
				Pen GridPen = new Pen(GridBrush, Settings.GridWidth);
				double[] LabelYY = Scale(YLabels, this.minY, this.maxY, y3, -h);
				int i = 0;
				float f;

				foreach (IElement Label in YLabels.ChildElements)
				{
					Size = Canvas.MeasureString(Label.ToString(), Font);
					f = (float)LabelYY[i++];

					if (Label is DoubleNumber && ((DoubleNumber)Label).Value == 0)
						Canvas.DrawLine(AxisPen, x3, f, x2, f);
					else
						Canvas.DrawLine(GridPen, x3, f, x2, f);

					f -= Size.Height * 0.5f;
					Canvas.DrawString(Label.AssociatedObjectValue.ToString(), Font, AxisBrush, x3 - Size.Width - Settings.MarginLabel, f);
				}

				double[] LabelXX = Scale(XLabels, this.minX, this.maxX, x3, w);
				i = 0;

				foreach (IElement Label in XLabels.ChildElements)
				{
					Size = Canvas.MeasureString(Label.ToString(), Font);
					f = (float)LabelXX[i++];

					if (Label is DoubleNumber && ((DoubleNumber)Label).Value == 0)
						Canvas.DrawLine(AxisPen, f, y1, f, y3);
					else
						Canvas.DrawLine(GridPen, f, y1, f, y3);

					if (i == LabelXX.Length)
						f -= Size.Width;
					else if (i > 1)
						f -= Size.Width * 0.5f;

					Canvas.DrawString(Label.AssociatedObjectValue.ToString(), Font, AxisBrush, f, y3 + Settings.MarginLabel);
				}

				IEnumerator<IVector> ex = this.x.GetEnumerator();
				IEnumerator<IVector> ey = this.y.GetEnumerator();
				IEnumerator<object[]> eParameters = this.parameters.GetEnumerator();
				IEnumerator<DrawCallback> eCallbacks = this.callbacks.GetEnumerator();
				PointF[] Points;

				while (ex.MoveNext() && ey.MoveNext() && eParameters.MoveNext() && eCallbacks.MoveNext())
				{
					Points = Scale(ex.Current, ey.Current, this.minX, this.maxX, this.minY, this.maxY, x3, y3, w, -h);
					eCallbacks.Current(Canvas, Points, eParameters.Current);
				}

				Font.Dispose();
				AxisBrush.Dispose();
				GridBrush.Dispose();
				GridPen.Dispose();
				AxisPen.Dispose();
			}

			return Bmp;
		}

	}
}
