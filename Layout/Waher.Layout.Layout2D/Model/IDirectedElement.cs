using System;
using System.Xml;
using SkiaSharp;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Interface for directed elements.
	/// </summary>
	public interface IDirectedElement
	{
		/// <summary>
		/// Tries to get start position and initial direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Initial direction.</param>
		/// <returns>If a start position was found.</returns>
		bool TryGetStart(out float X, out float Y, out float Direction);

		/// <summary>
		/// Tries to get end position and terminating direction.
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		/// <param name="Direction">Terminating direction.</param>
		/// <returns>If a terminating position was found.</returns>
		bool TryGetEnd(out float X, out float Y, out float Direction);
	}
}
