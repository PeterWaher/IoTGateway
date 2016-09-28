using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Events.Files
{
	/// <summary>
	/// Outputs sniffed data to an XML file.
	/// </summary>
	public class XmlFileEventSink : XmlWriterEventSink
	{
		private XmlWriterSettings settings;
		private StreamWriter file;
		private string fileName;
		private string lastFileName = null;
		private string transform = null;
		private int deleteAfterDays;

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
			this.fileName = FileName;
			this.transform = Transform;
			this.deleteAfterDays = DeleteAfterDays;

			this.settings = new XmlWriterSettings();
			this.settings.CloseOutput = true;
			this.settings.ConformanceLevel = ConformanceLevel.Document;
			this.settings.Encoding = Encoding.UTF8;
			this.settings.Indent = true;
			this.settings.IndentChars = "\t";
			this.settings.NewLineChars = "\r\n";
			this.settings.NewLineHandling = NewLineHandling.Entitize;
			this.settings.NewLineOnAttributes = false;
			this.settings.OmitXmlDeclaration = false;

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
			DateTime TP = DateTime.Now;
			string s = this.fileName.
				Replace("%YEAR%", TP.Year.ToString("D4")).
				Replace("%MONTH%", TP.Month.ToString("D2")).
				Replace("%DAY%", TP.Day.ToString("D2")).
				Replace("%HOUR%", TP.Hour.ToString("D2")).
				Replace("%MINUTE%", TP.Minute.ToString("D2")).
				Replace("%SECOND%", TP.Second.ToString("D2"));

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

			this.file = File.CreateText(s);
			this.lastFileName = s;

			this.output = XmlWriter.Create(this.file, this.settings);
			this.output.WriteStartDocument();

			if (!string.IsNullOrEmpty(this.transform))
				this.output.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + this.transform + "\"");

			this.output.WriteStartElement("EventOutput", "http://waher.se/EventOutput.xsd");
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
					catch (Exception ex)
					{
						Log.Critical(ex);
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
