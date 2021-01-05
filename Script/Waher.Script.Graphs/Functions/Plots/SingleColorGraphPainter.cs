using System;
using SkiaSharp;

namespace Waher.Script.Graphs.Functions.Plots
{
	/// <summary>
	/// Painters for single-color graphs
	/// </summary>
	public abstract class SingleColorGraphPainter : IPainter
	{
		/// <summary>
		/// If graph uses default color
		/// </summary>
		/// <param name="Parameters">Graph-specific parameters.</param>
		public virtual bool UsesDefaultColor(object[] Parameters)
		{
			return (Parameters?.Length ?? 0) >= 1 &&
				Parameters[0] is SKColor Color &&
				Color.Red == Graph.DefaultColor.Red &&
				Color.Red == Graph.DefaultColor.Green &&
				Color.Red == Graph.DefaultColor.Blue;
		}

		/// <summary>
		/// Tries to set the default color.
		/// </summary>
		/// <param name="Color">Default color.</param>
		/// <param name="Parameters">Graph-specific parameters.</param>
		/// <returns>If possible to set.</returns>
		public virtual bool TrySetDefaultColor(SKColor Color, object[] Parameters)
		{
			if ((Parameters?.Length ?? 0) >= 1 && Parameters[0] is SKColor PrevColor)
			{
				if (PrevColor.Alpha == Color.Alpha)
					Parameters[0] = Color;
				else
					Parameters[0] = new SKColor(Color.Red, Color.Green, Color.Blue, PrevColor.Alpha);

				return true;
			}
			else
				return false;
		}
	}
}
