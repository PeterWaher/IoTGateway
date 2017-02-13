using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Interface for HTTPX caches. HTTPX caches can improve performance by storing resources locally.
	/// </summary>
	public interface IHttpxCache
	{
		/// <summary>
		/// Tries to get a reference to the resource from the local cache.
		/// </summary>
		/// <param name="BareJid">Bare JID of resource.</param>
		/// <param name="LocalResource">Local resource.</param>
		/// <returns>Information about the cached item, if found, or null if not found.</returns>
		Task<IHttpxCachedResource> TryGetCachedResource(string BareJid, string LocalResource);

		/// <summary>
		/// Checks if content from a remote resource can be cached.
		/// 
		/// NOTE: Content that is marked as not cacheable by HTTP headers, will not be cached.
		/// </summary>
		/// <param name="BareJid">Bare JID.</param>
		/// <param name="LocalResource">Local resource.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the resource can be cached.</returns>
		bool CanCache(string BareJid, string LocalResource, string ContentType);

		/// <summary>
		/// Adds content to the cache.
		/// </summary>
		/// <param name="BareJid">Bare JID.</param>
		/// <param name="LocalResource">Local resource.</param>
		/// <param name="ContentType">Content type.</param>
		/// <param name="ETag">ETag of resource.</param>
		/// <param name="LastModified">When the resource was last modified.</param>
		/// <param name="Expires">When the content expires, if reported.</param>
		/// <param name="Content">Content stream.</param>
		Task AddToCache(string BareJid, string LocalResource, string ContentType, string ETag, DateTimeOffset LastModified, 
			DateTimeOffset? Expires, Stream Content);
	}
}
