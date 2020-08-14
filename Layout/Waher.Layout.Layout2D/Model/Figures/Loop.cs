using System;
using SkiaSharp;
using Waher.Layout.Layout2D.Model.References;

namespace Waher.Layout.Layout2D.Model.Figures
{
	/// <summary>
	/// A loop
	/// </summary>
	public class Loop : Vertices
	{
		/// <summary>
		/// A loop
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Loop(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Loop";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Loop(Document, Parent);
		}

		/// <summary>
		/// Creates a Loop (Closed Spline) path through a given set of points.
		/// </summary>
		/// <param name="Points">Points between which the loop will be created.</param>
		/// <returns>Loop path.</returns>
		public static SKPath CreateLoop(params Vertex[] Points)
		{
			return CreateLoop(null, Points);
		}

		/// <summary>
		/// Creates a Loop (Closed Spline) path through a given set of points.
		/// </summary>
		/// <param name="AppendTo">Spline should be appended to this path. If null, a new path will be created.</param>
		/// <param name="Points">Points between which the loop will be created.</param>
		/// <returns>Loop path.</returns>
		public static SKPath CreateLoop(SKPath AppendTo, params Vertex[] Points)
		{
			int c = Points.Length;
			int i;
			SKPoint[] P = new SKPoint[c];

			for (i = 0; i < c; i++)
				P[i] = new SKPoint(Points[i].XCoordinate, Points[i].YCoordinate);

			return CreateLoop(AppendTo, P);
		}

		/// <summary>
		/// Creates a Loop (Closed Spline) path through a given set of points.
		/// </summary>
		/// <param name="Points">Points between which the loop will be created.</param>
		/// <returns>Loop path.</returns>
		public static SKPath CreateLoop(params SKPoint[] Points)
		{
			return CreateLoop(null, Points);
		}

		/// <summary>
		/// Creates a Loop (Closed Spline) path through a given set of points.
		/// </summary>
		/// <param name="AppendTo">Spline should be appended to this path. If null, a new path will be created.</param>
		/// <param name="Points">Points between which the loop will be created.</param>
		/// <returns>Loop path.</returns>
		public static SKPath CreateLoop(SKPath AppendTo, params SKPoint[] Points)
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

			float[] V = new float[c];

			for (i = 0; i < c; i++)
				V[i] = Points[i].X;

			GetCubicBezierCoefficients(V, out float[] Ax, out float[] Bx);

			for (i = 0; i < c; i++)
				V[i] = Points[i].Y;

			GetCubicBezierCoefficients(V, out float[] Ay, out float[] By);

			for (i = 0; i < c - 1; i++)
				AppendTo.CubicTo(Ax[i], Ay[i], Bx[i], By[i], Points[i + 1].X, Points[i + 1].Y);

			AppendTo.CubicTo(Ax[i], Ay[i], Bx[i], By[i], Points[0].X, Points[0].Y);
			AppendTo.Close();

			return AppendTo;
		}

