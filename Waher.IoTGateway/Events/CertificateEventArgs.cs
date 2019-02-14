using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Waher.IoTGateway.Events
{
	/// <summary>
	/// Delegate for certificate events.
	/// </summary>
	/// <param name="Sender">Sender of event</param>
	/// <param name="e">Event arguments</param>
	public delegate void CertificateEventHandler(object Sender, CertificateEventArgs e);

	/// <summary>
	/// Event arguments for X.509 certificate events.
	/// </summary>
	public class CertificateEventArgs : EventArgs
	{
		private readonly X509Certificate2 certificate;

		/// <summary>
		/// Event arguments for X.509 certificate events.
		/// </summary>
		/// <param name="Certificate">Certificate</param>
		public CertificateEventArgs(X509Certificate2 Certificate)
			: base()
		{
			this.certificate = Certificate;
		}

		/// <summary>
		/// X.509 Certificate
		/// </summary>
		public X509Certificate2 Certificate => this.certificate;
	}
}
