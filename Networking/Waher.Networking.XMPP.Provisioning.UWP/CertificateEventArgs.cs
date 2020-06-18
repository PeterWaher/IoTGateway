using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Streams;
#else
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
#endif

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Delegate for Certificate callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task CertificateCallback(object Sender, CertificateEventArgs e);

	/// <summary>
	/// Event arguments for token callbacks.
	/// </summary>
	public class CertificateEventArgs : IqResultEventArgs
	{
#if WINDOWS_UWP
		private readonly Certificate certificate;
#else
		private readonly X509Certificate2 certificate;
#endif

		internal CertificateEventArgs(IqResultEventArgs e, object State, byte[] CertificateBlob)
			: base(e)
		{
			this.State = State;

#if WINDOWS_UWP
			IBuffer Buffer = CryptographicBuffer.CreateFromByteArray(CertificateBlob);
			this.certificate = new Certificate(Buffer);
#else
			this.certificate = new X509Certificate2(CertificateBlob);
#endif
		}

		/// <summary>
		/// Certificate corresponding to the given token.
		/// </summary>
#if WINDOWS_UWP
		public Certificate Certificate
#else
		public X509Certificate2 Certificate
#endif
		{
			get { return this.certificate; }
		}
	}
}
