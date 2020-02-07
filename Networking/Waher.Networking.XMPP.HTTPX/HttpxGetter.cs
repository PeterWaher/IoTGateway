using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.HeaderFields;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.HTTPX
{
	/// <summary>
	/// Content Getter, retrieving content using the HTTPX URI Scheme.
	/// 
	/// For the getter to work, the application needs to register a <see cref="HttpxProxy"/> object instance
	/// as a Model Parameter named HTTPX.
	/// </summary>
	public class HttpxGetter : IContentGetter
	{
		private static HttpxProxy proxy = null;

		/// <summary>
		/// Content Getter, retrieving content using the HTTPX URI Scheme.
		/// 
		/// For the getter to work, the application needs to register a <see cref="HttpxProxy"/> object instance
		/// as a Model Parameter named HTTPX.
		/// </summary>
		public HttpxGetter()
		{
		}

		/// <summary>
		/// Supported URI schemes.
		/// </summary>
		public string[] UriSchemes => new string[] { "httpx" };

		/// <summary>
		/// If the getter is able to get a resource, given its URI.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Grade">How well the getter would be able to get a resource given the indicated URI.</param>
		/// <returns>If the getter can get a resource with the indicated URI.</returns>
		public bool CanGet(Uri Uri, out Grade Grade)
		{
			switch (Uri.Scheme)
			{
				case "httpx":
					Grade = Grade.Ok;
					return true;

				default:
					Grade = Grade.NotAtAll;
					return false;
			}
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Decoded object.</returns>
		public Task<object> GetAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			return this.GetAsync(Uri, 60000, Headers);
		}

		/// <summary>
		/// Gets a resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <exception cref="InvalidOperationException">No <see cref="HttpxProxy"/> set in the HTTPX <see cref="Types"/> module parameter.</exception>
		/// <exception cref="ArgumentException">If the <paramref name="Uri"/> parameter is invalid.</exception>
		/// <exception cref="ArgumentException">If the object response be decoded.</exception>
		/// <exception cref="ConflictException">If an approved presence subscription with the remote entity does not exist.</exception>
		/// <exception cref="ServiceUnavailableException">If the remote entity is not online.</exception>
		/// <exception cref="TimeoutException">If the request times out.</exception>
		/// <exception cref="OutOfMemoryException">If resource too large to decode.</exception>
		/// <exception cref="IOException">If unable to read from temporary file.</exception>
		/// <returns>Decoded object.</returns>
		public async Task<object> GetAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			KeyValuePair<string, TemporaryFile> Rec = await this.GetTempFileAsync(Uri, TimeoutMs, Headers);
			string ContentType = Rec.Key;
			TemporaryFile File = Rec.Value;

			try
			{
				if (File is null)
					return null;

				File.Position = 0;

				if (File.Length > int.MaxValue)
					throw new OutOfMemoryException("Resource too large.");

				int Len = (int)File.Length;
				byte[] Bin = new byte[Len];
				if (await File.ReadAsync(Bin, 0, Len) != Len)
					throw new IOException("Unable to read from file.");

				return InternetContent.Decode(ContentType, Bin, Uri);
			}
			finally
			{
				File?.Dispose();
			}
		}

		/// <summary>
		/// Gets a (possibly big) resource, using a Uniform Resource Identifier (or Locator).
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <param name="TimeoutMs">Timeout, in milliseconds. (Default=60000)</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <exception cref="InvalidOperationException">No <see cref="HttpxProxy"/> set in the HTTPX <see cref="Types"/> module parameter.</exception>
		/// <exception cref="ArgumentException">If the <paramref name="Uri"/> parameter is invalid.</exception>
		/// <exception cref="ConflictException">If an approved presence subscription with the remote entity does not exist.</exception>
		/// <exception cref="ServiceUnavailableException">If the remote entity is not online.</exception>
		/// <exception cref="TimeoutException">If the request times out.</exception>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public async Task<KeyValuePair<string, TemporaryFile>> GetTempFileAsync(Uri Uri, int TimeoutMs, params KeyValuePair<string, string>[] Headers)
		{
			if (proxy is null)
			{
				if (!Types.TryGetModuleParameter("HTTPX", out object Obj) ||
					!(Obj is HttpxProxy Proxy))
				{
					throw new InvalidOperationException("A HTTPX Proxy object has not been registered.");
				}

				proxy = Proxy;
			}

			GetClientResponse Rec = await proxy.GetClientAsync(Uri);
			List<HttpField> Headers2 = new List<HttpField>();

			foreach (KeyValuePair<string, string> Header in Headers)
			{
				switch (Header.Key.ToLower())
				{
					case "host":
						Headers2.Add(new HttpField("Host", Rec.BareJid));
						break;

					case "cookie":
					case "set-cookie":
						// Do not forward cookies.
						break;

					default:
						Headers2.Add(new HttpField(Header.Key, Header.Value));
						break;
				}
			}

			State State = null;
			Timer Timer = null;

			try
			{
				State = new State();
				Timer = new Timer((P) =>
				{
					State.Done.TrySetResult(false);
				}, null, TimeoutMs, Timeout.Infinite);

				Rec.HttpxClient.Request(Rec.FullJid, "GET", Rec.LocalUrl, (sender, e) =>
				{
					if (e.Ok)
					{
						State.HttpResponse = e.HttpResponse;

						if (e.StatusCode >= 200 && e.StatusCode < 300)
						{
							if (e.HasData)
							{
								State.File = new TemporaryFile();

								if (!(e.Data is null))
								{
									State.File.Write(e.Data, 0, e.Data.Length);
									State.Done.TrySetResult(true);
								}
							}
							else
								State.Done.TrySetResult(true);
						}
						else
							State.Done.TrySetException(new HttpException(e.StatusCode, e.StatusMessage));
					}
					else
						State.Done.TrySetException(new IOException(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get resource." : e.ErrorText));

				}, (sender, e) =>
				{
					State.File.Write(e.Data, 0, e.Data.Length);
					if (e.Last)
						State.Done.TrySetResult(true);

				}, State, Headers2.ToArray());

				if (!await State.Done.Task)
					throw new TimeoutException("Request timed out.");

				Timer.Dispose();
				Timer = null;

				TemporaryFile Result = State.File;
				State.File = null;

				return new KeyValuePair<string, TemporaryFile>(State.HttpResponse?.ContentType, Result);
			}
			finally
			{
				State.File?.Dispose();
				State.File = null;

				State.HttpResponse?.Dispose();
				State.HttpResponse = null;

				Timer?.Dispose();
				Timer = null;
			}
		}

		private class State
		{
			public HttpResponse HttpResponse = null;
			public TemporaryFile File = null;
			public TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>();
		}

	}
}
