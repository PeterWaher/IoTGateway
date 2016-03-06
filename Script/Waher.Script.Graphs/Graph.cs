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

namespace Waher.Script.Graphs
{
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
		public abstract Bitmap CreateBitmap(GraphSettings Settings);

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
				return Scale(((ObjectVector)Vector).Values, Offset, Size);
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
			double Scale = Size / (Max - Min);

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
		/// Gets label values for a series vector.
		/// </summary>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="ApproxNrLabels">Number of labels.</param>
		/// <returns>Vector of labels.</returns>
		public IVector GetLabels(IElement Min, IElement Max, IEnumerable<IVector> Series, int ApproxNrLabels)
		{
			if (Min is DoubleNumber && Max is DoubleNumber)
				return new DoubleVector(GetLabels(((DoubleNumber)Min).Value, ((DoubleNumber)Max).Value, ApproxNrLabels));
			else if (Min is DateTimeValue && Max is DateTimeValue)
				return new DateTimeVector(GetLabels(((DateTimeValue)Min).Value, ((DateTimeValue)Max).Value, ApproxNrLabels));
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
		public double[] GetLabels(double Min, double Max, int ApproxNrLabels)
		{
			double Delta = Max - Min;
			double StepSize0 = Math.Pow(10, Math.Round(Math.Log10((Max - Min) / ApproxNrLabels)));
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

			List<double> Steps = new List<double>();
			int i = (int)Math.Ceiling(Min / BestStepSize);
			double d = i * BestStepSize;

			while (d <= Max)
			{
				Steps.Add(d);
				d = ++i * BestStepSize;
			}

			return Steps.ToArray();
		}

		/// <summary>
		/// Gets label values for a series vector.
		/// </summary>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="ApproxNrLabels">Number of labels.</param>
		/// <returns>Vector of labels.</returns>
		public DateTimeVector[] GetLabels(DateTime Min, DateTime Max, int ApproxNrLabels)
		{
			throw new NotImplementedException();	// TODO
		}

	}
}
