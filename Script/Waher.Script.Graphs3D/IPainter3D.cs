using System;
using System.Numerics;
using Waher.Script.Graphs;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// Interface for 3D graph drawing functions.
	/// </summary>
	public interface IPainter3D : IPainter
	{
		/// <summary>
		/// Draws the graph on a canvas.
		/// </summary>
		/// <param name="Canvas">Canvas to draw on.</param>
		/// <param name="Points">Points to draw.</param>
		/// <param name="Normals">Optional normals.</param>
		/// <param name="Parameters">Graph-specific parameters.</param>
		/// <param name="PrevPoints">Points of previous graph of same type (if available), null (if not available).</param>
		/// <param name="PrevNormals">Optional normals of previous graph of same type (if available), null (if not available).</param>
		/// <param name="PrevParameters">Parameters of previous graph of same type (if available), null (if not available).</param>
		/// <param name="DrawingVolume">Current drawing volume.</param>
		void DrawGraph(Canvas3D Canvas, Vector4[,] Points,
			Vector4[,] Normals, object[] Parameters, Vector4[,] PrevPoints,
			Vector4[,] PrevNormals, object[] PrevParameters, DrawingVolume DrawingVolume);
	}
}
