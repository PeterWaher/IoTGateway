using System;
using System.IO;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;

namespace Waher.Utility.Extract.ExportFormats
{
	/// <summary>
	/// Interface for export formats.
	/// </summary>
	public interface IExportFormat : IDatabaseExport, ILedgerExport, IDisposable
	{
		/// <summary>
		/// File name
		/// </summary>
		string FileName
		{
			get;
		}

		/// <summary>
		/// Optional array of collection nmes to export. If null, all collections will be exported.
		/// </summary>
		string[] CollectionNames
		{
			get;
		}

		/// <summary>
		/// Starts export
		/// </summary>
		Task Start();

		/// <summary>
		/// Ends export
		/// </summary>
		Task End();

		/// <summary>
		/// Starts export of files.
		/// </summary>
		Task StartFiles();

		/// <summary>
		/// Ends export of files.
		/// </summary>
		Task EndFiles();

		/// <summary>
		/// Export file.
		/// </summary>
		/// <param name="FileName">Name of file</param>
		/// <param name="File">File stream</param>
		Task ExportFile(string FileName, Stream File);

	}
}
