using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Xml;
using SkiaSharp;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Vectors;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Vectors;
using Waher.Script.Units;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// Handles three-dimensional graphs.
	/// </summary>
	public class Graph3D : Graph
	{
		private readonly LinkedList<IMatrix> x = new LinkedList<IMatrix>();
		private readonly LinkedList<IMatrix> y = new LinkedList<IMatrix>();
		private readonly LinkedList<IMatrix> z = new LinkedList<IMatrix>();
		private readonly LinkedList<Vector4[,]> normals = new LinkedList<Vector4[,]>();
		private readonly LinkedList<object[]> parameters = new LinkedList<object[]>();
		private readonly LinkedList<IPainter3D> painters = new LinkedList<IPainter3D>();
		private IElement minX, maxX;
		private IElement minY, maxY;
		private IElement minZ, maxZ;
		private Type axisTypeX;
		private Type axisTypeY;
		private Type axisTypeZ;
		private string title = string.Empty;
		private string labelX = string.Empty;
		private string labelY = string.Empty;
		private string labelZ = string.Empty;
		private double angle = 20;
		private double inclination = 30;
		private int overSampling = 2;
		private bool showXAxis = true;
		private bool showYAxis = true;
		private bool showZAxis = true;
		private bool showGrid = true;
		//private readonly bool showZeroX = false;
		//private readonly bool showZeroY = false;
		//private readonly bool showZeroZ = false;

		/// <summary>
		/// Base class for two-dimensional graphs.
		/// </summary>
		public Graph3D()
			: base()
		{
		}

		/// <summary>
		/// Base class for two-dimensional graphs.
		/// </summary>
		/// <param name="X">X-axis</param>
		/// <param name="Y">Y-axis</param>
		/// <param name="Z">Z-axis</param>
		/// <param name="Normals">Optional Normals</param>
		/// <param name="Painter">Painter of graph.</param>
		/// <param name="ShowZeroX">If the y-axis (x=0) should always be shown.</param>
		/// <param name="ShowZeroY">If the x-axis (y=0) should always be shown.</param>
		/// <param name="ShowZeroZ">If the z-axis (z=0) should always be shown.</param>
		/// <param name="Node">Node creating the graph.</param>
		/// <param name="Parameters">Graph-specific parameters.</param>
		public Graph3D(IMatrix X, IMatrix Y, IMatrix Z, Vector4[,] Normals, IPainter3D Painter,
			bool ShowZeroX, bool ShowZeroY, bool ShowZeroZ, ScriptNode Node, params object[] Parameters)
			: base()
		{
			int c = X.Columns;
			int d = X.Rows;
			IElement Zero;

			if (c != Y.Columns || d != Y.Rows || c != Z.Columns || d != Z.Rows)
				throw new ScriptException("X, Y and Z matrices must be of equal dimensions.");

			if (!(Normals is null) && (c != Normals.GetLength(0) || d != Normals.GetLength(1)))
				throw new ScriptException("Normal matrix must be of equal dimensions as axes.");

			//this.showZeroX = ShowZeroX;
			//this.showZeroY = ShowZeroY;
			//this.showZeroZ = ShowZeroZ;

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

			this.minZ = Min.CalcMin(Z, Node);
			this.maxZ = Max.CalcMax(Z, Node);

			if (ShowZeroZ && c > 0 && this.minZ.AssociatedSet is IAbelianGroup AG3)
			{
				Zero = AG3.AdditiveIdentity;

				this.minZ = Min.CalcMin(new ObjectVector(this.minZ, Zero), null);
				this.maxZ = Max.CalcMax(new ObjectVector(this.maxZ, Zero), null);
			}

			this.axisTypeX = X.GetType();
			this.axisTypeY = Y.GetType();
			this.axisTypeZ = Z.GetType();

			if (c > 0)
			{
				this.x.AddLast(X);
				this.y.AddLast(Y);
				this.z.AddLast(Z);
				this.normals.AddLast(Normals);
				this.painters.AddLast(Painter);
				this.parameters.AddLast(Parameters);
			}
		}

		/// <summary>
		/// X-axis series.
		/// </summary>
		public LinkedList<IMatrix> X
		{
			get { return this.x; }
		}

		/// <summary>
		/// Y-axis series.
		/// </summary>
		public LinkedList<IMatrix> Y
		{
			get { return this.y; }
		}

		/// <summary>
		/// Z-axis series.
		/// </summary>
		public LinkedList<IMatrix> Z
		{
			get { return this.z; }
		}

		/// <summary>
		/// Optional normals.
		/// </summary>
		public LinkedList<Vector4[,]> Normals
		{
			get { return this.normals; }
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
		/// Smallest Z-value.
		/// </summary>
		public IElement MinZ
		{
			get { return this.minZ; }
		}

		/// <summary>
		/// Largest Z-value.
		/// </summary>
		public IElement MaxZ
		{
			get { return this.maxZ; }
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
		/// Label for z-axis.
		/// </summary>
		public string LabelZ
		{
			get { return this.labelZ; }
			set { this.labelZ = value; }
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
		/// If the Z-axis is to be displayed.
		/// </summary>
		public bool ShowZAxis
		{
			get { return this.showZAxis; }
			set { this.showZAxis = value; }
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
		/// Rotation angle, in degrees, around the Y-axis.
		/// </summary>
		public double Angle
		{
			get { return this.angle; }
			set { this.angle = value; }
		}

		/// <summary>
		/// Inclination, in degrees (rotation angle, around the X-axis).
		/// </summary>
		public double Inclination
		{
			get { return this.inclination; }
			set { this.inclination = value; }
		}

		/// <summary>
		/// Oversampling (for anti-aliasing-purposes).
		/// </summary>
		public int Oversampling
		{
			get { return this.overSampling; }
			set
			{
				if (value < 1)
					throw new ArgumentException("Must be a positive integer.", nameof(Oversampling));

				this.overSampling = value;
			}
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

			if (!(Element is Graph3D G))
				return null;

			if (G.x.First is null)
				return this;

			Graph3D Result = new Graph3D()
			{
				axisTypeX = this.axisTypeX,
				axisTypeY = this.axisTypeY,
				axisTypeZ = this.axisTypeZ,
				title = this.title,
				labelX = this.labelX,
				labelY = this.labelY,
				labelZ = this.labelZ,
				SameScale = this.SameScale
			};

			foreach (IMatrix M in this.x)
				Result.x.AddLast(M);

			foreach (IMatrix M in this.y)
				Result.y.AddLast(M);

			foreach (IMatrix M in this.z)
				Result.z.AddLast(M);

			foreach (Vector4[,] M in this.normals)
				Result.normals.AddLast(M);

			foreach (IPainter3D Painter in this.painters)
				Result.painters.AddLast(Painter);

			foreach (object[] P in this.parameters)
				Result.parameters.AddLast(P);

			foreach (IMatrix M in G.x)
			{
				if (M.GetType() != this.axisTypeX)
					throw new ScriptException("Incompatible types of series.");

				Result.x.AddLast(M);
			}

			foreach (IMatrix M in G.y)
			{
				if (M.GetType() != this.axisTypeY)
					throw new ScriptException("Incompatible types of series.");

				Result.y.AddLast(M);
			}

			foreach (IMatrix M in G.z)
			{
				if (M.GetType() != this.axisTypeZ)
					throw new ScriptException("Incompatible types of series.");

				Result.z.AddLast(M);
			}

			foreach (Vector4[,] M in G.normals)
				Result.normals.AddLast(M);

			foreach (IPainter3D Painter in G.painters)
				Result.painters.AddLast(Painter);

			foreach (object[] P in G.parameters)
				Result.parameters.AddLast(P);

			Result.minX = Min.CalcMin((IVector)VectorDefinition.Encapsulate(new IElement[] { this.minX, G.minX }, false, null), null);
			Result.maxX = Max.CalcMax((IVector)VectorDefinition.Encapsulate(new IElement[] { this.maxX, G.maxX }, false, null), null);
			Result.minY = Min.CalcMin((IVector)VectorDefinition.Encapsulate(new IElement[] { this.minY, G.minY }, false, null), null);
			Result.maxY = Max.CalcMax((IVector)VectorDefinition.Encapsulate(new IElement[] { this.maxY, G.maxY }, false, null), null);
			Result.minZ = Min.CalcMin((IVector)VectorDefinition.Encapsulate(new IElement[] { this.minZ, G.minZ }, false, null), null);
			Result.maxZ = Max.CalcMax((IVector)VectorDefinition.Encapsulate(new IElement[] { this.maxZ, G.maxZ }, false, null), null);

			Result.showXAxis |= G.showXAxis;
			Result.showYAxis |= G.showYAxis;
			Result.showZAxis |= G.showZAxis;

			return Result;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is Graph3D G))
				return false;

			return (
				this.minX.Equals(G.minX) &&
				this.maxX.Equals(G.maxX) &&
				this.minY.Equals(G.minY) &&
				this.maxY.Equals(G.maxY) &&
				this.minZ.Equals(G.minZ) &&
				this.maxZ.Equals(G.maxZ) &&
				this.axisTypeX.Equals(G.axisTypeX) &&
				this.axisTypeY.Equals(G.axisTypeY) &&
				this.axisTypeZ.Equals(G.axisTypeZ) &&
				this.title.Equals(G.title) &&
				this.labelX.Equals(G.labelX) &&
				this.labelY.Equals(G.labelY) &&
				this.labelZ.Equals(G.labelZ) &&
				this.showXAxis.Equals(G.showXAxis) &&
				this.showYAxis.Equals(G.showYAxis) &&
				this.showZAxis.Equals(G.showZAxis) &&
				this.showGrid.Equals(G.showGrid) &&
				this.Equals(this.x.GetEnumerator(), G.x.GetEnumerator()) &&
				this.Equals(this.y.GetEnumerator(), G.y.GetEnumerator()) &&
				this.Equals(this.z.GetEnumerator(), G.z.GetEnumerator()) &&
				this.Equals(this.normals?.GetEnumerator(), G.normals?.GetEnumerator()) &&
				this.Equals(this.parameters.GetEnumerator(), G.parameters.GetEnumerator()) &&
				this.Equals(this.painters.GetEnumerator(), G.painters.GetEnumerator()));
		}

		private bool Equals(IEnumerator e1, IEnumerator e2)
		{
			if ((e1 is null) ^ (e2 is null))
				return false;

			if (e1 is null)
				return true;

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
			Result ^= Result << 5 ^ this.minZ.GetHashCode();
			Result ^= Result << 5 ^ this.maxZ.GetHashCode();
			Result ^= Result << 5 ^ this.axisTypeX.GetHashCode();
			Result ^= Result << 5 ^ this.axisTypeY.GetHashCode();
			Result ^= Result << 5 ^ this.axisTypeZ.GetHashCode();
			Result ^= Result << 5 ^ this.title.GetHashCode();
			Result ^= Result << 5 ^ this.labelX.GetHashCode();
			Result ^= Result << 5 ^ this.labelY.GetHashCode();
			Result ^= Result << 5 ^ this.labelZ.GetHashCode();
			Result ^= Result << 5 ^ this.showXAxis.GetHashCode();
			Result ^= Result << 5 ^ this.showYAxis.GetHashCode();
			Result ^= Result << 5 ^ this.showZAxis.GetHashCode();
			Result ^= Result << 5 ^ this.showGrid.GetHashCode();

			foreach (IElement E in this.x)
				Result ^= Result << 5 ^ E.GetHashCode();

			foreach (IElement E in this.y)
				Result ^= Result << 5 ^ E.GetHashCode();

			foreach (IElement E in this.z)
				Result ^= Result << 5 ^ E.GetHashCode();

			if (!(this.normals is null))
			{
				foreach (Vector4[,] Normals in this.normals)
					Result ^= Result << 5 ^ Normals.GetHashCode();
			}

			foreach (object Obj in this.parameters)
				Result ^= Result << 5 ^ Obj.GetHashCode();

			foreach (IPainter3D Painter in this.painters)
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
		public override PixelInformation CreatePixels(GraphSettings Settings, out object[] States)
		{
			IVector XLabels = GetLabels(ref this.minX, ref this.maxX, this.x, Settings.ApproxNrLabelsX, out LabelType XLabelType);
			IVector YLabels = GetLabels(ref this.minY, ref this.maxY, this.y, Settings.ApproxNrLabelsY, out LabelType YLabelType);
			IVector ZLabels = GetLabels(ref this.minZ, ref this.maxZ, this.z, Settings.ApproxNrLabelsX, out LabelType ZLabelType);

			int OffsetX = -500;
			int OffsetY = -500;
			int OffsetZ = 1000;
			int Width = 1000;
			int Height = 1000;
			int Depth = 1000;
			float? OrigoX;
			float? OrigoY;
			float? OrigoZ;

			if (this.SameScale &&
				this.minX is DoubleNumber MinX &&
				this.maxX is DoubleNumber MaxX &&
				this.minY is DoubleNumber MinY &&
				this.maxY is DoubleNumber MaxY &&
				this.minZ is DoubleNumber MinZ &&
				this.maxZ is DoubleNumber MaxZ)
			{
				double DX = MaxX.Value - MinX.Value;
				double DY = MaxY.Value - MinY.Value;
				double DZ = MaxZ.Value - MinZ.Value;
				double SX = Width / (DX == 0 ? 1 : DX);
				double SY = Height / (DY == 0 ? 1 : DY);
				double SZ = Depth / (DZ == 0 ? 1 : DZ);
				double MinScale = Math.Min(SX, Math.Min(SY, SZ));

				if (SX == MinScale)
				{
					int Height2 = (int)(Height * SX / SY + 0.5);
					OffsetY += (Height - Height2) / 2;
					Height = Height2;

					int Depth2 = (int)(Depth * SX / SZ + 0.5);
					OffsetZ += (Depth - Depth2) / 2;
					Depth = Depth2;
				}
				else if (SY == MinScale)
				{
					int Width2 = (int)(Width * SY / SX + 0.5);
					OffsetX += (Width - Width2) / 2;
					Width = Width2;

					int Depth2 = (int)(Depth * SY / SZ + 0.5);
					OffsetZ += (Depth - Depth2) / 2;
					Depth = Depth2;
				}
				else
				{
					int Width2 = (int)(Width * SZ / SX + 0.5);
					OffsetX += (Width - Width2) / 2;
					Width = Width2;

					int Height2 = (int)(Height * SZ / SY + 0.5);
					OffsetY += (Height - Height2) / 2;
					Height = Height2;
				}
			}

			if (this.minX.AssociatedSet is IAbelianGroup AgX)
				OrigoX = (float)Scale(new ObjectVector(AgX.AdditiveIdentity), this.minX, this.maxX, OffsetX, Width)[0];
			else
				OrigoX = null;

			if (this.minY.AssociatedSet is IAbelianGroup AgY)
				OrigoY = (float)Scale(new ObjectVector(AgY.AdditiveIdentity), this.minY, this.maxY, OffsetY, Height)[0];
			else
				OrigoY = null;

			if (this.minZ.AssociatedSet is IAbelianGroup AgZ)
				OrigoZ = (float)Scale(new ObjectVector(AgZ.AdditiveIdentity), this.maxZ, this.minZ, OffsetZ, Depth)[0];
			else
				OrigoZ = null;

			float PlaneX = OrigoX ?? OffsetX;
			float PlaneY = OrigoY ?? OffsetY;
			float PlaneZ = OrigoZ ?? OffsetZ + Depth;

			DrawingVolume DrawingVolume = new DrawingVolume(this.minX, this.maxX,
				this.minY, this.maxY, this.minZ, this.maxZ, OffsetX, OffsetY, OffsetZ,
				Width, Height, Depth, OrigoX, OrigoY, OrigoZ);

			Canvas3D Canvas = new Canvas3D(Settings.Width, Settings.Height, this.overSampling, Settings.BackgroundColor);

			Canvas.Perspective(Settings.Height / 2, 2000);
			Canvas.RotateX(-(float)this.inclination, new Vector3(0, 0, 1500));
			Canvas.RotateY((float)this.angle, new Vector3(0, 0, 1500));

			IEnumerator<IMatrix> ex = this.x.GetEnumerator();
			IEnumerator<IMatrix> ey = this.y.GetEnumerator();
			IEnumerator<IMatrix> ez = this.z.GetEnumerator();
			IEnumerator<Vector4[,]> eN = this.normals.GetEnumerator();
			IEnumerator<object[]> eParameters = this.parameters.GetEnumerator();
			IEnumerator<IPainter3D> ePainters = this.painters.GetEnumerator();
			Vector4[,] Points;
			Vector4[,] Normals;
			Vector4[,] PrevPoints = null;
			Vector4[,] PrevNormals = null;
			object[] PrevParameters = null;
			IPainter3D PrevPainter = null;

			while (ex.MoveNext() && ey.MoveNext() && ez.MoveNext() &&
				eN.MoveNext() && eParameters.MoveNext() && ePainters.MoveNext())
			{
				Points = DrawingVolume.Scale(ex.Current, ey.Current, ez.Current);
				Normals = eN.Current;

				if (PrevPainter != null && ePainters.Current.GetType() == PrevPainter.GetType())
					ePainters.Current.DrawGraph(Canvas, Points, Normals, eParameters.Current, PrevPoints, PrevNormals, PrevParameters, DrawingVolume);
				else
					ePainters.Current.DrawGraph(Canvas, Points, Normals, eParameters.Current, null, null, null, DrawingVolume);

				PrevPoints = Points;
				PrevNormals = Normals;
				PrevParameters = eParameters.Current;
				PrevPainter = ePainters.Current;
			}

			Matrix4x4 M = Canvas.ModelTransformation;
			double[] LabelXX = DrawingVolume.ScaleX(XLabels);
			float LabelMargin = (float)Settings.LabelFontSize * 0.1f;
			float TextSize0 = (float)Settings.LabelFontSize * 5;
			float TextSize = this.CalcTextSize(LabelXX, TextSize0);
			SKSize Size;
			string s;
			float f;
			int i = 0;

			foreach (IElement Label in XLabels.ChildElements)
			{
				s = LabelString(Label, XLabelType);
				Size = Canvas.TextDimensions(s, Settings.FontName, TextSize);

				f = (float)LabelXX[i++];

				if (this.showGrid)
				{
					if (Label is DoubleNumber DLbl && DLbl.Value == 0)
					{
						this.DrawPlaneLine(Canvas,
							new Vector4(f, PlaneY, OffsetZ, 1),
							new Vector4(f, PlaneY, OffsetZ + Depth, 1),
							Settings.AxisColor, 3);

						this.DrawPlaneLine(Canvas,
							new Vector4(f, OffsetY, PlaneZ, 1),
							new Vector4(f, OffsetY + Height, PlaneZ, 1),
							Settings.AxisColor, 3);
					}
					else
					{
						this.DrawPlaneLine(Canvas,
							new Vector4(f, PlaneY, OffsetZ, 1),
							new Vector4(f, PlaneY, OffsetZ + Depth, 1),
							Settings.GridColor, 3);

						this.DrawPlaneLine(Canvas,
							new Vector4(f, OffsetY, PlaneZ, 1),
							new Vector4(f, OffsetY + Height, PlaneZ, 1),
							Settings.GridColor, 3);
					}
				}

				if (this.showXAxis)
				{
					Canvas.Translate(f, PlaneY, OffsetZ);
					Canvas.RotateY(-90);
					Canvas.RotateX(90);
					Canvas.Text(s,
						new Vector4(-5 * LabelMargin - Size.Width, LabelMargin - Size.Height / 2, 0, 1),
						Settings.FontName, TextSize, Settings.AxisColor);
					Canvas.ModelTransformation = M;
				}
			}

			if (!string.IsNullOrEmpty(this.labelX))
			{
				Size = Canvas.TextDimensions(this.labelX, Settings.FontName, TextSize0 * 1.25f);

				Canvas.Translate(OffsetX + Width / 2, PlaneY, OffsetZ);
				Canvas.RotateX(90);
				Canvas.Text(this.labelX,
					new Vector4(-Size.Width / 2, -2 * Size.Height, 0, 1),
					Settings.FontName, TextSize0 * 1.25f, Settings.AxisColor);
				Canvas.ModelTransformation = M;
			}

			double[] LabelZZ = DrawingVolume.ScaleZ(ZLabels);

			i = 0;
			TextSize = this.CalcTextSize(LabelZZ, TextSize0);

			foreach (IElement Label in ZLabels.ChildElements)
			{
				s = LabelString(Label, ZLabelType);
				Size = Canvas.TextDimensions(s, Settings.FontName, TextSize);

				f = (float)LabelZZ[i++];

				if (this.showGrid)
				{
					if (Label is DoubleNumber DLbl && DLbl.Value == 0)
					{
						this.DrawPlaneLine(Canvas,
							new Vector4(OffsetX, PlaneY, f, 1),
							new Vector4(OffsetX + Width, PlaneY, f, 1),
							Settings.AxisColor, 3);

						this.DrawPlaneLine(Canvas,
							new Vector4(PlaneX, OffsetY, f, 1),
							new Vector4(PlaneX, OffsetY + Height, f, 1),
							Settings.AxisColor, 3);
					}
					else
					{
						this.DrawPlaneLine(Canvas,
							new Vector4(OffsetX, PlaneY, f, 1),
							new Vector4(OffsetX + Width, PlaneY, f, 1),
							Settings.GridColor, 3);

						this.DrawPlaneLine(Canvas,
							new Vector4(PlaneX, OffsetY, f, 1),
							new Vector4(PlaneX, OffsetY + Height, f, 1),
							Settings.GridColor, 3);
					}
				}

				if (this.showZAxis)
				{
					Canvas.Translate(OffsetX + Width, PlaneY, f);
					Canvas.RotateX(90);
					Canvas.Text(s,
						new Vector4(5 * LabelMargin, LabelMargin - Size.Height / 2, 0, 1),
						Settings.FontName, TextSize, Settings.AxisColor);
					Canvas.ModelTransformation = M;
				}
			}

			if (!string.IsNullOrEmpty(this.labelZ))
			{
				Size = Canvas.TextDimensions(this.labelZ, Settings.FontName, TextSize0 * 1.25f);

				Canvas.Translate(OffsetX + Width, PlaneY, OffsetZ + Depth / 2);
				Canvas.RotateY(-90);
				Canvas.RotateX(90);
				Canvas.Text(this.labelZ,
					new Vector4(-Size.Width / 2, -2 * Size.Height, 0, 1),
					Settings.FontName, TextSize0 * 1.25f, Settings.AxisColor);
				Canvas.ModelTransformation = M;
			}

			double[] LabelYY = DrawingVolume.ScaleY(YLabels);

			i = 0;
			TextSize = this.CalcTextSize(LabelYY, TextSize0);

			foreach (IElement Label in YLabels.ChildElements)
			{
				s = LabelString(Label, YLabelType);
				Size = Canvas.TextDimensions(s, Settings.FontName, TextSize);

				f = (float)LabelYY[i++];

				if (this.showGrid)
				{
					if (Label is DoubleNumber DLbl && DLbl.Value == 0)
					{
						this.DrawPlaneLine(Canvas,
							new Vector4(PlaneX, f, OffsetZ, 1),
							new Vector4(PlaneX, f, OffsetZ + Depth, 1),
							Settings.AxisColor, 3);

						this.DrawPlaneLine(Canvas,
							new Vector4(OffsetX, f, PlaneZ, 1),
							new Vector4(OffsetX + Width, f, PlaneZ, 1),
							Settings.AxisColor, 3);
					}
					else
					{
						this.DrawPlaneLine(Canvas,
							new Vector4(PlaneX, f, OffsetZ, 1),
							new Vector4(PlaneX, f, OffsetZ + Depth, 1),
							Settings.GridColor, 3);

						this.DrawPlaneLine(Canvas,
							new Vector4(OffsetX, f, PlaneZ, 1),
							new Vector4(OffsetX + Width, f, PlaneZ, 1),
							Settings.GridColor, 3);
					}
				}

				if (this.showYAxis)
				{
					Canvas.Translate(OffsetX, f, PlaneZ);
					Canvas.Text(s,
						new Vector4(-5 * LabelMargin - Size.Width, LabelMargin - Size.Height / 2, 0, 1),
						Settings.FontName, TextSize, Settings.AxisColor);
					Canvas.ModelTransformation = M;
				}
			}

			if (!string.IsNullOrEmpty(this.labelY))
			{
				Size = Canvas.TextDimensions(this.labelY, Settings.FontName, TextSize0 * 1.25f);

				Canvas.Translate(OffsetX, OffsetY + Height / 2, PlaneZ);
				Canvas.RotateZ(90);
				Canvas.Text(this.labelY,
					new Vector4(-Size.Width / 2, 2 * Size.Height, 0, 1),
					Settings.FontName, TextSize0 * 1.25f, Settings.AxisColor);
				Canvas.ModelTransformation = M;
			}

			if (!string.IsNullOrEmpty(this.title))
			{
				Size = Canvas.TextDimensions(this.title, Settings.FontName, TextSize0 * 1.5f);

				Canvas.Translate(OffsetX + Width / 2, OffsetY + Height, OffsetZ + Depth / 2);
				Canvas.Text(this.title,
					new Vector4(-Size.Width / 2, Size.Height / 2, 0, 1),
					Settings.FontName, TextSize0 * 1.5f, Settings.AxisColor);
				Canvas.ModelTransformation = M;
			}

			I3DShader AxisPlaneShader = ToShader(new SKColor(Settings.AxisColor.Red,
				Settings.AxisColor.Green, Settings.AxisColor.Blue, 32));

			this.DrawPlane(Canvas,
				new Vector4(OffsetX, OffsetY, PlaneZ, 1),
				new Vector4(OffsetX + Width, OffsetY, PlaneZ, 1),
				new Vector4(OffsetX + Width, OffsetY + Height, PlaneZ, 1),
				new Vector4(OffsetX, OffsetY + Height, PlaneZ, 1),
				AxisPlaneShader, true, 3);

			this.DrawPlane(Canvas,
				new Vector4(OffsetX, PlaneY, OffsetZ, 1),
				new Vector4(OffsetX + Width, PlaneY, OffsetZ, 1),
				new Vector4(OffsetX + Width, PlaneY, OffsetZ + Depth, 1),
				new Vector4(OffsetX, PlaneY, OffsetZ + Depth, 1),
				AxisPlaneShader, true, 3);

			this.DrawPlane(Canvas,
				new Vector4(PlaneX, OffsetY, OffsetZ, 1),
				new Vector4(PlaneX, OffsetY, OffsetZ + Depth, 1),
				new Vector4(PlaneX, OffsetY + Height, OffsetZ + Depth, 1),
				new Vector4(PlaneX, OffsetY + Height, OffsetZ, 1),
				AxisPlaneShader, true, 3);

			return Canvas.CreatePixels(Settings, out States);
		}

		private float CalcTextSize(double[] LabelPositions, float TextSize)
		{
			int i, c = LabelPositions.Length;
			float Diff;

			for (i = 1; i < c; i++)
			{
				Diff = (float)(Math.Abs(LabelPositions[i] - LabelPositions[i - 1]));
				if (Diff < TextSize)
					TextSize = Diff;
			}

			return TextSize;
		}

		private void DrawPlaneLine(Canvas3D Canvas, Vector4 P0, Vector4 P1,
			SKColor Color, int Halvings)
		{
			if (Halvings == 0)
				Canvas.Line(P0, P1, Color);
			else
			{
				Halvings--;

				Vector4 Pm = (P0 + P1) / 2;

				this.DrawPlaneLine(Canvas, P0, Pm, Color, Halvings);
				this.DrawPlaneLine(Canvas, Pm, P1, Color, Halvings);
			}
		}

		private void DrawPlane(Canvas3D Canvas, Vector4 P0, Vector4 P1, Vector4 P2, Vector4 P3,
			I3DShader Shader, bool TwoSided, int Halvings)
		{
			if (Halvings == 0)
				Canvas.Polygon(new Vector4[] { P0, P1, P2, P3 }, Shader, TwoSided);
			else
			{
				Halvings--;

				Vector4 Pm = (P0 + P1 + P2 + P3) / 4;
				Vector4 P0p = (P3 + P0) / 2;
				Vector4 P1p = (P0 + P1) / 2;
				Vector4 P2p = (P1 + P2) / 2;
				Vector4 P3p = (P2 + P3) / 2;

				this.DrawPlane(Canvas, Pm, P2p, P2, P3p, Shader, TwoSided, Halvings);
				this.DrawPlane(Canvas, P1p, P1, P2p, Pm, Shader, TwoSided, Halvings);
				this.DrawPlane(Canvas, P3, P0p, Pm, P3p, Shader, TwoSided, Halvings);
				this.DrawPlane(Canvas, P0p, P0, P1p, Pm, Shader, TwoSided, Halvings);
			}
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
		public static IVector GetLabels(ref IElement Min, ref IElement Max, IEnumerable<IMatrix> Series, int ApproxNrLabels, out LabelType LabelType)
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

				foreach (IMatrix Matrix in Series)
				{
					foreach (IElement E in Matrix.ChildElements)
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

				foreach (IMatrix Matrix in Series)
				{
					foreach (IElement E in Matrix.ChildElements)
						Labels[E.AssociatedObjectValue.ToString()] = true;
				}

				string[] Labels2 = new string[Labels.Count];
				Labels.Keys.CopyTo(Labels2, 0);

				LabelType = LabelType.String;
				return new ObjectVector(Labels2);
			}
		}

		/// <summary>
		/// Scales three matrices of equal size to point vectors in space.
		/// </summary>
		/// <param name="MatrixX">X-matrix.</param>
		/// <param name="MatrixY">Y-matrix.</param>
		/// <param name="MatrixZ">Z-matrix.</param>
		/// <param name="MinX">Smallest X-value.</param>
		/// <param name="MaxX">Largest X-value.</param>
		/// <param name="MinY">Smallest Y-value.</param>
		/// <param name="MaxY">Largest Y-value.</param>
		/// <param name="MinZ">Smallest Z-value.</param>
		/// <param name="MaxZ">Largest Z-value.</param>
		/// <param name="OffsetX">X-offset to volume.</param>
		/// <param name="OffsetY">Y-offset to volume.</param>
		/// <param name="OffsetZ">Z-offset to volume.</param>
		/// <param name="Width">Width of volume.</param>
		/// <param name="Height">Height of volume.</param>
		/// <param name="Depth">Depth of volume.</param>
		/// <returns>Mesh of point vectors.</returns>
		public static Vector4[,] Scale(IMatrix MatrixX, IMatrix MatrixY, IMatrix MatrixZ,
			IElement MinX, IElement MaxX, IElement MinY, IElement MaxY,
			IElement MinZ, IElement MaxZ, double OffsetX, double OffsetY, double OffsetZ,
			double Width, double Height, double Depth)
		{
			int Columns = MatrixX.Columns;
			int Rows = MatrixX.Rows;

			if (MatrixY.Columns != Columns || MatrixY.Rows != Rows ||
				MatrixZ.Columns != Columns || MatrixZ.Rows != Rows)
			{
				throw new ScriptException("Dimension mismatch.");
			}

			double[,] X = Scale(MatrixX, MinX, MaxX, OffsetX, Width);
			double[,] Y = Scale(MatrixY, MinY, MaxY, OffsetY, Height);
			double[,] Z = Scale(MatrixZ, MinZ, MaxZ, OffsetZ, Depth);
			int i, j;
			Vector4[,] Points = new Vector4[Columns, Rows];

			for (i = 0; i < Columns; i++)
			{
				for (j = 0; j < Rows; j++)
					Points[i, j] = new Vector4((float)X[i, j], (float)Y[i, j], (float)Z[i, j], 1);
			}

			return Points;
		}

		/// <summary>
		/// Scales a matrix to fit a given volume.
		/// </summary>
		/// <param name="Matrix">Matrix.</param>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="Offset">Offset to volume.</param>
		/// <param name="Size">Size of volume.</param>
		/// <returns>Matrix distributed in the available volume.</returns>
		public static double[,] Scale(IMatrix Matrix, IElement Min, IElement Max, double Offset, double Size)
		{
			if (Matrix is DoubleMatrix DM)
			{
				if (!(Min is DoubleNumber dMin) || !(Max is DoubleNumber dMax))
					throw new ScriptException("Incompatible values.");

				return Scale(DM.Values, dMin.Value, dMax.Value, Offset, Size);
			}
			else if (Matrix is ObjectMatrix OM)
			{
				IElement[,] Elements = OM.Values;
				IElement E;
				int c = OM.Columns;
				int r = OM.Rows;
				int i, j;

				if (Min is PhysicalQuantity MinQ && Max is PhysicalQuantity MaxQ)
				{
					if (MinQ.Unit != MaxQ.Unit)
					{
						if (!Unit.TryConvert(MaxQ.Magnitude, MaxQ.Unit, MinQ.Unit, out double d))
							throw new ScriptException("Incompatible units.");

						MaxQ = new PhysicalQuantity(d, MinQ.Unit);
					}

					PhysicalQuantity[,] Matrix2 = new PhysicalQuantity[c, r];
					PhysicalQuantity Q;

					for (i = 0; i < c; i++)
					{
						for (j = 0; j < r; j++)
						{
							E = Elements[i, j];
							Q = E as PhysicalQuantity ?? throw new ScriptException("Incompatible values.");
							Matrix2[i, j] = Q;
						}
					}

					return Scale(Matrix2, MinQ.Magnitude, MaxQ.Magnitude, MinQ.Unit, Offset, Size);
				}
				else
				{
					if (Min is DoubleNumber MinD && Max is DoubleNumber MaxD)
					{
						double[,] Matrix2 = new double[c, r];
						DoubleNumber D;

						for (i = 0; i < c; i++)
						{
							for (j = 0; j < r; j++)
							{
								E = Elements[i, j];
								D = E as DoubleNumber ?? throw new ScriptException("Incompatible values.");
								Matrix2[i, j] = D.Value;
							}
						}

						return Scale(Matrix2, MinD.Value, MaxD.Value, Offset, Size);
					}
					else
					{
						if (Min is DateTimeValue MinDT && Max is DateTimeValue MaxDT)
						{
							DateTime[,] Matrix2 = new DateTime[c, r];
							DateTimeValue DT;

							for (i = 0; i < c; i++)
							{
								for (j = 0; j < r; j++)
								{
									E = Elements[i, j];
									DT = E as DateTimeValue ?? throw new ScriptException("Incompatible values.");
									Matrix2[i, j] = DT.Value;
								}
							}

							return Scale(Matrix2, MinDT.Value, MaxDT.Value, Offset, Size);
						}
						else
							return Scale(OM.Values, Offset, Size);
					}
				}
			}
			else
				throw new ScriptException("Invalid vector type.");
		}

		/// <summary>
		/// Scales a matrix to fit a given volume.
		/// </summary>
		/// <param name="Matrix">Matrix.</param>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="Offset">Offset to volume.</param>
		/// <param name="Size">Size of area.</param>
		/// <returns>Matrix distributed in the available volume.</returns>
		public static double[,] Scale(double[,] Matrix, double Min, double Max, double Offset, double Size)
		{
			int c = Matrix.GetLength(0);
			int r = Matrix.GetLength(1);
			int i, j;
			double[,] Result = new double[c, r];
			double Scale = Min == Max ? 1 : Size / (Max - Min);

			for (i = 0; i < c; i++)
			{
				for (j = 0; j < r; j++)
					Result[i, j] = (Matrix[i, j] - Min) * Scale + Offset;
			}

			return Result;
		}

		/// <summary>
		/// Scales a matrix to fit a given volume.
		/// </summary>
		/// <param name="Matrix">Matrix.</param>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="Offset">Offset to volume.</param>
		/// <param name="Size">Size of area.</param>
		/// <returns>Matrix distributed in the available volume.</returns>
		public static double[,] Scale(DateTime[,] Matrix, DateTime Min, DateTime Max, double Offset, double Size)
		{
			int c = Matrix.GetLength(0);
			int r = Matrix.GetLength(1);
			int i, j;
			double[,] v = new double[c, r];

			for (i = 0; i < c; i++)
			{
				for (j = 0; j < r; j++)
					v[i, j] = (Matrix[i, j] - referenceTimestamp).TotalDays;
			}

			return Scale(v, (Min - referenceTimestamp).TotalDays, (Max - referenceTimestamp).TotalDays, Offset, Size);
		}

		/// <summary>
		/// Scales a matrix to fit a given volume.
		/// </summary>
		/// <param name="Matrix">Matrix.</param>
		/// <param name="Min">Smallest value.</param>
		/// <param name="Max">Largest value.</param>
		/// <param name="Unit">Unit.</param>
		/// <param name="Offset">Offset to volume.</param>
		/// <param name="Size">Size of area.</param>
		/// <returns>Matrix distributed in the available volume.</returns>
		public static double[,] Scale(PhysicalQuantity[,] Matrix, double Min, double Max, Unit Unit, double Offset, double Size)
		{
			int c = Matrix.GetLength(0);
			int r = Matrix.GetLength(1);
			int i, j;
			double[,] v = new double[c, r];
			PhysicalQuantity Q;

			for (i = 0; i < c; i++)
			{
				for (j = 0; j < r; j++)
				{
					Q = Matrix[i, j];
					if (Q.Unit.Equals(Unit) || Q.Unit.IsEmpty)
						v[i, j] = Q.Magnitude;
					else if (!Unit.TryConvert(Q.Magnitude, Q.Unit, Unit, out v[i, j]))
						throw new ScriptException("Incompatible units.");
				}
			}

			return Scale(v, Min, Max, Offset, Size);
		}

		/// <summary>
		/// Scales a matrix to fit a given volume.
		/// </summary>
		/// <param name="Matrix">Matrix.</param>
		/// <param name="Offset">Offset to volume.</param>
		/// <param name="Size">Size of area.</param>
		/// <returns>Matrix distributed in the available volume.</returns>
		public static double[,] Scale(object[,] Matrix, double Offset, double Size)
		{
			Dictionary<object, int> Values = new Dictionary<object, int>();
			int Index = 0;
			int c = Matrix.GetLength(0);
			int r = Matrix.GetLength(1);
			int i, j;
			double[,] v = new double[c, r];
			object Value;

			for (i = 0; i < c; i++)
			{
				for (j = 0; j < r; j++)
				{
					Value = Matrix[i, j];
					if (!Values.TryGetValue(Value, out int k))
					{
						k = Index++;
						Values[Value] = k;
					}

					v[i, j] = k + 0.5;
				}
			}

			return Scale(v, 0, c, Offset, Size);
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
		/// Gets a shader object from an argument.
		/// </summary>
		/// <param name="Argument">Argument value.</param>
		/// <returns>Shader object</returns>
		public static I3DShader ToShader(object Argument)
		{
			if (Argument is I3DShader Shader)
				return Shader;

			if (!(Argument is SKColor Color))
				Color = Graph.ToColor(Argument);

			// TODO: 3D-Graph settings to define default lighting.

			return new PhongShader(
				new PhongMaterial(1, 2, 0, 10),
				new PhongIntensity(64, 64, 64, Color.Alpha),
				new PhongLightSource(
					new PhongIntensity(Color.Red, Color.Green, Color.Blue, Color.Alpha),
					new PhongIntensity(255, 255, 255, 255),
					new Vector3(1000, 1000, 0)));
		}

		/// <summary>
		/// Exports graph specifics to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportGraph(XmlWriter Output)
		{
			Output.WriteStartElement("Graph3D");
			Output.WriteAttributeString("title", this.title);
			Output.WriteAttributeString("labelX", this.labelX);
			Output.WriteAttributeString("labelY", this.labelY);
			Output.WriteAttributeString("labelZ", this.labelZ);
			Output.WriteAttributeString("axisTypeX", this.axisTypeX.FullName);
			Output.WriteAttributeString("axisTypeY", this.axisTypeY.FullName);
			Output.WriteAttributeString("axisTypeZ", this.axisTypeZ.FullName);
			Output.WriteAttributeString("minX", Expression.ToString(this.minX));
			Output.WriteAttributeString("maxX", Expression.ToString(this.maxX));
			Output.WriteAttributeString("minY", Expression.ToString(this.minY));
			Output.WriteAttributeString("maxY", Expression.ToString(this.maxY));
			Output.WriteAttributeString("minZ", Expression.ToString(this.minZ));
			Output.WriteAttributeString("maxZ", Expression.ToString(this.maxZ));
			Output.WriteAttributeString("showXAxis", this.showXAxis ? "true" : "false");
			Output.WriteAttributeString("showYAxis", this.showYAxis ? "true" : "false");
			Output.WriteAttributeString("showZAxis", this.showZAxis ? "true" : "false");
			Output.WriteAttributeString("showGrid", this.showGrid ? "true" : "false");
			Output.WriteAttributeString("angle", Expression.ToString(this.angle));
			Output.WriteAttributeString("inclination", Expression.ToString(this.inclination));
			Output.WriteAttributeString("overSampling", this.overSampling.ToString());

			foreach (IVector v in this.x)
				Output.WriteElementString("X", Expression.ToString(v));

			foreach (IVector v in this.y)
				Output.WriteElementString("Y", Expression.ToString(v));

			foreach (IVector v in this.z)
				Output.WriteElementString("Z", Expression.ToString(v));

			foreach (Vector4[,] M in this.normals)
			{
				StringBuilder sb = new StringBuilder();

				if (M is null)
					sb.Append("null");
				else
				{
					int c = M.GetLength(0);
					int d = M.GetLength(1);
					int i, j;

					sb.Append('[');
					for (i = 0; i < c; i++)
					{
						if (i > 0)
							sb.Append(',');

						sb.Append('[');

						for (j = 0; j < d; j++)
						{
							if (j > 0)
								sb.Append(',');

							sb.Append(Expression.ToString(M[i, j]));
						}

						sb.Append(']');
					}
					sb.Append(']');
				}

				Output.WriteElementString("Normals", sb.ToString());
			}

			foreach (object[] v in this.parameters)
				Output.WriteElementString("Parameters", Expression.ToString(new ObjectVector(v)));

			foreach (IPainter3D Painter in this.painters)
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

					case "labelZ":
						this.labelZ = Attr.Value;
						break;

					case "axisTypeX":
						this.axisTypeX = Types.GetType(Attr.Value);
						break;

					case "axisTypeY":
						this.axisTypeY = Types.GetType(Attr.Value);
						break;

					case "axisTypeZ":
						this.axisTypeZ = Types.GetType(Attr.Value);
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

					case "minZ":
						this.minZ = this.Parse(Attr.Value, Variables);
						break;

					case "maxZ":
						this.maxZ = this.Parse(Attr.Value, Variables);
						break;

					case "showXAxis":
						this.showXAxis = Attr.Value == "true";
						break;

					case "showYAxis":
						this.showYAxis = Attr.Value == "true";
						break;

					case "showZAxis":
						this.showZAxis = Attr.Value == "true";
						break;

					case "showGrid":
						this.showGrid = Attr.Value == "true";
						break;

					case "angle":
						if (Expression.TryParse(Attr.Value, out double d))
							this.angle = d;
						break;

					case "inclination":
						if (Expression.TryParse(Attr.Value, out d))
							this.inclination = d;
						break;

					case "overSampling":
						this.overSampling = int.Parse(Attr.Value);
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
							this.x.AddLast((IMatrix)this.Parse(E.InnerText, Variables));
							break;

						case "Y":
							this.y.AddLast((IMatrix)this.Parse(E.InnerText, Variables));
							break;

						case "Z":
							this.z.AddLast((IMatrix)this.Parse(E.InnerText, Variables));
							break;

						case "Normals":
							IMatrix M = (IMatrix)this.Parse(E.InnerText, Variables);

							int i, j, c, d;

							if (M is null)
								this.normals.AddLast((Vector4[,])null);
							else
							{
								c = M.Rows;
								d = M.Columns;
								Vector4[,] M2 = new Vector4[c, d];

								for (i = 0; i < c; i++)
								{
									for (j = 0; i < d; j++)
										M2[i, j] = (Vector4)(M.GetElement(i, j).AssociatedObjectValue);
								}

								this.normals.AddLast(M2);
							}
							break;

						case "Parameters":
							IVector v = (IVector)this.Parse(E.InnerText, Variables);
							this.parameters.AddLast(this.ToObjectArray(v));
							break;

						case "Painter":
							this.painters.AddLast((IPainter3D)Activator.CreateInstance(Types.GetType(E.InnerText)));
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
				IEnumerator<IPainter3D> ePainter = this.painters.GetEnumerator();

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
			IEnumerator<IPainter3D> ePainter = this.painters.GetEnumerator();
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
