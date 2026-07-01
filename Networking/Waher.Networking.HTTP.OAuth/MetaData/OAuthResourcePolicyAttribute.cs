using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.OAuth.MetaData
{
	/// <summary>
	/// Defines the resource policy URI of an OAUTH web service.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class OAuthResourcePolicyAttribute : OAuthMetaDataAttribute
	{
		/// <summary>
		/// Defines the resource policy URI of an OAUTH web service.
		/// </summary>
		/// <param name="ResourcePolicyUri">URI to human-readable resource policy.</param>
		public OAuthResourcePolicyAttribute(string ResourcePolicyUri)
		{
			this.ResourcePolicyUri = ResourcePolicyUri;
		}

		/// <summary>
		/// Resource policy URI.
		/// </summary>
		public string ResourcePolicyUri { get; }

		/// <summary>
		/// Adds available meta-data to a dictionary of meta-data.
		/// </summary>
		/// <param name="Resource">Resource to add meta-data for.</param>
		/// <param name="MetaData">Dictionary to add meta-data to.</param>
		public override Task AddMetaData(HttpResource Resource, 
			Dictionary<string, object> MetaData)
		{
			MetaData["resource_policy_uri"] = this.ResourcePolicyUri;
			return Task.CompletedTask;
		}
	}
}
