using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Contains information from a POST request. Class is used to transfer posted information
	/// to the following GET request, for processing, when using the PRG pattern.
	/// </summary>
	public class PostedInformation
	{
		/// <summary>
		/// Contains information from a POST request. Class is used to transfer posted information
		/// to the following GET request, for processing, when using the PRG pattern.
		/// </summary>
		public PostedInformation()
		{
		}

		/// <summary>
		/// Decoded content.
		/// </summary>
		public IElement DecodedContent { get; set; }

		/// <summary>
		/// Resource the content was posted to.
		/// </summary>
		public string Resource { get; set; }

		/// <summary>
		/// Referer of the POST.
		/// </summary>
		public string Referer { get; set; }

		/// <summary>
		/// ID of request where posted content was processed.
		/// </summary>
		public Guid? RequestId { get; set; }
	}
}
