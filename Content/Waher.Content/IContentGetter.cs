using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;

namespace Waher.Content
{
	/// <summary>
	/// Basic interface for Internet Content getters. A class implementing this interface and having a default constructor, will be able
	/// to partake in retrieving content through the static <see cref="InternetContent"/> class. No registration is required.
	/// </summary>
	public interface IContentGetter
	{
		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		string[] UriSchemes
		{
			get;
		}

		/// <summary>
		/// If the getter is able to get a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the getter would be able to get a resource given the indicated URI.</param>
		/// <returns>If the getter can get a resource with the indicated URI.</returns>
		bool CanGet(Uri Uri, out Grade Grade);

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		Task<object> GetAsync(Uri Uri, params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		Task<object> GetAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		Task<KeyValuePair<string, TemporaryStream>> GetTempStreamAsync(Uri Uri, params KeyValuePair<string, string>[] Headers);

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		Task<KeyValuePair<string, TemporaryStream>> GetTempStreamAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers);
	}
}
