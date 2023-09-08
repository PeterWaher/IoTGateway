using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Queries;

namespace Waher.Things.Files.Commands
{
	/// <summary>
	/// Synchronizes a folder.
	/// </summary>
	internal class SynchronizeFolder : ICommand
	{
		private readonly FolderNode node;

		/// <summary>
		/// Synchronizes a folder.
		/// </summary>
		/// <param name="Node">Folder node.</param>
		public SynchronizeFolder(FolderNode Node)
		{
			this.node = Node;
			this.SynchronizationOptions = Node.SynchronizationOptions;
			this.FileFilter = Node.FileFilter;
		}

		/// <summary>
		/// Synchronization options
		/// </summary>
		[Page(2, "File System", 100)]
		[Header(7, "Synchronization Mode:")]
		[ToolTip(8, "If, and how, files in the folder (or subfolders) will be synchronized.")]
		[Option(SynchronizationOptions.NoSynchronization, 9, "Do not synchronize files.")]
		[Option(SynchronizationOptions.TopLevelOnly, 10, "Synchronize top-level files only.")]
		[Option(SynchronizationOptions.IncludeSubfolders, 11, "Synchronize files in folder and subfolders.")]
		[Text(TextPosition.AfterField, 16, "You can add default script templates to be used for files found, by adding string-valued meta-data tags to the node, where the meta-data key names correspond to file extensions.")]
		public SynchronizationOptions SynchronizationOptions { get; set; }

		/// <summary>
		/// File filter to monitor
		/// </summary>
		[Page(2, "File System", 100)]
		[Header(5, "File Filter:")]
		[ToolTip(6, "You can limit the files to be monitored using a file filter. If no filter is provided, all files within the scope will be monitored.")]
		public string FileFilter { get; set; }

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "SynchronizeFolder";

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Query;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "FileSystem";

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		public string SortKey => "Synchronize";

		/// <summary>
		/// If the command can be executed by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the command can be executed by the caller.</returns>
		public Task<bool> CanExecuteAsync(RequestOrigin Caller)
		{
			return this.node.CanEditAsync(Caller);
		}

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public ICommand Copy()
		{
			return new SynchronizeFolder(this.node);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public Task ExecuteCommandAsync()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(FolderNode), 17, "Synchronize folder...");
		}

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// Gets a confirmation string, if any, of the command. If no confirmation is necessary, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetConfirmationStringAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// Gets a failure string, if any, of the command. If no specific failure string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(FolderNode), 18, "Unable to start synchronization.");
		}

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		public Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			SynchronizationStatistics Statistics = new SynchronizationStatistics(this.node, Query);
			return this.node.SynchFolder(this.SynchronizationOptions, this.FileFilter, Statistics);
		}
	}
}
