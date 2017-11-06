using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SkiaSharp;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// Shader returning a constant color.
	/// </summary>
	class ConstantColor : I3DShader
	{
		private SKColor color;

		/// <summary>
		/// Shader returning a constant color.
		/// </summary>
		/// <param name="Color">Color</param>
		public ConstantColor(SKColor Color)
		{
			this.color = Color;
		}

		/// <summary>
		/// Gets a color for a position.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Z">Z-coordinate.</param>
		/// <param name="Normal">Surface normal vector.</param>
		/// <returns>Color</returns>
		public SKColor GetColor(float X, float Y, float Z, Vector3 Normal)
		{
			return this.color;
		}

		/// <summary>
		/// Gets an array of colors.
		/// </summary>
		/// <param name="X">X-coordinates.</param>
		/// <param name="Y">Y-coordinates.</param>
		/// <param name="Z">Z-coordinates.</param>
		/// <param name="Normals">Normal vectors.</param>
		/// <param name="N">Number of coordinates.</param>
		/// <param name="Colors">Where color values will be stored.</param>
		public void GetColors(float[] x, float[] y, float[] z, Vector3[] Normals, int N, SKColor[] Colors)
		{
			int i;

			for (i = 0; i < N; i++)
				Colors[i] = this.color;
		}
	}
}
