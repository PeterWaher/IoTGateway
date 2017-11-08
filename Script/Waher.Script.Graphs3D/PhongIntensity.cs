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
		private float alpha;

		/// <summary>
		/// Contains information about the intensity of a light component, as used in the Phong reflection model.
		/// https://en.wikipedia.org/wiki/Phong_reflection_model
		/// </summary>
		/// <param name="Red">Red intensity.</param>
		/// <param name="Green">Green intensity.</param>
		/// <param name="Blue">Blue intensity.</param>
		/// <param name="Alpha">Alpha intensity.</param>
		public PhongIntensity(float Red, float Green, float Blue, float Alpha)
		{
			this.red = Red;
			this.green = Green;
			this.blue = Blue;
			this.alpha = Alpha;
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

		/// <summary>
		/// Alpha intensity.
		/// </summary>
		public float Alpha => this.alpha;
	}
}
