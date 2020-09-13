using System;
using System.Collections.Generic;
using SkiaSharp;
using System.Globalization;
using System.Text;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Objects.Sets;
using Waher.Script.Units;

namespace Waher.Script.Graphs
{
	/// <summary>
	///  Type of labels
	/// </summary>
	public enum LabelType
	{
		/// <summary>
		/// Double-valued label.
		/// </summary>
		Double,

		/// <summary>
		/// Year DateTime label.
		/// </summary>
		DateTimeYear,

		/// <summary>
		/// Quarter DateTime label.
		/// </summary>
		DateTimeQuarter,

		/// <summary>
		/// Month DateTime label.
		/// </summary>
		DateTimeMonth,

		/// <summary>
		/// Week DateTime label.
		/// </summary>
		DateTimeWeek,

		/// <summary>
		/// Date DateTime label.
		/// </summary>
		DateTimeDate,

		/// <summary>
		/// Short Time DateTime label.
		/// </summary>
		DateTimeShortTime,

		/// <summary>
		/// Short Time DateTime label.
		/// </summary>
		DateTimeLongTime,

		/// <summary>
		/// Physical quantity labels.
		/// </summary>
		PhysicalQuantity,

		/// <summary>
		/// String label.
		/// </summary>
		String
	}

	/// <summary>
	/// Base class for graphs.
	/// </summary>
	public abstract class Graph : SemiGroupElement
	{
		/// <summary>
		/// Base class for graphs.
		/// </summary>
		public Graph()
			: base()
		{
		}

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup
		{
			get
			{
				return SetOfGraphs.Instance;
			}
		}

		/// <summary>
		/// Gets default graph settings for drawing the graph.
		/// </summary>
		/// <param name="Variables">Current set of variables, where graph settings might be available.</param>
		/// <returns>Graph settings.</returns>
		public GraphSettings GetSettings(Variables Variables)
		{
			return this.GetSettings(Variables, null, null);
		}

