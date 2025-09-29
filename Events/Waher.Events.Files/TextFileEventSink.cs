﻿using System;
using System.IO;
using Waher.Runtime.IO;

namespace Waher.Events.Files
{
	/// <summary>
	/// Outputs sniffed data to a text file.
	/// </summary>
	public class TextFileEventSink : TextWriterEventSink
	{
		private StreamWriter file;
		private readonly FileNameTimeSequence fileSequence;
		private readonly int deleteAfterDays;

		/// <summary>
		/// Outputs sniffed data to a text file.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
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
		public TextFileEventSink(string ObjectID, string FileName)
			: this(ObjectID, FileName, 7)
		{
		}

		/// <summary>
		/// Outputs sniffed data to a text file.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
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
		public TextFileEventSink(string ObjectID, string FileName, int DeleteAfterDays)
			: base(ObjectID, null)
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
		protected override void BeforeWrite()
		{
			if (!this.fileSequence.TryGetNewFileName(out string s))
				return;

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

				Runtime.IO.Files.DeleteOldFiles(FolderName, TimeSpan.FromDays(this.deleteAfterDays), SearchOption.AllDirectories, true);
			}
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
