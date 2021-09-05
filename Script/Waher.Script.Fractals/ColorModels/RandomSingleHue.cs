using System;
using SkiaSharp;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Fractals.ColorModels
{
    /// <summary>
    /// Calculates a palette of random color from a single random hue, oscillating
    /// the value, and keeping the saturation constant.
    ///
    /// RandomSingleHue()                  N = 1024 by default
    /// RandomSingleHue(N)                 BandSize=16 by default
    /// RandomSingleHue(N,BandSize)        Seed=random by default.
    /// RandomSingleHue(N,BandSize,Seed)
    /// </summary>
    public class RandomSingleHue : FunctionMultiVariate
    {
		public RandomSingleHue(ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { N }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		public RandomSingleHue(ScriptNode N, ScriptNode BandSize, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { N, BandSize }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		public RandomSingleHue(ScriptNode N, ScriptNode BandSize, ScriptNode Seed, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { N, BandSize, Seed }, argumentTypes3Scalar, Start, Length, Expression)
        {
        }

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int i = 0;
            int c = Arguments.Length;
            int N;
            int BandSize;
            int Seed;

            if (i < c)
                N = (int)Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
            else
            {
                N = 1024;
                Variables.ConsoleOut.WriteLine("N = " + N.ToString(), Variables);
            }

            if (i < c)
                BandSize = (int)Expression.ToDouble(Arguments[i++].AssociatedObjectValue);
            else
            {
                BandSize = 16;
                Variables.ConsoleOut.WriteLine("BandSize = " + BandSize.ToString(), Variables);
            }

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

            return new ObjectVector(CreatePalette(N, BandSize, Seed, this));
        }

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "N", "BandSize", "Seed" };
			}
		}

		public static SKColor[] CreatePalette(int N, int BandSize, ScriptNode Node)
        {
            return CreatePalette(N, BandSize, null, Node);
        }

        public static SKColor[] CreatePalette(int N, int BandSize, int? Seed, ScriptNode Node)
        {
            if (N <= 0)
                throw new ScriptRuntimeException("N in RandomSingleHue(N[,BandSize]) has to be positive.", Node);

            if (BandSize <= 0)
                throw new ScriptRuntimeException("BandSize in RandomSingleHue(N[,BandSize]) has to be positive.", Node);

            SKColor[] Result = new SKColor[N];
            double H, S, V;
            Random Generator;
            int i;

            if (Seed.HasValue)
                Generator = new Random(Seed.Value);
            else
                Generator = gen;

            lock (Generator)
            {
                H = Generator.NextDouble() * 360;
                S = 1.0;
                
                i = 0;
                while (i < N)
                {
                    S = V = Math.Cos((2.0 * Math.PI * i) / BandSize + Math.PI / 2) * 0.5 + 0.5;
					Result[i++] = Graph.ToColorHSV(H, S, V);
                }
            }

            return Result;
        }

        private static readonly Random gen = new Random();

        public override string FunctionName
        {
            get { return "RandomSingleHue"; }
        }
    }
}
