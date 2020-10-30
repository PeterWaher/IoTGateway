using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Graphs3D.Functions.Shading
{
	/// <summary>
	/// Generates a <see cref="Graphs3D.PhongShader"/> object.
	/// </summary>
	public class PhongShader : FunctionMultiVariate
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
		public PhongShader(ScriptNode Diffuse, ScriptNode Specular, ScriptNode Position, 
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Diffuse, Specular, Position }, 
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "PhongShader";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[]
		{
			"Material",
			"Ambient",
			"LightSources"
		};

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			Graphs3D.PhongIntensity Ambient = Graphs3D.Canvas3D.ToPhongIntensity(Arguments[1].AssociatedObjectValue);
			Graphs3D.PhongLightSource[] LightSources;
			object Obj;

			if (!(Arguments[0].AssociatedObjectValue is Graphs3D.PhongMaterial Material))
				throw new ScriptRuntimeException("Phong material expected in first argument.", this);

			Obj = Arguments[2].AssociatedObjectValue;
			if (Obj is Graphs3D.PhongLightSource Source)
				LightSources = new Graphs3D.PhongLightSource[] { Source };
			else if (Obj is IVector V)
			{
				List<Graphs3D.PhongLightSource> Sources = new List<Graphs3D.PhongLightSource>();

				foreach (IElement E in V.VectorElements)
				{
					if (E.AssociatedObjectValue is Graphs3D.PhongLightSource Source2)
						Sources.Add(Source2);
					else
						throw new ScriptRuntimeException("Expected one or more light sources for the third argument.", this);
				}

				LightSources = Sources.ToArray();
			}
			else
				throw new ScriptRuntimeException("Expected one or more light sources for the third argument.", this);

			return new ObjectValue(new Graphs3D.PhongShader(Material, Ambient, LightSources));
		}
	}
}
