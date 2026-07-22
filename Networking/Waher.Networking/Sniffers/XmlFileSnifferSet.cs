using System;
using System.IO;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Maintains a set of XML-file-based sniffers. File output is organized into
	/// subfolders of a given folder, and formatted using XSLT.
	/// </summary>
	public class XmlFileSnifferSet : SnifferSet<XmlFileSniffer>
	{
		/// <summary>
		/// Maintains a set of XML-file-based sniffers. File output is organized into
		/// subfolders of a given folder, and formatted using XSLT.
		/// </summary>
		/// <param name="Folder">Folder where set of sniffers will be stored.</param>
		/// <param name="FileNameBase">Base file name used for new sniffer files.
		/// The following strings will be replaced by current values:
		/// 
		/// %YEAR% = Current year.
		/// %MONTH% = Current month.
		/// %DAY% = Current day.
		/// %HOUR% = Current hour.
		/// %MINUTE% = Current minute.
		/// %SECOND% = Current second.</param>
		/// <param name="MaxTimeUnused">Maximum time unused, before being removed.</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public XmlFileSnifferSet(string Folder, string FileNameBase, TimeSpan MaxTimeUnused,
			BinaryPresentationMethod BinaryPresentationMethod)
			: this(Folder, FileNameBase, MaxTimeUnused, 7, BinaryPresentationMethod)
		{
		}

		/// <summary>
		/// Maintains a set of XML-file-based sniffers. File output is organized into
		/// subfolders of a given folder, and formatted using XSLT.
		/// </summary>
		/// <param name="Folder">Folder where set of sniffers will be stored.</param>
		/// <param name="FileNameBase">Base file name used for new sniffer files.
		/// The following strings will be replaced by current values:
		/// 
		/// %YEAR% = Current year.
		/// %MONTH% = Current month.
		/// %DAY% = Current day.
		/// %HOUR% = Current hour.
		/// %MINUTE% = Current minute.
		/// %SECOND% = Current second.</param>
		/// <param name="MaxTimeUnused">Maximum time unused, before being removed.</param>
		/// <param name="DeleteAfterDays">Number of days files will be kept. All files older than this
		/// in the corresponding folder will be removed. Default value is 7 days.</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public XmlFileSnifferSet(string Folder, string FileNameBase, TimeSpan MaxTimeUnused,
			int DeleteAfterDays, BinaryPresentationMethod BinaryPresentationMethod)
			: this(Folder, FileNameBase, MaxTimeUnused, string.Empty, DeleteAfterDays, 
				  BinaryPresentationMethod)
		{
		}

		/// <summary>
		/// Maintains a set of XML-file-based sniffers. File output is organized into
		/// subfolders of a given folder, and formatted using XSLT.
		/// </summary>
		/// <param name="Folder">Folder where set of sniffers will be stored.</param>
		/// <param name="FileNameBase">Base file name used for new sniffer files.
		/// The following strings will be replaced by current values:
		/// 
		/// %YEAR% = Current year.
		/// %MONTH% = Current month.
		/// %DAY% = Current day.
		/// %HOUR% = Current hour.
		/// %MINUTE% = Current minute.
		/// %SECOND% = Current second.</param>
		/// <param name="MaxTimeUnused">Maximum time unused, before being removed.</param>
		/// <param name="Transform">Transform file name.</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public XmlFileSnifferSet(string Folder, string FileNameBase, TimeSpan MaxTimeUnused, 
			string Transform, BinaryPresentationMethod BinaryPresentationMethod)
			: this(Folder, FileNameBase, MaxTimeUnused, Transform, 7, 
				  BinaryPresentationMethod)
		{
		}

		/// <summary>
		/// Maintains a set of XML-file-based sniffers. File output is organized into
		/// subfolders of a given folder, and formatted using XSLT.
		/// </summary>
		/// <param name="Folder">Folder where set of sniffers will be stored.</param>
		/// <param name="FileNameBase">Base file name used for new sniffer files.
		/// The following strings will be replaced by current values:
		/// 
		/// %YEAR% = Current year.
		/// %MONTH% = Current month.
		/// %DAY% = Current day.
		/// %HOUR% = Current hour.
		/// %MINUTE% = Current minute.
		/// %SECOND% = Current second.</param>
		/// <param name="MaxTimeUnused">Maximum time unused, before being removed.</param>
		/// <param name="Transform">Transform file name.</param>
		/// <param name="DeleteAfterDays">Number of days files will be kept. All files older than this
		/// in the corresponding folder will be removed. Default value is 7 days.</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public XmlFileSnifferSet(string Folder, string FileNameBase, TimeSpan MaxTimeUnused, 
			string Transform, int DeleteAfterDays, BinaryPresentationMethod BinaryPresentationMethod)
			: base(MaxTimeUnused, 
				  new XmlFileSnifferCreator(
					  Folder, 
					  FileNameBase, 
					  Transform, 
					  DeleteAfterDays, 
					  BinaryPresentationMethod).CreateSniffer)
		{
		}

		private class XmlFileSnifferCreator
		{
			private readonly BinaryPresentationMethod binaryPresentationMethod;
			private readonly string transform;
			private readonly string folder;
			private readonly string fileNameBase;
			private readonly int deleteAfterDays;

			public XmlFileSnifferCreator(string Folder, string FileNameBase, 
				string Transform, int DeleteAfterDays, 
				BinaryPresentationMethod BinaryPresentationMethod)
			{
				this.folder = Folder;
				this.fileNameBase = FileNameBase;
				this.deleteAfterDays = DeleteAfterDays;
				this.transform = Transform;
				this.binaryPresentationMethod = BinaryPresentationMethod;
			}

			public XmlFileSniffer CreateSniffer(string Discriminator)
			{
				string FileName = Path.Combine(this.folder, Discriminator, 
					this.fileNameBase);
				
				return new XmlFileSniffer(FileName, this.transform, this.deleteAfterDays, 
					this.binaryPresentationMethod);
			}
		}
	}
}
