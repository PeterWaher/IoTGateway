using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Base class for all asynchronous HTTP resources.
	/// </summary>
	public abstract class HttpAsynchronousResource : HttpResource
	{
		/// <summary>
		/// Base class for all asynchronous HTTP resources.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		public HttpAsynchronousResource(string ResourceName)
			: base(ResourceName)
		{
		}

		/// <summary>
		/// If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).
		/// </summary>
		public override bool Synchronous
		{
			get { return false; }
		}

	}
}
