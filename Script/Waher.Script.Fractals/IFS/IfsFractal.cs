using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;


namespace Waher.Script.Fractals.IFS
{
	/// <summary>
	/// Calculates a fractal based on an Iterated Function System, using the chaos game.
	/// </summary>
	/// <example>
	/// IfsFractal(0.5,0.5,1,1e6, [
	/// 	Scale2DH(0.5,0.5),
	/// 	Translate2DH(0.25,0.5)*Scale2DH(0.5,0.5),
	/// 	Translate2DH(0.5,0)*Scale2DH(0.5,0.5)],400,400);
	/// 
	/// IfsFractal(0.5,0.5,1,1e6, [
	///  	Scale2DH(0.5,0.5),
	///  	Translate2DH(0.5,0)*Scale2DH(0.5,0.5),
	///  	Translate2DH(0,0.5)*Scale2DH(0.5,0.5)],400,400);
	/// 
	/// IfsFractal(0.5,0.5,1,1e6, [
	///  	z->z/2,
	///  	z->z/2+(1+2*i)/4,
	///  	z->z/2+1/2],400,400);
	/// 
	/// IfsFractal(0,0,3,1e6, [
	/// 	 z->sqrt(z-(-0.748814392089844-0.0801434326171877*i)),
	/// 	 z->-sqrt(z-(-0.748814392089844-0.0801434326171877*i))],400,400);
	/// 
	/// 	Heighway dragon:
	/// 
	/// IfsFractal(0.4,0.2,2,1000000, [
	/// 	 Rotate2DH(-45°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Orange",
	/// 	 Translate2DH(1,0)*Rotate2DH(-135°)*Scale2DH(1/sqrt(2),1/sqrt(2)),"Green"],400,300)
	/// 
	/// Twin dragon:
	/// 
	/// IfsFractal(0.5,0,2,1e6, [
	/// 	 z->(1+i)* z/2,"Green",
	///      z->((1+i)* z+1-i)/2,"Orange"],400,400);
	/// 
	/// Terdragon:
	/// 
	/// L:=1/2-i/(2* sqrt(3));
	/// IfsFractal(0.5,0,1.2,1e6, [
	/// 	 z->L*z,"Red",
	/// 	 z->i/sqrt(3)*z+L,"Green",
	/// 	 z->L*z+conj(L),"Blue"],400,400);
	/// 
	/// Barnsley's Fern:
	/// 
	/// IfsFractal(0,5,6,1e6, [
	/// 	 [[0,0,0],
	/// 	 [0,0.16,0],
	/// 	 [0,0,1]],
	///      0.01,"Green",
	///      [[0.85,0.04,0],
	///      [-0.04,0.85,1.6],
	///      [0,0,1]],
	///      0.85,"Green",
	///      [[0.2,-0.26,0],
	///      [0.26,0.24,1.6],
	///      [0,0,1]],
	///      0.07,"Green",
	///      [[-0.15,0.28,0],
	///      [0.26,0.24,0.44],
	///      [0,0,1]],
	///      0.07,"Green"],300,600);
	/// 
	/// Koch Snowflake:
	/// 
	/// S:=Scale2DH(1/3,1/3);
	/// 	T1:=Translate2DH(0,2/3);
	/// 	T2:=Translate2DH(0,-1/3);
	/// 
	/// IfsFractal(0,0,2,1e7,
	/// 	join([foreach a in 0..5 do Rotate2DH(a * 60°)*T1*S], [foreach a in 3..8 do Rotate2DH(a * 60°)*T2*S]),400,400);
	/// 
	/// IfsFractal(0,0,2,1e5,
	/// 	join([foreach a in 0..5 do Rotate2DH(a * 60°)*T1*S], [foreach a in 3..8 do Rotate2DH(a * 60°)*T2*S]),400,400);
	/// 
	/// IfsFractal(0,0,2,1e6, [
	/// 	Rotate2DH(0°)*T1*S,"Red",
	/// 	Rotate2DH(60°)*T1*S,"Green",
	/// 	Rotate2DH(120°)*T1*S,"Blue",
	/// 	Rotate2DH(180°)*T1*S,"Orange",
	/// 	Rotate2DH(240°)*T1*S,"Magenta",
	/// 	Rotate2DH(300°)*T1*S,"Cyan",
	/// 	Rotate2DH(180°)*T2*S,"Salmon",
	/// 	Rotate2DH(240°)*T2*S,"LightGreen",
	/// 	Rotate2DH(300°)*T2*S,"LightBlue",
	/// 	Rotate2DH(0°)*T2*S,"Yellow",
	/// 	Rotate2DH(60°)*T2*S,"Pink",
	/// 	Rotate2DH(120°)*T2*S,"LightCyan"],400,400);
	/// 
	/// Césaro curves:
	/// 
	/// a:=0.3+0.3* i;
	/// IfsFractal(0.5,0.2,2,1e5, [z->a*z, z->a+(1-a)* z],400,200);  
	/// 
	/// a:=0.5+0.5* i;
	/// IfsFractal(0.5,0.4,2,1e5, [z->a*z, z->a+(1-a)* z],400,300); 
	/// 
	/// Koch-Peano curves:
	/// 
	/// a:=0.6+0.37* i;
	/// IfsFractal(0.5,0,2,1e5, [z->a*conj(z), z->a+(1-a)* conj(z)],400,200);  
	/// 
	/// a:=0.6+0.45* i;
	/// IfsFractal(0.5,0,2,1e5, [z->a*conj(z), z->a+(1-a)* conj(z)],400,200);  
	/// 
	/// a:=1/2+i* sqrt(3)/6;  
	/// IfsFractal(0.5,0,2,1e5, [z->a*conj(z), z->a+(1-a)* conj(z)],400,200);  
	/// 
	/// a:=(1+i)/2;  
	/// IfsFractal(0.5,0,2,1e5, [z->a*conj(z), z->a+(1-a)* conj(z)],400,200);  
	/// </example>
	public class IfsFractal : FunctionMultiVariate
	{
		public IfsFractal(ScriptNode xc, ScriptNode yc, ScriptNode dr, ScriptNode N, ScriptNode Transforms,
			ScriptNode DimX, ScriptNode DimY, ScriptNode Seed, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { xc, yc, dr, N, Transforms, DimX, DimY, Seed }, 
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Scalar, ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar,
					  ArgumentType.Scalar}, Start, Length, Expression)
		{
		}

