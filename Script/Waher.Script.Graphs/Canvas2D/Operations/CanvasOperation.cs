using System;
using System.Xml;
using SkiaSharp;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Abstract base class for canvas operations.
	/// </summary>
	public abstract class CanvasOperation
	{
		/// <summary>
		/// Performs a drawing operation.
		/// </summary>
		/// <param name="Canvas">Current canvas.</param>
		/// <param name="State">Current state.</param>
		public abstract void Draw(SKCanvas Canvas, CanvasState State);

		/// <inheritdoc/>
		public abstract override bool Equals(object obj);

		/// <inheritdoc/>
		public abstract override int GetHashCode();

		/// <summary>
		/// Exports graph specifics to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public abstract void ExportGraph(XmlWriter Output);

		/// <summary>
		/// Imports graph specifics from XML.
		/// </summary>
		/// <param name="Xml">XML input.</param>
		/// <param name="Variables">Set of variables.</param>
		public abstract void ImportGraph(XmlElement Xml, Variables Variables);

	}
}
