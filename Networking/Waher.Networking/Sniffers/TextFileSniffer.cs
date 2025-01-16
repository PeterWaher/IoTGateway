using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.IO;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to a text file.
	/// </summary>
	public class TextFileSniffer : TextWriterSniffer
	{
		private StreamWriter file;
		private readonly FileNameTimeSequence fileSequence;
		private readonly int deleteAfterDays;

		/// <summary>
		/// Outputs sniffed data to a text file.
		/// </summary>
		/// <param name="FileName">File Name. The following strings will be replaced by current values:
		/// 
		/// %YEAR% = Current year.
		/// %MONTH% = Current month.
		/// %DAY% = Current day.
		/// %HOUR% = Current hour.
		/// %MINUTE% = Current minute.
		/// %SECOND% = Current second.
		/// 
		/// NOTE: Make sure files are stored in a separate folder, as old files will be automatically deleted.
		/// </param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public TextFileSniffer(string FileName, BinaryPresentationMethod BinaryPresentationMethod)
			: this(FileName, 7, BinaryPresentationMethod)
		{
		}

		/// <summary>
		/// Outputs sniffed data to a text file.
		/// </summary>
		/// <param name="FileName">File Name. The following strings will be replaced by current values:
		/// 
		/// %YEAR% = Current year.
		/// %MONTH% = Current month.
		/// %DAY% = Current day.
		/// %HOUR% = Current hour.
		/// %MINUTE% = Current minute.
		/// %SECOND% = Current second.
		/// 
		/// NOTE: Make sure files are stored in a separate folder, as old files will be automatically deleted.
		/// </param>
		/// <param name="DeleteAfterDays">Number of days files will be kept. All files older than this
		/// in the corresponding folder will be removed. Default value is 7 days.</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public TextFileSniffer(string FileName, int DeleteAfterDays, BinaryPresentationMethod BinaryPresentationMethod)
			: base(null, BinaryPresentationMethod)
		{
			this.file = null;
			this.output = null;
			this.fileSequence = new FileNameTimeSequence(FileName, true);
			this.deleteAfterDays = DeleteAfterDays;

			string FolderName = Path.GetDirectoryName(FileName);

			if (!Directory.Exists(FolderName))
			{
				Log.Informational("Creating folder.", FolderName);
				Directory.CreateDirectory(FolderName);
			}
		}

		/// <summary>
		/// Method is called before writing something to the text file.
		/// </summary>
		protected override Task BeforeWrite()
		{
			if (!this.fileSequence.TryGetNewFileName(out string s))
				return Task.CompletedTask;

			try
			{
				this.file?.Dispose();
			}
			catch (Exception)
			{
				// Ignore
			}

			this.file = null;
			this.output = null;

			this.output = this.file = File.CreateText(s);

			if (this.deleteAfterDays < int.MaxValue)
			{
				string FolderName = Path.GetDirectoryName(s);
				if (string.IsNullOrEmpty(FolderName))
					FolderName = ".";

				string[] Files = Directory.GetFiles(FolderName, "*.*");

				foreach (string FileName in Files)
				{
					if ((DateTime.UtcNow - File.GetLastWriteTimeUtc(FileName)).TotalDays >= this.deleteAfterDays)
					{
						try
						{
							File.Delete(FileName);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					}
				}
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Disposes of the current output.
		/// </summary>
		public override void DisposeOutput()
		{
			base.DisposeOutput();

			this.file?.Dispose();
			this.file = null;
		}
	}
}
