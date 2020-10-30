using SkiaSharp;
using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs3D.Functions
{
	/// <summary>
	/// Generates a <see cref="Graphs3D.Canvas3D"/> object.
	/// </summary>
	public class Canvas3D : FunctionMultiVariate
	{
		/// <summary>
		/// Generates a <see cref="Graphs3D.Canvas3D"/> object.
		/// </summary>
		/// <param name="Width">Width of canvas.</param>
		/// <param name="Height">Height of canvas.</param>
		/// <param name="Oversampling">Oversampling.</param>
		/// <param name="BackgroupColor">Background color.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public Canvas3D(ScriptNode Width, ScriptNode Height, ScriptNode Oversampling,
			ScriptNode BackgroupColor, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Width, Height, Oversampling, BackgroupColor },
				  argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "Canvas3D";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[]
		{
			"Width",
			"Height",
			"Oversampling",
			"BackgroundColor"
		};

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int Width = (int)Expression.ToDouble(Arguments[0].AssociatedObjectValue);
			int Height = (int)Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			int Oversampling = (int)Expression.ToDouble(Arguments[2].AssociatedObjectValue);
			SKColor BackgroundColor = Graphs.Graph.ToColor(Arguments[3].AssociatedObjectValue);

			if (Width <= 0)
				throw new ScriptRuntimeException("Invalid width.", this);

			if (Height <= 0)
				throw new ScriptRuntimeException("Invalid height.", this);

			if (Oversampling <= 0)
				throw new ScriptRuntimeException("Invalid oversampling.", this);

			return new Graphs3D.Canvas3D(Width, Height, Oversampling, BackgroundColor);
		}
	}
}
