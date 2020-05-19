using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Networking.DNS.Communication
{
	/// <summary>
	/// Implements a DNS over HTTPS (DoH)-based client.
	/// </summary>
	public class DnsHttpsClient : DnsClient
	{
        private Uri uri;

		/// <summary>
		/// Implements a DNS over HTTPS (DoH)-based client.
		/// </summary>
		public DnsHttpsClient(Uri Uri)
			: base()
		{
			this.uri = Uri;
			this.Init();
		}

		/// <summary>
		/// DNS over HTTPS URI.
		/// </summary>
		public Uri Uri
		{
			get => this.uri;
			set => this.uri = value;
		}

		/// <summary>
		/// Sends a message to a destination.
		/// </summary>
		/// <param name="Message">Message</param>
		/// <param name="Destination">Destination. If null, default destination
		/// is assumed.</param>
		protected override async Task SendAsync(byte[] Message, IPEndPoint Destination)
		{
			using (HttpClient HttpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMilliseconds(10000)
			})
			{
				using (HttpRequestMessage Request = new HttpRequestMessage()
				{
					RequestUri = this.uri,
					Method = HttpMethod.Post
				})
				{
					Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/dns-message"));
					Request.Content = new ByteArrayContent(Message);
					Request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/dns-message");
					
					HttpResponseMessage Response = await HttpClient.SendAsync(Request);

					if (Response.IsSuccessStatusCode)
					{
						byte[] Bin = await Response.Content.ReadAsByteArrayAsync();

						this.ReceiveBinary(Bin);

						try
						{
							DnsMessage DnsResponse = new DnsMessage(Bin);
							this.ProcessIncomingMessage(DnsResponse);
						}
						catch (Exception ex)
						{
							Log.Error("Unable to process DNS packet: " + ex.Message);
						}
					}
					else
					{
						ushort ID = Message[0];
						ID <<= 8;
						ID |= Message[1];

						this.ProcessMessageFailure(ID);
					}
				}
			}
		}

	}
}
