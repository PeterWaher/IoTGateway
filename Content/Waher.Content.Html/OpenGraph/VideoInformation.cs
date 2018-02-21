using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.OpenGraph
{
	/// <summary>
	/// Video information, as defined by the Open Graph protocol.
	/// </summary>
    public class VideoInformation : AudioInformation
    {
		private int? width = null;
		private int? height = null;

		/// <summary>
		/// Video information, as defined by the Open Graph protocol.
		/// </summary>
		public VideoInformation()
		{
		}

		/// <summary>
		/// The number of pixels wide.
		/// If not defined, null is returned.
		/// </summary>
		public int? Width
		{
			get { return this.width; }
			set { this.width = value; }
		}

		/// <summary>
		/// The number of pixels high.
		/// If not defined, null is returned.
		/// </summary>
		public int? Height
		{
			get { return this.height; }
			set { this.height = value; }
		}
	}
}
