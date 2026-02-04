using System;
using Waher.Networking.HTTP;
using Waher.Script;

namespace Waher.Security.WAF.Model
{
	/// <summary>
	/// Contains the current state of a review process.
	/// </summary>
	public class ProcessingState
	{
		private readonly HttpRequest request;
		private readonly HttpResource resource;
		private readonly WebApplicationFirewall firewall;
		private Variables variables;

		/// <summary>
		/// Contains the current state of a review process.
		/// </summary>
		/// <param name="Request">Current HTTP Request</param>
		/// <param name="Resource">Corresponding HTTP Resource, if found.</param>
		/// <param name="Firewall">Reference to the Web Application Firewall</param>
		public ProcessingState(HttpRequest Request, HttpResource Resource,
			WebApplicationFirewall Firewall)
		{
			this.request = Request;
			this.resource = Resource;
			this.firewall = Firewall;
		}

		/// <summary>
		/// Current HTTP Request
		/// </summary>
		public HttpRequest Request => this.request;

		/// <summary>
		/// Corresponding HTTP Resource, if found.
		/// </summary>
		public HttpResource Resource => this.resource;

		/// <summary>
		/// Reference to the current Web Application Firewall object.
		/// </summary>
		public WebApplicationFirewall Firewall => this.firewall;

		/// <summary>
		/// Content decoded as a string.
		/// </summary>
		public string ContentAsString { get; set; }

		/// <summary>
		/// Set of variables.
		/// </summary>
		public Variables Variables
		{
			get
			{
				this.variables ??= this.request.GetSessionFromCookie();
				return this.variables;
			}
		}

		/// <summary>
		/// Tries to get a cached object of a given type.
		/// </summary>
		/// <typeparam name="T">Expected type of cached value.</typeparam>
		/// <param name="Key">Key</param>
		/// <param name="Value">Value, if found.</param>
		/// <returns>If a cached object of the given type was found.</returns>
		public bool TryGetCachedObject<T>(string Key, out T Value)
		{
			if (this.firewall.Cache.TryGetValue(Key, out object Obj) && Obj is T TypedObj)
			{
				Value = TypedObj;
				return true;
			}
			else
			{
				Value = default;
				return false;
			}
		}

		/// <summary>
		/// Adds a value to the cache.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Value">Value</param>
		public void AddToCache(string Key, object Value)
		{
			this.firewall.Cache.Add(Key, Value);
		}

		/// <summary>
		/// Adds a value to the cache.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Value">Value</param>
		/// <param name="Expires">When item expires</param>
		public void AddToCache(string Key, object Value, TimeSpan Expires)
		{
			this.firewall.Cache.Add(Key, Value, Expires);
		}

		/// <summary>
		/// Adds a value to the cache.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Value">Value</param>
		/// <param name="Expires">When item expires</param>
		public void AddToCache(string Key, object Value, DateTime Expires)
		{
			this.firewall.Cache.Add(Key, Value, Expires);
		}
	}
}
