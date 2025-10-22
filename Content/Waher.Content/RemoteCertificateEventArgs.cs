using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Waher.Content
{
	/// <summary>
	/// Remove certificate validation event arguments.
	/// </summary>
	public class RemoteCertificateEventArgs : EventArgs
	{
		private bool? isValid = null;

		/// <summary>
		/// Remove certificate validation event arguments.
		/// </summary>
		/// <param name="Certificate">Remote certificate.</param>
		/// <param name="Chain">Certificate chain</param>
		/// <param name="SslPolicyErrors">Any SSL policy errors detected.</param>
		public RemoteCertificateEventArgs(X509Certificate Certificate,
			X509Chain Chain, SslPolicyErrors SslPolicyErrors)
		{
			this.Certificate = Certificate;
			this.Chain = Chain;
			this.SslPolicyErrors = SslPolicyErrors;
		}

		/// <summary>
		/// If remote certificate is considered valid or not. null means default
		/// validation rules will be applied. Can be set once to a non-null value.
		/// </summary>
		public bool? IsValid
		{
			get => this.isValid;
			set
			{
				if (this.isValid.HasValue && this.isValid != value)
					throw new InvalidOperationException("Value has already been set.");

				this.isValid = value;
			}
		}

		/// <summary>
		/// Remote certificate.
		/// </summary>
		public X509Certificate Certificate
		{
			get;
			private set;
		}

		/// <summary>
		/// Certificate chain
		/// </summary>
		public X509Chain Chain
		{
			get;
			private set;
		}

		/// <summary>
		/// Any SSL policy errors detected.
		/// </summary>
		public SslPolicyErrors SslPolicyErrors
		{
			get;
			private set;
		}
	}
}
