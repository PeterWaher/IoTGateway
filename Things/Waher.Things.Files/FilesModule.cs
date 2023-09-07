using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Things.Metering;
using Waher.Things.Virtual;

namespace Waher.Things.Files
{
	/// <summary>
	/// Module maintaining active file system watchers.
	/// </summary>
	[Singleton]
	public class FilesModule : IModule
	{
		private static readonly Dictionary<string, KeyValuePair<FolderNode, FileSystemWatcher>> watchers = new Dictionary<string, KeyValuePair<FolderNode, FileSystemWatcher>>();
		private static SemaphoreSlim synchObj = new SemaphoreSlim(1);

		/// <summary>
		/// Starts the module.
		/// </summary>
		/// <returns></returns>
		public async Task Start()
		{
			try
			{
				await this.CheckNode(MeteringTopology.Root);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task CheckNode(INode Node)
		{
			foreach (INode Child in await Node.ChildNodes)
			{
				if (Child is FolderNode FolderNode)
				{
					try
					{
						await FolderNode.Synchronize();
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
				else if (Child is VirtualNode VirtualNode)
					await this.CheckNode(VirtualNode);
			}
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		/// <returns></returns>
		public async Task Stop()
		{
			await synchObj.WaitAsync();
			try
			{
				foreach (KeyValuePair<FolderNode, FileSystemWatcher> P in watchers.Values)
					P.Value.Dispose();

				watchers.Clear();
			}
			finally
			{
				synchObj.Release();
			}
		}

		internal static async Task StopSynchronization(string FolderPath)
		{
			await synchObj.WaitAsync();
			try
			{
				if (watchers.TryGetValue(FolderPath, out KeyValuePair<FolderNode, FileSystemWatcher> P))
				{
					watchers.Remove(FolderPath);
					P.Value.Dispose();
				}
			}
			finally
			{
				synchObj.Release();
			}
		}

		internal static async Task CheckSynchronization(FolderNode Node)
		{
			try
			{
				FileSystemWatcher Watcher;

				await synchObj.WaitAsync();
				try
				{
					if (watchers.TryGetValue(Node.FolderPath, out KeyValuePair<FolderNode, FileSystemWatcher> P))
					{
						watchers.Remove(Node.FolderPath);
						P.Value.Dispose();
					}

					if (!Directory.Exists(Node.FolderPath))
						Directory.CreateDirectory(Node.FolderPath);

					if (Node.SynchronizationOptions != SynchronizationOptions.NoSynchronization)
					{
						if (string.IsNullOrEmpty(Node.FileFilter))
							Watcher = new FileSystemWatcher(Node.FolderPath);
						else
							Watcher = new FileSystemWatcher(Node.FolderPath, Node.FileFilter);

						watchers[Node.FolderPath] = new KeyValuePair<FolderNode, FileSystemWatcher>(Node, Watcher);

						Watcher.NotifyFilter =
							NotifyFilters.Attributes |
							NotifyFilters.CreationTime |
							NotifyFilters.DirectoryName |
							NotifyFilters.FileName |
							NotifyFilters.LastAccess |
							NotifyFilters.LastWrite |
							NotifyFilters.Security |
							NotifyFilters.Size;

						Watcher.Changed += Node.Watcher_Changed;
						Watcher.Created += Node.Watcher_Created;
						Watcher.Deleted += Node.Watcher_Deleted;
						Watcher.Renamed += Node.Watcher_Renamed;
						Watcher.Error += Node.Watcher_Error;

						Watcher.IncludeSubdirectories = Node.SynchronizationOptions == SynchronizationOptions.IncludeSubfolders;
						Watcher.EnableRaisingEvents = true;
					}
				}
				finally
				{
					synchObj.Release();
				}

				await Node.SynchFolder();
				await Node.RemoveErrorAsync("SynchError");
			}
			catch (Exception ex)
			{
				await Node.LogErrorAsync("SynchError", ex.Message);
			}
		}
	}
}
