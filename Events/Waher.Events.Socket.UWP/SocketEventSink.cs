using System;
using System.Collections.Generic;
#if WINDOWS_UWP
using Windows.Networking.Sockets;
#else
using System.Security.Authentication;
#endif
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Events.Files;
using Waher.Networking;
using Waher.Networking.Sniffers;
using Waher.Security;

namespace Waher.Events.Socket
{
	/// <summary>
	/// Writes logged events to to a socket.
	/// </summary>
	public class SocketEventSink : EventSink
	{
		private readonly SemaphoreSlim synchObj = new SemaphoreSlim(1);
		private BinaryTcpClient client = null;
		private readonly ISniffer[] sniffers;
		private readonly string host;
		private readonly int port;
		private readonly bool tls;

		/// <summary>
		/// Writes logged events to to a socket.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="Host">Host name of server.</param>
		/// <param name="Port">Port number to connect to.</param>
		/// <param name="Tls">If TLS is to be used.</param>
		/// <param name="Sniffers">Sniffers</param>
		public SocketEventSink(string ObjectId, string Host, int Port, bool Tls, params ISniffer[] Sniffers)
			: base(ObjectId)
		{
			this.host = Host;
			this.port = Port;
			this.tls = Tls;
			this.sniffers = Sniffers;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose()"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.client?.Dispose();
			this.client = null;
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override Task Queue(Event Event)
		{
			StringBuilder sb = new StringBuilder();
			string ElementName = Event.Type.ToString();

			sb.Append('<');
			sb.Append(ElementName);
			sb.Append(" xmlns='");
			sb.Append(EventExtensions.LogNamespace);
			sb.Append("' timestamp='");
			sb.Append(XML.Encode(Event.Timestamp));
			sb.Append("' level='");
			sb.Append(Event.Level.ToString());

			if (!string.IsNullOrEmpty(Event.EventId))
			{
				sb.Append("' id='");
				sb.Append(Event.EventId);
			}

			if (!string.IsNullOrEmpty(Event.Object))
			{
				sb.Append("' object='");
				sb.Append(Event.Object);
			}

			if (!string.IsNullOrEmpty(Event.Actor))
			{
				sb.Append("' actor='");
				sb.Append(Event.Actor);
			}

			if (!string.IsNullOrEmpty(Event.Module))
			{
				sb.Append("' module='");
				sb.Append(Event.Module);
			}

			if (!string.IsNullOrEmpty(Event.Facility))
			{
				sb.Append("' facility='");
				sb.Append(Event.Facility);
			}

			sb.Append("'><Message>");

			foreach (string Row in GetRows(Event.Message))
			{
				sb.Append("<Row>");
				sb.Append(XML.Encode(Row));
				sb.Append("</Row>");
			}

			sb.Append("</Message>");

			if (!(Event.Tags is null) && Event.Tags.Length > 0)
			{
				foreach (KeyValuePair<string, object> Tag in Event.Tags)
				{
					sb.Append("<Tag key='");
					sb.Append(XML.Encode(Tag.Key));

					if (!(Tag.Value is null))
					{
						sb.Append("' value='");
						sb.Append(XML.Encode(Tag.Value.ToString()));
					}

					sb.Append("'/>");
				}
			}

			if (Event.Type >= EventType.Critical && !string.IsNullOrEmpty(Event.StackTrace))
			{
				sb.Append("<StackTrace>");

				foreach (string Row in GetRows(Event.StackTrace))
				{
					sb.Append("<Row>");
					sb.Append(XML.Encode(Row));
					sb.Append("</Row>");
				}

				sb.Append("</StackTrace>");
			}

			sb.Append("</");
			sb.Append(ElementName);
			sb.Append('>');

			return this.Queue(sb.ToString(), false);
		}

		/// <summary>
		/// Queues XML-encoded information to be output.
		/// </summary>
		/// <param name="Xml">XML to queue to the pipe.</param>
		public Task Queue(string Xml)
		{
			return this.Queue(Xml, true);
		}

		/// <summary>
		/// Queues XML-encoded information to be output.
		/// </summary>
		/// <param name="Xml">XML to queue to the pipe.</param>
		private async Task Queue(string Xml, bool ValidateXml)
		{
			if (ValidateXml && !XML.IsValidXml(Xml))
				throw new ArgumentException("Invalid XML.", nameof(Xml));

			try
			{
				byte[] Bin = Encoding.UTF8.GetBytes(Xml);

				await this.synchObj.WaitAsync();
				try
				{
					if (this.client is null || !this.client.Connected)
					{
						this.client?.Dispose();
						this.client = null;

						this.client = new BinaryTcpClient(false, this.sniffers);

						this.client.OnDisconnected += this.Client_OnDisconnected;
						this.client.OnReceived += this.Client_OnReceived;
						this.client.OnWriteQueueEmpty += this.Client_OnWriteQueueEmpty;

						if (!await this.client.ConnectAsync(this.host, this.port, this.tls))
						{
							Event Event = new Event(EventType.Error, "Unable to connect to event recipient.", this.ObjectID,
								string.Empty, string.Empty, EventLevel.Major, string.Empty, string.Empty, string.Empty);

							Event.Avoid(this);
							Log.Event(Event);

							this.client = null;
							return;
						}

						if (this.tls)
						{
#if WINDOWS_UWP
							await this.client.UpgradeToTlsAsClient(SocketProtectionLevel.Tls12);
#else
							await this.client.UpgradeToTlsAsClient(Crypto.SecureTls);
#endif
							this.client.Continue();
						}
					}

					if (!await this.client.SendAsync(true, Bin))
					{
						Event Event = new Event(EventType.Error, "Unable to sent event to event recipient.", this.ObjectID,
							string.Empty, string.Empty, EventLevel.Major, string.Empty, string.Empty, string.Empty);

						Event.Avoid(this);
						Log.Event(Event);

						this.client?.DisposeWhenDone();
						this.client = null;
					}
				}
				finally
				{
					this.synchObj.Release();
				}
			}
			catch (Exception ex)
			{
				Event Event = new Event(EventType.Critical, ex, this.ObjectID, string.Empty, string.Empty, EventLevel.Major, 
					string.Empty, string.Empty);

				Event.Avoid(this);
				Log.Event(Event);

				this.client?.DisposeWhenDone();
				this.client = null;
			}
		}

		private Task Client_OnWriteQueueEmpty(object Sender, EventArgs e)
		{
			BinaryTcpClient Client = this.client;
			this.client = null;

			Client.DisposeWhenDone();

			return Task.CompletedTask;
		}

		private Task<bool> Client_OnReceived(object Sender, byte[] Buffer, int Offset, int Count)
		{
			return Task.FromResult(true);	// Ignore incoming communication.
		}

		private Task Client_OnDisconnected(object Sender, EventArgs e)
		{
			if (this.client == Sender)
				this.client = null;

			return Task.CompletedTask;
		}

		private static string[] GetRows(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		}
	}
}
