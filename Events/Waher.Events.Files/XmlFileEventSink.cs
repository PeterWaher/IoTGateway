using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.IO;

namespace Waher.Events.Files
{
	/// <summary>
	/// Outputs sniffed data to an XML file.
	/// </summary>
	public class XmlFileEventSink : XmlWriterEventSink
	{
		private readonly XmlWriterSettings settings;
		private readonly FileNameTimeSequence fileSequence;
		private StreamWriter file;
		private readonly string transform = null;
		private readonly int deleteAfterDays;

		/// <summary>
		/// Outputs sniffed data to an XML file.
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
		public XmlFileEventSink(string ObjectID, string FileName)
			: this(ObjectID, FileName, string.Empty, 7)
		{
		}

		/// <summary>
		/// Outputs sniffed data to an XML file.
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
		public XmlFileEventSink(string ObjectID, string FileName, int DeleteAfterDays)
			: this(ObjectID, FileName, string.Empty, DeleteAfterDays)
		{
		}

		/// <summary>
		/// Outputs sniffed data to an XML file.
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
		/// <param name="Transform">Transform file name.</param>
		public XmlFileEventSink(string ObjectID, string FileName, string Transform)
			: this(ObjectID, FileName, Transform, 7)
		{
		}

		/// <summary>
		/// Outputs sniffed data to an XML file.
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
		/// <param name="Transform">Transform file name.</param>
		/// <param name="DeleteAfterDays">Number of days files will be kept. All files older than this
		/// in the corresponding folder will be removed. Default value is 7 days.</param>
		public XmlFileEventSink(string ObjectID, string FileName, string Transform, int DeleteAfterDays)
			: base(ObjectID, null)
		{
			this.file = null;
			this.output = null;
			this.fileSequence = new FileNameTimeSequence(FileName, true);
			this.transform = Transform;
			this.deleteAfterDays = DeleteAfterDays;

			this.settings = new XmlWriterSettings
			{
				CloseOutput = true,
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = Encoding.UTF8,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = false,
				WriteEndDocumentOnClose = true
			};

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
		protected override async Task BeforeWrite()
		{
			if (!this.fileSequence.TryGetNewFileName(out string s))
				return;

			try
			{
				this.output?.WriteEndElement();
				this.output?.WriteEndDocument();
				this.output?.Flush();
				this.file?.Dispose();
			}
			catch (Exception)
			{
				try
				{
					this.DisposeOutput();
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			this.file = null;
			this.output = null;

			this.file = File.CreateText(s);

			this.output = XmlWriter.Create(this.file, this.settings);
			this.output.WriteStartDocument();

			if (!string.IsNullOrEmpty(this.transform))
			{
				if (File.Exists(this.transform))
				{
					try
					{
						byte[] XsltBin = await Resources.ReadAllBytesAsync(this.transform);

						this.output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"data:text/xsl;base64," +
							Convert.ToBase64String(XsltBin) + "\"");
					}
					catch (Exception)
					{
						this.output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + this.transform + "\"");
					}
				}
				else
					this.output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + this.transform + "\"");
			}

			this.output.WriteStartElement("EventOutput", EventExtensions.LogNamespace);
			this.output.Flush();

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
