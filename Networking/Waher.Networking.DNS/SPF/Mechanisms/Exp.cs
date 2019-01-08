using System;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// If check_host() results in a "fail" due to a mechanism match (such as
	/// "-all"), and the "exp" modifier is present, then the explanation
	/// string returned is computed as described below.If no "exp" modifier
	/// is present, then either a default explanation string or an empty
	/// explanation string MUST be returned to the calling application.
	/// </summary>
	public class Exp : MechanismDomainSpec
	{
		/// <summary>
		/// If check_host() results in a "fail" due to a mechanism match (such as
		/// "-all"), and the "exp" modifier is present, then the explanation
		/// string returned is computed as described below.If no "exp" modifier
		/// is present, then either a default explanation string or an empty
		/// explanation string MUST be returned to the calling application.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public Exp(Term Term, SpfQualifier Qualifier)
			: base(Term, Qualifier)
		{
		}

		/// <summary>
		/// Mechanism separator
		/// </summary>
		public override char Separator => '=';

		/// <summary>
		/// Checks if the mechamism matches the current request.
		/// </summary>
		/// <returns>Match result</returns>
		public override Task<SpfResult> Matches()
		{
			return Task.FromResult<SpfResult>(SpfResult.Fail);
		}

		/// <summary>
		/// Evaluates the explanation.
		/// </summary>
		/// <returns></returns>
		public async Task<string> Evaluate()
		{
			try
			{
				await this.Expand();

				StringBuilder sb = new StringBuilder();

				foreach (string Text in await DnsResolver.LookupText(this.Domain))
					sb.Append(Text);    // No white-space delimiter

				this.term.Reset("=" + sb.ToString());
				Exp Exp = new Exp(this.term, this.qualifier);

				await Exp.Expand();

				return Exp.Domain;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
