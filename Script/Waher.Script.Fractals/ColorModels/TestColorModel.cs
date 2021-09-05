using System;
using SkiaSharp;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Fractals.ColorModels
{
    /// <summary>
    /// Returns an image displaying the colors in the array sent to the function.
    /// </summary>
    /// <example>
    /// testcolormodel(randomlinearrgb(300,16,1))
    /// </example>
    public class TestColorModel : FunctionOneVectorVariable
    {
		/// <summary>
		/// Returns an image displaying the colors in the array sent to the function.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public TestColorModel(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateVector(IVector Argument, Variables Variables)
		{
			int Width = 800;
			int i, c = Argument.Dimension;
            object Obj;
            SKColor cl;
            int j;

			using (SKSurface Surface = SKSurface.Create(new SKImageInfo(Width, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul)))
			{
				SKCanvas Canvas = Surface.Canvas;

				for (i = 0; i < Width; i++)
                {
                    j = i * c / Width;
                    if (j >= c)
                        j = c - 1;

					Obj = Argument.GetElement(j).AssociatedObjectValue;
                    if (Obj is SKColor || Obj is string)
                    {
                        cl = Graph.ToColor(Obj);

						SKPaint Pen = new SKPaint()
						{
							Style = SKPaintStyle.Stroke,
							Color = cl,
							StrokeWidth = 1
						};

                        Canvas.DrawLine(i, 0, i, 100, Pen);

                        Pen.Dispose();
                        Pen = null;
                    }
                }

				using (SKImage Result = Surface.Snapshot())
				{
					return new GraphBitmap(PixelInformation.FromImage(Result));
				}
			}
		}

        public override string FunctionName
        {
            get { return "TestColorModel"; }
        }
    }
}
