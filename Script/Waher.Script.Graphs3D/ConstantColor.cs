using System;
using System.Numerics;
using SkiaSharp;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// Shader returning a constant color.
	/// </summary>
	class ConstantColor : I3DShader
	{
		private readonly SKColor color;

		/// <summary>
		/// Shader returning a constant color.
		/// </summary>
		/// <param name="Color">Color</param>
		public ConstantColor(SKColor Color)
		{
			this.color = Color;
		}

		/// <summary>
		/// Constant color.
		/// </summary>
		public SKColor Color => this.color;

		/// <summary>
		/// Gets a color for a position.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Z">Z-coordinate.</param>
		/// <param name="Normal">Surface normal vector.</param>
		/// <param name="Canvas">Current canvas.</param>
		/// <returns>Color</returns>
		public SKColor GetColor(float X, float Y, float Z, Vector3 Normal, Canvas3D Canvas)
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
		/// <param name="Canvas">Current canvas.</param>
		public void GetColors(float[] X, float[] Y, float[] Z, Vector3[] Normals, int N,
			SKColor[] Colors, Canvas3D Canvas)
		{
			int i;

			for (i = 0; i < N; i++)
				Colors[i] = this.color;
		}

		/// <summary>
		/// If shader is 100% opaque.
		/// </summary>
		public bool Opaque => this.color.Alpha == 255;

		/// <summary>
		/// Exports shader specifics to script.
		/// </summary>
		/// <returns>Exports the shader to parsable script.</returns>
		public string ToScript()
		{
			return "ConstantColor(" + Expression.ToString(this.color) + ")";
		}
	}
}
