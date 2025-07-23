﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Getters;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.Events;
using Waher.Runtime.IO;

namespace Waher.Networking.XMPP.HttpFileUpload
{
	/// <summary>
	/// Event arguments for HTTP File Upload callback methods.
	/// </summary>
	public class HttpFileUploadEventArgs : IqResultEventArgs
	{
		/// <summary>
		/// Maximum chunk size, in ranged PUT/PATCH requests, is 20 MB by default.
		/// </summary>
		public const int DefaultMaxChunkSize = 20 * 1024 * 1024;

		private readonly KeyValuePair<string, string>[] putHeaders = null;
		private readonly HttpFileUploadClient client;
		private readonly string putUrl = null;
		private readonly string getUrl = null;
		private readonly ISniffer[] sniffers;
		private readonly bool hasSniffers;
		private int maxChunkSize;

		/// <summary>
		/// Event arguments for HTTP File Upload callback methods.
		/// </summary>
		/// <param name="e">IQ response.</param>
		/// <param name="Client">HTTP File Upload Client reference.</param>
		/// <param name="GetUrl">GET URL.</param>
		/// <param name="PutUrl">PUT URL.</param>
		/// <param name="PutHeaders">HTTP Headers for PUT request.</param>
		/// <param name="Sniffers">Sniffers to log HTTP request and response to.</param>
		public HttpFileUploadEventArgs(IqResultEventArgs e, HttpFileUploadClient Client, 
			string GetUrl, string PutUrl, KeyValuePair<string, string>[] PutHeaders,
			params ISniffer[] Sniffers)
			: this(e, Client, GetUrl, PutUrl, PutHeaders, DefaultMaxChunkSize, Sniffers)
		{
		}

		/// <summary>
		/// Event arguments for HTTP File Upload callback methods.
		/// </summary>
		/// <param name="e">IQ response.</param>
		/// <param name="Client">HTTP File Upload Client reference.</param>
		/// <param name="GetUrl">GET URL.</param>
		/// <param name="PutUrl">PUT URL.</param>
		/// <param name="PutHeaders">HTTP Headers for PUT request.</param>
		/// <param name="MaxChunkSize">Maximum chunk size in ranged PUT requests. (20 MB by default)</param>
		/// <param name="Sniffers">Sniffers to log HTTP request and response to.</param>
		public HttpFileUploadEventArgs(IqResultEventArgs e, HttpFileUploadClient Client,
			string GetUrl, string PutUrl, KeyValuePair<string, string>[] PutHeaders, 
			int MaxChunkSize, params ISniffer[] Sniffers)
			: base(e)
		{
			this.client = Client;
			this.getUrl = GetUrl;
			this.putUrl = PutUrl;
			this.putHeaders = PutHeaders;
			this.maxChunkSize = MaxChunkSize;
			this.sniffers = Sniffers;
			this.hasSniffers = (this.sniffers?.Length ?? 0) > 0;
		}

		/// <summary>
		/// HTTP File Upload Client reference.
		/// </summary>
		public HttpFileUploadClient Client => this.client;

		/// <summary>
		/// GET URL.
		/// </summary>
		public string GetUrl => this.getUrl;

		/// <summary>
		/// PUT URL.
		/// </summary>
		public string PutUrl => this.putUrl;

		/// <summary>
		/// HTTP Headers for PUT request.
		/// </summary>
		public KeyValuePair<string, string>[] PutHeaders => this.putHeaders;

		/// <summary>
		/// Maximum Chunk Size. If uploading a larger file, ranged PUT/PATCH requests will
		/// be performed until file is completely uploaded. The timeout property is
		/// calculated on each ranged PUT request individually, and not on the total
		/// time it takes to upload the entire file.
		/// </summary>
		public int MaxChunkSize
		{
			get => this.maxChunkSize;
			set
			{
				if (value <= 0 || value > this.maxChunkSize)
					throw new ArgumentOutOfRangeException(nameof(this.MaxChunkSize), "Maximum cannot be negative, or larger than the suggested maximum file size.");

				this.maxChunkSize = value;
			}
		}

