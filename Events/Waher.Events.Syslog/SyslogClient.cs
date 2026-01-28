using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Runtime.IO;

namespace Waher.Events.Syslog
{
	/// <summary>
	/// Syslog over TCP/IP client application, as defined in RFCs 5224, 5425, 6587:
	/// https://datatracker.ietf.org/doc/html/rfc5424
	/// https://datatracker.ietf.org/doc/html/rfc5425
	/// https://datatracker.ietf.org/doc/html/rfc6587
	/// </summary>
	public class SyslogClient : IDisposable, IDisposableAsync
	{
		/// <summary>
		/// Default Syslog over TCP/IP port number (514).
		/// </summary>
		public const int DefaultPort = 514;

		/// <summary>
		/// Default Syslog over TLS over TCP/IP port number (6514).
		/// </summary>
		public const int DefaultTlsPort = 6514;

		private readonly ISniffer[] sniffers;
		private readonly byte[] localHostName;
		private readonly byte[] appName;
		private readonly string host;
		private readonly int port;
		private readonly bool tls;
		private BinaryTcpClient client = null;

		/// <summary>
		/// Syslog over TCP/IP client application, as defined in RFCs 5224, 5425, 6587:
		/// https://datatracker.ietf.org/doc/html/rfc5424
		/// https://datatracker.ietf.org/doc/html/rfc5425
		/// https://datatracker.ietf.org/doc/html/rfc6587
		/// </summary>
		/// <param name="Host">Syslog server to send events to.</param>
		/// <param name="Port">Syslog server port number to use.</param>
		/// <param name="Tls">If TLS is to be used.</param>
		/// <param name="LocalHostName">Local host name</param>
		/// <param name="AppName">Application name</param>
		/// <param name="Sniffers">Sniffers</param>
		public SyslogClient(string Host, int Port, bool Tls, string LocalHostName,
			string AppName, params ISniffer[] Sniffers)
		{
			this.host = Host;
			this.port = Port;
			this.tls = Tls;
			this.localHostName = EncodeName(LocalHostName);
			this.appName = EncodeName(AppName);
			this.sniffers = Sniffers;
		}

		/// <summary>
		/// Sends an event to the syslog server.
		/// </summary>
		/// <param name="Event">Event to send</param>
		public async Task Send(Event Event)
		{
			bool ResendIfError = true;

			while (true)
			{
				if (!(this.client is null) && !this.client.Connected)
				{
					await this.client.DisposeAsync();
					this.client = null;
				}

				if (this.client is null)
				{
					this.client = new BinaryTcpClient(true, true, this.sniffers);
					await this.client.ConnectAsync(this.host, this.port, this.tls);

					if (this.tls)
					{
						await this.client.UpgradeToTlsAsClient(SslProtocols.Tls12);
						this.client.Continue();
					}
				}

				byte[] Message = this.CreateMessage(Event);
				int MessageLength = Message.Length;
				byte[] Prefix = Encoding.ASCII.GetBytes(MessageLength.ToString());
				int PrefixLength = Prefix.Length;
				byte[] Packet = new byte[PrefixLength + MessageLength + 1];

				Buffer.BlockCopy(Prefix, 0, Packet, 0, PrefixLength);
				Packet[PrefixLength] = (byte)' ';
				Buffer.BlockCopy(Message, 0, Packet, PrefixLength + 1, MessageLength);

				bool Ok;

				try
				{
					Ok = await this.client.SendAsync(true, Packet);
				}
				catch (Exception ex)
				{
					this.client.Exception(ex);
					Ok = false;
				}
				
				if (!Ok)
				{
					if (ResendIfError)
					{
						await this.client.DisposeAsync();
						this.client = null;
					}
					else
						throw new IOException("Unable to send message to Syslog server.");
				}
			}
		}

