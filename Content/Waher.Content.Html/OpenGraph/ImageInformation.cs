using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.OpenGraph
{
	/// <summary>
	/// Image information, as defined by the Open Graph protocol.
	/// </summary>
    public class ImageInformation : VideoInformation
    {
		private string description = null;

		/// <summary>
		/// Image information, as defined by the Open Graph protocol.
		/// </summary>
		public ImageInformation()
		{
		}

		/// <summary>
		/// A description of what is in the image (not a caption).
		/// If not defined, null is returned.
		/// </summary>
		public string Description
		{
			get { return this.description; }
			set { this.description = value; }
		}

	}
}
