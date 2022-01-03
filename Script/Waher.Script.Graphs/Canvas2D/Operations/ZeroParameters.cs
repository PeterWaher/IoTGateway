using System;
using SkiaSharp;
using System.Xml;
using System.Threading.Tasks;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Abstract base class for operations lacking parameters.
	/// </summary>
	public abstract class ZeroParameters : CanvasOperation
	{
		/// <summary>
		/// Abstract base class for operations lacking parameters.
		/// </summary>
		public ZeroParameters()
		{
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is ZeroParameters);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return 0;
		}

		/// <inheritdoc/>
		public override void ExportGraph(XmlWriter Output)
		{
		}

		/// <inheritdoc/>
		public override Task ImportGraph(XmlElement Xml, Variables _)
		{
			return Task.CompletedTask;
		}
	}
}
