using System;
using SkiaSharp;
using System.Xml;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Abstract base class for operations using one color parameter.
	/// </summary>
	public abstract class OneColorParameter : CanvasOperation
	{
		private SKColor parameter;

		/// <summary>
		/// Abstract base class for operations using one color parameter.
		/// </summary>
		public OneColorParameter()
		{
		}

		/// <summary>
		/// Abstract base class for operations using one color parameter.
		/// </summary>
		/// <param name="Parameter">Parameter.</param>
		public OneColorParameter(SKColor Parameter)
		{
			this.parameter = Parameter;
		}

		/// <summary>
		/// Parameter
		/// </summary>
		public SKColor Parameter => this.parameter;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is OneColorParameter Obj &&
				this.parameter == Obj.parameter);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.parameter.GetHashCode();
		}

		/// <inheritdoc/>
		public override void ExportGraph(XmlWriter Output)
		{
			Output.WriteAttributeString("p", Expression.ToString(this.parameter));
		}

		/// <inheritdoc/>
		public override void ImportGraph(XmlElement Xml, Variables Variables)
		{
			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "p":
						this.parameter = Graph.ToColor(Graph.Parse(Attr.Value, Variables));
						break;
				}
			}
		}
	}
}
