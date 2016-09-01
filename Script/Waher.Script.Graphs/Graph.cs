using System;
using System.Collections.Generic;
using System.Drawing;
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
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <returns>Bitmap</returns>
		public Bitmap CreateBitmap(GraphSettings Settings)
		{
			object[] States;
			return this.CreateBitmap(Settings, out States);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <param name="States">State objects that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public abstract Bitmap CreateBitmap(GraphSettings Settings, out object[] States);

		/// <summary>
		/// Gets script corresponding to a point in a generated bitmap representation of the graph.
		/// </summary>
		/// <param name="X">X-Coordinate.</param>
		/// <param name="Y">Y-Coordinate.</param>
		/// <param name="States">State objects for the generated bitmap.</param>
		/// <returns>Script.</returns>
		public abstract string GetBitmapClickScript(double X, double Y, object[] States);

		/// <summary>
		/// The recommended bitmap size of the graph, if such is available.
		/// </summary>
		public virtual Size? RecommendedBitmapSize
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
		public static PointF[] Scale(IVector VectorX, IVector VectorY, IElement MinX, IElement MaxX,
			IElement MinY, IElement MaxY, double OffsetX, double OffsetY, double Width, double Height)
		{
			if (VectorX.Dimension != VectorY.Dimension)
				throw new ScriptException("Dimension mismatch.");

			double[] X = Scale(VectorX, MinX, MaxX, OffsetX, Width);
			double[] Y = Scale(VectorY, MinY, MaxY, OffsetY, Height);
			int i, c = X.Length;
			PointF[] Points = new PointF[c];

			for (i = 0; i < c; i++)
				Points[i] = new PointF((float)X[i], (float)Y[i]);

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
			if (Vector is DoubleVector)
			{
				DoubleNumber dMin = Min as DoubleNumber;
				DoubleNumber dMax = Max as DoubleNumber;

				if (dMin == null || dMax == null)
					throw new ScriptException("Incompatible values.");

				return Scale(((DoubleVector)Vector).Values, dMin.Value, dMax.Value, Offset, Size);
			}
			else if (Vector is Interval)
			{
				DoubleNumber dMin = Min as DoubleNumber;
				DoubleNumber dMax = Max as DoubleNumber;

				if (dMin == null || dMax == null)
					throw new ScriptException("Incompatible values.");

				return Scale(((Interval)Vector).GetArray(), dMin.Value, dMax.Value, Offset, Size);
			}
			else if (Vector is DateTimeVector)
			{
				DateTimeValue dMin = Min as DateTimeValue;
				DateTimeValue dMax = Max as DateTimeValue;

				if (dMin == null || dMax == null)
					throw new ScriptException("Incompatible values.");

				return Scale(((DateTimeVector)Vector).Values, dMin.Value, dMax.Value, Offset, Size);
			}
			else if (Vector is ObjectVector)
			{
				PhysicalQuantity MinQ = Min as PhysicalQuantity;
				PhysicalQuantity MaxQ = Max as PhysicalQuantity;

				if (MinQ != null && MaxQ != null)
				{
					if (MinQ.Unit != MaxQ.Unit)
					{
						double d;

						if (!Unit.TryConvert(MaxQ.Magnitude, MaxQ.Unit, MinQ.Unit, out d))
							throw new ScriptException("Incompatible units.");

						MaxQ = new PhysicalQuantity(d, MinQ.Unit);
					}

					int i = 0;
					int c = Vector.Dimension;
					PhysicalQuantity[] Vector2 = new PhysicalQuantity[c];
					PhysicalQuantity Q;

					foreach (IElement E in Vector.ChildElements)
					{
						Q = E as PhysicalQuantity;
						if (Q == null)
							throw new ScriptException("Incompatible values.");

						Vector2[i++] = Q;
					}

					return Scale(Vector2, MinQ.Magnitude, MaxQ.Magnitude, MinQ.Unit, Offset, Size);
				}
				else
				{
					DoubleNumber MinD = Min as DoubleNumber;
					DoubleNumber MaxD = Max as DoubleNumber;

					if (MinD != null && MaxD != null)
					{
						int i = 0;
						int c = Vector.Dimension;
						double[] Vector2 = new double[c];
						DoubleNumber D;

						foreach (IElement E in Vector.ChildElements)
						{
							D = E as DoubleNumber;
							if (D == null)
								throw new ScriptException("Incompatible values.");

							Vector2[i++] = D.Value;
						}

						return Scale(Vector2, MinD.Value, MaxD.Value, Offset, Size);
					}
					else
					{
						DateTimeValue MinDT = Min as DateTimeValue;
						DateTimeValue MaxDT = Max as DateTimeValue;

						if (MinDT != null && MaxDT != null)
						{
							int i = 0;
							int c = Vector.Dimension;
							DateTime[] Vector2 = new DateTime[c];
							DateTimeValue DT;

							foreach (IElement E in Vector.ChildElements)
							{
								DT = E as DateTimeValue;
								if (DT == null)
									throw new ScriptException("Incompatible values.");

								Vector2[i++] = DT.Value;
							}

							return Scale(Vector2, MinDT.Value, MaxDT.Value, Offset, Size);
						}
						else
							return Scale(((ObjectVector)Vector).Values, Offset, Size);
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
				v[i] = Vector[i].ToOADate();

			return Scale(v, Min.ToOADate(), Max.ToOADate(), Offset, Size);
		}

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

			return Scale(v, 0, c + 1, Offset, Size);
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
				if (Q.Unit.Equals(Unit))
					v[i] = Q.Magnitude;
				else if (!Unit.TryConvert(Q.Magnitude, Q.Unit, Unit, out v[i]))
					throw new ScriptException("Incompatible units.");
			}

			return Scale(v, Min, Max, Offset, Size);
		}

		/// <summary>
		/// Converts an object to a pen value.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Size">Optional size.</param>
		/// <returns>Pen object.</returns>
		public static Pen ToPen(object Object, object Size)
		{
			double Width = Size == null ? 1 : Expression.ToDouble(Size);

			if (Object is Pen)
				return (Pen)Object;
			else if (Object is Color)
				return new Pen((Color)Object, (float)Width);
			else if (Object is Brush)
				return new Pen((Brush)Object, (float)Width);
			else
				return new Pen(ToColor(Object), (float)Width);
		}

		/// <summary>
		/// Converts an object to a color.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Color value.</returns>
		public static Color ToColor(object Object)
		{
			if (Object is Color)
				return (Color)Object;
			else if (Object is string)
			{
				string s = (string)Object;
				KnownColor cl;

				if (s.StartsWith("#"))
				{
					byte R, G, B, A;

					switch (s.Length)
					{
						case 7:
							if (byte.TryParse(s.Substring(1, 2), NumberStyles.HexNumber, null, out R) &&
								byte.TryParse(s.Substring(3, 2), NumberStyles.HexNumber, null, out G) &&
								byte.TryParse(s.Substring(5, 2), NumberStyles.HexNumber, null, out B))
							{
								return Color.FromArgb(R, G, B);
							}
							break;

						case 9:
							if (byte.TryParse(s.Substring(1, 2), NumberStyles.HexNumber, null, out R) &&
								byte.TryParse(s.Substring(3, 2), NumberStyles.HexNumber, null, out G) &&
								byte.TryParse(s.Substring(5, 2), NumberStyles.HexNumber, null, out B) &&
								byte.TryParse(s.Substring(7, 2), NumberStyles.HexNumber, null, out A))
							{
								return Color.FromArgb(A, R, G, B);
							}
							break;
					}
				}
				else if (Enum.TryParse<KnownColor>(s, out cl))
					return Color.FromKnownColor(cl);
			}

			throw new ScriptException("Expected a color.");
		}

		/// <summary>
		/// Creates a Color from its HSL representation.
		/// 
		/// RGB conversion done according to:
		/// https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSL
		/// </summary>
		/// <param name="H">Hue H ∈ [0°, 360°).</param>
		/// <param name="S">Saturation SHSL ∈ [0, 1].</param>
		/// <param name="L">Lightness L ∈ [0, 1].</param>
		/// <returns>Color</returns>
		public static Color ToColorHSL(double H, double S, double L)
		{
			return ToColorHSL(H, S, L, 255);
		}

		/// <summary>
		/// Creates a Color from its HSL representation.
		/// 
		/// RGB conversion done according to:
		/// https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSL
		/// </summary>
		/// <param name="H">Hue H ∈ [0°, 360°).</param>
		/// <param name="S">Saturation SHSL ∈ [0, 1].</param>
		/// <param name="L">Lightness L ∈ [0, 1].</param>
		/// <param name="A">Alpha A ∈ [0, 255].</param>
		/// <returns>Color</returns>
		public static Color ToColorHSL(double H, double S, double L, int A)
		{
			H = Math.IEEERemainder(H, 360);
			if (H < 0)
				H += 360;

			if (S < 0 || S > 1)
				throw new ArgumentException("Valid saturations are 0 <= S <= 1.", "S");

			if (L < 0 || L > 1)
				throw new ArgumentException("Valid lightnesses are 0 <= L <= 1.", "L");

			if (A < 0 || A > 255)
				throw new ArgumentException("Valid alpha values are 0 <= A <= 255.", "A");

			double C = (1 - Math.Abs(2 * L - 1)) * S;						// C ∈ [0, 1]
			double H2 = H / 60;                                             // H2 ∈ [0, 6)
			double H3 = Math.IEEERemainder(H2, 2);
			if (H3 < 0)
				H3 += 2;
			double X = C * (1 - Math.Abs(H3 - 1));   // X ∈ [0, 1]
			double R, G, B;

			switch ((int)H2)
			{
				case 0:
					R = C;
					G = X;
					B = 0;
					break;

				case 1:
					R = X;
					G = C;
					B = 0;
					break;

				case 2:
					R = 0;
					G = C;
					B = X;
					break;

				case 3:
					R = 0;
					G = X;
					B = C;
					break;

				case 4:
					R = X;
					G = 0;
					B = C;
					break;

				case 5:
				default:
					R = C;
					G = 0;
					B = X;
					break;
			}

			double m = L - 0.5 * C;
			R = (R + m) * 255;
			G = (G + m) * 255;
			B = (B + m) * 255;

			return Color.FromArgb(A, (int)(R + 0.5), (int)(G + 0.5), (int)(B + 0.5));
		}

		/// <summary>
		/// Creates a Color from its HSV representation.
		/// 
		/// RGB conversion done according to:
		/// https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSV
		/// </summary>
		/// <param name="H">Hue H ∈ [0°, 360°).</param>
		/// <param name="S">Saturation SHSL ∈ [0, 1].</param>
		/// <param name="V">Value V ∈ [0, 1].</param>
		/// <returns>Color</returns>
		public static Color ToColorHSV(double H, double S, double V)
		{
			return ToColorHSV(H, S, V, 255);
		}

		/// <summary>
		/// Creates a Color from its HSV representation.
		/// 
		/// RGB conversion done according to:
		/// https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSV
		/// </summary>
		/// <param name="H">Hue H ∈ [0°, 360°).</param>
		/// <param name="S">Saturation SHSL ∈ [0, 1].</param>
		/// <param name="V">Value V ∈ [0, 1].</param>
		/// <param name="A">Alpha A ∈ [0, 255].</param>
		/// <returns>Color</returns>
		public static Color ToColorHSV(double H, double S, double V, int A)
		{
			H = Math.IEEERemainder(H, 360);
			if (H < 0)
				H += 360;

			if (S < 0 || S > 1)
				throw new ArgumentException("Valid saturations are 0 <= S <= 1.", "S");

			if (V < 0 || V > 1)
				throw new ArgumentException("Valid values are 0 <= V <= 1.", "V");

			if (A < 0 || A > 255)
				throw new ArgumentException("Valid alpha values are 0 <= A <= 255.", "A");

			double C = V * S;												// C ∈ [0, 1]
			double H2 = H / 60;                                             // H2 ∈ [0, 6)
			double H3 = Math.IEEERemainder(H2, 2);
			if (H3 < 0)
				H3 += 2;
			double X = C * (1 - Math.Abs(H3 - 1));   // X ∈ [0, 1]
			double R, G, B;

			switch ((int)H2)
			{
				case 0:
					R = C;
					G = X;
					B = 0;
					break;

				case 1:
					R = X;
					G = C;
					B = 0;
					break;

				case 2:
					R = 0;
					G = C;
					B = X;
					break;

				case 3:
					R = 0;
					G = X;
					B = C;
					break;

				case 4:
					R = X;
					G = 0;
					B = C;
					break;

				case 5:
				default:
					R = C;
					G = 0;
					B = X;
					break;
			}

			double m = V - C;	// = V - V*S = V*(1-S)
			R = (R + m) * 255;
			if (R < 0)
				R = 0;
			else if (R > 255)
				R = 255;

			G = (G + m) * 255;
			if (G < 0)
				G = 0;
			else if (G > 255)
				G = 255;

			B = (B + m) * 255;
			if (B < 0)
				B = 0;
			else if (B > 255)
				B = 255;

			return Color.FromArgb(A, (int)(R + 0.5), (int)(G + 0.5), (int)(B + 0.5));
		}

		/// <summary>
		/// Gets label values for a series vector.
		/// </summary>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="ApproxNrLabels">Number of labels.</param>
		/// <param name="LabelType">Type of labels produced.</param>
		/// <returns>Vector of labels.</returns>
		public static IVector GetLabels(IElement Min, IElement Max, IEnumerable<IVector> Series, int ApproxNrLabels, out LabelType LabelType)
		{
			if (Min is DoubleNumber && Max is DoubleNumber)
			{
				LabelType = LabelType.Double;
				return new DoubleVector(GetLabels(((DoubleNumber)Min).Value, ((DoubleNumber)Max).Value, ApproxNrLabels));
			}
			else if (Min is DateTimeValue && Max is DateTimeValue)
				return new DateTimeVector(GetLabels(((DateTimeValue)Min).Value, ((DateTimeValue)Max).Value, ApproxNrLabels, out LabelType));
			else if (Min is PhysicalQuantity && Max is PhysicalQuantity)
			{
				LabelType = LabelType.PhysicalQuantity;
				return new ObjectVector(GetLabels((PhysicalQuantity)Min, (PhysicalQuantity)Max, ApproxNrLabels));
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
						{
							BestDiff = Diff;
							BestStepSize = StepSize;
						}
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
						{
							BestDiff = Diff;
							BestStepSize = StepSize;
						}
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
				if (StepSize > timeStepSizes[i] && StepSize < timeStepSizes[i + 1])
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

									i = (int)Math.Floor(GetStepSize(Min.ToOADate() / 365.25, Max.ToOADate() / 365.25, ApproxNrLabels));
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
			new TimeSpan(0,0,0,0,1),
			new TimeSpan(0,0,0,0,2),
			new TimeSpan(0,0,0,0,5),
			new TimeSpan(0,0,0,0,10),
			new TimeSpan(0,0,0,0,20),
			new TimeSpan(0,0,0,0,25),
			new TimeSpan(0,0,0,0,50),
			new TimeSpan(0,0,0,0,100),
			new TimeSpan(0,0,0,0,200),
			new TimeSpan(0,0,0,0,250),
			new TimeSpan(0,0,0,0,500),
			new TimeSpan(0,0,0,1),
			new TimeSpan(0,0,0,2),
			new TimeSpan(0,0,0,2,500),
			new TimeSpan(0,0,0,5),
			new TimeSpan(0,0,0,10),
			new TimeSpan(0,0,0,15),
			new TimeSpan(0,0,0,20),
			new TimeSpan(0,0,0,30),
			new TimeSpan(0,0,1,0),
			new TimeSpan(0,0,2,0),
			new TimeSpan(0,0,2,30),
			new TimeSpan(0,0,5,0),
			new TimeSpan(0,0,10,0),
			new TimeSpan(0,0,15,0),
			new TimeSpan(0,0,20,0),
			new TimeSpan(0,0,30,0),
			new TimeSpan(0,1,0,0),
			new TimeSpan(0,2,0,0),
			new TimeSpan(0,4,0,0),
			new TimeSpan(0,6,0,0),
			new TimeSpan(0,8,0,0),
			new TimeSpan(0,12,0,0),
			new TimeSpan(1,0,0,0)
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
					return ((DateTimeValue)Label).Value.ToShortDateString();

				case LabelType.DateTimeShortTime:
					return ((DateTimeValue)Label).Value.ToShortTimeString();

				case LabelType.DateTimeLongTime:
					return ((DateTimeValue)Label).Value.ToLongTimeString();

				case LabelType.Double:
				case LabelType.PhysicalQuantity:
				case LabelType.String:
				default:
					return Label.ToString();
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