		/// <summary>
		/// If the server certificate can be trusted, by default.
		/// </summary>
		public bool TrustServer => this.client?.Client?.TrustServer ?? false;

		/// <summary>
		/// Uploads file content to the server.
		/// </summary>
		/// <param name="Content">Content to upload</param>
		/// <param name="ContentType">Content-Type</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public Task PUT(byte[] Content, string ContentType, int Timeout)
		{
			return this.PUT(Content, ContentType, Timeout, this.TrustServer);
		}

		/// <summary>
		/// Uploads file content to the server.
		/// </summary>
		/// <param name="Content">Content to upload</param>
		/// <param name="ContentType">Content-Type</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="TrustServer">If server certificate should be trusted.</param>
		public async Task PUT(byte[] Content, string ContentType, int Timeout, bool TrustServer)
		{
			if (Content.Length <= this.maxChunkSize)
			{
				HttpContent Body = new ByteArrayContent(Content);
				await this.PUT(Body, ContentType, Timeout, TrustServer);
			}
			else
			{
				using (MemoryStream ms = new MemoryStream(Content))
				{
					await this.PUT(ms, ContentType, Timeout, TrustServer);
				}
			}
		}

		/// <summary>
		/// Uploads file content to the server.
		/// </summary>
		/// <param name="Content">Content to upload</param>
		/// <param name="ContentType">Content-Type</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public Task PUT(Stream Content, string ContentType, int Timeout)
		{
			return this.PUT(Content, ContentType, Timeout, this.TrustServer);
		}

		/// <summary>
		/// Uploads file content to the server.
		/// </summary>
		/// <param name="Content">Content to upload</param>
		/// <param name="ContentType">Content-Type</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="TrustServer">If server certificate should be trusted.</param>
		public async Task PUT(Stream Content, string ContentType, int Timeout, bool TrustServer)
		{
			Content.Position = 0;

			if (Content.Length <= this.maxChunkSize)
			{
				HttpContent Body = new StreamContent(Content);
				await this.PUT(Body, ContentType, Timeout, TrustServer);
			}
			else
			{
				HttpClientHandler Handler = WebGetter.GetClientHandler(TrustServer);
				
				using (HttpClient HttpClient = new HttpClient(Handler, true))
				{
					long Pos = 0;
					long Len = Content.Length;
					byte[] Buffer = null;

					HttpClient.Timeout = TimeSpan.FromMilliseconds(Timeout);
					HttpClient.DefaultRequestHeaders.ExpectContinue = false;

					while (Pos < Len)
					{
						int c = (int)Math.Min(Len - Pos, this.maxChunkSize);

						if (Buffer is null || Buffer.Length != c)
							Buffer = new byte[c];

						await Content.ReadAllAsync(Buffer, 0, c);

						HttpContent Body = new ByteArrayContent(Buffer);

						if (!(this.putHeaders is null))
						{
							foreach (KeyValuePair<string, string> P in this.putHeaders)
								Body.Headers.Add(P.Key, P.Value);
						}

						Body.Headers.Add("Content-Type", ContentType);
						Body.Headers.Add("Content-Range", "bytes " + Pos.ToString() + "-" + (Pos + c - 1).ToString() + "/" + Len.ToString());

						// Ranged PATCH
				
						using (HttpRequestMessage RequestMessage = new HttpRequestMessage(
							new HttpMethod("PATCH"), this.putUrl)
							{
								Content = Body
							})
						{
							this.LogSent(HttpClient, this.PutUrl, Body, "PATCH");

							HttpResponseMessage Response = await HttpClient.SendAsync(RequestMessage);

							this.LogReceived(Response);

							if (!Response.IsSuccessStatusCode)
							{
								ContentResponse Temp = await Waher.Content.Getters.WebGetter.ProcessResponse(Response, new Uri(this.PutUrl));
								Temp.AssertOk();
							}
						}

						Pos += c;
					}
				}
			}
		}

