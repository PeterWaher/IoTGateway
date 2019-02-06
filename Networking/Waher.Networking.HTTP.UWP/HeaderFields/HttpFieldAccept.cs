using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Accept HTTP Field header. (RFC 2616, §14.1)
	/// </summary>
	public class HttpFieldAccept : HttpFieldAcceptRecords
	{
		/// <summary>
		/// Accept HTTP Field header. (RFC 2616, §14.1)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldAccept(string Key, string Value)
			: base(Key, Value)
		{
		}

		/// <summary>
		/// Gets the best content type acceptable to the client.
		/// </summary>
		/// <param name="ContentTypes">Array of content types to choose from.</param>
		/// <returns>The best choice. If none are acceptable, null is returned.</returns>
		[Obsolete("Use GetBestAlternative() instead.")]
		public string GetBestContentType(params string[] ContentTypes)
		{
			return this.GetBestAlternative(ContentTypes);
		}

		/// <summary>
		/// Gets the best content type acceptable to the client.
		/// </summary>
		/// <param name="ContentTypes">Array of content types to choose from, together with arrays of any parameters that might 
		/// be relevant in the comparison.</param>
		/// <returns>The best choice. If none are acceptable, (null, null) is returned.</returns>
		[Obsolete("Use GetBestAlternative() instead.")]
		public KeyValuePair<string, KeyValuePair<string, string>[]> GetBestContentType(
			params KeyValuePair<string, KeyValuePair<string, string>[]>[] ContentTypes)
		{
			return this.GetBestAlternative(ContentTypes);
		}

	}
}
