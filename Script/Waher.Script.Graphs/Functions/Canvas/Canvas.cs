using System;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Graphs.Canvas2D;

namespace Waher.Script.Graphs.Functions.Canvas
{
	/// <summary>
	/// Creates a 2D Canvas for custom drawing.
	/// </summary>
	public class Canvas : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a 2D Canvas for custom drawing.
		/// </summary>
		/// <param name="Width">Width of canvas</param>
		/// <param name="Height">Height of canvas</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Canvas(ScriptNode Width, ScriptNode Height, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Width, Height }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a 2D Canvas for custom drawing.
		/// </summary>
		/// <param name="Width">Width of canvas</param>
		/// <param name="Height">Height of canvas</param>
		/// <param name="Color">Default color of graph.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Canvas(ScriptNode Width, ScriptNode Height, ScriptNode Color, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Width, Height, Color }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a 2D Canvas for custom drawing.
		/// </summary>
		/// <param name="Width">Width of canvas</param>
		/// <param name="Height">Height of canvas</param>
		/// <param name="Color">Default color of graph.</param>
		/// <param name="BgColor">Background color.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Canvas(ScriptNode Width, ScriptNode Height, ScriptNode Color, ScriptNode BgColor, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Width, Height, Color, BgColor }, argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "Canvas";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Width", "Height" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int c = Arguments.Length;
			int Width = (int)(Expression.ToDouble(Arguments[0].AssociatedObjectValue) + 0.5);
			int Height = (int)(Expression.ToDouble(Arguments[1].AssociatedObjectValue) + 0.5);
			SKColor? Color = c > 2 ? Graph.ToColor(Arguments[2].AssociatedObjectValue) : (SKColor?)null;
			SKColor? BgColor = c > 3 ? Graph.ToColor(Arguments[3].AssociatedObjectValue) : (SKColor?)null;

			return new CanvasGraph(Width, Height, Color, BgColor);
		}
	}
}
