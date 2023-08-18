using Waher.Runtime.Text;

namespace Waher.Networking.HTTP.Vanity
{
	/// <summary>
	/// Transforms vanity resource names to actual resource names.
	/// </summary>
	public class VanityResources
	{
		private readonly HarmonizedTextMap vanityResources = new HarmonizedTextMap();

		/// <summary>
		/// Transforms vanity resource names to actual resource names.
		/// </summary>
		public VanityResources()
		{
		}

		/// <summary>
		/// Registers a vanity resource.
		/// </summary>
		/// <param name="RegexPattern">Regular expression used to match incoming requests.</param>
		/// <param name="MapTo">Resources matching <paramref name="RegexPattern"/> will be mapped to resources of this type.
		/// Named group values found using the regular expression can be used in the map, between curly braces { and }.</param>
		public void RegisterVanityResource(string RegexPattern, string MapTo)
		{
			this.vanityResources.RegisterMapping(RegexPattern, MapTo);
		}

		/// <summary>
		/// Registers a vanity resource.
		/// </summary>
		/// <param name="RegexPattern">Regular expression used to match incoming requests.</param>
		/// <param name="MapTo">Resources matching <paramref name="RegexPattern"/> will be mapped to resources of this type.
		/// Named group values found using the regular expression can be used in the map, between curly braces { and }.</param>
		/// <param name="Tag">Tags the expression with an object. This tag can be used when
		/// unregistering all vanity resources tagged with the given tag.</param>
		public void RegisterVanityResource(string RegexPattern, string MapTo, object Tag)
		{
			this.vanityResources.RegisterMapping(RegexPattern, MapTo, Tag);
		}

		/// <summary>
		/// Unregisters a vanity resource.
		/// </summary>
		/// <param name="RegexPattern">Regular expression used to match incoming requests.</param>
		/// <returns>If a vanity resource matching the parameters was found, and consequently removed.</returns>
		public bool UnregisterVanityResource(string RegexPattern)
		{
			return this.vanityResources.UnregisterMapping(RegexPattern);
		}

		/// <summary>
		/// Unregisters vanity resources tagged with a specific object.
		/// </summary>
		/// <param name="Tag">Remove all vanity resources tagged with this object.</param>
		/// <returns>Number of vanity resources removed.</returns>
		public int UnregisterVanityResources(object Tag)
		{
			return this.vanityResources.UnregisterMappings(Tag);
		}

		/// <summary>
		/// Checks if a resource name is a vanity resource name. If so, it is expanded to the true resource name.
		/// </summary>
		/// <param name="ResourceName">Resource name.</param>
		/// <returns>If resource was a vanity resource, and has been updated to reflect the true resource name.</returns>
		public bool CheckVanityResource(ref string ResourceName)
		{
			if (this.vanityResources.TryMap(ResourceName, out string VanityResource))
			{
				ResourceName = VanityResource;
				return true;
			}
			else
				return false;
		}
	}
}
