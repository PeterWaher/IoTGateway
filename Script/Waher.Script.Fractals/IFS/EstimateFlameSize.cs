using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Fractals.IFS
{
    /// <summary>
    /// Estimates the size of a flame fractal.
    /// </summary>
    /// <example>
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

		public override string FunctionName
        {
            get { return "EstimateFlameSize"; }
        }

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

            object Obj = Arguments[i].AssociatedObjectValue;
            string FunctionsExpression = this.Arguments[i++].SubExpression;

			if (!(Obj is Array FlameArray))
				throw new ScriptRuntimeException("the second parameter to EstimateFlameSize must be an array, containing flame definitions.", this);

			List<FlameFunction> FlameFunctions = new List<FlameFunction>();
            DoubleMatrix M;
            double Weight;
            FlameFunction CurrentFunction = null;

            foreach (object FlameItem in FlameArray)
            {
                if (FlameItem is DoubleMatrix)
                {
                    M = (DoubleMatrix)FlameItem;
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
                }
                else if (FlameItem is IFlameVariation)
                {
                    if (CurrentFunction is null)
                    {
                        M = new DoubleMatrix(new double[,] { { 1, 0 }, { 0, 1 } });
                        CurrentFunction = new FlameFunction(M, this);
                        FlameFunctions.Add(CurrentFunction);
                    }

                    CurrentFunction.Add((IFlameVariation)FlameItem);
                }
                else if (FlameItem is ILambdaExpression)
                {
                    if (CurrentFunction is null)
                    {
                        M = new DoubleMatrix(new double[,] { { 1, 0 }, { 0, 1 } });
                        CurrentFunction = new FlameFunction(M, this);
                        FlameFunctions.Add(CurrentFunction);
                    }

                    CurrentFunction.Add((ILambdaExpression)FlameItem);
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

				Variables.ConsoleOut.WriteLine("Seed = " + Seed.ToString(), Variables);
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

		private static Random gen = new Random();

	}
}