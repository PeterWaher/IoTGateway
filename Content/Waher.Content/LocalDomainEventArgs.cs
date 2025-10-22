using System;

namespace Waher.Content
{
	/// <summary>
	/// Local domain check event arguments.
	/// </summary>
	public class LocalDomainEventArgs : EventArgs
	{
		private bool? isLocal = null;

		/// <summary>
		/// Local domain check event arguments.
		/// </summary>
		/// <param name="DomainOrHost">Domain or host name.</param>
		/// <param name="IncludeAlternativeDomains">If alternative domains are to be checked as well.</param>
		public LocalDomainEventArgs(string DomainOrHost, bool IncludeAlternativeDomains)
		{
			this.DomainOrHost = DomainOrHost;
			this.IncludeAlternativeDomains = IncludeAlternativeDomains;
		}

		/// <summary>
		/// If the domain is local or not.
		/// </summary>
		public bool? IsLocal
		{
			get => this.isLocal;
			set
			{
				if (this.isLocal.HasValue && this.isLocal != value)
					throw new InvalidOperationException("Value has already been set.");

				this.isLocal = value;
			}
		}

		/// <summary>
		/// Domain or host name.
		/// </summary>
		public string DomainOrHost
		{
			get;
			private set;
		}

		/// <summary>
		/// If alternative domains are to be checked.
		/// </summary>
		public bool IncludeAlternativeDomains
		{
			get;
			private set;
		}
	}
}
