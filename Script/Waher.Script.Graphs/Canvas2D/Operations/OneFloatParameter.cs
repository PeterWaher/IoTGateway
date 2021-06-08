using System;
using SkiaSharp;
using System.Xml;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Abstract base class for operations using one floating point parameter.
	/// </summary>
	public abstract class OneFloatParameter : CanvasOperation
	{
		private float parameter;

		/// <summary>
		/// Abstract base class for operations using one floating point parameter.
		/// </summary>
		public OneFloatParameter()
		{
		}

		/// <summary>
		/// Abstract base class for operations using one floating point parameter.
		/// </summary>
		/// <param name="Parameter">Parameter.</param>
		public OneFloatParameter(float Parameter)
		{
			this.parameter = Parameter;
		}

		/// <summary>
		/// Parameter
		/// </summary>
		public float Parameter => this.parameter;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is OneFloatParameter Obj &&
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
		public override void ImportGraph(XmlElement Xml, Variables _)
		{
			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "p":
						if (Expression.TryParse(Attr.Value, out float Value))
							this.parameter = Value;
						break;
				}
			}
		}
	}
}