		/// <summary>
		/// Gets a set of coefficients for cubic Bezier curves, forming a spline, one coordinate at a time.
		/// </summary>
		/// <param name="V">One set of coordinates, where the first and last must be the same.</param>
		/// <param name="A">Corresponding coefficients for first control points.</param>
		/// <param name="B">Corresponding coefficients for second control points.</param>
		public static void GetCubicBezierCoefficients(float[] V, out float[] A, out float[] B)
		{
			// Calculate closed spline loop between points P[0], ..., P[N], P[0].
			// Divide into segments, B[0], ...., B[N] of cubic Bezier curves:
			//
			// B[i](t) = (1-t)³P[i] + 3t(1-t)²A[i] + 3t²(1-t)B[i] + t³P[i+1]
			//
			// B'[i](t) = (-3+6t-3t²)P[i]+(3-12t+9t²)A[i]+(6t-9t²)B[i]+3t²P[i+1]
			// B"[i](t) = (6-6t)P[i]+(-12+18t)A[i]+(6-18t)B[i]+6tP[i+1]
			//
			// Choose control points A[i] and B[i] such that:
			//
			// B'[i](1) = B'[i+1](0) => A[i+1]+B[i]=2P[i+1], i≤N		(eq 1)
			// B"[i](1) = B"[i+1](0) => A[i]-2B[i]+2A[i+1]-B[i+1]=0		(eq 2)
			//
			// NOTE: i+1 is calculated mod (N+1).
			//
			// No additional boundary conditions are required.
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
			// |  1 -2  2 -1                       |   0 | (eq 2, i=0)
			// |     1  1                          | 2P1 | (eq 1, i=0)
			// |        1 -2  2 -1                 |   0 | (eq 2, i=1)
			// |           1  1                    | 2P2 | (eq 1, i=1)
			// |              1 -2  2 -1           |   0 | (eq 2, i=2)
			// |                 1  1              | 2P3 | (eq 1, i=2)
			// |                    ...            |   . | .
			// |                       ...         |   . | .
			// |                          ...      |   . | .
			// |  2 -1                        1 -2 |   0 | (eq 2, i=N)
			// |  1                              1 | 2P0 | (eq 1, i=N)

			int N = V.Length;
			int N2 = N << 1;
			float[] Row1 = new float[N2 + 1];
			float[] Row2 = new float[N2 + 1];
			float a, b;
			int i, j;

			Row1[0] = 2;
			Row1[1] = -1;
			Row1[N2 - 2] = 1;
			Row1[N2 - 1] = -2;

			Row2[0] = 1;
			Row2[N2 - 1] = 1;
			Row2[N2] = 2 * V[0];

			// Reduce the two ultimate rows:

			for (i = 1, j = 0; i < N; i++)
			{
				a = Row1[j];
				if (a != 0)
				{
					Row1[j] = 0;
					Row1[j + 1] += (b = 2 * a);
					Row1[j + 2] -= b;
					Row1[j + 3] += a;
				}

				a = Row2[j];
				if (a != 0)
				{
					Row2[j] = 0;
					Row2[j + 1] += (b = 2 * a);
					Row2[j + 2] -= b;
					Row2[j + 3] += a;
				}

				j++;

				a = Row1[j];
				if (a != 0)
				{
					Row1[j] = 0;
					Row1[j + 1] -= a;
					Row1[N2] -= a * 2 * V[i];
				}

				a = Row2[j];
				if (a != 0)
				{
					Row2[j] = 0;
					Row2[j + 1] -= a;
					Row2[N2] -= a * 2 * V[i];
				}

				j++;
			}

			A = new float[N];
			B = new float[N];

			if (Row1[N2 - 2] == 0)
			{
				float[] Temp = Row1;
				Row1 = Row2;
				Row2 = Temp;
			}

			a = 1 / Row1[N2 - 2];
			Row1[N2 - 2] = 1;
			Row1[N2 - 1] *= a;
			Row1[N2] *= a;

			a = Row2[N2 - 2];
			if (a != 0)
			{
				Row2[N2 - 2] = 0;
				Row2[N2 - 1] -= Row1[N2 - 1] * a;
				Row2[N2] -= Row1[N2] * a;
			}

			a = 1 / Row2[N2 - 1];
			Row2[N2 - 1] = 1;
			Row2[N2] *= a;

			j = N - 1;
			B[j] = Row2[N2];

			a = Row1[N2 - 1];
			if (a != 0)
			{
				Row1[N2 - 1] = 0;
				Row1[N2] -= Row2[N2] * a;
			}

			A[j] = Row1[N2];

			i = N - 1;
			while (--j >= 0)
			{
				B[j] = 2 * V[i--] - A[j + 1];
				A[j] = 2 * B[j] - 2 * A[j + 1] + B[j + 1];
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
				using (SKPath Path = CreateLoop(this.points))
				{
					if (this.TryGetFill(State, out SKPaint Fill))
						State.Canvas.DrawPath(Path, Fill);

					if (this.TryGetPen(State, out SKPaint Pen))
						State.Canvas.DrawPath(Path, Pen);
				}
			}
		}

	}
}
