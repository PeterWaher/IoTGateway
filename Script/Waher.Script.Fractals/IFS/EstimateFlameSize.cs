using System;
using System.Collections.Generic;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Fractals.IFS
{
	/// <summary>
	/// Estimates the size of a flame fractal.
	/// </summary>
	/// <example>
	/// N:=1000;
	/// do
	/// (
	///     cx:=Uniform(-1,1,6);
	/// 	cy:=Uniform(-1,1,6);
	/// 	f:=[
	/// 		Identity(2), QuadraticVariation(cx, cy),"Red",
	/// 		Identity(2), GaussianVariation(),"Yellow"
	///     ];
	///     v:=EstimateFlameSize(5000, f);
	/// 	Dim:=max(abs(v))
	/// ) while N-->0 && Dim>100;
	/// 
	/// if N<=0 then error("No coefficients found.");
	/// 
	/// [xc, yc, dr]:=v;
	/// 
	/// printline("X-coefficients: "+Str(cx));
	/// printline("Y-coefficients: "+Str(cy));
	/// printline("X-center: "+Str(xc));
	/// printline("Y-center: "+Str(yc));
	/// printline("Size: "+Str(dr));
	/// 
	/// FlameFractalHsl(xc, yc, dr, 2e7, f, false, false, 400, 300, 1, 2.5, 2)
	/// </example>
	public class EstimateFlameSize : FunctionMultiVariate
    {
		public EstimateFlameSize(ScriptNode N, ScriptNode FlameFunctions, ScriptNode DimX, ScriptNode DimY, ScriptNode Seed, 
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { N, FlameFunctions, DimX, DimY, Seed }, 
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar, 
				  ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		public EstimateFlameSize(ScriptNode N, ScriptNode FlameFunctions, ScriptNode DimX, ScriptNode DimY, 
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { N, FlameFunctions, DimX, DimY },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		public EstimateFlameSize(ScriptNode N, ScriptNode FlameFunctions, ScriptNode DimX, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { N, FlameFunctions, DimX },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Vector, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		public EstimateFlameSize(ScriptNode N, ScriptNode FlameFunctions, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { N, FlameFunctions },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Vector }, Start, Length, Expression)
		{
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] 
				{
					"N", "FlameFunctions", "DimX", "DimY", "Seed"
				};
			}
		}

		public override string FunctionName => nameof(EstimateFlameSize);

        public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            long N;
            int dimx, dimy;
            int i, c;
            int Seed;

            i = 0;
            c = Arguments.Length;
            N = (long)Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
            if (N <= 0)
                throw new ScriptRuntimeException("N in calls to EstimateFlameSize() must be a positive integer.", this);

            object Obj = Arguments[i++].AssociatedObjectValue;

			if (!(Obj is Array FlameArray))
				throw new ScriptRuntimeException("The second parameter to EstimateFlameSize must be an array, containing flame definitions.", this);

			List<FlameFunction> FlameFunctions = new List<FlameFunction>();
            double Weight;
            FlameFunction CurrentFunction = null;

            foreach (object FlameItem in FlameArray)
            {
                if (FlameItem is DoubleMatrix M)
                {
                    CurrentFunction = new FlameFunction(M, this);
                    FlameFunctions.Add(CurrentFunction);
                }
                else if (FlameItem is SKColor || FlameItem is string)
                {
                    if (CurrentFunction is null)
                    {
                        M = new DoubleMatrix(new double[,] { { 1, 0 }, { 0, 1 } });
                        CurrentFunction = new FlameFunction(M, this);
                        FlameFunctions.Add(CurrentFunction);
                    }

                    SKColor cl = Graph.ToColor(FlameItem);
					cl.ToHsl(out float H, out float S, out float L);
                    
                    CurrentFunction.SetColorHsl(H, S, L);
                    CurrentFunction = null;
				}
				else if (FlameItem is IFlameVariation Var)
                {
                    if (CurrentFunction is null)
                    {
                        M = new DoubleMatrix(new double[,] { { 1, 0 }, { 0, 1 } });
                        CurrentFunction = new FlameFunction(M, this);
                        FlameFunctions.Add(CurrentFunction);
                    }

                    CurrentFunction.Add(Var);
                }
                else if (FlameItem is ILambdaExpression Lambda)
                {
                    if (CurrentFunction is null)
                    {
                        M = new DoubleMatrix(new double[,] { { 1, 0 }, { 0, 1 } });
                        CurrentFunction = new FlameFunction(M, this);
                        FlameFunctions.Add(CurrentFunction);
                    }

                    CurrentFunction.Add(Lambda);
                }
                else
                {
                    try
                    {
                        Weight = Expression.ToDouble(FlameItem);

                        if (CurrentFunction is null)
                        {
                            M = new DoubleMatrix(new double[,] { { 1, 0 }, { 0, 1 } });
                            CurrentFunction = new FlameFunction(M, this);
                            FlameFunctions.Add(CurrentFunction);
                        }

                        CurrentFunction.SetWeight(Weight);
                    }
                    catch (Exception)
                    {
                        throw new ScriptRuntimeException("Invalid flame variation definition.", this);
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

				Variables.ConsoleOut?.WriteLine("Seed = " + Seed.ToString(), Variables);
			}

			if (i < c)
            {
                throw new ScriptRuntimeException("Parameter mismatch in call to EstimateFlameSize(N,FlameFunctions[dimx[,dimy[,Seed]]]).",
                    this);
            }

            if (dimx <= 0 || dimx > 5000 || dimy <= 0 || dimy > 5000)
                throw new ScriptRuntimeException("Image size must be within 1x1 to 5000x5000", this);

            FlameFunction[] Functions = FlameFunctions.ToArray();

            FlameFractalRgba.EstimateSize(out double xc, out double yc, out double dr, Functions, dimx, dimy, Seed, N, Variables, this);

			return new DoubleVector(xc, yc, dr);
        }

		private static readonly Random gen = new Random();

	}
}