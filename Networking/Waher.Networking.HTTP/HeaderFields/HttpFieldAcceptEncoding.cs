using System;
using System.Collections.Generic;
using Waher.Events;
using Waher.Networking.HTTP.ContentEncodings;
using Waher.Runtime.Inventory;

namespace Waher.Networking.HTTP.HeaderFields
{
	/// <summary>
	/// Accept-Encoding HTTP Field header. (RFC 2616, §14.3)
	/// </summary>
	public class HttpFieldAcceptEncoding : HttpFieldAcceptRecords
	{
		private static readonly Dictionary<string, IContentEncoding> dynamicEncoders = new Dictionary<string, IContentEncoding>();
		private static readonly Dictionary<string, IContentEncoding> staticEncoders = new Dictionary<string, IContentEncoding>();
		private static readonly object syncObject = new object();
		private static string[] dynamicEncoderLabels = GetEncoderLabels(true, true);
		private static string[] staticEncoderLabels = GetEncoderLabels(false, false);

		private static string[] GetEncoderLabels(bool Dynamic, bool FirstTime)
		{
			Dictionary<string, bool> Labels = new Dictionary<string, bool>();
			bool Supported;

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IContentEncoding)))
			{
				try
				{
					IContentEncoding Encoder = (IContentEncoding)Types.InstantiateDefault(true, T);
					if (Encoder is null)
						continue;

					if (Dynamic)
						Supported = Encoder.SupportsDynamicEncoding;
					else
						Supported = Encoder.SupportsStaticEncoding;

					if (Supported)
						Labels[Encoder.Label] = true;

					if (Encoder is IDisposable Disposable)
						Disposable.Dispose();
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			string[] Result = new string[Labels.Count];
			Labels.Keys.CopyTo(Result, 0);

			if (FirstTime)
				Types.OnInvalidated += Types_OnInvalidated;

			return Result;
		}

		private static void Types_OnInvalidated(object Sender, EventArgs e)
		{
			ContentEncodingsReconfigured();
		}

		/// <summary>
		/// If Content-Encodings have been reconfigured.
		/// </summary>
		public static void ContentEncodingsReconfigured()
		{
			lock (syncObject)
			{
				dynamicEncoders.Clear();
				staticEncoders.Clear();
			}

			dynamicEncoderLabels = GetEncoderLabels(true, false);
			staticEncoderLabels = GetEncoderLabels(false, false);
		}

		/// <summary>
		/// Accept-Encoding HTTP Field header. (RFC 2616, §14.3)
		/// </summary>
		/// <param name="Key">HTTP Field Name</param>
		/// <param name="Value">HTTP Field Value</param>
		public HttpFieldAcceptEncoding(string Key, string Value)
				: base(Key, Value)
		{
		}

		/// <summary>
		/// Tries to get an <see cref="IContentEncoding"/> that best matches this header.
		/// </summary>
		/// <param name="ETag">Optional ETag header value.</param>
		/// <returns>Best <see cref="IContentEncoding"/>, or null if not found.</returns>
		public IContentEncoding TryGetBestContentEncoder(string ETag)
		{
			IContentEncoding Result;
			bool Dynamic = string.IsNullOrEmpty(ETag);

			string BestLabel = this.GetBestAlternative(Dynamic ? dynamicEncoderLabels : staticEncoderLabels);
			if (string.IsNullOrEmpty(BestLabel))
				return null;

			lock (syncObject)
			{
				if ((Dynamic ? dynamicEncoders : staticEncoders).TryGetValue(BestLabel, out Result))
					return Result;
			}

			Result = Types.FindBest<IContentEncoding, string>(BestLabel);

			lock (syncObject)
			{
				if ((Dynamic ? dynamicEncoders : staticEncoders).TryGetValue(BestLabel, out IContentEncoding Result2))
				{
					if (Result is IDisposable Disposable)
						Disposable.Dispose();

					return Result2;
				}
				else
					(Dynamic ? dynamicEncoders : staticEncoders)[BestLabel] = Result;
			}

			return Result;
		}
	}
}
