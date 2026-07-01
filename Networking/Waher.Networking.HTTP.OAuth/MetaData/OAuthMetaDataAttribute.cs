using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.OAuth.MetaData
{
	/// <summary>
	/// Abstract base class for OAUTH meta-data attributes.
	/// </summary>
	public abstract class OAuthMetaDataAttribute : Attribute
	{
		/// <summary>
		/// Abstract base class for OAUTH meta-data attributes.
		/// </summary>
		public OAuthMetaDataAttribute()
		{
		}

		/// <summary>
		/// Registers any meta-data used that requires registration.
		/// </summary>
		/// <param name="Resource">Resource containing meta-data to be registered.</param>
		public virtual Task RegisterMetaData(HttpResource Resource)
		{ 
			return Task.CompletedTask;
		}

		/// <summary>
		/// Adds available meta-data to a dictionary of meta-data.
		/// </summary>
		/// <param name="Resource">Resource to add meta-data for.</param>
		/// <param name="MetaData">Dictionary to add meta-data to.</param>
		public abstract Task AddMetaData(HttpResource Resource,
			Dictionary<string, object> MetaData);
	}
}
