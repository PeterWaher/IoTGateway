using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.HTTP.OAuth.MetaData;

namespace Waher.Networking.HTTP.OAuth
{
	/// <summary>
	/// Base class for all asynchronous HTTP resources.
	/// An asynchronous resource responds outside of the method handler,
	/// from a separate thread or task. The application is responsible
	/// for returning the response and disposing of the response object
	/// when done.
	/// </summary>
	public abstract class HttpProtectedResource : HttpResource
	{
		/// <summary>
		/// Base class for all asynchronous HTTP resources.
		/// An asynchronous resource responds outside of the method handler,
		/// from a separate thread or task. The application is responsible
		/// for returning the response and disposing of the response object
		/// when done.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		public HttpProtectedResource(string ResourceName)
			: base(ResourceName)
		{
		}

		/// <summary>
		/// Method called when a resource has been registered on a server.
		/// </summary>
		/// <param name="Server">Server</param>
		public override void AddReference(HttpServer Server)
		{
			base.AddReference(Server);

			Task.Run(async () =>
			{
				try
				{
					IEnumerable<OAuthMetaDataAttribute> OAuthMetaDAtaAttributes = this.GetType().GetCustomAttributes<OAuthMetaDataAttribute>(true);

					foreach (OAuthMetaDataAttribute Attribute in OAuthMetaDAtaAttributes)
						await Attribute.RegisterMetaData(this);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			});
		}
	}
}
