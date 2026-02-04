using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Events.Syslog
{
	/// <summary>
	/// Event sink that sends events to a Syslog server using the Syslog protocol.
	/// </summary>
	public class SyslogEventSink : EventSink, ITlsCertificateEndpoint
	{
		private SyslogClient client;

		/// <summary>
		/// Event sink that sends events to a Syslog server using the Syslog protocol.
		/// </summary>
		/// <param name="Host">Syslog server to send events to.</param>
		/// <param name="Port">Syslog server port number to use.</param>
		/// <param name="Tls">If TLS is to be used.</param>
		/// <param name="LocalHostName">Local host name</param>
		/// <param name="AppName">Application name</param>
		/// <param name="Separation">How events are separated in the event stream.</param>
		/// <param name="ObjectID">Object ID</param>
		public SyslogEventSink(string Host, int Port, bool Tls, string LocalHostName,
			string AppName, SyslogEventSeparation Separation, string ObjectID)
			: base(ObjectID)
		{
			this.client = new SyslogClient(Host, Port, Tls, LocalHostName, AppName, Separation);
		}

		/// <summary>
		/// Event sink that sends events to a Syslog server using the Syslog protocol.
		/// </summary>
		/// <param name="Host">Syslog server to send events to.</param>
		/// <param name="Port">Syslog server port number to use.</param>
		/// <param name="Certificate">Client certificate for use with mTLS.</param>
		/// <param name="LocalHostName">Local host name</param>
		/// <param name="AppName">Application name</param>
		/// <param name="Separation">How events are separated in the event stream.</param>
		/// <param name="ObjectID">Object ID</param>
		public SyslogEventSink(string Host, int Port, X509Certificate Certificate, 
			string LocalHostName, string AppName, SyslogEventSeparation Separation, 
			string ObjectID)
			: base(ObjectID)
		{
			this.client = new SyslogClient(Host, Port, Certificate, LocalHostName, AppName, Separation);
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override async Task Queue(Event Event)
		{
			try
			{
				await this.client.Send(Event);
			}
			catch (Exception ex)
			{
				Event Event2 = new Event(EventType.Critical, ex, this.ObjectID,
					string.Empty, string.Empty, EventLevel.Medium, string.Empty,
					string.Empty);
				Event2.Avoid(this);

				Log.Event(Event2);
			}
		}

		/// <summary>
		/// <see cref="IDisposableAsync.DisposeAsync()"/>
		/// </summary>
		public override async Task DisposeAsync()
		{
			if (!(this.client is null))
			{
				await this.client.DisposeAsync();
				this.client = null;
			}

			await base.DisposeAsync();
		}

		/// <summary>
		/// Updates the certificate used in mTLS negotiation.
		/// </summary>
		/// <param name="Certificate">Updated Certificate</param>
		public void UpdateCertificate(X509Certificate Certificate)
		{
			this.client.UpdateCertificate(Certificate);
		}
	}
}
