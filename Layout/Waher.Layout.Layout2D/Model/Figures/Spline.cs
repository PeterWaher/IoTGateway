using System;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.References;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A spline
	/// </summary>
	public class Spline : Vertices
	{
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
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Spline";

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
			int c = Points.Length;
			int i;
			SKPoint[] P = new SKPoint[c];

			for (i = 0; i < c; i++)
				P[i] = new SKPoint(Points[i].XCoordinate, Points[i].YCoordinate);

			return CreateSpline(AppendTo, P);
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
				return AppendTo;

			if (c == 2)
			{
				AppendTo.LineTo(Points[1]);
				return AppendTo;
			}

			float[] V = new float[c];

			for (i = 0; i < c; i++)
				V[i] = Points[i].X;

			GetCubicBezierCoefficients(V, out float[] Ax, out float[] Bx);

			for (i = 0; i < c; i++)
				V[i] = Points[i].Y;

			GetCubicBezierCoefficients(V, out float[] Ay, out float[] By);

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
		/// Draws layout entities.
		/// </summary>
		/// <param name="State">Current drawing state.</param>
		public override void Draw(DrawingState State)
		{
			base.Draw(State);

			if (this.defined)
			{
				using (SKPath Path = CreateSpline(this.points))
				{
					State.Canvas.DrawPath(Path, this.GetPen(State));
				}
			}
		}

	}
}
