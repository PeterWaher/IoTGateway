using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Contains a reference to an image in the theme.
	/// </summary>
    public class ThemeImage
    {
		private readonly string resource;
		private readonly int width;
		private readonly int height;

		/// <summary>
		/// Contains a reference to an image in the theme.
		/// </summary>
		/// <param name="Resource">Resource of image.</param>
		/// <param name="Width">Width of image.</param>
		/// <param name="Height">Height of image.</param>
		public ThemeImage(string Resource, int Width, int Height)
		{
			this.resource = Resource;
			this.width = Width;
			this.height = Height;
		}

		/// <summary>
		/// Resource of image.
		/// </summary>
		public string Resource => this.resource;

		/// <summary>
		/// Width of image.
		/// </summary>
		public int Width => this.width;

		/// <summary>
		/// Height of image.
		/// </summary>
		public int Height => this.height;

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.resource;
		}
	}
}
