using System;
using System.Collections.Generic;
using System.Numerics;
using SkiaSharp;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// Interface for 3D shaders.
	/// </summary>
    public interface I3DShader
    {
		/// <summary>
		/// Gets a color for a position.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Z">Z-coordinate.</param>
		/// <param name="Normal">Surface normal vector.</param>
		/// <param name="Canvas">Current canvas.</param>
		/// <returns>Color</returns>
		SKColor GetColor(float X, float Y, float Z, Vector3 Normal, Canvas3D Canvas);

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
		void GetColors(float[] X, float[] Y, float[] Z, Vector3[] Normals, int N, SKColor[] Colors, Canvas3D Canvas);

		/// <summary>
		/// If shader is 100% opaque.
		/// </summary>
		bool Opaque
		{
			get;
		}
    }
}
