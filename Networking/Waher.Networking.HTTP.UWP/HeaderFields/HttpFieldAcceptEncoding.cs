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
		private static readonly Dictionary<string, IContentEncoding> encoders = new Dictionary<string, IContentEncoding>();
		private static string[] encoderLabels = GetEncoderLabels();

		private static string[] GetEncoderLabels()
		{
			Dictionary<string, bool> Labels = new Dictionary<string, bool>();

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(IContentEncoding)))
			{
				try
				{
					IContentEncoding Encoder = (IContentEncoding)Types.InstantiateDefault(true, T);
					if (Encoder is null)
						continue;

					Labels[Encoder.Label] = true;

					if (Encoder is IDisposable Disposable)
						Disposable.Dispose();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			string[] Result = new string[Labels.Count];
			Labels.Keys.CopyTo(Result, 0);

			Types.OnInvalidated += Types_OnInvalidated;

			return Result;
		}

		private static void Types_OnInvalidated(object sender, System.EventArgs e)
		{
			lock (encoders)
			{
				encoders.Clear();
			}

			encoderLabels = GetEncoderLabels();
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
		/// <returns>Best <see cref="IContentEncoding"/>, or null if not found.</returns>
		public IContentEncoding TryGetBestContentEncoder()
		{
			IContentEncoding Result;

			string BestLabel = this.GetBestAlternative(encoderLabels);
			if (string.IsNullOrEmpty(BestLabel))
				return null;

            lock (encoders)
			{
				if (encoders.TryGetValue(BestLabel, out Result))
					return Result;
			}

			Result = Types.FindBest<IContentEncoding, string>(BestLabel);

			lock (encoders)
			{
				if (encoders.TryGetValue(BestLabel, out IContentEncoding Result2))
				{
					if (Result is IDisposable Disposable)
						Disposable.Dispose();

					return Result2;
				}
				else
					encoders[BestLabel] = Result;
			}

			return Result;
		}
	}
}