		/// <summary>
		/// Gets default graph settings for drawing the graph.
		/// </summary>
		/// <param name="Variables">Current set of variables, where graph settings might be available.</param>
		/// <param name="DefaultWidth">Default width.</param>
		/// <param name="DefaultHeight">Default height.</param>
		/// <returns>Graph settings.</returns>
		public GraphSettings GetSettings(Variables Variables, int? DefaultWidth, int? DefaultHeight)
		{
			GraphSettings Settings = new GraphSettings();
			Tuple<int, int> Size;

			if (DefaultWidth.HasValue)
				Settings.Width = DefaultWidth.Value;

			if (DefaultHeight.HasValue)
				Settings.Height = DefaultHeight.Value;

			if ((Size = this.RecommendedBitmapSize) != null)
			{
				Settings.Width = Size.Item1;
				Settings.Height = Size.Item2;

				Settings.MarginLeft = (int)Math.Round(15.0 * Settings.Width / 640);
				Settings.MarginRight = Settings.MarginLeft;

				Settings.MarginTop = (int)Math.Round(15.0 * Settings.Height / 480);
				Settings.MarginBottom = Settings.MarginTop;
				Settings.LabelFontSize = 12.0 * Settings.Height / 480;
			}
			else
			{
				if (Variables.TryGetVariable("GraphWidth", out Variable v) && v.ValueObject is double d && d >= 1)
				{
					Settings.Width = (int)Math.Round(d);
					Settings.MarginLeft = (int)Math.Round(15 * d / 640);
					Settings.MarginRight = Settings.MarginLeft;
				}
				else if (!Variables.ContainsVariable("GraphWidth"))
					Variables["GraphWidth"] = (double)Settings.Width;

				if (Variables.TryGetVariable("GraphHeight", out v) && v.ValueObject is double d2 && d2 >= 1)
				{
					Settings.Height = (int)Math.Round(d2);
					Settings.MarginTop = (int)Math.Round(15 * d2 / 480);
					Settings.MarginBottom = Settings.MarginTop;
					Settings.LabelFontSize = 12 * d2 / 480;
				}
				else if (!Variables.ContainsVariable("GraphHeight"))
					Variables["GraphHeight"] = (double)Settings.Height;
			}

			return Settings;
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Variables">Variables from where default settings can be retrieved if not available in graph.</param>
		/// <returns>Bitmap</returns>
		public SKImage CreateBitmap(Variables Variables)
		{
			return this.CreateBitmap(Variables, out GraphSettings _, out object[] _);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Variables">Variables from where default settings can be retrieved if not available in graph.</param>
		/// <param name="Settings">Settings used to create the graph.</param>
		/// <returns>Bitmap</returns>
		public SKImage CreateBitmap(Variables Variables, out GraphSettings Settings)
		{
			return this.CreateBitmap(Variables, out Settings, out object[] _);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Variables">Variables from where default settings can be retrieved if not available in graph.</param>
		/// <param name="States">State objects that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public SKImage CreateBitmap(Variables Variables, out object[] States)
		{
			return this.CreateBitmap(Variables, out GraphSettings _, out States);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Variables">Variables from where default settings can be retrieved if not available in graph.</param>
		/// <param name="Settings">Settings used to create the graph.</param>
		/// <param name="States">State objects that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public SKImage CreateBitmap(Variables Variables, out GraphSettings Settings, out object[] States)
		{
			Settings = this.GetSettings(Variables);
			return this.CreateBitmap(Settings, out States);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <returns>Bitmap</returns>
		public SKImage CreateBitmap(GraphSettings Settings)
		{
			return this.CreateBitmap(Settings, out object[] _);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <param name="States">State objects that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public abstract SKImage CreateBitmap(GraphSettings Settings, out object[] States);

		/// <summary>
		/// Gets script corresponding to a point in a generated bitmap representation of the graph.
		/// </summary>
		/// <param name="X">X-Coordinate.</param>
		/// <param name="Y">Y-Coordinate.</param>
		/// <param name="States">State objects for the generated bitmap.</param>
		/// <returns>Script.</returns>
		public abstract string GetBitmapClickScript(double X, double Y, object[] States);

		/// <summary>
		/// The recommended bitmap size of the graph, if such is available, or null if not.
		/// </summary>
		public virtual Tuple<int, int> RecommendedBitmapSize
		{
			get { return null; }
		}

		/// <summary>
		/// Scales two vectors of equal size to points in a rectangular area.
		/// </summary>
		/// <param name="VectorX">X-vector.</param>
		/// <param name="VectorY">Y-vector.</param>
		/// <param name="MinX">Smallest X-value.</param>
		/// <param name="MaxX">Largest X-value.</param>
		/// <param name="MinY">Smallest Y-value.</param>
		/// <param name="MaxY">Largest Y-value.</param>
		/// <param name="OffsetX">X-offset to area.</param>
		/// <param name="OffsetY">Y-offset to area.</param>
		/// <param name="Width">Width of area.</param>
		/// <param name="Height">Height of area.</param>
		/// <returns>Sequence of points.</returns>
		public static SKPoint[] Scale(IVector VectorX, IVector VectorY, IElement MinX, IElement MaxX,
			IElement MinY, IElement MaxY, double OffsetX, double OffsetY, double Width, double Height)
		{
			if (VectorX.Dimension != VectorY.Dimension)
				throw new ScriptException("Dimension mismatch.");

			double[] X = Scale(VectorX, MinX, MaxX, OffsetX, Width);
			double[] Y = Scale(VectorY, MinY, MaxY, OffsetY, Height);
			int i, c = X.Length;
			SKPoint[] Points = new SKPoint[c];

			for (i = 0; i < c; i++)
				Points[i] = new SKPoint((float)X[i], (float)Y[i]);

			return Points;
		}

		/// <summary>
		/// Scales a vector to fit a given area.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="Offset">Offset to area.</param>
		/// <param name="Size">Size of area.</param>
		/// <returns>Vector distributed in the available area.</returns>
		public static double[] Scale(IVector Vector, IElement Min, IElement Max, double Offset, double Size)
		{
			if (Vector is DoubleVector DV)
			{
				if (!(Min is DoubleNumber dMin) || !(Max is DoubleNumber dMax))
					throw new ScriptException("Incompatible values.");

				return Scale(DV.Values, dMin.Value, dMax.Value, Offset, Size);
			}
			else if (Vector is Interval I)
			{
				if (!(Min is DoubleNumber dMin) || !(Max is DoubleNumber dMax))
					throw new ScriptException("Incompatible values.");

				return Scale(I.GetArray(), dMin.Value, dMax.Value, Offset, Size);
			}
			else if (Vector is DateTimeVector DTV)
			{
				if (!(Min is DateTimeValue dMin) || !(Max is DateTimeValue dMax))
					throw new ScriptException("Incompatible values.");

				return Scale(DTV.Values, dMin.Value, dMax.Value, Offset, Size);
			}
			else if (Vector is ObjectVector OV)
			{
				if (Min is PhysicalQuantity MinQ && Max is PhysicalQuantity MaxQ)
				{
					if (MinQ.Unit != MaxQ.Unit)
					{
						if (!Unit.TryConvert(MaxQ.Magnitude, MaxQ.Unit, MinQ.Unit, out double d))
							throw new ScriptException("Incompatible units.");

						MaxQ = new PhysicalQuantity(d, MinQ.Unit);
					}

					int i = 0;
					int c = Vector.Dimension;
					PhysicalQuantity[] Vector2 = new PhysicalQuantity[c];
					PhysicalQuantity Q;

					foreach (IElement E in Vector.ChildElements)
					{
						Q = E as PhysicalQuantity ?? throw new ScriptException("Incompatible values.");
						Vector2[i++] = Q;
					}

					return Scale(Vector2, MinQ.Magnitude, MaxQ.Magnitude, MinQ.Unit, Offset, Size);
				}
				else
				{
					if (Min is DoubleNumber MinD && Max is DoubleNumber MaxD)
					{
						int i = 0;
						int c = Vector.Dimension;
						double[] Vector2 = new double[c];
						DoubleNumber D;

						foreach (IElement E in Vector.ChildElements)
						{
							D = E as DoubleNumber;
							if (D is null)
								throw new ScriptException("Incompatible values.");

							Vector2[i++] = D.Value;
						}

						return Scale(Vector2, MinD.Value, MaxD.Value, Offset, Size);
					}
					else
					{
						if (Min is DateTimeValue MinDT && Max is DateTimeValue MaxDT)
						{
							int i = 0;
							int c = Vector.Dimension;
							DateTime[] Vector2 = new DateTime[c];
							DateTimeValue DT;

							foreach (IElement E in Vector.ChildElements)
							{
								DT = E as DateTimeValue;
								if (DT is null)
									throw new ScriptException("Incompatible values.");

								Vector2[i++] = DT.Value;
							}

							return Scale(Vector2, MinDT.Value, MaxDT.Value, Offset, Size);
						}
						else
							return Scale(OV.Values, Offset, Size);
					}
				}
			}
			else
				throw new ScriptException("Invalid vector type.");
		}

		/// <summary>
		/// Scales a vector to fit a given area.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="Offset">Offset to area.</param>
		/// <param name="Size">Size of area.</param>
		/// <returns>Vector distributed in the available area.</returns>
		public static double[] Scale(double[] Vector, double Min, double Max, double Offset, double Size)
		{
			int i, c = Vector.Length;
			double[] Result = new double[c];
			double Scale = Min == Max ? 1 : Size / (Max - Min);

			for (i = 0; i < c; i++)
				Result[i] = (Vector[i] - Min) * Scale + Offset;

			return Result;
		}

		/// <summary>
		/// Scales a vector to fit a given area.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="Offset">Offset to area.</param>
		/// <param name="Size">Size of area.</param>
		/// <returns>Vector distributed in the available area.</returns>
		public static double[] Scale(DateTime[] Vector, DateTime Min, DateTime Max, double Offset, double Size)
		{
			int i, c = Vector.Length;
			double[] v = new double[c];

			for (i = 0; i < c; i++)
				v[i] = (Vector[i] - referenceTimestamp).TotalDays;

			return Scale(v, (Min - referenceTimestamp).TotalDays, (Max - referenceTimestamp).TotalDays, Offset, Size);
		}

		private static readonly DateTime referenceTimestamp = new DateTime(2000, 1, 1, 0, 0, 0);

		/// <summary>
		/// Scales a vector to fit a given area.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Offset">Offset to area.</param>
		/// <param name="Size">Size of area.</param>
		/// <returns>Vector distributed in the available area.</returns>
		public static double[] Scale(object[] Vector, double Offset, double Size)
		{
			int i, c = Vector.Length;
			double[] v = new double[c];

			for (i = 0; i < c; i++)
				v[i] = i + 0.5;

			return Scale(v, 0, c, Offset, Size);
		}

		/// <summary>
		/// Scales a vector to fit a given area.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="Unit">Unit.</param>
		/// <param name="Offset">Offset to area.</param>
		/// <param name="Size">Size of area.</param>
		/// <returns>Vector distributed in the available area.</returns>
		public static double[] Scale(PhysicalQuantity[] Vector, double Min, double Max, Unit Unit, double Offset, double Size)
		{
			int i, c = Vector.Length;
			double[] v = new double[c];
			PhysicalQuantity Q;

			for (i = 0; i < c; i++)
			{
				Q = Vector[i];
				if (Q.Unit.Equals(Unit) || Q.Unit.IsEmpty)
					v[i] = Q.Magnitude;
				else if (!Unit.TryConvert(Q.Magnitude, Q.Unit, Unit, out v[i]))
					throw new ScriptException("Incompatible units.");
			}

			return Scale(v, Min, Max, Offset, Size);
		}

		/// <summary>
		/// Descales a scaled value.
		/// </summary>
		/// <param name="Value">Scaled value.</param>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="Offset">Drawing offset.</param>
		/// <param name="Size">Size of drawing area.</param>
		/// <returns>Descaled value.</returns>
		public static IElement Descale(double Value, IElement Min, IElement Max, double Offset, double Size)
		{
			// (v-Offset)*(Max-Min)/Size+Min

			if (Min is DoubleNumber DMin && Max is DoubleNumber DMax)
				return new DoubleNumber((Value - Offset) * (DMax.Value - DMin.Value) / Size + DMin.Value);
			else
			{
				IElement Delta = Operators.Arithmetics.Subtract.EvaluateSubtraction(Max, Min, null);
				IElement Result = Operators.Arithmetics.Multiply.EvaluateMultiplication(new DoubleNumber(Value - Offset), Delta, null);
				Result = Operators.Arithmetics.Divide.EvaluateDivision(Result, new DoubleNumber(Size), null);
				return Operators.Arithmetics.Add.EvaluateAddition(Result, Min, null);
			}
		}

		/// <summary>
		/// Converts an object to a pen value.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Size">Optional size.</param>
		/// <returns>Pen object.</returns>
		public static SKPaint ToPen(object Object, object Size)
		{
			double Width = Size is null ? 1 : Expression.ToDouble(Size);

			if (Object is SKPaint Pen)
				return Pen;
			else if (Object is SKColor Color)
			{
				return new SKPaint()
				{
					FilterQuality = SKFilterQuality.High,
					IsAntialias = true,
					Style = SKPaintStyle.Stroke,
					Color = Color,
					StrokeWidth = (float)Width
				};
			}
			else if (Object is SKPaint Brush)
				return Brush;
			else
			{
				return new SKPaint()
				{
					IsAntialias = true,
					Style = SKPaintStyle.Stroke,
					Color = ToColor(Object),
					StrokeWidth = (float)Width
				};
			}
		}

		/// <summary>
		/// Converts an object to a color.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Color value.</returns>
		public static SKColor ToColor(object Object)
		{
			if (Object is SKColor Color)
				return Color;
			else if (Object is string s && Functions.Colors.Color.TryParse(s, out Color))
				return Color;

			throw new ScriptException("Expected a color.");
		}

		/// <summary>
		/// Creates a Color from its HSL representation.
		/// 
		/// RGB conversion done according to:
		/// https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSL
		/// </summary>
		/// <param name="H">Hue H ∈ [0°, 360°).</param>
		/// <param name="S">Saturation S ∈ [0, 1].</param>
		/// <param name="L">Lightness L ∈ [0, 1].</param>
		/// <returns>Color</returns>
		public static SKColor ToColorHSL(double H, double S, double L)
		{
			return SKColor.FromHsl((float)H, (float)(S * 100), (float)(L * 100));
		}

		/// <summary>
		/// Creates a Color from its HSL representation.
		/// 
		/// RGB conversion done according to:
		/// https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSL
		/// </summary>
		/// <param name="H">Hue H ∈ [0°, 360°).</param>
		/// <param name="S">Saturation S ∈ [0, 1].</param>
		/// <param name="L">Lightness L ∈ [0, 1].</param>
		/// <param name="A">Alpha A ∈ [0, 255].</param>
		/// <returns>Color</returns>
		public static SKColor ToColorHSL(double H, double S, double L, byte A)
		{
			return SKColor.FromHsl((float)H, (float)(S * 100), (float)(L * 100), A);
		}

		/// <summary>
		/// Creates a Color from its HSV representation.
		/// 
		/// RGB conversion done according to:
		/// https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSV
		/// </summary>
		/// <param name="H">Hue H ∈ [0°, 360°).</param>
		/// <param name="S">Saturation S ∈ [0, 1].</param>
		/// <param name="V">Value V ∈ [0, 1].</param>
		/// <returns>Color</returns>
		public static SKColor ToColorHSV(double H, double S, double V)
		{
			return SKColor.FromHsv((float)H, (float)(S * 100), (float)(V * 100));
		}

		/// <summary>
		/// Creates a Color from its HSV representation.
		/// 
		/// RGB conversion done according to:
		/// https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSV
		/// </summary>
		/// <param name="H">Hue H ∈ [0°, 360°).</param>
		/// <param name="S">Saturation S ∈ [0, 1].</param>
		/// <param name="V">Value V ∈ [0, 1].</param>
		/// <param name="A">Alpha A ∈ [0, 255].</param>
		/// <returns>Color</returns>
		public static SKColor ToColorHSV(double H, double S, double V, byte A)
		{
			return SKColor.FromHsv((float)H, (float)(S * 100), (float)(V * 100), A);
		}

		/// <summary>
		/// Gets label values for a series vector.
		/// </summary>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="Series">Series to draw.</param>
		/// <param name="ApproxNrLabels">Number of labels.</param>
		/// <param name="LabelType">Type of labels produced.</param>
		/// <returns>Vector of labels.</returns>
		public static IVector GetLabels(ref IElement Min, ref IElement Max, IEnumerable<IVector> Series, int ApproxNrLabels, out LabelType LabelType)
		{
			if (Min is DoubleNumber DMin && Max is DoubleNumber DMax)
			{
				LabelType = LabelType.Double;
				return new DoubleVector(GetLabels(DMin.Value, DMax.Value, ApproxNrLabels));
			}
			else if (Min is DateTimeValue DTMin && Max is DateTimeValue DTMax)
				return new DateTimeVector(GetLabels(DTMin.Value, DTMax.Value, ApproxNrLabels, out LabelType));
			else if (Min is PhysicalQuantity QMin && Max is PhysicalQuantity QMax)
			{
				LabelType = LabelType.PhysicalQuantity;
				return new ObjectVector(GetLabels(QMin, QMax, ApproxNrLabels));
			}
			else if (Min is StringValue && Max is StringValue)
			{
				Dictionary<string, bool> Indices = new Dictionary<string, bool>();
				List<IElement> Labels = new List<IElement>();
				string s;

				foreach (IVector Vector in Series)
				{
					foreach (IElement E in Vector.ChildElements)
					{
						s = E.AssociatedObjectValue.ToString();
						if (Indices.ContainsKey(s))
							continue;

						Labels.Add(E);
						Indices[s] = true;
					}
				}

				LabelType = LabelType.String;

				if (Labels.Count > 0)
				{
					Min = Labels[0];
					Max = Labels[Labels.Count - 1];
				}

				return new ObjectVector(Labels.ToArray());
			}
			else
			{
				SortedDictionary<string, bool> Labels = new SortedDictionary<string, bool>();

				foreach (IVector Vector in Series)
				{
					foreach (IElement E in Vector.ChildElements)
						Labels[E.AssociatedObjectValue.ToString()] = true;
				}

				string[] Labels2 = new string[Labels.Count];
				Labels.Keys.CopyTo(Labels2, 0);

				LabelType = LabelType.String;
				return new ObjectVector(Labels2);
			}
		}

		/// <summary>
		/// Gets label values for a series vector.
		/// </summary>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="ApproxNrLabels">Number of labels.</param>
		/// <returns>Vector of labels.</returns>
		public static double[] GetLabels(double Min, double Max, int ApproxNrLabels)
		{
			if (double.IsInfinity(Min))
				throw new ArgumentException("Infinite interval.", nameof(Min));

			if (double.IsInfinity(Max))
				throw new ArgumentException("Infinite interval.", nameof(Max));

			double StepSize = GetStepSize(Min, Max, ApproxNrLabels);
			List<double> Steps = new List<double>();
			int i = (int)Math.Ceiling(Min / StepSize);
			double d = i * StepSize;

			while (d <= Max)
			{
				Steps.Add(d);
				d = ++i * StepSize;
			}

			return Steps.ToArray();
		}

		/// <summary>
		/// Gets a human readable step size for an interval, given its limits and desired number of steps.
		/// </summary>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="ApproxNrLabels">Number of labels.</param>
		/// <returns>Recommended step size.</returns>
		public static double GetStepSize(double Min, double Max, int ApproxNrLabels)
		{
			double Delta = Max - Min;
			if (Delta == 0)
				return 1;

			double StepSize0 = Math.Pow(10, Math.Round(Math.Log10(Delta / ApproxNrLabels)));
			double StepSize = StepSize0;
			double BestStepSize = StepSize0;
			int NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
			int BestDiff = Math.Abs(NrSteps - ApproxNrLabels);
			int Diff;

			if (NrSteps > ApproxNrLabels)
			{
				StepSize = StepSize0 * 2;
				NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
				Diff = Math.Abs(NrSteps - ApproxNrLabels);
				if (Diff < BestDiff)
				{
					BestDiff = Diff;
					BestStepSize = StepSize;

					StepSize = StepSize0 * 2.5;
					NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
					Diff = Math.Abs(NrSteps - ApproxNrLabels);
					if (Diff < BestDiff)
					{
						BestDiff = Diff;
						BestStepSize = StepSize;

						StepSize = StepSize0 * 5;
						NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
						Diff = Math.Abs(NrSteps - ApproxNrLabels);
						if (Diff < BestDiff)
							BestStepSize = StepSize;
					}
				}
			}
			else if (NrSteps < ApproxNrLabels)
			{
				StepSize = StepSize0 / 2;
				NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
				Diff = Math.Abs(NrSteps - ApproxNrLabels);
				if (Diff < BestDiff)
				{
					BestDiff = Diff;
					BestStepSize = StepSize;

					StepSize = StepSize0 / 4;
					NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
					Diff = Math.Abs(NrSteps - ApproxNrLabels);
					if (Diff < BestDiff)
					{
						BestDiff = Diff;
						BestStepSize = StepSize;

						StepSize = StepSize0 / 5;
						NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
						Diff = Math.Abs(NrSteps - ApproxNrLabels);
						if (Diff < BestDiff)
							BestStepSize = StepSize;
					}
				}
			}

			return BestStepSize;
		}

		/// <summary>
		/// Gets label values for a series vector.
		/// </summary>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="ApproxNrLabels">Number of labels.</param>
		/// <param name="LabelType">Type of labels produced.</param>
		/// <returns>Vector of labels.</returns>
		public static DateTime[] GetLabels(DateTime Min, DateTime Max, int ApproxNrLabels, out LabelType LabelType)
		{
			List<DateTime> Labels = new List<DateTime>();
			TimeSpan Span = Max - Min;
			TimeSpan StepSize = Span == TimeSpan.Zero ? new TimeSpan(0, 0, 1) : new TimeSpan((Span.Ticks + (ApproxNrLabels >> 1)) / ApproxNrLabels);
			int i, c;

			for (i = 0, c = timeStepSizes.Length - 2; i < c; i++)
			{
				if (StepSize >= timeStepSizes[i] && StepSize < timeStepSizes[i + 1])
					break;
			}

			int Nr1 = (int)Math.Round(Span.TotalSeconds / timeStepSizes[i].TotalSeconds);
			int Diff1 = Math.Abs(ApproxNrLabels - Nr1);

			int Nr2 = (int)Math.Round(Span.TotalSeconds / timeStepSizes[i + 1].TotalSeconds);
			int Diff2 = Math.Abs(ApproxNrLabels - Nr1);

			if (Diff1 < Diff2)
				StepSize = timeStepSizes[i];
			else
				StepSize = timeStepSizes[i + 1];

			if (StepSize.TotalDays >= 1)
			{
				DateTime TP;

				TP = Min.Date;
				if (Min.TimeOfDay != TimeSpan.Zero)
					TP = TP.AddDays(1);

				Nr1 = (int)Math.Floor((Max - TP).TotalDays);
				Diff1 = Math.Abs(ApproxNrLabels - Nr1);

				Nr2 = (int)Math.Floor((Max - TP).TotalDays / 2);
				Diff2 = Math.Abs(ApproxNrLabels - Nr2);

				if (Diff1 <= Diff2)
				{
					LabelType = LabelType.DateTimeDate;

					while (TP <= Max)
					{
						Labels.Add(TP);
						TP = TP.AddDays(1);
					}
				}
				else
				{
					Nr1 = Nr2;
					Diff1 = Diff2;

					Nr2 = (int)Math.Floor((Max - TP).TotalDays / 7);
					Diff2 = Math.Abs(ApproxNrLabels - Nr2);

					if (Diff1 <= Diff2)
					{
						LabelType = LabelType.DateTimeDate; // Step every 2 days.

						while (TP <= Max)
						{
							Labels.Add(TP);
							TP = TP.AddDays(2);
						}
					}
					else
					{
						Nr1 = Nr2;
						Diff1 = Diff2;

						Nr2 = (int)Math.Floor((Max - TP).TotalDays / 30);
						Diff2 = Math.Abs(ApproxNrLabels - Nr2);

						if (Diff1 <= Diff2)
						{
							LabelType = LabelType.DateTimeWeek;

							i = (int)TP.DayOfWeek;
							if (i == 0)
								TP = TP.AddDays(1);
							else if (i != 1)
								TP = TP.AddDays(8 - i);

							while (TP <= Max)
							{
								Labels.Add(TP);
								TP = TP.AddDays(7);
							}
						}
						else
						{
							Nr1 = Nr2;
							Diff1 = Diff2;

							Nr2 = (int)Math.Floor((Max - TP).TotalDays / 180);
							Diff2 = Math.Abs(ApproxNrLabels - Nr2);

							if (Diff1 <= Diff2)
							{
								LabelType = LabelType.DateTimeMonth;

								if (TP.Day != 1)
									TP = TP.AddDays(-TP.Day + 1).AddMonths(1);

								while (TP <= Max)
								{
									Labels.Add(TP);
									TP = TP.AddMonths(1);
								}
							}
							else
							{
								Nr1 = Nr2;
								Diff1 = Diff2;

								Nr2 = (int)Math.Floor((Max - TP).TotalDays / 700);
								Diff2 = Math.Abs(ApproxNrLabels - Nr2);

								if (Diff1 <= Diff2)
								{
									LabelType = LabelType.DateTimeQuarter;

									if (TP.Day != 1)
										TP = TP.AddDays(-TP.Day + 1).AddMonths(1);

									i = (TP.Month - 1) % 3;
									if (i != 0)
										TP = TP.AddMonths(3 - i);

									while (TP <= Max)
									{
										Labels.Add(TP);
										TP = TP.AddMonths(3);
									}
								}
								else
								{
									LabelType = LabelType.DateTimeYear;

									i = (int)Math.Floor(GetStepSize((Min - referenceTimestamp).TotalDays / 365.25,
										(Max - referenceTimestamp).TotalDays / 365.25, ApproxNrLabels));
									if (i == 0)
										i++;

									if (TP.Day > 1)
										TP = TP.AddDays(-TP.Day + 1).AddMonths(1);

									if (TP.Month > 1)
										TP = TP.AddMonths(13 - TP.Month);

									c = TP.Year % i;
									if (c > 0)
										TP = TP.AddYears(i - c);

									while (TP <= Max)
									{
										Labels.Add(TP);
										TP = TP.AddYears(i);
									}
								}
							}
						}
					}
				}
			}
			else
			{
				long Ticks = Min.Ticks;
				long MaxTicks = Max.Ticks;
				long Step = StepSize.Ticks;
				long Residue = Ticks % Step;
				if (Residue > 0)
					Ticks += Step - Residue;

				while (Ticks <= MaxTicks)
				{
					Labels.Add(new DateTime(Ticks));
					Ticks += Step;
				}

				if (StepSize.TotalMinutes >= 1)
					LabelType = LabelType.DateTimeShortTime;
				else
					LabelType = LabelType.DateTimeLongTime;
			}

			return Labels.ToArray();
		}

		private static readonly TimeSpan[] timeStepSizes = new TimeSpan[]
		{
			TimeSpan.FromMilliseconds(1),
			TimeSpan.FromMilliseconds(2),
			TimeSpan.FromMilliseconds(5),
			TimeSpan.FromMilliseconds(10),
			TimeSpan.FromMilliseconds(20),
			TimeSpan.FromMilliseconds(25),
			TimeSpan.FromMilliseconds(50),
			TimeSpan.FromMilliseconds(100),
			TimeSpan.FromMilliseconds(200),
			TimeSpan.FromMilliseconds(250),
			TimeSpan.FromMilliseconds(500),
			TimeSpan.FromSeconds(1),
			TimeSpan.FromSeconds(2),
			new TimeSpan(0,0,0,2,500),
			TimeSpan.FromSeconds(5),
			TimeSpan.FromSeconds(10),
			TimeSpan.FromSeconds(15),
			TimeSpan.FromSeconds(20),
			TimeSpan.FromSeconds(30),
			TimeSpan.FromMinutes(1),
			TimeSpan.FromMinutes(2),
			new TimeSpan(0,0,2,30),
			TimeSpan.FromMinutes(5),
			TimeSpan.FromMinutes(10),
			TimeSpan.FromMinutes(15),
			TimeSpan.FromMinutes(20),
			TimeSpan.FromMinutes(30),
			TimeSpan.FromHours(1),
			TimeSpan.FromHours(2),
			TimeSpan.FromHours(4),
			TimeSpan.FromHours(6),
			TimeSpan.FromHours(8),
			TimeSpan.FromHours(12),
			TimeSpan.FromDays(1)
		};

		/// <summary>
		/// Gets label values for a series vector.
		/// </summary>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="ApproxNrLabels">Number of labels.</param>
		/// <returns>Vector of labels.</returns>
		public static PhysicalQuantity[] GetLabels(PhysicalQuantity Min, PhysicalQuantity Max, int ApproxNrLabels)
		{
			double MinValue = Min.Magnitude;
			double MaxValue;
			Unit Unit = Min.Unit;

			if (!Min.Unit.Equals(Max.Unit))
			{
				if (!Unit.TryConvert(Max.Magnitude, Max.Unit, Unit, out MaxValue))
					throw new ScriptException("Incompatible units.");
			}
			else
				MaxValue = Max.Magnitude;

			double[] Labels = GetLabels(MinValue, MaxValue, ApproxNrLabels);
			int i, c = Labels.Length;
			PhysicalQuantity[] Result = new PhysicalQuantity[c];

			for (i = 0; i < c; i++)
				Result[i] = new PhysicalQuantity(Labels[i], Unit);

			return Result;
		}

		/// <summary>
		/// Converts a label to a string.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <param name="LabelType">Type of label.</param>
		/// <returns>String-representation.</returns>
		public static string LabelString(IElement Label, LabelType LabelType)
		{
			switch (LabelType)
			{
				case LabelType.DateTimeYear:
					return ((DateTimeValue)Label).Value.Year.ToString();

				case LabelType.DateTimeQuarter:
					DateTime TP = ((DateTimeValue)Label).Value;
					return TP.Year.ToString() + " Q" + ((TP.Month + 2) / 3).ToString();

				case LabelType.DateTimeMonth:
					return ((DateTimeValue)Label).Value.ToString("MMM");

				case LabelType.DateTimeWeek:
					return GetIso8601WeekOfYear(((DateTimeValue)Label).Value).ToString();

				case LabelType.DateTimeDate:
					return ((DateTimeValue)Label).Value.ToString("d");

				case LabelType.DateTimeShortTime:
					return ((DateTimeValue)Label).Value.ToString("t");

				case LabelType.DateTimeLongTime:
					return ((DateTimeValue)Label).Value.ToString("T");

				case LabelType.Double:
				case LabelType.PhysicalQuantity:
				case LabelType.String:
				default:
					return Label.AssociatedObjectValue.ToString();
			}
		}

		/// <summary>
		/// Gets the week number of a date, according to ISO-8601.
		/// 
		/// https://blogs.msdn.microsoft.com/shawnste/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net/
		/// </summary>
		/// <param name="Time">Time</param>
		/// <returns>Week number, according to ISO-8601.</returns>
		public static int GetIso8601WeekOfYear(DateTime Time)
		{
			// Seriously cheat.  If its Monday, Tuesday or Wednesday, then it’ll 
			// be the same week# as whatever Thursday, Friday or Saturday are,
			// and we always get those right
			DayOfWeek day = cal.GetDayOfWeek(Time);
			if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
			{
				Time = Time.AddDays(3);
			}

			// Return the week of our adjusted day
			return cal.GetWeekOfYear(Time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
		}

		// Need a calendar.  Culture’s irrelevent since we specify start day of week
		private static readonly Calendar cal = CultureInfo.InvariantCulture.Calendar;

	}
}
