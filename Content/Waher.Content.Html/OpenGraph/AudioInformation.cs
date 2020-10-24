using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Html.OpenGraph
{
	/// <summary>
	/// Audio information, as defined by the Open Graph protocol.
	/// </summary>
	public class AudioInformation
	{
		private string url = null;
		private string secureUrl = null;
		private string contentType = null;

		/// <summary>
		/// Audio information, as defined by the Open Graph protocol.
		/// </summary>
		public AudioInformation()
		{
		}

		/// <summary>
		/// An image URL which should represent your object within the graph.
		/// If not defined, null is returned.
		/// </summary>
		public string Url
		{
			get { return this.url; }
			set { this.url = value; }
		}

		/// <summary>
		/// An alternate url to use if the webpage requires HTTPS.
		/// If not defined, null is returned.
		/// </summary>
		public string SecureUrl
		{
			get { return this.secureUrl; }
			set { this.secureUrl = value; }
		}

		/// <summary>
		/// A MIME type for this image.
		/// If not defined, null is returned.
		/// </summary>
		public string ContentType
		{
			get { return this.contentType; }
			set { this.contentType = value; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (obj is AudioInformation Audio)
			{
				return this.url == Audio.url &&
					this.secureUrl == Audio.secureUrl &&
					this.contentType == Audio.contentType;
			}
			else
				return false;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = 0;

			if (!(this.url is null))
				Result = this.url.GetHashCode();

			if (!(this.secureUrl is null))
				Result ^= Result << 5 ^ this.secureUrl.GetHashCode();

			if (!(this.contentType is null))
				Result ^= Result << 5 ^ this.contentType.GetHashCode();

			return Result;
		}

	}
}
