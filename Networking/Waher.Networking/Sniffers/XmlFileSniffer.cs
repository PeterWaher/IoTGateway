using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Events;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to an XML file.
	/// </summary>
	public class XmlFileSniffer : XmlWriterSniffer
	{
		private XmlWriterSettings settings;
		private StreamWriter file;
		private DateTime lastEvent = DateTime.MinValue;
		private string fileName;
		private string lastFileName = null;
		private string transform = null;
		private int deleteAfterDays;

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
		public XmlFileSniffer(string FileName, string Transform, int DeleteAfterDays,
			BinaryPresentationMethod BinaryPresentationMethod)
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
				NewLineHandling = NewLineHandling.Entitize,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = false
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
		public string FileName
		{
			get { return this.fileName; }
		}

		/// <summary>
		/// Transform to use.
		/// </summary>
		public string Transform
		{
			get { return this.transform; }
		}

		/// <summary>
		/// Timestamp of Last event
		/// </summary>
		public DateTime LastEvent
		{
			get { return this.lastEvent; }
		}

		/// <summary>
		/// Method is called before writing something to the text file.
		/// </summary>
		protected override void BeforeWrite()
		{
			DateTime TP = DateTime.Now;
			string s = this.fileName.
				Replace("%YEAR%", TP.Year.ToString("D4")).
				Replace("%MONTH%", TP.Month.ToString("D2")).
				Replace("%DAY%", TP.Day.ToString("D2")).
				Replace("%HOUR%", TP.Hour.ToString("D2")).
				Replace("%MINUTE%", TP.Minute.ToString("D2")).
				Replace("%SECOND%", TP.Second.ToString("D2"));

			this.lastEvent = TP;

			if (this.lastFileName != null && this.lastFileName == s)
				return;

			if (this.file != null)
			{
				try
				{
					this.output.WriteEndElement();
					this.output.WriteEndDocument();
					this.output.Flush();
					this.file.Dispose();
				}
				catch (Exception)
				{
					// Ignore
				}

				this.file = null;
				this.output = null;
			}

			try
			{
				this.file = File.CreateText(s);
				this.lastFileName = s;
				this.output = XmlWriter.Create(this.file, this.settings);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				this.file = null;
				this.output = null;
				return;
			}

			this.output.WriteStartDocument();

			if (!string.IsNullOrEmpty(this.transform))
				this.output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + this.transform + "\"");

			this.output.WriteStartElement("SnifferOutput", "http://waher.se/Schema/SnifferOutput.xsd");
			this.output.Flush();

			string FolderName = Path.GetDirectoryName(s);
			string[] Files = Directory.GetFiles(FolderName, "*.*");

			foreach (string FileName in Files)
			{
				if ((DateTime.Now - File.GetLastWriteTime(FileName)).TotalDays >= this.deleteAfterDays)
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
						Log.Critical(ex, FileName);
					}
				}
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (this.output != null)
			{
				try
				{
					this.output.WriteEndElement();
					this.output.WriteEndDocument();
					this.output.Flush();
					this.output.Dispose();
				}
				catch (Exception)
				{
					// Ignore
				}
				finally
				{
					this.output = null;
				}
			}

			if (this.file != null)
			{
				try
				{
					this.file.Dispose();
				}
				catch (Exception)
				{
					// Ignore
				}
				finally
				{
					this.file = null;
				}
			}
		}
	}
}
