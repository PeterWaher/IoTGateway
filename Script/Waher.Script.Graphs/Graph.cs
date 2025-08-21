﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Runtime;
using Waher.Script.Graphs.Functions.Colors;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Sets;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators;
using Waher.Script.Operators.Assignments;
using Waher.Script.Operators.Membership;
using Waher.Script.Units;
using Boolean = Waher.Script.Functions.Scalar.Boolean;

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
	public abstract class Graph : SemiGroupElement, ISemiGroupElementWise
	{
		/// <summary>
		/// http://waher.se/Schema/Graph.xsd
		/// </summary>
		public const string GraphNamespace = "http://waher.se/Schema/Graph.xsd";

		/// <summary>
		/// Graph
		/// </summary>
		public const string GraphLocalName = "Graph";

		/// <summary>
		/// Variable name for graph background color.
		/// </summary>
		public const string GraphBgColorVariableName = "GraphBgColor";

		/// <summary>
		/// Variable name for graph foreground color.
		/// </summary>
		public const string GraphFgColorVariableName = "GraphFgColor";

		/// <summary>
		/// Variable name for graph width
		/// </summary>
		public const string GraphWidthVariableName = "GraphWidth";

		/// <summary>
		/// Variable name for graph height
		/// </summary>
		public const string GraphHeightVariableName = "GraphHeight";

		/// <summary>
		/// Variable name for graph label font size
		/// </summary>
		public const string GraphLabelFontSizeVariableName = "GraphLabelFontSize";

		/// <summary>
		/// Default color: Red
		/// </summary>
		public static readonly SKColor DefaultColor = SKColors.Red;

		private GraphSettings settings;
		private bool sameScale = false;

		/// <summary>
		/// Base class for graphs.
		/// </summary>
		public Graph()
			: this(new Variables())
		{
		}

		/// <summary>
		/// Base class for graphs.
		/// </summary>
		/// <param name="Variables">Current set of variables, where graph settings might be available.</param>
		public Graph(Variables Variables)
			: this(Variables, null, null)
		{
		}

		/// <summary>
		/// Base class for graphs.
		/// </summary>
		/// <param name="Variables">Current set of variables, where graph settings might be available.</param>
		/// <param name="DefaultWidth">Default width.</param>
		/// <param name="DefaultHeight">Default height.</param>
		public Graph(Variables Variables, int? DefaultWidth, int? DefaultHeight)
			: base()
		{
			this.settings = this.GetSettingsProt(Variables, DefaultWidth, DefaultHeight);
		}

		/// <summary>
		/// Base class for graphs.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		public Graph(GraphSettings Settings)
			: base()
		{
			this.settings = Settings;
		}

		/// <summary>
		/// Base class for graphs.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <param name="DefaultWidth">Default width.</param>
		/// <param name="DefaultHeight">Default height.</param>
		public Graph(GraphSettings Settings, int? DefaultWidth, int? DefaultHeight)
			: base()
		{
			this.settings = this.GetSettingsProt(Settings, DefaultWidth, DefaultHeight);
		}

		/// <summary>
		/// If the same scale should be used for all axes.
		/// </summary>
		public bool SameScale
		{
			get => this.sameScale;
			set => this.sameScale = value;
		}

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue => this;

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override ISemiGroup AssociatedSemiGroup => SetOfGraphs.Instance;

		/// <summary>
		/// Graph settings available during creation.
		/// </summary>
		public GraphSettings Settings => this.settings;

		/// <summary>
		/// Tries to add an element to the current element, from the left, element-wise.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual ISemiGroupElementWise AddLeftElementWise(ISemiGroupElementWise Element)
		{
			return this.AddLeft(Element) as ISemiGroupElementWise;
		}

		/// <summary>
		/// Tries to add an element to the current element, from the right, element-wise.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public virtual ISemiGroupElementWise AddRightElementWise(ISemiGroupElementWise Element)
		{
			return this.AddRight(Element) as ISemiGroupElementWise;
		}

		/// <summary>
		/// Gets default graph settings for drawing the graph.
		/// </summary>
		/// <param name="Variables">Current set of variables, where graph settings might be available.</param>
		/// <returns>Graph settings.</returns>
		[Obsolete("Graph Settings now available via the Settings property on the Graph object directly.")]
		public GraphSettings GetSettings(Variables Variables)
		{
			return this.GetSettingsProt(Variables, null, null);
		}

		/// <summary>
		/// Gets default graph settings for drawing the graph.
		/// </summary>
		/// <param name="Variables">Current set of variables, where graph settings might be available.</param>
		/// <param name="DefaultWidth">Default width.</param>
		/// <param name="DefaultHeight">Default height.</param>
		/// <returns>Graph settings.</returns>
		[Obsolete("Graph Settings now available via the Settings property on the Graph object directly.")]
		public GraphSettings GetSettings(Variables Variables, int? DefaultWidth, int? DefaultHeight)
		{
			return this.GetSettingsProt(Variables, DefaultWidth, DefaultHeight);
		}

		/// <summary>
		/// Extracts Graph Settings from a collection of variables.
		/// </summary>
		/// <param name="Variables">Collection of variables.</param>
		/// <param name="DefaultWidth">Default width, if defined.</param>
		/// <param name="DefaultHeight">Default height, if defined.</param>
		/// <returns>Graph Settings.</returns>
		protected GraphSettings GetSettingsProt(Variables Variables, int? DefaultWidth, int? DefaultHeight)
		{
			GraphSettings Settings = new GraphSettings();
			Tuple<int, int> Size;
			Variable v;

			if (DefaultWidth.HasValue)
			{
				Settings.Width = DefaultWidth.Value;
				Settings.MarginLeft = (int)Math.Round(15.0 * Settings.Width / 640);
				Settings.MarginRight = Settings.MarginLeft;
			}

			if (DefaultHeight.HasValue)
			{
				Settings.Height = DefaultHeight.Value;
				Settings.MarginTop = (int)Math.Round(15.0 * Settings.Height / 480);
				Settings.MarginBottom = Settings.MarginTop;
				Settings.LabelFontSize = 12.0 * Math.Min(Settings.Width, Settings.Height) / 480;
			}

			if (!((Size = this.RecommendedBitmapSize) is null) && Size.Item1 > 0 && Size.Item2 > 0)
			{
				Settings.Width = Size.Item1;
				Settings.Height = Size.Item2;

				Settings.MarginLeft = (int)Math.Round(15.0 * Settings.Width / 640);
				Settings.MarginRight = Settings.MarginLeft;

				Settings.MarginTop = (int)Math.Round(15.0 * Settings.Height / 480);
				Settings.MarginBottom = Settings.MarginTop;
				Settings.LabelFontSize = 12.0 * Math.Min(Settings.Width, Settings.Height) / 480;
			}
			else
			{
				bool SizeChanged = false;

				if (Variables.TryGetVariable(GraphWidthVariableName, out v) && v.ValueObject is double d && d >= 1)
				{
					Settings.Width = (int)Math.Round(d);
					Settings.MarginLeft = (int)Math.Round(15 * d / 640);
					Settings.MarginRight = Settings.MarginLeft;
					SizeChanged = true;
				}
				else if (!Variables.ContainsVariable(GraphWidthVariableName))
					Variables[GraphWidthVariableName] = (double)Settings.Width;

				if (Variables.TryGetVariable(GraphHeightVariableName, out v) && v.ValueObject is double d2 && d2 >= 1)
				{
					Settings.Height = (int)Math.Round(d2);
					Settings.MarginTop = (int)Math.Round(15 * d2 / 480);
					Settings.MarginBottom = Settings.MarginTop;
					SizeChanged = true;
				}
				else if (!Variables.ContainsVariable(GraphHeightVariableName))
					Variables[GraphHeightVariableName] = (double)Settings.Height;

				if (SizeChanged)
					Settings.LabelFontSize = 12 * Math.Min(Settings.Width, Settings.Height) / 480;
			}

			int i = 0;

			if (Variables.TryGetVariable(GraphBgColorVariableName, out v) && TryConvertToColor(v.ValueObject, out SKColor Color))
			{
				Settings.BackgroundColor = Color;
				i++;
			}

			if (Variables.TryGetVariable(GraphFgColorVariableName, out v) && TryConvertToColor(v.ValueObject, out Color))
			{
				Settings.AxisColor = Color;
				i++;
			}

			if (i == 2)
				Settings.GridColor = Functions.Colors.Blend.BlendColors(Settings.BackgroundColor, Settings.AxisColor, 0.4);

			if (Variables.TryGetVariable(GraphLabelFontSizeVariableName, out v) && v.ValueObject is double d3 && d3 > 0)
			{
				Settings.LabelFontSize = d3;
				Settings.MarginTop = (int)(15 * Settings.LabelFontSize / 12);
				Settings.MarginBottom = Settings.MarginTop;
				Settings.MarginLeft = Settings.MarginTop;
				Settings.MarginRight = Settings.MarginTop;
			}

			return Settings;
		}

		/// <summary>
		/// Creates a new set of Graph Settings from an existing set of settings, optionally modified for a new size.
		/// </summary>
		/// <param name="Settings0">Original graph settings.</param>
		/// <param name="DefaultWidth">Default width, if defined.</param>
		/// <param name="DefaultHeight">Default height, if defined.</param>
		/// <returns>Graph Settings.</returns>
		protected GraphSettings GetSettingsProt(GraphSettings Settings0, int? DefaultWidth, int? DefaultHeight)
		{
			GraphSettings Settings = Settings0.Copy();
			Tuple<int, int> Size;

			if (DefaultWidth.HasValue)
				Settings.Width = DefaultWidth.Value;

			if (DefaultHeight.HasValue)
				Settings.Height = DefaultHeight.Value;

			if (!((Size = this.RecommendedBitmapSize) is null) && Size.Item1 > 0 && Size.Item2 > 0)
			{
				Settings.Width = Size.Item1;
				Settings.Height = Size.Item2;
			}

			return Settings;
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <returns>Bitmap</returns>
		public PixelInformation CreatePixels()
		{
			return this.CreatePixels(null, out GraphSettings _, out object[] _);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Variables">Variables from where default settings can be retrieved if not available in graph.</param>
		/// <returns>Bitmap</returns>
		public PixelInformation CreatePixels(Variables Variables)
		{
			return this.CreatePixels(Variables, out GraphSettings _, out object[] _);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Settings used to create the graph.</param>
		/// <returns>Bitmap</returns>
		public PixelInformation CreatePixels(out GraphSettings Settings)
		{
			return this.CreatePixels(null, out Settings, out object[] _);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Variables">Variables from where default settings can be retrieved if not available in graph.</param>
		/// <param name="Settings">Settings used to create the graph.</param>
		/// <returns>Bitmap</returns>
		public PixelInformation CreatePixels(Variables Variables, out GraphSettings Settings)
		{
			return this.CreatePixels(Variables, out Settings, out object[] _);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="States">State objects that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public PixelInformation CreatePixels(out object[] States)
		{
			return this.CreatePixels(null, out GraphSettings _, out States);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Variables">Variables from where default settings can be retrieved if not available in graph.</param>
		/// <param name="States">State objects that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public PixelInformation CreatePixels(Variables Variables, out object[] States)
		{
			return this.CreatePixels(Variables, out GraphSettings _, out States);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Settings used to create the graph.</param>
		/// <param name="States">State objects that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public PixelInformation CreatePixels(out GraphSettings Settings, out object[] States)
		{
			return this.CreatePixels(null, out Settings, out States);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Variables">Variables from where default settings can be retrieved if not available in graph.</param>
		/// <param name="Settings">Settings used to create the graph.</param>
		/// <param name="States">State objects that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public PixelInformation CreatePixels(Variables Variables, out GraphSettings Settings, out object[] States)
		{
			Settings = Variables is null ? this.settings : this.GetSettingsProt(Variables, null, null);
			return this.CreatePixels(this.settings, out States);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <returns>Bitmap</returns>
		public PixelInformation CreatePixels(GraphSettings Settings)
		{
			return this.CreatePixels(Settings, out object[] _);
		}

		/// <summary>
		/// Creates a bitmap of the graph.
		/// </summary>
		/// <param name="Settings">Graph settings.</param>
		/// <param name="States">State objects that contain graph-specific information about its inner states.
		/// These can be used in calls back to the graph object to make actions on the generated graph.</param>
		/// <returns>Bitmap</returns>
		public abstract PixelInformation CreatePixels(GraphSettings Settings, out object[] States);

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
		public virtual Tuple<int, int> RecommendedBitmapSize => null;

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
		/// <param name="XLabelPositions">Optional fixed X-label positions.</param>
		/// <param name="YLabelPositions">Optional fixed Y-label positions.</param>
		/// <returns>Sequence of points.</returns>
		public static SKPoint[] Scale(IVector VectorX, IVector VectorY, IElement MinX, IElement MaxX,
			IElement MinY, IElement MaxY, double OffsetX, double OffsetY, double Width, double Height,
			Dictionary<string, double> XLabelPositions, Dictionary<string, double> YLabelPositions)
		{
			if (VectorX.Dimension != VectorY.Dimension)
				throw new ScriptException("Dimension mismatch.");

			double[] X = Scale(VectorX, MinX, MaxX, OffsetX, Width, XLabelPositions);
			double[] Y = Scale(VectorY, MinY, MaxY, OffsetY, Height, YLabelPositions);
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
		/// <param name="LabelPositions">Optional fixed label positions.</param>
		/// <returns>Vector distributed in the available area.</returns>
		public static double[] Scale(IVector Vector, IElement Min, IElement Max, double Offset,
			double Size, Dictionary<string, double> LabelPositions)
		{
			if (Vector is DoubleVector DV)
			{
				if (!(Min.AssociatedObjectValue is double dMin) ||
					!(Max.AssociatedObjectValue is double dMax))
				{
					throw new ScriptException("Incompatible values.");
				}

				return Scale(DV.Values, dMin, dMax, Offset, Size);
			}
			else if (Vector is Interval I)
			{
				if (!(Min.AssociatedObjectValue is double dMin) ||
					!(Max.AssociatedObjectValue is double dMax))
				{
					throw new ScriptException("Incompatible values.");
				}

				return Scale(I.GetArray(), dMin, dMax, Offset, Size);
			}
			else if (Vector is DateTimeVector DTV)
			{
				if (!(Min.AssociatedObjectValue is DateTime dMin) ||
					!(Max.AssociatedObjectValue is DateTime dMax))
				{
					throw new ScriptException("Incompatible values.");
				}

				return Scale(DTV.Values, dMin, dMax, Offset, Size);
			}
			else if (Vector is ObjectVector OV)
			{
				if (Min.AssociatedObjectValue is IPhysicalQuantity PMinQ &&
					Max.AssociatedObjectValue is IPhysicalQuantity PMaxQ)
				{
					PhysicalQuantity MinQ = PMinQ.ToPhysicalQuantity();
					PhysicalQuantity MaxQ = PMaxQ.ToPhysicalQuantity();

					if (MinQ.Unit != MaxQ.Unit)
					{
						if (!Unit.TryConvert(MaxQ.Magnitude, MaxQ.Unit, MinQ.Unit, out double d))
							throw new ScriptException("Incompatible units.");

						MaxQ = new PhysicalQuantity(d, MinQ.Unit);
					}

					int i = 0;
					int c = Vector.Dimension;
					PhysicalQuantity[] Vector2 = new PhysicalQuantity[c];
					IPhysicalQuantity Q;

					foreach (IElement E in Vector.ChildElements)
					{
						Q = E.AssociatedObjectValue as IPhysicalQuantity ?? throw new ScriptException("Incompatible values.");
						Vector2[i++] = Q.ToPhysicalQuantity();
					}

					return Scale(Vector2, MinQ.Magnitude, MaxQ.Magnitude, MinQ.Unit, Offset, Size);
				}
				else if (Min.AssociatedObjectValue is double MinD &&
					Max.AssociatedObjectValue is double MaxD)
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

					return Scale(Vector2, MinD, MaxD, Offset, Size);
				}
				else if (Min.AssociatedObjectValue is DateTime MinDT &&
					Max.AssociatedObjectValue is DateTime MaxDT)
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

					return Scale(Vector2, MinDT, MaxDT, Offset, Size);
				}
				else
					return Scale(OV.Values, Offset, Size, LabelPositions);
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

		/// <summary>
		/// Reference time stamp when converting <see cref="DateTime"/> to <see cref="System.Double"/>.
		/// </summary>
		protected static readonly DateTime referenceTimestamp = new DateTime(2000, 1, 1, 0, 0, 0);

		/// <summary>
		/// Scales a vector to fit a given area.
		/// </summary>
		/// <param name="Vector">Vector.</param>
		/// <param name="Offset">Offset to area.</param>
		/// <param name="Size">Size of area.</param>
		/// <param name="LabelPositions">Optional fixed label positions.</param>
		/// <returns>Vector distributed in the available area.</returns>
		public static double[] Scale(IElement[] Vector, double Offset, double Size,
			Dictionary<string, double> LabelPositions)
		{
			int i, c = Vector.Length;
			double[] v = new double[c];

			for (i = 0; i < c; i++)
				v[i] = i + 0.5;

			double[] Result = Scale(v, 0, c, Offset, Size);

			if (!(LabelPositions is null))
			{
				for (i = 0; i < c; i++)
				{
					string s = Vector[i].AssociatedObjectValue?.ToString() ?? string.Empty;

					if (LabelPositions.TryGetValue(s, out double d))
						Result[i] = d;
				}
			}

			return Result;
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

			if (Min.AssociatedObjectValue is double DMin &&
				Max.AssociatedObjectValue is double DMax)
				return new DoubleNumber((Value - Offset) * (DMax - DMin) / Size + DMin);
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
		/// Tries to convert an object to a color.
		/// </summary>
		/// <param name="Object">Object to convert.</param>
		/// <param name="Color">Resulting color, if possible to convert.</param>
		/// <returns>Of <paramref name="Object"/> could be converted to a color.</returns>
		public static bool TryConvertToColor(object Object, out SKColor Color)
		{
			if (Object is SKColor Color2)
			{
				Color = Color2;
				return true;
			}
			else if (Object is string s && Functions.Colors.Color.TryParse(s, out Color))
				return true;
			else
			{
				Color = DefaultColor;
				return (Object is null);
			}
		}


		/// <summary>
		/// Converts an object to a color.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Color value.</returns>
		public static SKColor ToColor(object Object)
		{
			if (!TryConvertToColor(Object, out SKColor Color))
				throw new ScriptException("Expected a color.");

			return Color;
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
		/// Converts a color to an RGB(A) style string.
		/// </summary>
		/// <param name="Color">Color</param>
		/// <returns>Color style value.</returns>
		public static string ToRGBAStyle(SKColor Color)
		{
			StringBuilder Result = new StringBuilder();

			Result.Append('#');
			Result.Append(Color.Red.ToString("x2"));
			Result.Append(Color.Green.ToString("x2"));
			Result.Append(Color.Blue.ToString("x2"));

			if (Color.Alpha != 255)
				Result.Append(Color.Alpha.ToString("x2"));

			return Result.ToString();
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
			if (Min.AssociatedObjectValue is double DMin &&
				Max.AssociatedObjectValue is double DMax)
			{
				LabelType = LabelType.Double;
				return new DoubleVector(GetLabels(DMin, DMax, ApproxNrLabels));
			}
			else if (Min.AssociatedObjectValue is DateTime DTMin &&
				Max.AssociatedObjectValue is DateTime DTMax)
			{
				return new DateTimeVector(GetLabels(DTMin, DTMax, ApproxNrLabels, out LabelType));
			}
			else if (Min.AssociatedObjectValue is IPhysicalQuantity PQMin &&
				Max.AssociatedObjectValue is IPhysicalQuantity PQMax)
			{
				LabelType = LabelType.PhysicalQuantity;
				return new ObjectVector(GetLabels(PQMin.ToPhysicalQuantity(), PQMax.ToPhysicalQuantity(), ApproxNrLabels));
			}
			else if (Min.AssociatedObjectValue is string &&
				Max.AssociatedObjectValue is string)
			{
				Dictionary<string, bool> Indices = new Dictionary<string, bool>();
				ChunkedList<IElement> Labels = new ChunkedList<IElement>();
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
			if (double.IsInfinity(Min) || double.IsNaN(Min) ||
				double.IsInfinity(Max) || double.IsNaN(Max))
			{
				return Array.Empty<double>();
			}

			// Calculate steps, without introducing growing round-off errors.

			GetStepFraction(Min, Max, ApproxNrLabels, out int Numerator, out int Denominator, out int Exponent);
			double Num = Numerator;
			double Den = Denominator;

			if (Exponent > 0)
				Num *= Math.Pow(10, Exponent);
			else if (Exponent < 0)
				Den *= Math.Pow(10, -Exponent);

			double StepSize = Num / Den;
			ChunkedList<double> Steps = new ChunkedList<double>();
			double i = Math.Ceiling(Min / StepSize);
			int j = 0;
			double d = (i * Num) / Den;

			while (d <= Max && j++ < 1000)
			{
				Steps.Add(d);
				i++;
				d = (i * Num) / Den;
			}

			if (j >= 1000)
			{
				Log.Alert("Graph label algorithm failure.",
					new KeyValuePair<string, object>("Min", Min),
					new KeyValuePair<string, object>("Max", Max),
					new KeyValuePair<string, object>("ApproxNrLabels", ApproxNrLabels),
					new KeyValuePair<string, object>("Numerator", Numerator),
					new KeyValuePair<string, object>("Denominator", Denominator),
					new KeyValuePair<string, object>("StepSize", StepSize));
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
			GetStepFraction(Min, Max, ApproxNrLabels, out int Numerator, out int Denominator, out int Exponent);
			return (Math.Pow(10, Exponent) * Numerator) / Denominator;
		}

		/// <summary>
		/// Gets a human readable step size for an interval, given its limits and desired number of steps.
		/// The step size is given as a fraction and an exponent of power 10.
		/// </summary>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="ApproxNrLabels">Number of labels.</param>
		/// <param name="Numerator">Numerator of fraction.</param>
		/// <param name="Denominator">Denominator of fraction.</param>
		/// <param name="Exponent">Power of 10 exponent.</param>
		public static void GetStepFraction(double Min, double Max, int ApproxNrLabels, out int Numerator, out int Denominator, out int Exponent)
		{
			Numerator = 1;
			Denominator = 1;

			double Delta = Max - Min;
			if (Delta == 0)
			{
				Exponent = 0;
				return;
			}

			Exponent = (int)Math.Round(Math.Log10(Delta / ApproxNrLabels));

			double StepSize0 = Math.Pow(10, Exponent);
			double StepSize = StepSize0;
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
					Numerator = 2;

					StepSize = StepSize0 * 2.5;
					NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
					Diff = Math.Abs(NrSteps - ApproxNrLabels);
					if (Diff < BestDiff)
					{
						BestDiff = Diff;
						Numerator = 5;
						Denominator = 2;

						StepSize = StepSize0 * 5;
						NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
						Diff = Math.Abs(NrSteps - ApproxNrLabels);
						if (Diff < BestDiff)
							Denominator = 1;
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
					Denominator = 2;

					StepSize = StepSize0 / 4;
					NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
					Diff = Math.Abs(NrSteps - ApproxNrLabels);
					if (Diff < BestDiff)
					{
						BestDiff = Diff;
						Denominator = 4;

						StepSize = StepSize0 / 5;
						NrSteps = (int)Math.Floor(Max / StepSize) - (int)Math.Ceiling(Min / StepSize) + 1;
						Diff = Math.Abs(NrSteps - ApproxNrLabels);
						if (Diff < BestDiff)
							Denominator = 5;
					}
				}
			}
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
			ChunkedList<DateTime> Labels = new ChunkedList<DateTime>();
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
			int Diff2 = Math.Abs(ApproxNrLabels - Nr2);

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
					//Nr1 = Nr2;
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
						//Nr1 = Nr2;
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
							//Nr1 = Nr2;
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
								//Nr1 = Nr2;
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
		/// Converts a vector of labels to a string array.
		/// </summary>
		/// <param name="Labels">Labels</param>
		/// <param name="LabelType">Type of label.</param>
		/// <returns>String-representation of elements.</returns>
		public static string[] LabelStrings(IVector Labels, LabelType LabelType)
		{
			int i = 0;
			int c = Labels.Dimension;
			string[] Result = new string[c];

			foreach (IElement Label in Labels.ChildElements)
				Result[i++] = LabelString(Label, LabelType);

			return Result;
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

		/// <summary>
		/// Exports the graph to XML.
		/// </summary>
		/// <returns>XML string.</returns>
		public string ToXml()
		{
			StringBuilder Output = new StringBuilder();
			this.ToXml(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Exports the graph to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public void ToXml(StringBuilder Output)
		{
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "\t",
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = true
			};

			using (XmlWriter Writer = XmlWriter.Create(Output, Settings))
			{
				this.ToXml(Writer);
				Writer.Flush();
			}
		}

		/// <summary>
		/// Exports the graph to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public void ToXml(XmlWriter Output)
		{
			Output.WriteStartElement("Graph", GraphNamespace);
			Output.WriteAttributeString("sameScale", this.sameScale ? "true" : "false");
			Output.WriteAttributeString("type", this.GetType().FullName);

			this.ExportGraph(Output);
			this.settings?.ExportSettings(Output);

			Output.WriteEndElement();
		}

		/// <summary>
		/// Exports graph specifics to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public abstract void ExportGraph(XmlWriter Output);

		/// <summary>
		/// Imports graph specifics from XML.
		/// </summary>
		/// <param name="Xml">XML input.</param>
		[Obsolete("Use the ImportGraphAsync method for more efficient processing of script containing asynchronous processing elements in parallel environments.")]
		public void ImportGraph(XmlElement Xml)
		{
			this.ImportGraphAsync(Xml).Wait();
		}

		/// <summary>
		/// Imports graph specifics from XML.
		/// </summary>
		/// <param name="Xml">XML input.</param>
		public abstract Task ImportGraphAsync(XmlElement Xml);

		/// <summary>
		/// Parses an element expression string.
		/// </summary>
		/// <param name="s">Expression</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Parsed element.</returns>
		[Obsolete("Use the ParseAsync method for more efficient processing of script containing asynchronous processing elements in parallel environments.")]
		public static IElement Parse(string s, Variables Variables)
		{
			return ParseAsync(s, Variables).Result;
		}

		/// <summary>
		/// Parses an element expression string.
		/// </summary>
		/// <param name="s">Expression</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Parsed element.</returns>
		public static async Task<IElement> ParseAsync(string s, Variables Variables)
		{
			Expression Exp = new Expression(s);
			string ScriptAssemblyName = typeof(Expression).Assembly.FullName;
			string GraphAssemblyName = typeof(Graph).Assembly.FullName;
			ScriptNode Prohibited = null;
			IElement Result;

			bool Safe = Exp.ForAll((ScriptNode Node, out ScriptNode NewNode, object State) =>
			{
				NewNode = null;

				if (Node is ConstantElement || Node is Color)
					return true;

				string AssemblyName = Node.GetType().Assembly.FullName;

				if (AssemblyName != ScriptAssemblyName && AssemblyName != GraphAssemblyName)
				{
					Prohibited = Node;
					return false;
				}

				if (Node is NamedMemberAssignment ||
					Node is LambdaDefinition ||
					Node is NamedMethodCall ||
					Node is DynamicFunctionCall ||
					Node is DynamicMember ||
					Node is DynamicIndex ||
					Node is Create ||
					Node is Destroy ||
					Node is Error ||
					Node is ImplicitPrint)
				{
					Prohibited = Node;
					return false;
				}
				else if (Node is NamedMember Property)
				{
					ScriptNode Loop = Property.Operand;

					while (Loop is NamedMember Property2)
						Loop = Property2.Operand;

					if (!(Loop is VariableReference))   // Enum values are permitted.
					{
						Prohibited = Node;
						return false;
					}
				}
				else if (Node is Function)
				{
					if (Node.GetType().Namespace != typeof(Script.Functions.DateAndTime.DateTimeUtc).Namespace)
					{
						Prohibited = Node;
						return false;
					}
				}

				return true;

			}, null, SearchMethod.TreeOrder);

			if (!Safe)
				throw new UnauthorizedAccessException("Expression not permitted: " + Prohibited?.SubExpression);

			try
			{
				Result = await Exp.Root.EvaluateAsync(Variables);
			}
			catch (ScriptReturnValueException ex)
			{
				Result = ex.ReturnValue;
				//ScriptReturnValueException.Reuse(ex);
			}
			catch (ScriptBreakLoopException ex)
			{
				Result = ex.LoopValue ?? ObjectValue.Null;
				//ScriptBreakLoopException.Reuse(ex);
			}
			catch (ScriptContinueLoopException ex)
			{
				Result = ex.LoopValue ?? ObjectValue.Null;
				//ScriptContinueLoopException.Reuse(ex);
			}

			return Result;
		}

		/// <summary>
		/// Gets an array of objects corresponding to the elements of a vector.
		/// </summary>
		/// <param name="v">Vector.</param>
		/// <returns>Array of objects.</returns>
		protected object[] ToObjectArray(IVector v)
		{
			int i, c = v.Dimension;
			object[] Result = new object[c];

			for (i = 0; i < c; i++)
				Result[i] = v.GetElement(i).AssociatedObjectValue;

			return Result;
		}

		/// <summary>
		/// If graph uses default color
		/// </summary>
		public abstract bool UsesDefaultColor
		{
			get;
		}

		/// <summary>
		/// Tries to set the default color.
		/// </summary>
		/// <param name="Color">Default color.</param>
		/// <returns>If possible to set.</returns>
		public abstract bool TrySetDefaultColor(SKColor Color);

		/// <summary>
		/// Trims a numeric label, removing apparent roundoff errors.
		/// </summary>
		/// <param name="Label">Numeric label.</param>
		/// <returns>Trimmed label.</returns>
		public static string TrimLabel(string Label)
		{
			int i = Label.IndexOf('.');
			if (i < 0)
				return Label;

			int j = Label.LastIndexOf("00000");
			if (j > i)
			{
				i = Label.Length - 5;
				while (Label[i] == '0')
					i--;

				if (Label[i] == '.')
					i--;

				return Label.Substring(0, i + 1);
			}

			j = Label.LastIndexOf("99999");
			if (j > i)
			{
				bool DecimalSection = true;

				i = j;
				while (Label[i] == '9')
					i--;

				if (Label[i] == '.')
				{
					i--;
					DecimalSection = false;
				}

				char[] ch = Label.Substring(0, i + 1).ToCharArray();
				char ch2;
				bool CheckOne = true;

				while (i >= 0)
				{
					ch2 = ch[i];
					if (ch2 == '9')
					{
						ch[i] = '0';
						i--;
					}
					else if (ch2 == '.')
					{
						i--;
						DecimalSection = false;
					}
					else if (ch2 >= '0' && ch2 <= '8')
					{
						ch[i]++;
						CheckOne = false;
						break;
					}
					else
						break;
				}

				if (CheckOne)
				{
					int c = ch.Length;
					i++;
					Array.Resize(ref ch, c + 1);
					Array.Copy(ch, i, ch, i + 1, c - i);
					ch[i] = '1';
				}
				else if (DecimalSection)
				{
					i = ch.Length - 1;
					while (i >= 0 && ch[i] == '0')
						i--;

					if (ch[i] == '.')
						i--;

					if (i < ch.Length - 1)
						Array.Resize(ref ch, i + 1);
				}

				Label = new string(ch);
			}

			return Label;
		}

		/// <summary>
		/// Imports a graph from an XML element definition.
		/// </summary>
		/// <param name="Xml">XML definition</param>
		/// <returns>Graph, or null if not able to parse Graph.</returns>
		public static Task<Graph> TryImport(XmlElement Xml)
		{
			return TryImport(Xml, false);
		}

		/// <summary>
		/// Imports a graph from an XML element definition. If not able to parse a graph,
		/// an exception will be thrown.
		/// </summary>
		/// <param name="Xml">XML definition</param>
		/// <returns>Graph.</returns>
		public static Task<Graph> Import(XmlElement Xml)
		{
			return TryImport(Xml, true);
		}

		/// <summary>
		/// Imports a graph from an XML element definition.
		/// </summary>
		/// <param name="Xml">XML definition</param>
		/// <param name="ThrowException">If an exception should be thrown if a graph
		/// cannot be imported. Otherwise, null will be returned.</param>
		/// <returns>Graph</returns>
		public static async Task<Graph> TryImport(XmlElement Xml, bool ThrowException)
		{
			if (Xml is null ||
				Xml.LocalName != GraphLocalName ||
				Xml.NamespaceURI != GraphNamespace)
			{
				if (ThrowException)
					throw new ArgumentException("Invalid graph XML.", nameof(Xml));
				else
					return null;
			}

			string TypeName = Xml.GetAttribute("type");
			if (string.IsNullOrEmpty(TypeName))
			{
				if (ThrowException)
					throw new Exception("Empty graph type.");
				else
					return null;
			}

			Type T = Types.GetType(TypeName);
			if (T is null)
			{
				if (ThrowException)
					throw new Exception("Type not recognized: " + TypeName);
				else
					return null;
			}

			Graph G = (Graph)Types.Instantiate(T);
			G.SameScale = Boolean.ToBoolean(Xml.GetAttribute("sameScale")) ?? false;

			bool First = true;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				if (First)
				{
					await G.ImportGraphAsync(E);
					First = false;
				}
				else if (E.LocalName=="Settings" && E.NamespaceURI == GraphNamespace)
					G.settings = GraphSettings.Import(E);
			}

			return G;
		}

	}
}
