using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Waher.Things.Queries;

namespace Waher.Things.Files.Commands
{
	/// <summary>
	/// Generates basic statistics around a folder synchronization process.
	/// </summary>
	public class SynchronizationStatistics
	{
		private readonly Dictionary<string, string> folderIds = new Dictionary<string, string>();
		private readonly Query query;
		private readonly FolderNode node;
		private int nrFoldersAdded = 0;
		private int nrFoldersFound = 0;
		private int nrFoldersDeleted = 0;
		private int nrFilesAdded = 0;
		private int nrFilesFound = 0;
		private int nrFilesDeleted = 0;

		/// <summary>
		/// Generates basic statistics around a folder synchronization process.
		/// </summary>
		/// <param name="Node">Folder node.</param>
		/// <param name="Query">Query feedback object.</param>
		public SynchronizationStatistics(FolderNode Node, Query Query)
		{
			this.node = Node;
			this.query = Query;
		}

		private async Task<string> GetFolderTableId(string Folder)
		{
			if (this.folderIds.TryGetValue(Folder, out string TableId))
				return TableId;

			TableId = "F" + (this.folderIds.Count + 1).ToString();
			this.folderIds[Folder] = TableId;

			await this.query.BeginSection(Folder);
			await this.query.NewTable(TableId, "Files in " + Folder,
				new Column("FileName", "Filename", null, null, null, null, ColumnAlignment.Left, null),
				new Column("Action", "Action", null, null, null, null, ColumnAlignment.Left, null));

			return TableId;
		}

		/// <summary>
		/// File found, corresponding to node in topology.
		/// </summary>
		/// <param name="Folder">Folder hosting file.</param>
		/// <param name="FileName">File name.</param>
		public async Task FileFound(string Folder, string FileName)
		{
			string TableId = await this.GetFolderTableId(Folder);
			
			await this.query.NewRecords(TableId, new Record(FileName, "-"));
			
			this.nrFilesFound++;
		}

		/// <summary>
		/// Subfolder found, corresponding to node in topology.
		/// </summary>
		/// <param name="Folder">Folder hosting folder.</param>
		/// <param name="SubFolder">Subfolder.</param>
		public async Task FolderFound(string Folder, string SubFolder)
		{
			string TableId = await this.GetFolderTableId(Folder);

			await this.query.NewRecords(TableId, new Record(SubFolder, "-"));

			this.nrFoldersFound++;
		}

		/// <summary>
		/// File node added to topology.
		/// </summary>
		/// <param name="Folder">Folder hosting file.</param>
		/// <param name="FileName">File name.</param>
		public async Task FileAdded(string Folder, string FileName)
		{
			string TableId = await this.GetFolderTableId(Folder);

			await this.query.NewRecords(TableId, new Record(FileName, "Added"));

			this.nrFilesAdded++;
		}

		/// <summary>
		/// Subfolder node added to topology.
		/// </summary>
		/// <param name="Folder">Folder hosting folder.</param>
		/// <param name="SubFolder">Subfolder.</param>
		public async Task FolderAdded(string Folder, string SubFolder)
		{
			string TableId = await this.GetFolderTableId(Folder);

			await this.query.NewRecords(TableId, new Record(SubFolder, "Added"));

			this.nrFoldersAdded++;
		}

		/// <summary>
		/// File node deleted from topology.
		/// </summary>
		/// <param name="Folder">Folder hosting file.</param>
		/// <param name="FileName">File name.</param>
		public async Task FileDeleted(string Folder, string FileName)
		{
			string TableId = await this.GetFolderTableId(Folder);

			await this.query.NewRecords(TableId, new Record(FileName, "Deleted"));

			this.nrFilesDeleted++;
		}

		/// <summary>
		/// Subfolder node deleted from topology.
		/// </summary>
		/// <param name="Folder">Folder hosting folder.</param>
		/// <param name="SubFolder">Subfolder.</param>
		public async Task FolderDeleted(string Folder, string SubFolder)
		{
			string TableId = await this.GetFolderTableId(Folder);

			await this.query.NewRecords(TableId, new Record(SubFolder, "Deleted"));

			this.nrFoldersDeleted++;
		}

		/// <summary>
		/// Synchronization starts.
		/// </summary>
		public async Task Start()
		{
			await this.query.Start();
			await this.query.SetTitle(this.node.FolderPath);

			await this.query.BeginSection("Title");
			await this.query.NewTable("Totals", "Totals",
				new Column("Key", "Metric", string.Empty, string.Empty, null, null, ColumnAlignment.Left, null),
				new Column("Value", "Value", string.Empty, string.Empty, null, null, ColumnAlignment.Right, null));
			await this.query.EndSection();
		}

		/// <summary>
		/// An exception has occurred during synchronization.
		/// </summary>
		/// <param name="ex">Exception</param>
		public async Task Error(Exception ex)
		{
			await this.query.LogMessage(QueryEventType.Exception, QueryEventLevel.Major, ex.Message);
		}

		/// <summary>
		/// Synchronization has completed.
		/// </summary>
		public async Task Done()
		{
			await this.query.NewRecords("Totals",
				new Record("#Files Existed", this.nrFilesFound),
				new Record("#Files Added", this.nrFilesAdded),
				new Record("#Files Deleted", this.nrFilesDeleted),
				new Record("#Folders Existed", this.nrFoldersFound),
				new Record("#Folders Added", this.nrFoldersAdded),
				new Record("#Folders Deleted", this.nrFoldersDeleted));
			await this.query.TableDone("Totals");

			foreach (KeyValuePair<string, string> P in this.folderIds)
				await this.query.TableDone(P.Value);

			await this.query.Done();
		}
	}
}
