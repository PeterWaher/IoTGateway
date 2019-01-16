using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.SPF
{
	/// <summary>
	/// Contains information about a SPF string.
	/// </summary>
	public class SpfExpression
	{
		private readonly string domain;
		private readonly string domainSuffix;
		private readonly string spf;
		private readonly bool includeSubdomains;

		/// <summary>
		/// Contains information about a SPF string.
		/// </summary>
		/// <param name="Domain"></param>
		/// <param name="IncludeSubdomains"></param>
		/// <param name="Spf"></param>
		public SpfExpression(string Domain, bool IncludeSubdomains, string Spf)
		{
			this.domain = Domain;
			this.domainSuffix = "+" + Domain;
			this.includeSubdomains = IncludeSubdomains;
			this.spf = Spf;
		}

		/// <summary>
		/// Domain name.
		/// </summary>
		public string Domain => this.domain;

		/// <summary>
		/// SPF expression.
		/// </summary>
		public string Spf => this.spf;

		/// <summary>
		/// If expression is valid for subdomains to <see cref="Domain"/> also.
		/// </summary>
		public bool IncludeSubdomains => this.includeSubdomains;

		/// <summary>
		/// Checks if the expression is applicable to a given domain.
		/// </summary>
		/// <param name="Domain">Domain to check.</param>
		/// <returns>If the expression is applicable.</returns>
		public bool IsApplicable(string Domain)
		{
			if (string.Compare(Domain, this.domain, true) == 0)
				return true;

			if (this.includeSubdomains && Domain.EndsWith(this.domainSuffix, StringComparison.CurrentCultureIgnoreCase))
				return true;

			return false;
		}
	}
}
