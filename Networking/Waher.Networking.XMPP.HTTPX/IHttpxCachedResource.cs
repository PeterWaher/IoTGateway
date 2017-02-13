using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Interface for HTTPX Cache resource items.
	/// </summary>
	public interface IHttpxCachedResource
	{
		/// <summary>
		/// Name of file of local resource.
		/// </summary>
		string FileName
		{
			get;
		}

		/// <summary>
		/// ETag of resource.
		/// </summary>
		string ETag
		{
			get;
		}

		/// <summary>
		/// Content Type of resource.
		/// </summary>
		string ContentType
		{
			get;
		}

		/// <summary>
		/// When resource was last modified on remote peer.
		/// </summary>
		DateTimeOffset LastModified
		{
			get;
		}
	}
}
