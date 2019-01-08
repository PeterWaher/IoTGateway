using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.DNS.SPF.Mechanisms
{
	/// <summary>
	/// Abstract base class for SPF Mechanisms.
	/// </summary>
	public abstract class Mechanism
	{
		/// <summary>
		/// Qualifier
		/// </summary>
		protected SpfQualifier qualifier;

		/// <summary>
		/// Current request.
		/// </summary>
		protected readonly Term term;

		/// <summary>
		/// Abstract base class for SPF Mechanisms.
		/// </summary>
		/// <param name="Term">Term</param>
		/// <param name="Qualifier">Qualifier</param>
		public Mechanism(Term Term, SpfQualifier Qualifier)
		{
			this.term = Term;
			this.qualifier = Qualifier;
		}

		/// <summary>
		/// Mechanism qualifier
		/// </summary>
		public SpfQualifier Qualifier => this.qualifier;

		/// <summary>
		/// Expands any macros in the domain specification.
		/// </summary>
		public virtual Task Expand()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Checks if the mechamism matches the current request.
		/// </summary>
		/// <returns>Match result</returns>
		public abstract Task<SpfResult> Matches();
	}
}
