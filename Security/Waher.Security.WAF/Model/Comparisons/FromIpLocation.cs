using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Events;
using Waher.Networking.HTTP.Interfaces;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the requests come from a specific location, using available locale 
	/// information derived from the IP address of the caller.
	/// </summary>
	public class FromIpLocation : WafComparison
	{
		private static IEndpointLocalizationService[] localizationServices;

		private readonly StringAttribute country;
		private readonly StringAttribute region;
		private readonly StringAttribute city;

		static FromIpLocation()
		{
			InitServices();
			Types.OnInvalidated += (sender, e) => InitServices();
		}

		private static void InitServices()
		{
			ChunkedList<IEndpointLocalizationService> Services = new ChunkedList<IEndpointLocalizationService>();

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IEndpointLocalizationService)))
			{
				ConstructorInfo CI = Types.GetDefaultConstructor(T);
				if (CI is null)
					continue;

				try
				{
					IEndpointLocalizationService Service = (IEndpointLocalizationService)CI.Invoke(Array.Empty<object>());
					Services.Add(Service);
				}
				catch (Exception ex)
				{
					Log.Exception(ex, T.FullName);
				}
			}

			localizationServices = Services.ToArray();
		}

		/// <summary>
		/// Checks if the requests come from a specific location, using available locale 
		/// information derived from the IP address of the caller.
		/// </summary>
		public FromIpLocation()
			: base()
		{
		}

		/// <summary>
		/// Checks if the requests come from a specific location, using available locale 
		/// information derived from the IP address of the caller.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public FromIpLocation(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.country = new StringAttribute(Xml, "country");
			this.region = new StringAttribute(Xml, "region");
			this.city = new StringAttribute(Xml, "city");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(FromIpLocation);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new FromIpLocation(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			string Endpoint = State.Request.RemoteEndPoint.RemovePortNumber();
			string Country = await this.country.EvaluateAsync(State.Variables, string.Empty);
			string Region = await this.region.EvaluateAsync(State.Variables, string.Empty);
			string City = await this.city.EvaluateAsync(State.Variables, string.Empty);
			string Key = Endpoint + "|Loc|" + Country + "|" + Region + "|" + City;

			if (!State.TryGetCachedObject(Key, out bool IsMatch))
			{
				IEndpointLocalization Loc = null;

				foreach (IEndpointLocalizationService Service in localizationServices)
				{
					Loc = await Service.TryGetLocation(Endpoint);
					if (!(Loc is null))
						break;
				}

				if (Loc is null)
					IsMatch = false;
				else
				{
					IsMatch =
						CheckMatch(Loc.CountryCode, Country) &&
						CheckMatch(Loc.Region, Region) &&
						CheckMatch(Loc.City, City);
				}

				State.AddToCache(Key, IsMatch, fiveMinutes);
			}

			return IsMatch ? await this.ReviewChildren(State) : null;
		}

		private static bool CheckMatch(string Value, string List)
		{
			if (string.IsNullOrEmpty(List))
				return true;

			string[] Items = List.Split(',', StringSplitOptions.RemoveEmptyEntries);

			foreach (string Item in Items)
			{
				if (string.Equals(Value, Item.Trim(), StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}
	}
}