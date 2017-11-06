using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// Contains information about the intensity of a light component, as used in the Phong reflection model.
	/// https://en.wikipedia.org/wiki/Phong_reflection_model
	/// </summary>
	public class PhongIntensity
	{
		private float red;
		private float green;
		private float blue;

		/// <summary>
		/// Contains information about the intensity of a light component, as used in the Phong reflection model.
		/// https://en.wikipedia.org/wiki/Phong_reflection_model
		/// </summary>
		/// <param name="Red">Red intensity.</param>
		/// <param name="Green">Green intensity.</param>
		/// <param name="Blue">Blue intensity.</param>
		public PhongIntensity(float Ambient, float Diffuse, float Specular)
		{
			this.red = Ambient;
			this.green = Diffuse;
			this.blue = Specular;
		}

		/// <summary>
		/// Red intensity.
		/// </summary>
		public float Red => this.red;

		/// <summary>
		/// Green intensity.
		/// </summary>
		public float Green => this.green;

		/// <summary>
		/// Blue intensity.
		/// </summary>
		public float Blue => this.blue;
	}
}
