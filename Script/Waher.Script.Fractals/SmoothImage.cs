using System;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Graphs;
using Waher.Script.Model;

namespace Waher.Script.Fractals
{
	/// <summary>
	/// Creates a smooth image from an image source.
	/// </summary>
	public class SmoothImage : FunctionOneScalarVariable
	{
		/// <summary>
		/// Creates a smooth image from an image source.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SmoothImage(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "SmoothImage";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Image" };

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			PixelInformation Pixels;

			if (Argument is Graph Graph)
				Pixels = Graph.CreatePixels(Variables);
			else if (Argument is SKImage Image)
				Pixels = PixelInformation.FromImage(Image);
			else
				throw new ScriptRuntimeException("Expected an image or a graph.", this);

			int Width = Pixels.Width;
			int Height = Pixels.Height;
			PixelInformationRaw Raw = Pixels.GetRaw();
			byte[] Bin = (byte[])Raw.Binary.Clone();
			int i, j, c = Bin.Length / 4;
			double[] R = new double[c];
			double[] G = new double[c];
			double[] B = new double[c];
			double[] A = new double[c];

			for (i = j = 0; i < c; i++)
			{
				R[i] = Bin[j++];
				G[i] = Bin[j++];
				B[i] = Bin[j++];
				A[i] = Bin[j++];
			}

			(double[] BoundaryR, double[] BoundaryG, double[] BoundaryB, double[] BoundaryA) =
				FractalGraph.FindBoundaries(R, G, B, A, Width, Height);

			FractalGraph.Smooth(R, G, B, A, BoundaryR, BoundaryG, BoundaryB, BoundaryA, Width, Height, this, Variables);

			return new GraphBitmap(FractalGraph.ToPixels(R, G, B, A, Width, Height));
		}
	}
}
