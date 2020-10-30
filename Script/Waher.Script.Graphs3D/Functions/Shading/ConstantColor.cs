using SkiaSharp;
using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs3D.Functions.Shading
{
	/// <summary>
	/// Generates a <see cref="Graphs3D.ConstantColor"/> object.
	/// </summary>
	public class ConstantColor : FunctionMultiVariate
	{
		/// <summary>
		/// Generates a <see cref="Graphs3D.PhongIntensity"/> object.
		/// </summary>
		/// <param name="Color">Color tha will be converted to an intensity object.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ConstantColor(ScriptNode Color, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Color }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a <see cref="Graphs3D.ConstantColor"/> object.
		/// </summary>
		/// <param name="Red">Ratio of reflection of the ambient term present in all points in the scene rendered.</param>
		/// <param name="Green">Ratio of reflection of the diffuse term of incoming light.</param>
		/// <param name="Blue">Ratio of reflection of the specular term of incoming light.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ConstantColor(ScriptNode Red, ScriptNode Green, ScriptNode Blue,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Red, Green, Blue },
				  argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Generates a <see cref="Graphs3D.ConstantColor"/> object.
		/// </summary>
		/// <param name="Red">Ratio of reflection of the ambient term present in all points in the scene rendered.</param>
		/// <param name="Green">Ratio of reflection of the diffuse term of incoming light.</param>
		/// <param name="Blue">Ratio of reflection of the specular term of incoming light.</param>
		/// <param name="Alpha">Larger for surfaces that are smoother and more mirror-like.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ConstantColor(ScriptNode Red, ScriptNode Green, ScriptNode Blue,
			ScriptNode Alpha, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Red, Green, Blue, Alpha },
				  argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "ConstantColor";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[]
		{
			"Color"
		};

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			switch (Arguments.Length)
			{
				case 1:
					SKColor Color = Graphs.Graph.ToColor(Arguments[0].AssociatedObjectValue);
					return new ObjectValue(new Graphs3D.ConstantColor(Color));

				case 3:
					return new ObjectValue(new Graphs3D.ConstantColor(new SKColor(
						(byte)Expression.ToDouble(Arguments[0].AssociatedObjectValue),
						(byte)Expression.ToDouble(Arguments[1].AssociatedObjectValue),
						(byte)Expression.ToDouble(Arguments[2].AssociatedObjectValue),
						255)));

				case 4:
					return new ObjectValue(new Graphs3D.ConstantColor(new SKColor(
						(byte)Expression.ToDouble(Arguments[0].AssociatedObjectValue),
						(byte)Expression.ToDouble(Arguments[1].AssociatedObjectValue),
						(byte)Expression.ToDouble(Arguments[2].AssociatedObjectValue),
						(byte)Expression.ToDouble(Arguments[3].AssociatedObjectValue))));

				default:
					throw new ScriptRuntimeException("Argument number mismatch.", this);
			}
		}
	}
}
