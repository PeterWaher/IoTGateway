using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;
using Waher.Runtime.IO;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the requests come from a specific IP or IPs using a comma-separated
	/// list of CIDR ranges.
	/// </summary>
	public class FromIp : WafComparison
	{
		private readonly StringAttribute value;

		/// <summary>
		/// Checks if the requests come from a specific IP or IPs using a comma-separated
		/// list of CIDR ranges.
		/// </summary>
		public FromIp()
			: base()
		{
		}

		/// <summary>
		/// Checks if the requests come from a specific IP or IPs using a comma-separated
		/// list of CIDR ranges.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public FromIp(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.value = new StringAttribute(Xml, "value");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(FromIp);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new FromIp(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			string Endpoint = State.Request.RemoteEndPoint.RemovePortNumber();

			if (!IPAddress.TryParse(Endpoint, out IPAddress Address))
				return null;

			string Value = await this.value.EvaluateAsync(State.Variables, string.Empty);
			string Key = Endpoint + "|IP|" + Value;

			if (!State.TryGetCachedObject(Key, out bool IsMatch))
			{
				IsMatch = CheckMatch(Address, Value);
				State.AddToCache(Key, IsMatch, fiveMinutes);
			}

			return IsMatch ? await this.ReviewChildren(State) : null;
		}

		private static bool CheckMatch(IPAddress Value, string List)
		{
			string[] Items = List.Split(',', StringSplitOptions.RemoveEmptyEntries);

			foreach (string Item in Items)
			{
				string s = Item.Trim();

				if (s.Contains('/'))
				{
					if (IpCidr.TryParse(s, out IpCidr Cidr) && Cidr.Matches(Value))
						return true;
				}
				else if (IPAddress.TryParse(s, out IPAddress Addr))
				{
					if (Addr.Equals(Value))
						return true;
				}
			}

			return false;
		}
	}
}
