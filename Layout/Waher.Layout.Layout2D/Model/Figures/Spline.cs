using System;
using System.Xml;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.Attributes;
using Waher.Layout.Layout2D.Model.References;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A spline
	/// </summary>
	public class Spline : Vertices, IDirectedElement
	{
		private StringAttribute head;
		private StringAttribute tail;

		/// <summary>
		/// A spline
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Spline(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.spline?.Dispose();
			this.spline = null;
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Spline";

		/// <summary>
		/// Head
		/// </summary>
		public StringAttribute HeadAttribute
		{
			get => this.head;
			set => this.head = value;
		}

		/// <summary>
		/// Tail
		/// </summary>
		public StringAttribute TailAttribute
		{
			get => this.tail;
			set => this.tail = value;
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.head = new StringAttribute(Input, "head");
			this.tail = new StringAttribute(Input, "tail");
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.head.Export(Output);
			this.tail.Export(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Spline(Document, Parent);
		}

		/// <summary>
		/// Creates a Spline path through a given set of points.
		/// </summary>
		/// <param name="Points">Points between which the spline will be created.</param>
		/// <returns>Spline path.</returns>
		public static SKPath CreateSpline(params Vertex[] Points)
		{
			return CreateSpline(null, Points);
		}

		/// <summary>
		/// Creates a Spline path through a given set of points.
		/// </summary>
		/// <param name="AppendTo">Spline should be appended to this path. If null, a new path will be created.</param>
		/// <param name="Points">Points between which the spline will be created.</param>
		/// <returns>Spline path.</returns>
		public static SKPath CreateSpline(SKPath AppendTo, params Vertex[] Points)
		{
			return CreateSpline(AppendTo, ToPoints(Points));
		}

		private static SKPoint[] ToPoints(Vertex[] Points)
		{
			int c = Points.Length;
			int i;
			SKPoint[] P = new SKPoint[c];

			for (i = 0; i < c; i++)
				P[i] = new SKPoint(Points[i].XCoordinate, Points[i].YCoordinate);

			return P;
		}

		/// <summary>
		/// Creates a Spline path through a given set of points.
		/// </summary>
		/// <param name="Points">Points between which the spline will be created.</param>
		/// <returns>Spline path.</returns>
		public static SKPath CreateSpline(params SKPoint[] Points)
		{
			return CreateSpline(null, Points);
		}

		/// <summary>
		/// Creates a Spline path through a given set of points.
		/// </summary>
		/// <param name="AppendTo">Spline should be appended to this path. If null, a new path will be created.</param>
		/// <param name="Points">Points between which the spline will be created.</param>
		/// <returns>Spline path.</returns>
		public static SKPath CreateSpline(SKPath AppendTo, params SKPoint[] Points)
		{
			return CreateSpline(AppendTo, out _, out _, out _, out _, Points);
		}

		private static SKPath CreateSpline(SKPath AppendTo,
			out float[] Ax, out float[] Ay, out float[] Bx, out float[] By,
			params SKPoint[] Points)
		{
			int i, c = Points.Length;
			if (c == 0)
				throw new ArgumentException("No points provided.", nameof(Points));

			if (AppendTo is null)
			{
				AppendTo = new SKPath();
				AppendTo.MoveTo(Points[0]);
			}
			else
			{
				SKPoint P = AppendTo.LastPoint;

				if (P.X != Points[0].X || P.Y != Points[0].Y)
					AppendTo.LineTo(Points[0]);
			}

			if (c == 1)
			{
				Ax = Ay = Bx = By = null;
				return AppendTo;
			}

			if (c == 2)
			{
				AppendTo.LineTo(Points[1]);
				Ax = Ay = Bx = By = null;
				return AppendTo;
			}

			float[] V = new float[c];

			for (i = 0; i < c; i++)
				V[i] = Points[i].X;

			GetCubicBezierCoefficients(V, out Ax, out Bx);

			for (i = 0; i < c; i++)
				V[i] = Points[i].Y;

			GetCubicBezierCoefficients(V, out Ay, out By);

			for (i = 0; i < c - 1; i++)
				AppendTo.CubicTo(Ax[i], Ay[i], Bx[i], By[i], Points[i + 1].X, Points[i + 1].Y);

			return AppendTo;
		}

		/// <summary>
		/// Gets a set of coefficients for cubic Bezier curves, forming a spline, one coordinate at a time.
		/// </summary>
		/// <param name="V">One set of coordinates.</param>
		/// <param name="A">Corresponding coefficients for first control points.</param>
		/// <param name="B">Corresponding coefficients for second control points.</param>
		public static void GetCubicBezierCoefficients(float[] V, out float[] A, out float[] B)
		{
			// Calculate Spline between points P[0], ..., P[N].
			// Divide into segments, B[0], ...., B[N-1] of cubic Bezier curves:
			//
			// B[i](t) = (1-t)³P[i] + 3t(1-t)²A[i] + 3t²(1-t)B[i] + t³P[i+1]
			//
			// B'[i](t) = (-3+6t-3t²)P[i]+(3-12t+9t²)A[i]+(6t-9t²)B[i]+3t²P[i+1]
			// B"[i](t) = (6-6t)P[i]+(-12+18t)A[i]+(6-18t)B[i]+6tP[i+1]
			//
			// Choose control points A[i] and B[i] such that:
			//
			// B'[i](1) = B'[i+1](0) => A[i+1]+B[i]=2P[i+1], i<N		(eq 1)
			// B"[i](1) = B"[i+1](0) => A[i]-2B[i]+2A[i+1]-B[i+1]=0		(eq 2)
			//
			// Also add the boundary conditions:
			//
			// B"[0](0)=0 => 2A[0]-B[0]=P[0]			(eq 3)
			// B"[N-1](1)=0 => -A[N-1]+2B[N-1]=P[N]		(eq 4)
			//
			// Method solves this linear equation for one coordinate of A[i] and B[i] at a time.
			//
			// First, the linear equation, is reduced downwards. Only coefficients close to
			// the diagonal, and in the right-most column need to be processed. Furthermore,
			// we don't have to store values we know are zero or one. Since number of operations
			// depend linearly on number of vertices, algorithm is O(N).
			//
			// Matrix of system of linear equations has the following form (zeroes excluded):
			//
			// | A0 B0 A1 B1 A2 B2 A3 B3 ... AN BN |  EQ |
			// |-----------------------------------|-----|
			// |  2 -1                             |  P0 | (eq 3)
			// |  1 -2  2 -1                       |   0 | (eq 2)
			// |     1  1                          | 2P1 | (eq 1)
			// |        1 -2  2 -1                 |   0 | (eq 2)
			// |           1  1                    | 2P2 | (eq 1)
			// |              1 -2  2 -1           |   0 | (eq 2)
			// |                 1  1              | 2P3 | (eq 1)
			// |                    ...            |   . |
			// |                       ...         |   . |
			// |                          ...      |   . |
			// |                             -1  2 |  PN | (eq 4)

			int N = V.Length - 1;
			int N2 = N << 1;
			int i = 0;
			int j = 0;
			float r11, r12, r15;               // r13 & r14 always 0.
			float r22, r23, r25;               // r21 & r24 always 0 for all except last equation, where r21 is -1.
			float /*r31,*/ r32, r33, r34, r35;
			float[,] Rows = new float[N2, 3];
			float a;

			A = new float[N];
			B = new float[N];

			r11 = 2;        // eq 3
			r12 = -1;
			r15 = V[j++];

			r22 = 1;        // eq 1
			r23 = 1;
			r25 = 2 * V[j++];

			// r31 = 1;     // eq 2
			r32 = -2;
			r33 = 2;
			r34 = -1;
			r35 = 0;

			while (true)
			{
				a = 1 / r11;
				// r11 = 1;
				r12 *= a;
				r15 *= a;

				// r21 is always 0. No need to eliminate column.
				// r22 is always 1. No need to scale row.

				// r31 is always 1 at this point.
				// r31 -= r11;
				r32 -= r12;
				r35 -= r15;

				if (r32 != 0)
				{
					r33 -= r32 * r23;
					r35 -= r32 * r25;
					// r32 = 0;
				}

				// r33 is always 0.

				// r11 always 1.
				Rows[i, 0] = r12;
				Rows[i, 1] = 0;
				Rows[i, 2] = r15;
				i++;

				// r21, r24 always 0.
				Rows[i, 0] = r22;
				Rows[i, 1] = r23;
				Rows[i, 2] = r25;
				i++;

				if (i >= N2 - 2)
					break;

				r11 = r33;
				r12 = r34;
				r15 = r35;

				r22 = 1;        // eq 1
				r23 = 1;
				r25 = 2 * V[j++];

				// r31 = 1;        // eq 2
				r32 = -2;
				r33 = 2;
				r34 = -1;
				r35 = 0;
			}

			r11 = r33;
			r12 = r34;
			r15 = r35;

			//r21 = -1;		// eq 4
			r22 = 2;
			r23 = 0;
			r25 = V[j++];

			a = 1 / r11;
			//r11 = 1;
			r12 *= a;
			r15 *= a;

			//r21 += r11;
			r22 += r12;
			r25 += r15;

			r25 /= r22;
			r22 = 1;

			// r11 always 1.
			Rows[i, 0] = r12;
			Rows[i, 1] = 0;
			Rows[i, 2] = r15;
			i++;

			// r21 and r24 always 0.
			Rows[i, 0] = r22;
			Rows[i, 1] = r23;
			Rows[i, 2] = r25;
			i++;

			// Then eliminate back up:

			j--;
			while (i > 0)
			{
				i--;
				if (i < N2 - 1)
				{
					a = Rows[i, 1];
					if (a != 0)
					{
						Rows[i, 1] = 0;
						Rows[i, 2] -= a * Rows[i + 1, 2];
					}
				}

				B[--j] = Rows[i, 2];

				i--;
				a = Rows[i, 0];
				if (a != 0)
				{
					Rows[i, 0] = 0;
					Rows[i, 2] -= a * Rows[i + 1, 2];
				}

				A[j] = Rows[i, 2];
			}
		}

		/// <summary>
		/// Copies contents (attributes and children) to the destination element.
		/// </summary>
		/// <param name="Destination">Destination element</param>
		public override void CopyContents(ILayoutElement Destination)
		{
			base.CopyContents(Destination);

			if (Destination is Spline Dest)
			{
				Dest.head = this.head.CopyIfNotPreset();
				Dest.tail = this.tail.CopyIfNotPreset();
			}
		}

		/// <summary>
		/// Measures layout entities and defines unassigned properties, related to dimensions.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void MeasureDimensions(DrawingState State)
		{
			base.MeasureDimensions(State);

			if (this.head.TryEvaluate(State.Session, out string RefId) &&
				this.Document.TryGetElement(RefId, out ILayoutElement Element) &&
				Element is Shape Shape)
			{
				this.headElement = Shape;
			}

			if (this.tail.TryEvaluate(State.Session, out RefId) &&
				this.Document.TryGetElement(RefId, out Element) &&
				Element is Shape Shape2)
			{
				this.tailElement = Shape2;
			}

			this.spline?.Dispose();
			this.spline = null;
		}

		private Shape headElement;
		private Shape tailElement;
		private SKPath spline;
		private float[] ax;
		private float[] ay;
		private float[] bx;
		private float[] by;

		/// <summary>
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			if (this.defined)
			{
				SKPaint Pen = this.GetPen(State);

				this.CheckSpline();

				State.Canvas.DrawPath(this.spline, Pen);

				if (!(this.tailElement is null) || !(this.headElement is null))
				{
					if (!this.TryGetFill(State, out SKPaint Fill))
						Fill = null;

					this.tailElement?.DrawTail(State, this, Pen, Fill);
					this.headElement?.DrawHead(State, this, Pen, Fill);
				}
			}
		
			base.Draw(State);
		}

		private void CheckSpline()
		{
			if (this.spline is null)
			{
				this.spline = CreateSpline(null, out this.ax, out this.ay,
					out this.bx, out this.by, ToPoints(this.points));
			}
		}

		/// <summary>
		/// Tries to get start position and initial direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Initial direction.</param>
		/// <returns>If a start position was found.</returns>
		public bool TryGetStart(out float X, out float Y, out float Direction)
		{
			int c;

			if (this.defined && !(this.points is null) && (c = this.points.Length) >= 2)
			{
				Vertex P1 = this.points[0];

				X = P1.XCoordinate;
				Y = P1.YCoordinate;

				if (c == 2)
				{
					Vertex P2 = this.points[1];
			
					Direction = CalcDirection(P1, P2);
				}
				else
				{
					// B'[i](t) = (-3+6t-3t²)P[i]+(3-12t+9t²)A[i]+(6t-9t²)B[i]+3t²P[i+1]
					// B'[i](0) = -3P[i]+3A[i]

					float dx = 3 * (this.ax[0] - P1.XCoordinate);
					float dy = 3 * (this.ay[0] - P1.YCoordinate);

					Direction = CalcDirection(dx, dy);
				}

				return true;
			}

			X = Y = Direction = 0;
			return false;
		}

		/// <summary>
		/// Tries to get end position and terminating direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Terminating direction.</param>
		/// <returns>If a terminating position was found.</returns>
		public bool TryGetEnd(out float X, out float Y, out float Direction)
		{
			int c;

			if (this.defined && !(this.points is null) && (c = this.points.Length) >= 2)
			{
				Vertex P2 = this.points[c - 1];

				X = P2.XCoordinate;
				Y = P2.YCoordinate;

				if (c == 2)
				{
					Vertex P1 = this.points[c - 2];

					Direction = CalcDirection(P1, P2);
				}
				else
				{
					// B'[i](t) = (-3+6t-3t²)P[i]+(3-12t+9t²)A[i]+(6t-9t²)B[i]+3t²P[i+1]
					// B'[i](1) = -3B[i]+3P[i+1]

					float dx = 3 * (P2.XCoordinate - this.bx[c - 2]);
					float dy = 3 * (P2.YCoordinate - this.by[c - 2]);

					Direction = CalcDirection(dx, dy);
				}

				return true;
			}

			X = Y = Direction = 0;
			return false;
		}

	}
}
