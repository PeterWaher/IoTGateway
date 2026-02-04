namespace Waher.Security.WAF.Test
{
	public class IpLocalizationService : IEndpointLocalizationService, IEndpointLocalization
	{
		private static string countryCode;
		private static string country;
		private static string region;
		private static string city;
		private static double latitude;
		private static double longitude;

		public IpLocalizationService()
		{
		}

		public string CountryCode => countryCode;
		public string Country => country;
		public string Region => region;
		public string City => city;
		public double Latitude => latitude;
		public double Longitude => longitude;

		public static void SetLocation(string CountryCode, string Country, string Region,
			string City, double Latitude, double Longitude)
		{
			countryCode = CountryCode;
			country = Country;
			region = Region;
			city = City;
			latitude = Latitude;
			longitude = Longitude;
		}

		public Task<IEndpointLocalization> TryGetLocation(string Endpoint)
		{
			return Task.FromResult<IEndpointLocalization>(this);
		}
	}
}
