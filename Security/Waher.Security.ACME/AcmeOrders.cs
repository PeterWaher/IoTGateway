using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using Waher.Content;

namespace Waher.Security.ACME
{
	/// <summary>
	/// Represents a set of ACME orders.
	/// </summary>
	public class AcmeOrders : AcmeObject
	{
		private static readonly Regex nextUrl = new Regex("^\\s*[<](?'URL'[^>]+)[>]\\s*;\\s*rel\\s*=\\s*['\"]next['\"]\\s*$", RegexOptions.Singleline | RegexOptions.Compiled);
		private readonly Uri[] orders = null;
		private readonly Uri next = null;

		internal AcmeOrders(AcmeClient Client, HttpResponseMessage Response, IEnumerable<KeyValuePair<string, object>> Obj)
			: base(Client)
		{
			if (Response.Headers.TryGetValues("Link", out IEnumerable<string> Values))
			{
				foreach (string s in Values)
				{
					Match M = nextUrl.Match(s);
					if (M.Success)
					{
						this.next = new Uri(M.Groups["URL"].Value);
						break;
					}
				}
			}

			foreach (KeyValuePair<string, object> P in Obj)
			{
				switch (P.Key)
				{
					case "orders":
						if (P.Value is Array A)
						{
							List<Uri> Orders = new List<Uri>();

							foreach (object Obj2 in A)
							{
								if (Obj2 is string s)
									Orders.Add(new Uri(s));
							}

							this.orders = Orders.ToArray();
						}
						break;
				}
			}
		}

		/// <summary>
		/// An array of URLs, each identifying an order belonging to the account.
		/// </summary>
		public Uri[] Orders => this.orders;

		/// <summary>
		/// If provided, indicates where further entries can be acquired.
		/// </summary>
		public Uri Next => this.next;
	}
}