		/// <summary>
		/// Uploads file content to the server.
		/// </summary>
		/// <param name="Content">Content to upload</param>
		/// <param name="ContentType">Content-Type</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		public Task PUT(HttpContent Content, string ContentType, int Timeout)
		{
			return this.PUT(Content, ContentType, Timeout, this.TrustServer);
		}

		/// <summary>
		/// Uploads file content to the server.
		/// </summary>
		/// <param name="Content">Content to upload</param>
		/// <param name="ContentType">Content-Type</param>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="TrustServer">If server certificate should be trusted.</param>
		public async Task PUT(HttpContent Content, string ContentType, int Timeout, bool TrustServer)
		{
			HttpClientHandler Handler = WebGetter.GetClientHandler(TrustServer);

			using (HttpClient HttpClient = new HttpClient(Handler, true))
			{
				HttpClient.Timeout = TimeSpan.FromMilliseconds(Timeout);
				HttpClient.DefaultRequestHeaders.ExpectContinue = false;

				if (!(this.putHeaders is null))
				{
					foreach (KeyValuePair<string, string> P in this.putHeaders)
						Content.Headers.Add(P.Key, P.Value);
				}

				Content.Headers.Add("Content-Type", ContentType);
				this.LogSent(HttpClient, this.PutUrl, Content, "PUT");

				HttpResponseMessage Response = await HttpClient.PutAsync(this.putUrl, Content);

				this.LogReceived(Response);

				if (!Response.IsSuccessStatusCode)
				{
					ContentResponse Temp = await Waher.Content.Getters.WebGetter.ProcessResponse(Response, new Uri(this.PutUrl));
					Temp.AssertOk();
				}

				if (Response.StatusCode != System.Net.HttpStatusCode.Created)
					throw new IOException("Unexpected response.");
			}
		}

		private void LogSent(HttpClient Client, string PutUrl, HttpContent Content, string Method)
		{
			if (!this.hasSniffers)
				return;

			StringBuilder sb = new StringBuilder();

			sb.Append(Method);

			if (Content.Headers.ContentLength.HasValue)
			{
				sb.Append(' ');
				sb.Append(Content.Headers.ContentLength.Value.ToString());
				sb.Append(" bytes");
			}

			sb.Append(" to ");
			sb.AppendLine(PutUrl);

			SortedDictionary<string, IEnumerable<string>> Headers = new SortedDictionary<string, IEnumerable<string>>();

			foreach (KeyValuePair<string, IEnumerable<string>> Header in Client.DefaultRequestHeaders)
				Headers[Header.Key] = Header.Value;

			foreach (KeyValuePair<string, IEnumerable<string>> Header in Content.Headers)
				Headers[Header.Key] = Header.Value;

			foreach (KeyValuePair<string, IEnumerable<string>> Header in Headers)
			{
				foreach (string Value in Header.Value)
				{
					sb.Append(Header.Key);
					sb.Append(": ");
					sb.AppendLine(Value);
				}
			}

			string Msg = sb.ToString();

			foreach (ISniffer Sniffer in this.sniffers)
				Sniffer.TransmitText(Msg);
		}

		private void LogReceived(HttpResponseMessage Response)
		{
			if (!this.hasSniffers)
				return;

			StringBuilder sb = new StringBuilder();

			sb.Append(((int)Response.StatusCode).ToString());
			sb.Append(' ');
			sb.AppendLine(Response.StatusCode.ToString());

			SortedDictionary<string, IEnumerable<string>> Headers = new SortedDictionary<string, IEnumerable<string>>();

			foreach (KeyValuePair<string, IEnumerable<string>> Header in Response.Headers)
				Headers[Header.Key] = Header.Value;

			foreach (KeyValuePair<string, IEnumerable<string>> Header in Headers)
			{
				foreach (string Value in Header.Value)
				{
					sb.Append(Header.Key);
					sb.Append(": ");
					sb.AppendLine(Value);
				}
			}

			string Msg = sb.ToString();

			foreach (ISniffer Sniffer in this.sniffers)
				Sniffer.ReceiveText(Msg);
		}

	}
}
