using System;
using System.IO;
using Waher.Events;
using Waher.Networking.HTTP.ContentEncodings;
using Waher.Runtime.Inventory;
using Waher.Runtime.Timing;

namespace Waher.Networking.HTTP.Brotli
{
	/// <summary>
	/// Content-Encoding: br
	/// </summary>
	public class BrotliContentEncoding : IContentEncoding
	{
		private static readonly Random rnd = new Random();
		private static Scheduler? scheduler = null;
		private static string? appDataFolder = null;
		private static string? brotliFolder = null;

		/// <summary>
		/// Label identifying the Content-Encoding
		/// </summary>
		public string Label => "br";

		/// <summary>
		/// If encoding can be used for dynamic encoding.
		/// </summary>
		public bool SupportsDynamicEncoding => false;       // Brotly encoding is slow, but efficient, suitable only for static content.

		/// <summary>
		/// If encoding can be used for static encoding.
		/// </summary>
		public bool SupportsStaticEncoding => true;

		/// <summary>
		/// How well the Content-Encoding handles the encoding specified by <paramref name="Label"/>.
		/// </summary>
		/// <param name="Label">Content-Encoding label.</param>
		/// <returns>How well the Content-Encoding is supported.</returns>
		public Grade Supports(string Label) => Label == this.Label ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets an encoder.
		/// </summary>
		/// <param name="Output">Output stream.</param>
		/// <param name="ExpectedContentLength">Expected content length, if known.</param>
		/// <param name="ETag">Optional ETag value for content.</param>
		/// <returns>Encoder</returns>
		public TransferEncoding GetEncoder(TransferEncoding Output, long? ExpectedContentLength, string ETag)
		{
			string? CompressedFileName;

			if (!string.IsNullOrEmpty(brotliFolder))
			{
				CompressedFileName = Path.Combine(brotliFolder, ETag + ".br");

				if (File.Exists(CompressedFileName))
					return new BrotliReturner(CompressedFileName, Output);
			}
			else
				CompressedFileName = null;

			BrotliEncoder Encoder = new BrotliEncoder(CompressedFileName, Output, ExpectedContentLength);
			Encoder.PrepareForCompression();
			return Encoder;
		}

		/// <summary>
		/// Initializes the Brotli-compression integration.
		/// </summary>
		/// <param name="AppDataFolder">Content application data folder. This folder would correspond to the Gateway ProgramData folder.
		/// Compressed files will be maintained in a subfolder to this folder named "Brotli".</param>
		public static void Init(string AppDataFolder)
		{
			try
			{
				appDataFolder = AppDataFolder;
				brotliFolder = Path.Combine(appDataFolder, "Brotli");

				if (!Directory.Exists(brotliFolder))
					Directory.CreateDirectory(brotliFolder);

				if (scheduler is null)
				{
					if (Types.TryGetModuleParameter("Scheduler", out object Obj) && Obj is Scheduler Scheduler)
						scheduler = Scheduler;
					else
					{
						scheduler = new Scheduler();

						Log.Terminating += (sender, e) =>
						{
							scheduler?.Dispose();
							scheduler = null;
						};
					}
				}

				DeleteOldFiles(TimeSpan.FromDays(7));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private static void DeleteOldFiles(object P)
		{
			if (P is TimeSpan MaxAge)
				DeleteOldFiles(MaxAge, true);
		}

		/// <summary>
		/// Deletes generated files older than <paramref name="MaxAge"/>.
		/// </summary>
		/// <param name="MaxAge">Age limit.</param>
		/// <param name="Reschedule">If rescheduling should be done.</param>
		public static void DeleteOldFiles(TimeSpan MaxAge, bool Reschedule)
		{
			DateTime Limit = DateTime.Now - MaxAge;
			int Count = 0;

			DirectoryInfo BrotliFolder = new DirectoryInfo(brotliFolder);
			FileInfo[] Files = BrotliFolder.GetFiles("*.*");

			foreach (FileInfo FileInfo in Files)
			{
				if (FileInfo.LastAccessTime < Limit)
				{
					try
					{
						File.Delete(FileInfo.FullName);
						Count++;
					}
					catch (Exception ex)
					{
						Log.Error("Unable to delete old file: " + ex.Message, FileInfo.FullName);
					}
				}
			}

			if (Count > 0)
				Log.Informational(Count.ToString() + " old file(s) deleted.", brotliFolder);

			if (Reschedule)
			{
				lock (rnd)
				{
					scheduler?.Add(DateTime.Now.AddDays(rnd.NextDouble() * 2), DeleteOldFiles, MaxAge);
				}
			}
		}

	}
}
