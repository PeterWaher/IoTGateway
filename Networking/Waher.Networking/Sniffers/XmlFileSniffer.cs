using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to an XML file.
	/// </summary>
	public class XmlFileSniffer : XmlWriterSniffer
	{
		private readonly XmlWriterSettings settings;
		private StreamWriter file;
		private DateTime lastEvent = DateTime.MinValue;
		private readonly string fileName;
		private string lastFileName = null;
		private readonly string transform = null;
		private readonly int deleteAfterDays;

		/// <summary>
		/// Outputs sniffed data to an XML file.
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
		public XmlFileSniffer(string FileName, BinaryPresentationMethod BinaryPresentationMethod)
			: this(FileName, string.Empty, 7, BinaryPresentationMethod)
		{
		}

		/// <summary>
		/// Outputs sniffed data to an XML file.
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
		public XmlFileSniffer(string FileName, int DeleteAfterDays, BinaryPresentationMethod BinaryPresentationMethod)
			: this(FileName, string.Empty, DeleteAfterDays, BinaryPresentationMethod)
		{
		}

		/// <summary>
		/// Outputs sniffed data to an XML file.
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
		/// <param name="Transform">Transform file name.</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public XmlFileSniffer(string FileName, string Transform, BinaryPresentationMethod BinaryPresentationMethod)
			: this(FileName, Transform, 7, BinaryPresentationMethod)
		{
		}

		/// <summary>
		/// Outputs sniffed data to an XML file.
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
		/// <param name="Transform">Transform file name.</param>
		/// <param name="DeleteAfterDays">Number of days files will be kept. All files older than this
		/// in the corresponding folder will be removed. Default value is 7 days.</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public XmlFileSniffer(string FileName, string Transform, int DeleteAfterDays, BinaryPresentationMethod BinaryPresentationMethod)
			: base(null, BinaryPresentationMethod)
		{
			this.file = null;
			this.output = null;
			this.fileName = FileName;
			this.transform = Transform;
			this.deleteAfterDays = DeleteAfterDays;

			this.settings = new XmlWriterSettings()
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

			if (!string.IsNullOrEmpty(FolderName) && !Directory.Exists(FolderName))
			{
				Log.Informational("Creating folder.", FolderName);
				Directory.CreateDirectory(FolderName);
			}
		}

		/// <summary>
		/// File Name.
		/// </summary>
		public string FileName => this.fileName;

		/// <summary>
		/// Transform to use.
		/// </summary>
		public string Transform => this.transform;

		/// <summary>
		/// Timestamp of Last event
		/// </summary>
		public DateTime LastEvent => this.lastEvent;

		/// <summary>
		/// Gets the name of a file, given a file name template.
		/// </summary>
		/// <param name="TemplateFileName">File Name template.</param>
		/// <param name="TP">Timestamp</param>
		/// <returns>File name</returns>
		public static string GetFileName(string TemplateFileName, DateTime TP)
		{
			return TemplateFileName.
				Replace("%YEAR%", TP.Year.ToString("D4")).
				Replace("%MONTH%", TP.Month.ToString("D2")).
				Replace("%DAY%", TP.Day.ToString("D2")).
				Replace("%HOUR%", TP.Hour.ToString("D2")).
				Replace("%MINUTE%", TP.Minute.ToString("D2")).
				Replace("%SECOND%", TP.Second.ToString("D2"));
		}

		/// <summary>
		/// Makes a file name unique.
		/// </summary>
		/// <param name="FileName">File name.</param>
		public static void MakeUnique(ref string FileName)
		{
			if (File.Exists(FileName))
			{
				int i = FileName.LastIndexOf('.');
				int j = 2;

				if (i < 0)
					i = FileName.Length;

				string s;

				do
				{
					s = FileName.Insert(i, " (" + (j++).ToString() + ")");
				}
				while (File.Exists(s));

				FileName = s;
			}
		}

		/// <summary>
		/// Method is called before writing something to the text file.
		/// </summary>
		protected override async Task BeforeWrite()
		{
			DateTime TP = DateTime.Now;
			string s = GetFileName(this.fileName, TP);
			this.lastEvent = TP;

			if (!(this.lastFileName is null) && this.lastFileName == s && !(this.file is null) && this.file.BaseStream.CanWrite)
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

			string s2 = s;
			MakeUnique(ref s2);

			try
			{
				this.file = File.CreateText(s2);
				this.lastFileName = s;
				this.output = XmlWriter.Create(this.file, this.settings);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				this.output = null;
				return;
			}

			this.output.WriteStartDocument();

			if (!string.IsNullOrEmpty(this.transform))
			{
				if (File.Exists(this.transform))
				{
					try
					{
						byte[] XsltBin = await ReadAllBytesAsync(this.transform);

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

			this.output.WriteStartElement("SnifferOutput", "http://waher.se/Schema/SnifferOutput.xsd");
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
						catch (IOException ex)
						{
							Log.Error("Unable to delete file: " + ex.Message, FileName);
						}
						catch (Exception ex)
						{
							Log.Exception(ex, FileName);
						}
					}
				}
			}
		}

		/// <summary>
		/// Reads a binary file asynchronously.
		/// </summary>
		/// <param name="FileName">Filename.</param>
		/// <returns>Binary content.</returns>
		public static async Task<byte[]> ReadAllBytesAsync(string FileName)
		{
			using (FileStream fs = File.OpenRead(FileName))
			{
				return await fs.ReadAllAsync();
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
