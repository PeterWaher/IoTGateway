using System;
using System.Xml;
using SkiaSharp;
using Waher.Script.Graphs.Functions.Colors;

namespace Waher.Script.Graphs
{
	/// <summary>
	/// Graph settings.
	/// </summary>
	public class GraphSettings
	{
		private SKColor backgroundColor = SKColors.White;
		private SKColor axisColor = SKColors.Black;
		private SKColor gridColor = SKColors.LightGray;
		private string fontName = "Segoe UI";
		private double labelFontSize = 12;
		private int axisWidth = 2;
		private int gridWidth = 1;
		private int approxNrLabelsX = 5;
		private int approxNrLabelsY = 10;
		private int width = 640;
		private int height = 480;
		private int marginTop = 10;
		private int marginBottom = 10;
		private int marginLeft = 15;
		private int marginRight = 15;
		private int marginLabel = 5;

		/// <summary>
		/// Graph settings.
		/// </summary>
		public GraphSettings()
		{
		}

		/// <summary>
		/// Copies the graph settings.
		/// </summary>
		/// <returns>Copy of graph settings.</returns>
		public GraphSettings Copy()
		{
			return new GraphSettings()
			{
				backgroundColor = this.backgroundColor,
				axisColor = this.axisColor,
				gridColor = this.gridColor,
				fontName = this.fontName,
				labelFontSize = this.labelFontSize,
				axisWidth = this.axisWidth,
				gridWidth = this.gridWidth,
				approxNrLabelsX = this.approxNrLabelsX,
				approxNrLabelsY = this.approxNrLabelsY,
				width = this.width,
				height = this.height,
				marginTop = this.marginTop,
				marginBottom = this.marginBottom,
				marginLeft = this.marginLeft,
				marginRight = this.marginRight,
				marginLabel = this.marginLabel
			};
		}

		/// <summary>
		/// Width of graph, in pixels. (Default=640 pixels.)
		/// </summary>
		public int Width
		{
			get => this.width;
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(this.Width));

