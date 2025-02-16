using System.IO;

namespace Waher.Reports.Files
{
	/// <summary>
	/// Reports folder representing a folder on the file system containing file-based reports.
	/// </summary>
	public class ReportFilesFolder : ReportsFolder
	{
		private readonly string folder;

		/// <summary>
		/// Reports folder representing a folder on the file system containing file-based reports.
		/// </summary>
		/// <param name="Folder">Folder path</param>
		/// <param name="NodeId">Node ID</param>
		/// <param name="Parent">Parent node.</param>
		public ReportFilesFolder(string Folder, string NodeId, ReportNode Parent)
			: this(Folder, NodeId, Parent, true)
		{
		}

		/// <summary>
		/// Reports folder representing a folder on the file system containing file-based reports.
		/// </summary>
		/// <param name="Folder">Folder path</param>
		/// <param name="NodeId">Node ID</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="ScanFolder">If folder should be scanned.</param>
		private ReportFilesFolder(string Folder, string NodeId, ReportNode Parent, bool ScanFolder)
			: base(NodeId, Parent)
		{
			Folder = Path.GetFullPath(Folder);

			if (!Folder.EndsWith(Path.DirectorySeparatorChar))
				Folder += Path.DirectorySeparatorChar;

			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			this.folder = Folder;

			if (ScanFolder)
				this.AppendReportFileNodes(Folder, Folder, this);
		}

		private void AppendReportFileNodes(string BaseFolder, string Folder, ReportNode Parent)
		{
			string[] FileNames = Directory.GetFiles(Folder, "*.rpx", SearchOption.TopDirectoryOnly);
			string[] Folders = Directory.GetDirectories(Folder, "*.*", SearchOption.TopDirectoryOnly);

			foreach (string FileName in FileNames)
				new ReportFileNode(FileName, FileName[BaseFolder.Length..], Parent);

			foreach (string SubFolder in Folders)
			{
				ReportFilesFolder SubFolderNode = new ReportFilesFolder(SubFolder, SubFolder[BaseFolder.Length..], Parent, false);
				this.AppendReportFileNodes(BaseFolder, SubFolder, SubFolderNode);
			}
		}

		/// <summary>
		/// Path of reports folder.
		/// </summary>
		public string Folder => this.folder;
	}
}
