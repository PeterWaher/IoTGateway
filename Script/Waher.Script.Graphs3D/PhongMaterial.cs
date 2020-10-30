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
		private readonly float ambientReflectionConstantFront;
		private readonly float diffuseReflectionConstantFront;
		private readonly float specularReflectionConstantFront;
		private readonly float shininessFront;
		private readonly float ambientReflectionConstantBack;
		private readonly float diffuseReflectionConstantBack;
		private readonly float specularReflectionConstantBack;
		private readonly float shininessBack;

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
			: this(AmbientReflectionConstant, DiffuseReflectionConstant, SpecularReflectionConstant, Shininess,
				  AmbientReflectionConstant, DiffuseReflectionConstant, SpecularReflectionConstant, Shininess)
		{
		}

		/// <summary>
		/// Contains information about a material, as used in the Phong reflection model.
		/// https://en.wikipedia.org/wiki/Phong_reflection_model
		/// </summary>
		/// <param name="AmbientReflectionConstantFront">Front side ratio of reflection of the ambient term present in all points in the scene rendered.</param>
		/// <param name="DiffuseReflectionConstantFront">Front side ratio of reflection of the diffuse term of incoming light.</param>
		/// <param name="SpecularReflectionConstantFront">Front side ratio of reflection of the specular term of incoming light.</param>
		/// <param name="ShininessFront">Front side shininess coefficient.</param>
		/// <param name="AmbientReflectionConstantBack">Back side ratio of reflection of the ambient term present in all points in the scene rendered.</param>
		/// <param name="DiffuseReflectionConstantBack">Back side ratio of reflection of the diffuse term of incoming light.</param>
		/// <param name="SpecularReflectionConstantBack">Back side ratio of reflection of the specular term of incoming light.</param>
		/// <param name="ShininessBack">Back side shininess coefficient.</param>
		public PhongMaterial(float AmbientReflectionConstantFront,
			float DiffuseReflectionConstantFront, float SpecularReflectionConstantFront,
			float ShininessFront, float AmbientReflectionConstantBack,
			float DiffuseReflectionConstantBack, float SpecularReflectionConstantBack,
			float ShininessBack)
		{
			this.ambientReflectionConstantFront = AmbientReflectionConstantFront;
			this.diffuseReflectionConstantFront = DiffuseReflectionConstantFront;
			this.specularReflectionConstantFront = SpecularReflectionConstantFront;
			this.shininessFront = ShininessFront;
			this.ambientReflectionConstantBack = AmbientReflectionConstantBack;
			this.diffuseReflectionConstantBack = DiffuseReflectionConstantBack;
			this.specularReflectionConstantBack = SpecularReflectionConstantBack;
			this.shininessBack = ShininessBack;
		}

		/// <summary>
		/// Front-side ratio of reflection of the ambient term present in all points in the scene rendered.
		/// </summary>
		public float AmbientReflectionConstantFront => this.ambientReflectionConstantFront;

		/// <summary>
		/// Front-side ratio of reflection of the diffuse term of incoming light.
		/// </summary>
		public float DiffuseReflectionConstantFront => this.diffuseReflectionConstantFront;

		/// <summary>
		/// Front-side ratio of reflection of the specular term of incoming light.
		/// </summary>
		public float SpecularReflectionConstantFront => this.specularReflectionConstantFront;

		/// <summary>
		/// Front-side shininess coefficient.
		/// </summary>
		public float ShininessFront => this.shininessFront;

		/// <summary>
		/// Back-side ratio of reflection of the ambient term present in all points in the scene rendered.
		/// </summary>
		public float AmbientReflectionConstantBack => this.ambientReflectionConstantBack;

		/// <summary>
		/// Back-side ratio of reflection of the diffuse term of incoming light.
		/// </summary>
		public float DiffuseReflectionConstantBack => this.diffuseReflectionConstantBack;

		/// <summary>
		/// Back-side ratio of reflection of the specular term of incoming light.
		/// </summary>
		public float SpecularReflectionConstantBack => this.specularReflectionConstantBack;

		/// <summary>
		/// Back-side shininess coefficient.
		/// </summary>
		public float ShininessBack => this.shininessBack;
	}
}
