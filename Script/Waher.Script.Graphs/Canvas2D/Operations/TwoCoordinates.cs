using System;
using SkiaSharp;
using System.Xml;

namespace Waher.Script.Graphs.Canvas2D.Operations
{
	/// <summary>
	/// Abstract base class for operations using two coordinates
	/// </summary>
	public abstract class TwoCoordinates : OneCoordinate
	{
		private float x2;
		private float y2;

		/// <summary>
		/// Abstract base class for operations using two coordinates
		/// </summary>
		public TwoCoordinates()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for operations using two coordinates
		/// </summary>
		/// <param name="X1">First X-coordinate.</param>
		/// <param name="Y1">First Y-coordinate.</param>
		/// <param name="X2">Second X-coordinate.</param>
		/// <param name="Y2">Second Y-coordinate.</param>
		public TwoCoordinates(float X1, float Y1, float X2, float Y2)
			: base(X1, Y1)
		{
			this.x2 = X2;
			this.y2 = Y2;
		}

		/// <summary>
		/// X2-coordinate
		/// </summary>
		public float X2 => this.x2;

		/// <summary>
		/// Y2-coordinate
		/// </summary>
		public float Y2 => this.y2;

		/// <summary>
		/// Width
		/// </summary>
		public float Width => this.x2 - this.X;

		/// <summary>
		/// Height
		/// </summary>
		public float Height => this.y2 - this.Y;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is TwoCoordinates Obj &&
				this.x2 == Obj.x2 &&
				this.y2 == Obj.y2 &&
				base.Equals(Obj));
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.x2.GetHashCode();
			Result ^= Result << 5 ^ this.y2.GetHashCode();
			Result ^= Result << 5 ^ base.GetHashCode();
			return Result;
		}

		/// <inheritdoc/>
		public override void ExportGraph(XmlWriter Output)
		{
			Output.WriteAttributeString("x2", Expression.ToString(this.x2));
			Output.WriteAttributeString("y2", Expression.ToString(this.y2));
			base.ExportGraph(Output);
		}

		/// <inheritdoc/>
		public override void ImportGraph(XmlElement Xml, Variables _)
		{
			foreach (XmlAttribute Attr in Xml.Attributes)
			{
				switch (Attr.Name)
				{
					case "x2":
						if (Expression.TryParse(Attr.Value, out float Value))
							this.x2 = Value;
						break;

					case "y2":
						if (Expression.TryParse(Attr.Value, out Value))
							this.y2 = Value;
						break;
				}
			}
		}
	}
}
