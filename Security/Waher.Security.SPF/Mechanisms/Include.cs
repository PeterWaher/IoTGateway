using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Security.SPF.Mechanisms
{
	/// <summary>
	/// The "include" mechanism triggers a recursive evaluation of check_host().
	/// </summary>
	public class Include : MechanismDomainSpec	
	{
		private SpfExpression[] spfExpressions;

		/// <summary>
		/// The "include" mechanism triggers a recursive evaluation of check_host().
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		/// <param name="SpfRecords">SPF Expressions that can be used, in case a domain lacks SPF records in the DNS.</param>
		public Include(Term Term, SpfQualifier Qualifier, params SpfExpression[] SpfExpressions)
			: base(Term, Qualifier)
		{
			this.spfExpressions = SpfExpressions;
		}

		/// <summary>
		/// Checks if the mechamism matches the current request.
		/// </summary>
		/// <returns>Match result</returns>
		public override async Task<SpfResult> Matches()
		{
			string Bak = this.term.domain;
			this.term.domain = this.Domain;
			try
			{
				KeyValuePair<SpfResult, string> Result = await SpfResolver.CheckHost(this.term, this.spfExpressions);

				switch (Result.Key)
				{
					case SpfResult.Pass:
						return SpfResult.Pass;

					case SpfResult.Fail:
					case SpfResult.SoftFail:
					case SpfResult.Neutral:
						return SpfResult.Fail;

					case SpfResult.TemporaryError:
						return SpfResult.TemporaryError;

					case SpfResult.PermanentError:
					case SpfResult.None:
					default:
						return SpfResult.PermanentError;
				}
			}
			finally
			{
				this.term.domain = Bak;
			}
		}

	}
}
