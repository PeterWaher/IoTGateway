using System;
using SkiaSharp;

namespace Waher.Script.Graphs
{
	/// <summary>
	/// Interface for graph drawing functions.
	/// </summary>
	public interface IPainter
	{
		/// <summary>
		/// If graph uses default color
		/// </summary>
		/// <param name="Parameters">Graph-specific parameters.</param>
		bool UsesDefaultColor(object[] Parameters);

		/// <summary>
		/// Tries to set the default color.
		/// </summary>
		/// <param name="Color">Default color.</param>
		/// <param name="Parameters">Graph-specific parameters.</param>
		/// <returns>If possible to set.</returns>
		bool TrySetDefaultColor(SKColor Color, object[] Parameters);
	}
}
