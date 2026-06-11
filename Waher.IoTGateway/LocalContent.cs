using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Runtime.Temporary;

namespace Waher.IoTGateway
{
	/// <summary>
	/// Static class that gives access to local content published by the gateway, without
	/// having to perform requests to the gateway itself. If requests reference non-local
	/// content, requests are deferred to <see cref="InternetContent"/> instead.
	/// </summary>
	public static class LocalContent
	{
		/// <summary>
		/// Checks if a <see cref="Uri"/> is local to the gateway.
		/// </summary>
		/// <param name="Uri">The <see cref="Uri"/> to check.</param>
		/// <returns>True if the <see cref="Uri"/> is local, false otherwise.</returns>
		public static bool IsLocal(Uri Uri)
		{
			if (!Uri.IsAbsoluteUri)
				return true;

			int[] Ports;

			switch (Uri.Scheme.ToLower())
			{
				case "http":
					Ports = Gateway.HttpServer.OpenHttpPorts;
					break;

				case "https":
					Ports = Gateway.HttpServer.OpenHttpsPorts;
					break;

				default:
					return false;
			}

			if (Ports is null)
				return false;

			if (string.Compare(Uri.Host, "localhost", true) != 0 &&
				!Gateway.IsDomain(Uri.Host, true))
			{
				return false;
			}

			return Array.IndexOf(Ports, Uri.Port) >= 0;
		}

		/// <summary>
		/// Gets a resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Object.</returns>
		public static async Task<ContentResponse> GetAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			if (!IsLocal(Uri) || Gateway.HttpServer is null)
				return await InternetContent.GetAsync(Uri, Gateway.Certificate, Headers);

			using SessionVariables v = new SessionVariables();
			Tuple<int, string, byte[]> T = await Gateway.HttpServer.GET(Uri.AbsolutePath, v);
			int Code = T.Item1;
			string ContentType = T.Item2;
			byte[] Bin = T.Item3;

			if (Code < 200 || Code >= 300)
				return new ContentResponse(new HttpException(Code, HttpException.GetStatusMessage(Code), Bin, ContentType));

			return await InternetContent.DecodeAsync(ContentType, Bin, Uri);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, params KeyValuePair<string, string>[] Headers)
		{
			return GetTempStreamAsync(Uri, null, Headers);
		}

		/// <summary>
		/// Gets a (possibly big) resource, given its URI.
		/// </summary>
		/// <param name="Uri">Uniform resource identifier.</param>
		/// <param name="Destination">Optional destination. Content will be output to this stream. If not provided, a new temporary stream will be created.</param>
		/// <param name="Headers">Optional headers. Interpreted in accordance with the corresponding URI scheme.</param>
		/// <returns>Content-Type, together with a Temporary file, if resource has been downloaded, or null if resource is data-less.</returns>
		public static async Task<ContentStreamResponse> GetTempStreamAsync(Uri Uri, TemporaryStream Destination, params KeyValuePair<string, string>[] Headers)
		{
			if (!IsLocal(Uri) || Gateway.HttpServer is null)
				return await InternetContent.GetTempStreamAsync(Uri, Gateway.Certificate, Headers);

			using SessionVariables v = new SessionVariables();
			Tuple<int, string, byte[]> T = await Gateway.HttpServer.GET(Uri.AbsolutePath, v);
			int Code = T.Item1;
			string ContentType = T.Item2;
			byte[] Bin = T.Item3;

			if (Code < 200 || Code >= 300)
				return new ContentStreamResponse(new HttpException(Code, HttpException.GetStatusMessage(Code), Bin, ContentType));

			bool DestinationCreated = false;

			if (Destination is null)
			{
				Destination = new TemporaryStream();
				DestinationCreated = true;
			}

			try
			{
				await Destination.WriteAsync(Bin, 0, Bin.Length);
			}
			catch (Exception ex)
			{
				if (DestinationCreated)
					Destination.Dispose();

				return new ContentStreamResponse(ex);
			}

			return new ContentStreamResponse(ContentType, Destination);
		}
	}
}
