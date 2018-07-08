using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Security;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// User Avatar Image
	/// </summary>
	public class UserAvatarImage
	{
		private Uri url = null;

		/// <summary>
		/// User Avatar Image
		/// </summary>
		public UserAvatarImage()
		{
		}

		/// <summary>
		/// Internet content Type of image.
		/// </summary>
		public string ContentType
		{
			get;
			set;
		}

		/// <summary>
		/// Optional URL of image.
		/// </summary>
		public Uri URL
		{
			get => this.url;
			set => this.url = value;
		}

		/// <summary>
		/// Binary representation of image.
		/// </summary>
		public byte[] Data
		{
			get;
			set;
		}

		/// <summary>
		/// Width of image, in pixels.
		/// </summary>
		public int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Height of image, in pixels.
		/// </summary>
		public int Height
		{
			get;
			set;
		}

	}
}