		public IfsFractal(ScriptNode xc, ScriptNode yc, ScriptNode dr, ScriptNode N, ScriptNode Transforms,
			ScriptNode DimX, ScriptNode DimY, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { xc, yc, dr, N, Transforms, DimX, DimY },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Scalar, ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		public IfsFractal(ScriptNode xc, ScriptNode yc, ScriptNode dr, ScriptNode N, ScriptNode Transforms,
			ScriptNode DimX, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { xc, yc, dr, N, Transforms, DimX },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Scalar, ArgumentType.Vector, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		public IfsFractal(ScriptNode xc, ScriptNode yc, ScriptNode dr, ScriptNode N, ScriptNode Transforms,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { xc, yc, dr, N, Transforms },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
				  ArgumentType.Scalar, ArgumentType.Vector }, Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "xc", "yc", "dr", "N", "Transforms", "DimX", "DimY", "Seed" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			double xc, yc;
			double dr;
			long N;
			int dimx, dimy;
			int i, c;
			int Seed;

			i = 0;
			c = Arguments.Length;
			xc = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			yc = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			dr = Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			N = (long)Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			if (N <= 0)
				throw new ScriptRuntimeException("N in calls to IfsFractal() must be a positive integer.", this);

			Array Functions = Arguments[i].AssociatedObjectValue as Array;
			if (Functions == null)
				throw new ScriptRuntimeException("the fifth parameter to IfsFractal must be an array of homogenous 2D-transformations or lambda expressions.", this);

			List<DoubleMatrix> Matrices = null;
			List<ILambdaExpression> LambdaExpressions = null;
			List<double> Weights = new List<double>();
			List<SKColor> Colors = new List<SKColor>();
			string FunctionsExpression = this.Arguments[i++].SubExpression;

			foreach (object f in Functions)
			{
				if (f is DoubleMatrix M)
				{
					if (LambdaExpressions != null)
						throw new ScriptRuntimeException("Cannot mix homogenous 2D-transforms with lambda expressions.", this);

					if (Matrices == null)
						Matrices = new List<DoubleMatrix>();

					Matrices.Add(M);
					Weights.Add(1);
					Colors.Add(SKColors.Black);
				}
				else if (f is ILambdaExpression Lambda)
				{
					if (Matrices != null)
						throw new ScriptRuntimeException("Cannot mix homogenous 2D-transforms with lambda expressions.", this);

					if (LambdaExpressions == null)
						LambdaExpressions = new List<ILambdaExpression>();

					LambdaExpressions.Add(Lambda);
					Weights.Add(1);
					Colors.Add(SKColors.Black);
				}
				else if (f is SKColor || f is string)
				{
					if (Colors.Count == 0)
						throw new ScriptRuntimeException("Color definitions can only be specified after each transformation.", this);

					Colors[Colors.Count - 1] = Graph.ToColor(f);
				}
				else
				{
					try
					{
						double d = Expression.ToDouble(f);

						if (Weights.Count == 0)
							throw new ScriptRuntimeException("Weight definitions can only be specified after each transformation.", this);

						Weights[Weights.Count - 1] = d;
					}
					catch (Exception)
					{
						throw new ScriptRuntimeException("The fifth parameter to IfsFractal must be an array of homogenous 2D-transformations or lambda expressions, optionally followed by their corresponding weights and/or colors.", this);
					}
				}
			}

			if (i < c)
				dimx = (int)Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			else
				dimx = 320;

			if (i < c)
				dimy = (int)Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			else
				dimy = 200;

			if (i < c)
				Seed = (int)Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
			else
			{
				lock (gen)
				{
					Seed = gen.Next();
				}
			}

			if (i < c)
			{
				throw new ScriptRuntimeException("Parameter mismatch in call to IfsFractal(xc,yc,dr,N,Transforms[,dimx[,dimy[,Seed]]]).",
					this);
			}

			if (dimx <= 0 || dimx > 5000 || dimy <= 0 || dimy > 5000)
				throw new ScriptRuntimeException("Image size must be within 1x1 to 5000x5000", this);

			if (Matrices != null)
			{
				return CalcIfs(xc, yc, dr, N, Matrices.ToArray(), Weights.ToArray(), Colors.ToArray(),
					dimx, dimy, Seed, this, this.FractalZoomScript,
					new object[] { dimx, dimy, N, FunctionsExpression, Seed });
			}
			else
			{
				return CalcIfs(xc, yc, dr, N, LambdaExpressions.ToArray(), Weights.ToArray(), Colors.ToArray(),
					dimx, dimy, Seed, this, Variables, this.FractalZoomScript,
					new object[] { dimx, dimy, N, FunctionsExpression, Seed });
			}
		}

		private static Random gen = new Random();

		public override string FunctionName
		{
			get { return "IfsFractal"; }
		}

		private string FractalZoomScript(double r, double i, double Size, object State)
		{
			object[] Parameters = (object[])State;
			int DimX = (int)Parameters[0];
			int DimY = (int)Parameters[1];
			long N = (long)Parameters[2];
			string FunctionsExpression = (string)Parameters[3];
			int Seed = (int)Parameters[4];

			StringBuilder sb = new StringBuilder();

			sb.Append("IfsFractal(");
			sb.Append(Expression.ToString(r));
			sb.Append(",");
			sb.Append(Expression.ToString(i));
			sb.Append(",");
			sb.Append(Expression.ToString(Size / 4));
			sb.Append(",");
			sb.Append(N.ToString());
			sb.Append(",");
			sb.Append(FunctionsExpression);
			sb.Append(",");
			sb.Append(DimX.ToString());
			sb.Append(",");
			sb.Append(DimY.ToString());
			sb.Append(",");
			sb.Append(Seed.ToString());
			sb.Append(")");

			return sb.ToString();
		}

		public static FractalGraph CalcIfs(double xCenter, double yCenter, double rDelta, long N,
			DoubleMatrix[] Functions, double[] Weights, SKColor[] Colors, int Width, int Height, int Seed,
			ScriptNode Node, FractalZoomScript FractalZoomScript, object State)
		{
			DoubleMatrix M;
			double[,] E;
			double[][] Coefficients;
			double TotWeight = 0;
			double Weight;
			SKColor cl;
			byte[] Reds;
			byte[] Greens;
			byte[] Blues;
			int i, c = Functions.Length;
			Random Gen = new Random(Seed);

			if (c < 2)
				throw new ScriptRuntimeException("At least two transformations need to be provided.", Node);

			if (Weights.Length != c)
				throw new ArgumentException("Weights must be of equal length as Functions.", "Weights");

			if (Colors.Length != c)
				throw new ArgumentException("Colors must be of equal length as Functions.", "Colors");

			for (i = 0; i < c; i++)
			{
				Weight = Weights[i];
				if (Weight < 0)
					throw new ScriptRuntimeException("Weights must be non-negative.", Node);

				Weights[i] += TotWeight;
				TotWeight += Weight;
			}

			if (TotWeight == 0)
				throw new ScriptRuntimeException("The total weight of all functions must be postitive.", Node);

			for (i = 0; i < c; i++)
				Weights[i] /= TotWeight;

			Coefficients = new double[c][];
			Reds = new byte[c];
			Greens = new byte[c];
			Blues = new byte[c];

			for (i = 0; i < c; i++)
			{
				cl = Colors[i];
				Reds[i] = cl.Red;
				Greens[i] = cl.Green;
				Blues[i] = cl.Blue;

				M = Functions[i];
				E = M.Values;

				if (M.Columns == 2 && M.Rows == 2)
				{
					Coefficients[i] = new double[]
					{
						(double)E[0, 0], (double)E[0, 1], 0,
						(double)E[1, 0], (double)E[1, 1], 0,
						0, 0, 1
					};
				}
				else if (M.Columns == 3 && M.Rows == 3)
				{
					Coefficients[i] = new double[]
					{
						(double)E[0, 0], (double)E[0, 1], (double)E[0, 2],
						(double)E[1, 0], (double)E[1, 1], (double)E[1, 2],
						(double)E[2, 0], (double)E[2, 1], (double)E[2, 2]
					};
				}
				else
					throw new ScriptRuntimeException("Matrix not a linear 2D-transformation or a homogenous 2D-transformation.", Node);
			}

			int size = Width * Height * 4;
			byte[] rgb = new byte[size];

			double AspectRatio = ((double)Width) / Height;
			double x = Gen.NextDouble();
			double y = Gen.NextDouble();
			double p = 1;
			double x2, y2, p2;
			double[] C;
			int j;
			double xMin, xMax, yMin, yMax;
			double sx, sy;
			int xi, yi;

			xMin = xCenter - rDelta / 2;
			xMax = xMin + rDelta;
			yMin = yCenter - rDelta / (2 * AspectRatio);
			yMax = yMin + rDelta / AspectRatio;

			sx = Width / (xMax - xMin);
			sy = Height / (yMax - yMin);

			for (i = 0; i < 20; i++)
			{
				Weight = Gen.NextDouble();
				j = 0;
				while (j < c - 1 && Weights[j] <= Weight)
					j++;

				C = Coefficients[j];

				x2 = C[0] * x + C[1] * y + C[2] * p;
				y2 = C[3] * x + C[4] * y + C[5] * p;
				p2 = C[6] * x + C[7] * y + C[8] * p;

				x = x2;
				y = y2;
				p = p2;
			}

			while (N-- > 0)
			{
				Weight = Gen.NextDouble();
				j = 0;
				while (j < c - 1 && Weights[j] <= Weight)
					j++;

				C = Coefficients[j];

				x2 = C[0] * x + C[1] * y + C[2] * p;
				y2 = C[3] * x + C[4] * y + C[5] * p;
				p2 = C[6] * x + C[7] * y + C[8] * p;

				x = x2;
				y = y2;
				p = p2;

				if (p == 0)
					break;

				if (x < xMin || x > xMax || y < yMin || y > yMax)
					continue;

				xi = (int)((x / p - xMin) * sx + 0.5);
				yi = Height - 1 - (int)((y / p - yMin) * sy + 0.5);

				if (xi < 0 || xi >= Width || yi < 0 || yi >= Height)
					continue;

				i = (yi * Width + xi) << 2;

				if (rgb[i + 3] == 0)
				{
					rgb[i++] = Blues[j];
					rgb[i++] = Greens[j];
					rgb[i++] = Reds[j];
					rgb[i] = 0xff;
				}
				else
				{
					rgb[i] = (byte)((rgb[i] + Blues[j]) >> 1);
					i++;
					rgb[i] = (byte)((rgb[i] + Greens[j]) >> 1);
					i++;
					rgb[i] = (byte)((rgb[i] + Reds[j]) >> 1);
				}
			}

			using (SKData Data = SKData.Create(new MemoryStream(rgb)))
			{
				SKImage Bitmap = SKImage.FromPixelData(new SKImageInfo(Width, Height, SKColorType.Bgra8888), Data, Width * 4);
				return new FractalGraph(Bitmap, xMin, yMin, xMax, yMax, rDelta, false, Node, FractalZoomScript, State);
			}
		}

		public static FractalGraph CalcIfs(double xCenter, double yCenter, double rDelta, long N,
			ILambdaExpression[] Functions, double[] Weights, SKColor[] Colors, int Width, int Height, int Seed,
			ScriptNode Node, Variables Variables, FractalZoomScript FractalZoomScript, object State)
		{
			ILambdaExpression Lambda;
			double TotWeight = 0;
			double Weight;
			bool[] Real;
			byte[] Reds;
			byte[] Greens;
			byte[] Blues;
			SKColor cl;
			int i, c = Functions.Length;
			Random Gen = new Random(Seed);

			if (c < 2)
				throw new ScriptRuntimeException("At least two transformations need to be provided.", Node);

			if (Weights.Length != c)
				throw new ArgumentException("Weights must be of equal length as Functions.", "Weights");

			if (Colors.Length != c)
				throw new ArgumentException("Colors must be of equal length as Functions.", "Colors");

			for (i = 0; i < c; i++)
			{
				Weight = Weights[i];
				if (Weight < 0)
					throw new ScriptRuntimeException("Weights must be non-negative.", Node);

				Weights[i] += TotWeight;
				TotWeight += Weight;
			}

			if (TotWeight == 0)
				throw new ScriptRuntimeException("The total weight of all functions must be postitive.", Node);

			for (i = 0; i < c; i++)
				Weights[i] /= TotWeight;

			Real = new bool[c];
			Reds = new byte[c];
			Greens = new byte[c];
			Blues = new byte[c];

			for (i = 0; i < c; i++)
			{
				cl = Colors[i];
				Reds[i] = cl.Red;
				Greens[i] = cl.Green;
				Blues[i] = cl.Blue;

				Lambda = Functions[i];

				switch (Lambda.NrArguments)
				{
					case 1:
						Real[i] = false;
						break;

					case 2:
						Real[i] = true;
						break;

					default:
						throw new ScriptRuntimeException("Lambda expressions in calls to IfsFractal() must be either real-values (taking two parameters) or complex valued (taking one parameter).", Node);
				}
			}

			int size = Width * Height * 4;
			byte[] rgb = new byte[size];

			double AspectRatio = ((double)Width) / Height;
			double x = Gen.NextDouble();
			double y = Gen.NextDouble();
			int j;
			double xMin, xMax, yMin, yMax;
			double sx, sy;
			DoubleNumber xv = new DoubleNumber(0);
			DoubleNumber yv = new DoubleNumber(0);
			ComplexNumber zv = new ComplexNumber(0);
			IElement[] RealParameters = new IElement[] { xv, yv };
			IElement[] ComplexParameters = new IElement[] { zv };
			Variables v = new Variables();
			int xi, yi;

			Variables.CopyTo(v);

			xMin = xCenter - rDelta / 2;
			xMax = xMin + rDelta;
			yMin = yCenter - rDelta / (2 * AspectRatio);
			yMax = yMin + rDelta / AspectRatio;

			sx = Width / (xMax - xMin);
			sy = Height / (yMax - yMin);

			Complex z;

			for (i = 0; i < 20; i++)
			{
				Weight = Gen.NextDouble();
				j = 0;
				while (j < c - 1 && Weights[j] <= Weight)
					j++;

				Lambda = Functions[j];

				if (Real[j])
				{
					xv.Value = x;
					yv.Value = y;
					IVector Result = Lambda.Evaluate(RealParameters, v) as IVector;

					if (Result == null || Result.Dimension != 2)
						throw new ScriptRuntimeException("Expected 2-dimensional numeric vector as a result.", Node);

					x = Expression.ToDouble(Result.GetElement(0).AssociatedObjectValue);
					y = Expression.ToDouble(Result.GetElement(1).AssociatedObjectValue);
				}
				else
				{
					zv.Value = new Complex(x, y);
					z = Expression.ToComplex(Lambda.Evaluate(ComplexParameters, v).AssociatedObjectValue);

					x = z.Real;
					y = z.Imaginary;
				}
			}

			while (N-- > 0)
			{
				Weight = Gen.NextDouble();
				j = 0;
				while (j < c - 1 && Weights[j] <= Weight)
					j++;

				Lambda = Functions[j];

				if (Real[j])
				{
					xv.Value = x;
					yv.Value = y;
					IVector Result = Lambda.Evaluate(RealParameters, v) as IVector;

					if (Result == null || Result.Dimension != 2)
						throw new ScriptRuntimeException("Expected 2-dimensional numeric vector as a result.", Node);

					x = Expression.ToDouble(Result.GetElement(0).AssociatedObjectValue);
					y = Expression.ToDouble(Result.GetElement(1).AssociatedObjectValue);
				}
				else
				{
					zv.Value = new Complex(x, y);
					z = Expression.ToComplex(Lambda.Evaluate(ComplexParameters, v).AssociatedObjectValue);

					x = z.Real;
					y = z.Imaginary;
				}

				if (x < xMin || x > xMax || y < yMin || y > yMax)
					continue;

				xi = (int)((x - xMin) * sx + 0.5);
				yi = Height - 1 - (int)((y - yMin) * sy + 0.5);

				if (xi < 0 || xi >= Width || yi < 0 || yi >= Height)
					continue;

				i = (yi * Width + xi) << 2;

				if (rgb[i + 3] == 0)
				{
					rgb[i++] = Blues[j];
					rgb[i++] = Greens[j];
					rgb[i++] = Reds[j];
					rgb[i] = 0xff;
				}
				else
				{
					rgb[i] = (byte)((rgb[i] + Blues[j]) >> 1);
					i++;
					rgb[i] = (byte)((rgb[i] + Greens[j]) >> 1);
					i++;
					rgb[i] = (byte)((rgb[i] + Reds[j]) >> 1);
				}
			}

			using (SKData Data = SKData.Create(new MemoryStream(rgb)))
			{
				SKImage Bitmap = SKImage.FromPixelData(new SKImageInfo(Width, Height, SKColorType.Bgra8888), Data, Width * 4);
				return new FractalGraph(Bitmap, xMin, yMin, xMax, yMax, rDelta, false, Node, FractalZoomScript, State);
			}
		}
	}
}
