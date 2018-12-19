using System;
using System.Numerics;
using System.Text;
using SkiaSharp;
using Waher.Script.Exceptions;
using Waher.Script.Functions;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Fractals.ColorModels
{
	/// <summary>
	/// Calculates a palette of cycling and blending colors into bands.
	/// 
	/// LinearColors(Colors)                   N = 1024 by default
	/// LinearColors(Colors,N)                 BandSize=16 by default
	/// LinearColors(Colors,N,BandSize)
	/// </summary>
	/// <example>
	/// TestColorModel(LinearColors(["Red","Green","Blue"],1024,64))
	/// </example>
	public class LinearColors : FunctionMultiVariate
    {
		public LinearColors(ScriptNode Colors, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Colors }, new ArgumentType[] { ArgumentType.Vector }, Start, Length, Expression)
		{
		}

		public LinearColors(ScriptNode Colors, ScriptNode N, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Colors, N },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		public LinearColors(ScriptNode Colors, ScriptNode N, ScriptNode BandSize, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Colors, N, BandSize }, 
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
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
            SKColor[] Colors;
            int i = 0;
            int c = Arguments.Length;
            int N;
            int BandSize;

            if (i < c)
            {
                object Obj = Arguments[i++].AssociatedObjectValue;

                Colors = Obj as SKColor[];
                if (Colors is null)
                {
                    Array a = Obj as Array;
                    if (a is null)
                        throw new ScriptRuntimeException("A fixed set of colors needs to be provided in calls to LinearColors().", this);

                    int d = a.GetLength(0);
                    int j;

                    Colors = new SKColor[d];
                    for (j = 0; j < d; j++)
                    {
                        Obj = a.GetValue(j);
                        Colors[j] = Graph.ToColor(Obj);
                    }
                }
            }
            else
                throw new ScriptRuntimeException("A fixed set of colors needs to be provided in calls to LinearColors().", this);

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

			return new ObjectVector(CreatePalette(N, BandSize, Colors, this));
        }

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "Colors", "N", "BandSize" };
			}
		}

		/// <summary>
		/// Calculates a palette of cycling and blending colors into bands.
		/// </summary>
		/// <param name="N">Number of colors to generate.</param>
		/// <param name="BandSize">Size of each color band.</param>
		/// <param name="Colors">Fixed set of key frame colors.</param>
		/// <param name="Node">Node generating the palette.</param>
		/// <returns>Color palette.</returns>
		public static SKColor[] CreatePalette(int N, int BandSize, SKColor[] Colors, ScriptNode Node)
        {
            if (N <= 0)
                throw new ScriptRuntimeException("N in LinearColors(Colors,N[,BandSize]) has to be positive.", Node);

            if (BandSize <= 0)
                throw new ScriptRuntimeException("BandSize in LinearColors(Colors,N[,BandSize]) has to be positive.", Node);

            SKColor[] Result = new SKColor[N];
            SKColor Color;
            int R1, G1, B1;
            int R2, G2, B2;
            int R, G, B;
            int i, j, c, d;
            int ColorIndex = 0;
            int NrColors = Colors.Length;
            int BandSize2 = BandSize / 2;

            Color = Colors[ColorIndex];
            ColorIndex = (ColorIndex + 1) % NrColors;

            R2 = Color.Red;
            G2 = Color.Green;
            B2 = Color.Blue;

            i = 0;
            while (i < N)
            {
                R1 = R2;
                G1 = G2;
                B1 = B2;

                Color = Colors[ColorIndex];
                ColorIndex = (ColorIndex + 1) % NrColors;

                R2 = Color.Red;
                G2 = Color.Green;
                B2 = Color.Blue;

                c = BandSize;
                j = N - i;
                if (c > j)
                    c = j;

                d = N - i;
                if (d > c)
                    d = c;

                for (j = 0; j < d; j++)
                {
                    R = ((R2 * j) + (R1 * (BandSize - j)) + BandSize2) / BandSize;
                    G = ((G2 * j) + (G1 * (BandSize - j)) + BandSize2) / BandSize;
                    B = ((B2 * j) + (B1 * (BandSize - j)) + BandSize2) / BandSize;

                    if (R > 255)
                        R = 255;

                    if (G > 255)
                        G = 255;

                    if (B > 255)
                        B = 255;

                    Result[i++] = new SKColor((byte)R, (byte)G, (byte)B);
                }
            }

            return Result;
        }

        private static Random gen = new Random();

        public override string FunctionName
        {
            get { return "LinearColors"; }
        }

    }
}
