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
			: base(NodeId, Parent)
		{
			Folder = Path.GetFullPath(Folder);

			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);

			this.folder = Folder;

			this.AppendReportFileNodes(Folder, Folder, this);
		}

		private void AppendReportFileNodes(string BaseFolder, string Folder, ReportNode Parent)
		{
			foreach (string FileName in Directory.GetFiles(Folder, "*.rpx"))
				new ReportFileNode(FileName, FileName[BaseFolder.Length..], Parent);

			foreach (string SubFolder in Directory.GetDirectories(Folder))
			{
				this.AppendReportFileNodes(BaseFolder, SubFolder, 
					new ReportFilesFolder(SubFolder, SubFolder[BaseFolder.Length..], Parent));
			}
		}

		/// <summary>
		/// Path of reports folder.
		/// </summary>
		public string Folder => this.folder;
	}
}
