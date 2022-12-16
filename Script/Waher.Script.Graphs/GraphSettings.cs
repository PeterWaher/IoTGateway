using System;
using SkiaSharp;

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
		/// Width of graph, in pixels. (Default=640 pixels.)
		/// </summary>
		public int Width
		{
			get => this.width;
			set
			{
				if (value < 1)
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(Width));

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
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(Height));

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
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(AxisWidth));

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
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(GridWidth));

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
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(MarginTop));

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
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(MarginBottom));

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
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(MarginLeft));

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
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(MarginRight));

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
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(MarginLabel));

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
					throw new ArgumentException("Value cannot be empty.", nameof(FontName));

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
					throw new ArgumentOutOfRangeException("Value must be positive.", nameof(LabelFontSize));

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
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(ApproxNrLabelsX));

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
					throw new ArgumentOutOfRangeException("Value must be non-negative.", nameof(ApproxNrLabelsY));

				this.approxNrLabelsY = value;
			}
		}

	}
}
