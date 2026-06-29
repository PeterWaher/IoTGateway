using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.OAuth.MetaData
{
	/// <summary>
	/// Defines the resource terms of service URI of an OAUTH web service.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class OAuthResourceTermsOfServiceAttribute : OAuthMetaDataAttribute
	{
		/// <summary>
		/// Defines the resource terms of service URI of an OAUTH web service.
		/// </summary>
		/// <param name="ResourceTermsOfServiceUri">URI to human-readable resource terms of service.</param>
		public OAuthResourceTermsOfServiceAttribute(string ResourceTermsOfServiceUri)
		{
			this.ResourceTermsOfServiceUri = ResourceTermsOfServiceUri;
		}

		/// <summary>
		/// Resource terms of service URI.
		/// </summary>
		public string ResourceTermsOfServiceUri { get; }

		/// <summary>
		/// Adds available meta-data to a dictionary of meta-data.
		/// </summary>
		/// <param name="Resource">Resource to add meta-data for.</param>
		/// <param name="MetaData">Dictionary to add meta-data to.</param>
		public override void AddMetaData(HttpResource Resource, 
			Dictionary<string, object> MetaData)
		{
			MetaData["resource_tos_uri"] = this.ResourceTermsOfServiceUri;
		}
	}
}
