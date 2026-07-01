using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.OAuth.MetaData
{
	/// <summary>
	/// Defines the resource documentation URI of an OAUTH web service.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class OAuthResourceDocumentationAttribute : OAuthMetaDataAttribute
	{
		/// <summary>
		/// Defines the resource documentation URI of an OAUTH web service.
		/// </summary>
		/// <param name="ResourceDocumentationUri">URI to human-readable resource documentation.</param>
		public OAuthResourceDocumentationAttribute(string ResourceDocumentationUri)
		{
			this.ResourceDocumentationUri = ResourceDocumentationUri;
		}

		/// <summary>
		/// Resource documentation URI.
		/// </summary>
		public string ResourceDocumentationUri { get; }

		/// <summary>
		/// Adds available meta-data to a dictionary of meta-data.
		/// </summary>
		/// <param name="Resource">Resource to add meta-data for.</param>
		/// <param name="MetaData">Dictionary to add meta-data to.</param>
		public override Task AddMetaData(HttpResource Resource, 
			Dictionary<string, object> MetaData)
		{
			MetaData["resource_documentation"] = this.ResourceDocumentationUri;
			return Task.CompletedTask;
		}
	}
}
