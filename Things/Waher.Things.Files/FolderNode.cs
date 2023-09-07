using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.Virtual;

namespace Waher.Things.Files
{
	/// <summary>
	/// How a folder will synchronize nodes with contents of folders.
	/// </summary>
	public enum SynchronizationOptions
	{
		NoSynchronization,
		TopLevelOnly,
		IncludeSubfolders
	}

	/// <summary>
	/// Represents a file folder in the file system.
	/// </summary>
	public class FolderNode : VirtualNode
	{
		private SynchronizationOptions synchronizationOptions = SynchronizationOptions.NoSynchronization;
		private string folderPath;
		private string fileFilter;
		private Timer timer;

		/// <summary>
		/// Represents a file folder in the file system.
		/// </summary>
		public FolderNode()
		{
		}

		/// <summary>
		/// Destroys the node. If it is a child to a parent node, it is removed from the parent first.
		/// </summary>
		public override async Task DestroyAsync()
		{
			this.timer?.Dispose();
			this.timer = null;

			await FilesModule.StopSynchronization(this.folderPath);

			await base.DestroyAsync();
		}

		/// <summary>
		/// Full path to folder.
		/// </summary>
		[Page(2, "File System", 100)]
		[Header(3, "Folder:")]
		[ToolTip(4, "Full path to folder (on host).")]
		public string FolderPath
		{
			get => this.folderPath;
			set
			{
				if (this.folderPath != value)
				{
					this.folderPath = value;
					this.CheckSynchronization();
				}
			}
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
		[DefaultValue(SynchronizationOptions.NoSynchronization)]
		public SynchronizationOptions SynchronizationOptions
		{
			get => this.synchronizationOptions;
			set
			{
				if (this.synchronizationOptions != value)
				{
					this.synchronizationOptions = value;
					this.CheckSynchronization();
				}
			}
		}

		/// <summary>
		/// File filter to monitor
		/// </summary>
		[Page(2, "File System", 100)]
		[Header(5, "File Filter:")]
		[ToolTip(6, "You can limit the files to be monitored using a file filter. If no filter is provided, all files within the scope will be monitored.")]
		public string FileFilter
		{
			get => this.fileFilter;
			set
			{
				if (this.fileFilter != value)
				{
					this.fileFilter = value;
					this.CheckSynchronization();
				}
			}
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(FolderNode), 1, "File Folder");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is SubFolderNode || Child is FileNode);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root || Parent is VirtualNode);
		}

		private void CheckSynchronization()
		{
			this.timer?.Dispose();
			this.timer = null;

			this.timer = new Timer(this.DelayedCheckSynchronization, null, 500, Timeout.Infinite);
		}

		/// <summary>
		/// Synchronizes folder, subfolders, files and nodes.
		/// </summary>
		/// <returns></returns>
		public Task Synchronize()
		{
			this.timer?.Dispose();
			this.timer = null;

			return this.DelayedCheckSynchronization();
		}

		private void DelayedCheckSynchronization(object P)
		{
			Task.Run(async () =>
			{
				try
				{
					await this.DelayedCheckSynchronization();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			});
		}

		private Task DelayedCheckSynchronization()
		{
			return FilesModule.CheckSynchronization(this);
		}

		internal async void Watcher_Error(object sender, ErrorEventArgs e)
		{
			try
			{
				await this.LogErrorAsync(e.GetException().Message);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal async void Watcher_Renamed(object sender, RenamedEventArgs e)
		{
			try
			{
				await this.Renamed(e.OldFullPath, e.FullPath);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal async void Watcher_Deleted(object sender, FileSystemEventArgs e)
		{
			try
			{
				await this.Deleted(e.FullPath);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal async void Watcher_Created(object sender, FileSystemEventArgs e)
		{
			try
			{
				await this.Created(e.FullPath);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal async void Watcher_Changed(object sender, FileSystemEventArgs e)
		{
			try
			{
				if (e.ChangeType == WatcherChangeTypes.Changed)
					await this.Changed(e.FullPath);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal async Task SynchFolder()
		{
			if (this.synchronizationOptions != SynchronizationOptions.NoSynchronization)
			{
				string[] Files = Directory.GetFiles(this.FolderPath,
					string.IsNullOrEmpty(this.FileFilter) ? "*.*" : this.FileFilter,
					this.SynchronizationOptions == SynchronizationOptions.IncludeSubfolders ? 
					SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

				foreach (string File in Files)
				{
					try
					{
						await this.Changed(File);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
		}

		private async Task Renamed(string OldPath, string NewPath)
		{
		}

		private async Task Deleted(string Path)
		{
		}

		private async Task Created(string Path)
		{
		}

		private async Task Changed(string Path)
		{
		}

	}
}
