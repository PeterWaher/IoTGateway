using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs3D.Functions.Shading
{
	/// <summary>
	/// Generates a <see cref="Graphs3D.PhongLightSource"/> object.
	/// </summary>
	public class PhongLightSource : FunctionMultiVariate
	{
		/// <summary>
		/// Generates a <see cref="Graphs3D.PhongIntensity"/> object.
		/// </summary>
		/// <param name="Diffuse">Diffuse light.</param>
		/// <param name="Specular">Specular light.</param>
		/// <param name="Position">Light source position.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public PhongLightSource(ScriptNode Diffuse, ScriptNode Specular, ScriptNode Position, 
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Diffuse, Specular, Position }, 
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "PhongLightSource";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[]
		{
			"Diffuse",
			"Specular",
			"Position"
		};

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			Graphs3D.PhongIntensity Diffuse = Graphs3D.Canvas3D.ToPhongIntensity(Arguments[0].AssociatedObjectValue);
			Graphs3D.PhongIntensity Specular = Graphs3D.Canvas3D.ToPhongIntensity(Arguments[1].AssociatedObjectValue);
			Vector3 Position = Graphs3D.Canvas3D.ToVector3(Arguments[2].AssociatedObjectValue);

			return new ObjectValue(new Graphs3D.PhongLightSource(Diffuse, Specular, Position));
		}
	}
}