		/// <summary>
		/// Creates a binary Syslog message from an event.
		/// </summary>
		/// <param name="Event">Event object.</param>
		/// <returns>Binary event object representation.</returns>
		public byte[] CreateMessage(Event Event)
		{
			// Building message, as defined in RFC 5424:

			using MemoryStream ms = new MemoryStream();

			#region HEADER

			// PRI (Priority)

			int Facility = 16;  // Local use 0
			int Severity = Event.Type switch
			{
				EventType.Emergency => 0,
				EventType.Alert => 1,
				EventType.Critical => 2,
				EventType.Error => 3,
				EventType.Warning => 4,
				EventType.Notice => 5,
				EventType.Debug => 7,
				_ => 6,
			};

			ms.WriteByte((byte)'<');
			ms.Write(Encoding.ASCII.GetBytes((Facility * 8 + Severity).ToString()));
			ms.WriteByte((byte)'>');

			// VERSION 

			ms.WriteByte((byte)' ');
			ms.WriteByte((byte)'1');

			// TIMESTAMP

			ms.WriteByte((byte)' ');
			ms.Write(Encoding.ASCII.GetBytes(XML.Encode(Event.Timestamp.ToUniversalTime())));

			// HOSTNAME

			ms.WriteByte((byte)' ');
			ms.Write(this.localHostName);

			// APP-NAME

			ms.WriteByte((byte)' ');
			ms.Write(this.appName);

			// PROCID

			ms.WriteByte((byte)' ');
			ms.Write(EncodeName(Process.GetCurrentProcess().Id.ToString()));

			// MSGID

			ms.WriteByte((byte)' ');
			ms.Write(EncodeName(Event.EventId));

			#endregion

			#region STRUCTURED-DATA

			ms.Write(eventType);
			ms.Write(EncodeValue(Event.Type.ToString()));
			ms.Write(eventLevel);
			ms.Write(EncodeValue(Event.Level.ToString()));

			if (!string.IsNullOrEmpty(Event.Object))
			{
				ms.Write(eventObject);
				ms.Write(EncodeValue(Event.Object));
			}

			if (!string.IsNullOrEmpty(Event.Actor))
			{
				ms.Write(eventActor);
				ms.Write(EncodeValue(Event.Actor));
			}

			if (!string.IsNullOrEmpty(Event.Facility))
			{
				ms.Write(eventFacility);
				ms.Write(EncodeValue(Event.Facility));
			}

			if (!string.IsNullOrEmpty(Event.Module))
			{
				ms.Write(eventModule);
				ms.Write(EncodeValue(Event.Module));
			}

			if (!string.IsNullOrEmpty(Event.StackTrace))
			{
				ms.Write(eventStackTrace);
				ms.Write(EncodeValue(Event.StackTrace));
			}

			if ((Event.Tags?.Length ?? 0) > 0)
			{
				foreach (KeyValuePair<string, object> Tag in Event.Tags)
				{
					ms.WriteByte((byte)'"');
					ms.WriteByte((byte)' ');
					ms.Write(EncodeName(Tag.Key));
					ms.WriteByte((byte)'=');
					ms.WriteByte((byte)'"');
					ms.Write(EncodeValue(Tag.Value?.ToString() ?? string.Empty));
				}
			}

			ms.WriteByte((byte)'"');
			ms.WriteByte((byte)']');

			#endregion

			ms.WriteByte((byte)' ');
			ms.Write(EncodeValue(Event.Message));

			return ms.ToArray();
		}

		private static byte[] EncodeName(string s)
		{
			if (string.IsNullOrEmpty(s))
				return nil;
			else
				return Encoding.ASCII.GetBytes(s.Replace(' ', '_'));
		}

		private static byte[] EncodeValue(string s)
		{
			return Strings.Utf8WithBom.GetBytes(s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("]", "\\]"));
		}

		/// <summary>
		/// Disposes the syslog client.
		/// </summary>
		[Obsolete("Use DisposeAsync instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// Disposes the syslog client.
		/// </summary>
		public async Task DisposeAsync()
		{
			if (!(this.client is null))
			{
				await this.client.DisposeAsync();
				this.client = null;
			}
		}

		private static readonly byte[] nil = new byte[] { (byte)'-' };
		private static readonly byte[] eventType = Encoding.ASCII.GetBytes(" [Event Type=\"");
		private static readonly byte[] eventLevel = Encoding.ASCII.GetBytes("\" Level=\"");
		private static readonly byte[] eventObject = Encoding.ASCII.GetBytes("\" Object=\"");
		private static readonly byte[] eventActor = Encoding.ASCII.GetBytes("\" Actor=\"");
		private static readonly byte[] eventFacility = Encoding.ASCII.GetBytes("\" Facility=\"");
		private static readonly byte[] eventModule = Encoding.ASCII.GetBytes("\" Module=\"");
		private static readonly byte[] eventStackTrace = Encoding.ASCII.GetBytes("\" StackTrace=\"");
	}
}
