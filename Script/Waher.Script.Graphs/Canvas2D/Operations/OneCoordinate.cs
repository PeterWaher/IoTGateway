using System;
using SkiaSharp;
using System.Xml;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Abstract base class for operations using one coordinate
	/// </summary>
	public abstract class OneCoordinate : CanvasOperation
	{
		private float x;
		private float y;

		/// <summary>
		/// Abstract base class for operations using one coordinate
		/// </summary>
		public OneCoordinate()
		{
		}

		/// <summary>
		/// Abstract base class for operations using one coordinate
		/// </summary>
		/// <param name="X">X-coordinate.</param>
		/// <param name="Y">Y-coordinate.</param>
		public OneCoordinate(float X, float Y)
		{
			this.x = X;
			this.y = Y;
		}

		/// <summary>
		/// X-coordinate
		/// </summary>
		public float X => this.x;

		/// <summary>
		/// Y-coordinate
		/// </summary>
		public float Y => this.y;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is OneCoordinate Obj &&
				this.x == Obj.x &&
				this.y == Obj.y);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.x.GetHashCode();
			Result ^= Result << 5 ^ this.y.GetHashCode();
			return Result;
		}

		/// <inheritdoc/>
		public override void ExportGraph(XmlWriter Output)
		{
			Output.WriteAttributeString("x", Expression.ToString(this.x));
			Output.WriteAttributeString("y", Expression.ToString(this.y));
		}

		/// <inheritdoc/>
		public override void ImportGraph(XmlElement Xml, Variables _)
		{
			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "x":
						if (Expression.TryParse(Attr.Value, out float Value))
							this.x = Value;
						break;

					case "y":
						if (Expression.TryParse(Attr.Value, out Value))
							this.y = Value;
						break;
				}
			}
		}
	}
}
