using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// Contains information about a material, as used in the Phong reflection model.
	/// https://en.wikipedia.org/wiki/Phong_reflection_model
	/// </summary>
	public class PhongMaterial
	{
		private float ambientReflectionConstant;
		private float diffuseReflectionConstant;
		private float specularReflectionConstant;
		private float shininess;

		/// <summary>
		/// Contains information about a material, as used in the Phong reflection model.
		/// https://en.wikipedia.org/wiki/Phong_reflection_model
		/// </summary>
		/// <param name="AmbientReflectionConstant">Ratio of reflection of the ambient term present in all points in the scene rendered.</param>
		/// <param name="DiffuseReflectionConstant">Ratio of reflection of the diffuse term of incoming light.</param>
		/// <param name="SpecularReflectionConstant">Ratio of reflection of the specular term of incoming light.</param>
		/// <param name="Shininess">Larger for surfaces that are smoother and more mirror-like.</param>
		public PhongMaterial(float AmbientReflectionConstant,
			float DiffuseReflectionConstant, float SpecularReflectionConstant,
			float Shininess)
		{
			this.ambientReflectionConstant = AmbientReflectionConstant;
			this.diffuseReflectionConstant = DiffuseReflectionConstant;
			this.specularReflectionConstant = SpecularReflectionConstant;
			this.shininess = Shininess;
		}

		/// <summary>
		/// Ratio of reflection of the ambient term present in all points in the scene rendered.
		/// </summary>
		public float AmbientReflectionConstant => this.ambientReflectionConstant;

		/// <summary>
		/// Ratio of reflection of the diffuse term of incoming light.
		/// </summary>
		public float DiffuseReflectionConstant => this.diffuseReflectionConstant;

		/// <summary>
		/// Ratio of reflection of the specular term of incoming light.
		/// </summary>
		public float SpecularReflectionConstant => this.specularReflectionConstant;

		/// <summary>
		/// Larger for surfaces that are smoother and more mirror-like.
		/// </summary>
		public float Shininess => this.shininess;
	}
}
