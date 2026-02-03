using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.Security.WAF
{
	/// <summary>
	/// Interface for Enndpoint localizations.
	/// </summary>
	public interface IEndpointLocalization
	{
		/// <summary>
		/// ISO Country Code
		/// </summary>
		string CountryCode { get; }

		/// <summary>
		/// Name of country
		/// </summary>
		string Country { get; }

		/// <summary>
		/// Region
		/// </summary>
		string Region { get; }

		/// <summary>
		/// City
		/// </summary>
		string City { get; }

		/// <summary>
		/// Latitude
		/// </summary>
		double Latitude { get; }

		/// <summary>
		/// Longitude
		/// </summary>
		double Longitude { get; }
	}
}
