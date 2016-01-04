using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Abstract base class for selectable colorable item.
	/// </summary>
	public abstract class ColorableItem : SelectableItem
	{
		private Color foregroundColor;
		private Color backgroundColor;

		/// <summary>
		/// Abstract base class for selectable colorable item.
		/// </summary>
		/// <param name="ForegroundColor">Foreground Color</param>
		/// <param name="BackgroundColor">Background Color</param>
		public ColorableItem(Color ForegroundColor, Color BackgroundColor)
		{
			this.foregroundColor = ForegroundColor;
			this.backgroundColor = BackgroundColor;
		}

		/// <summary>
		/// Foreground color
		/// </summary>
		public Color ForegroundColor
		{
			get { return this.foregroundColor; }
			set { this.foregroundColor = value; }
		}

		/// <summary>
		/// Foreground color, as a string
		/// </summary>
		public string ForegroundColorString
		{
			get { return this.foregroundColor.ToString(); }
		}

		/// <summary>
		/// Background color
		/// </summary>
		public Color BackgroundColor
		{
			get { return this.backgroundColor; }
			set { this.backgroundColor = value; }
		}

		/// <summary>
		/// Background color, as a string
		/// </summary>
		public string BackgroundColorString 
		{
			get { return this.backgroundColor.ToString(); } 
		}

	}
}
