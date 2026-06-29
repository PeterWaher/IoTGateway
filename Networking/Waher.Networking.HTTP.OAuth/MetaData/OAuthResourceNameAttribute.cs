using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.OAuth.MetaData
{
	/// <summary>
	/// Defines the resource name of an OAUTH web service.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class OAuthResourceNameAttribute : OAuthMetaDataAttribute
	{
		/// <summary>
		/// Defines the resource name of an OAUTH web service.
		/// </summary>
		/// <param name="ResourceName">Human-readable resource name.</param>
		public OAuthResourceNameAttribute(string ResourceName)
		{
			this.ResourceName = ResourceName;
		}

		/// <summary>
		/// Resource name.
		/// </summary>
		public string ResourceName { get; }

		/// <summary>
		/// Adds available meta-data to a dictionary of meta-data.
		/// </summary>
		/// <param name="Resource">Resource to add meta-data for.</param>
		/// <param name="MetaData">Dictionary to add meta-data to.</param>
		public override void AddMetaData(HttpResource Resource, 
			Dictionary<string, object> MetaData)
		{
			MetaData["resource_name"] = this.ResourceName;
		}
	}
}
