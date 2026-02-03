using System.Threading.Tasks;

namespace Waher.Security.WAF
{
	/// <summary>
	/// Interface for Enndpoint localizations.
	/// </summary>
	public interface IEndpointLocalizationService
	{
		/// <summary>
		/// Tries to get localization information about an endpoint.
		/// </summary>
		/// <param name="Endpoint">Endpoint</param>
		/// <returns>Location, if found, null otherwise.</returns>
		Task<IEndpointLocalization> TryGetLocation(string Endpoint);
	}
}