				this.width = value;
			}
		}

		/// <summary>
		/// Height of graph, in pixels. (Default=480 pixels.)
		/// </summary>
		public int Height
		{
			get => this.height;
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(this.Height));

				this.height = value;
			}
		}

		/// <summary>
		/// Background color.
		/// </summary>
		public SKColor BackgroundColor
		{
			get => this.backgroundColor;
			set => this.backgroundColor = value;
		}

		/// <summary>
		/// Axis color.
		/// </summary>
		public SKColor AxisColor
		{
			get => this.axisColor;
			set => this.axisColor = value;
		}

		/// <summary>
		/// Axis width.
		/// </summary>
		public int AxisWidth
		{
			get => this.axisWidth;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(this.AxisWidth));

				this.axisWidth = value;
			}
		}

		/// <summary>
		/// Grid color.
		/// </summary>
		public SKColor GridColor
		{
			get => this.gridColor;
			set => this.gridColor = value;
		}

		/// <summary>
		/// Grid width.
		/// </summary>
		public int GridWidth
		{
			get => this.gridWidth;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(this.GridWidth));

				this.gridWidth = value;
			}
		}

		/// <summary>
		/// Top margin.
		/// </summary>
		public int MarginTop
		{
			get => this.marginTop;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(this.MarginTop));

				this.marginTop = value;
			}
		}

		/// <summary>
		/// Bottom margin.
		/// </summary>
		public int MarginBottom
		{
			get => this.marginBottom;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(this.MarginBottom));

				this.marginBottom = value;
			}
		}

		/// <summary>
		/// Left margin.
		/// </summary>
		public int MarginLeft
		{
			get => this.marginLeft;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(this.MarginLeft));

				this.marginLeft = value;
			}
		}

		/// <summary>
		/// Right margin.
		/// </summary>
		public int MarginRight
		{
			get => this.marginRight;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(this.MarginRight));

				this.marginRight = value;
			}
		}

		/// <summary>
		/// Label margin.
		/// </summary>
		public int MarginLabel
		{
			get => this.marginLabel;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(this.MarginLabel));

				this.marginLabel = value;
			}
		}

		/// <summary>
		/// Font name.
		/// </summary>
		public string FontName
		{
			get => this.fontName;
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentException("Value cannot be empty.", nameof(this.FontName));

				this.fontName = value;
			}
		}

		/// <summary>
		/// Label font size
		/// </summary>
		public double LabelFontSize
		{
			get => this.labelFontSize;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(this.LabelFontSize));

				this.labelFontSize = value;
			}
		}

		/// <summary>
		/// Approximate number of labels along the X-axis.
		/// </summary>
		public int ApproxNrLabelsX
		{
			get => this.approxNrLabelsX;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(this.ApproxNrLabelsX));

				this.approxNrLabelsX = value;
			}
		}

		/// <summary>
		/// Approximate number of labels along the Y-axis.
		/// </summary>
		public int ApproxNrLabelsY
		{
			get => this.approxNrLabelsY;
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(this.ApproxNrLabelsY));

				this.approxNrLabelsY = value;
			}
		}

		/// <summary>
		/// Exports the settings to an XML writer.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public void ExportSettings(XmlWriter Output)
		{
			Output.WriteStartElement("Settings");
			Output.WriteAttributeString("backgroundColor", Graph.ToRGBAStyle(this.backgroundColor));
			Output.WriteAttributeString("axisColor", Graph.ToRGBAStyle(this.axisColor));
			Output.WriteAttributeString("gridColor", Graph.ToRGBAStyle(this.gridColor));
			Output.WriteAttributeString("fontName", this.fontName);
			Output.WriteAttributeString("labelFontSize", Expression.ToString(this.labelFontSize));
			Output.WriteAttributeString("axisWidth", this.axisWidth.ToString());
			Output.WriteAttributeString("gridWidth", this.gridWidth.ToString());
			Output.WriteAttributeString("approxNrLabelsX", this.approxNrLabelsX.ToString());
			Output.WriteAttributeString("approxNrLabelsY", this.approxNrLabelsY.ToString());
			Output.WriteAttributeString("width", this.width.ToString());
			Output.WriteAttributeString("height", this.height.ToString());
			Output.WriteAttributeString("marginTop", this.marginTop.ToString());
			Output.WriteAttributeString("marginBottom", this.marginBottom.ToString());
			Output.WriteAttributeString("marginLeft", this.marginLeft.ToString());
			Output.WriteAttributeString("marginRight", this.marginRight.ToString());
			Output.WriteAttributeString("marginLabel", this.marginLabel.ToString());
			Output.WriteEndElement();
		}

		/// <summary>
		/// Imports graph settings from an XML element.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>Graph settings.</returns>
		public static GraphSettings Import(XmlElement Xml)
		{
			GraphSettings Result = new GraphSettings();
			string s;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("backgroundColor")) && Color.TryParse(s, out SKColor cl))
				Result.backgroundColor = cl;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("axisColor")) && Color.TryParse(s, out cl))
				Result.axisColor = cl;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("gridColor")) && Color.TryParse(s, out cl))
				Result.gridColor = cl;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("fontName")))
				Result.fontName = s;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("labelFontSize")) && Expression.TryParse(s, out double d))
				Result.labelFontSize = d;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("axisWidth")) && int.TryParse(s, out int i))
				Result.axisWidth = i;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("gridWidth")) && int.TryParse(s, out i))
				Result.gridWidth = i;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("approxNrLabelsX")) && int.TryParse(s, out i))
				Result.approxNrLabelsX = i;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("approxNrLabelsY")) && int.TryParse(s, out i))
				Result.approxNrLabelsY = i;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("width")) && int.TryParse(s, out i))
				Result.width = i;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("height")) && int.TryParse(s, out i))
				Result.height = i;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("marginTop")) && int.TryParse(s, out i))
				Result.marginTop = i;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("marginBottom")) && int.TryParse(s, out i))
				Result.marginBottom = i;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("marginLeft")) && int.TryParse(s, out i))
				Result.marginLeft = i;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("marginRight")) && int.TryParse(s, out i))
				Result.marginRight = i;

			if (!string.IsNullOrEmpty(s = Xml.GetAttribute("marginLabel")) && int.TryParse(s, out i))
				Result.marginLabel = i;

			return Result;
		}

	}
}
