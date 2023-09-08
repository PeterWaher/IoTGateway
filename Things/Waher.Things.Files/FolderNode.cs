using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.XMPP.Sensor;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Files.Commands;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.Script;
using Waher.Things.Virtual;

namespace Waher.Things.Files
{
	/// <summary>
	/// How a folder will synchronize nodes with contents of folders.
	/// </summary>
	public enum SynchronizationOptions
	{
		/// <summary>
		/// No synchronization of underlying content.
		/// </summary>
		NoSynchronization,

		/// <summary>
		/// Changes to files in the folder will be mirrored in the topology.
		/// </summary>
		TopLevelOnly,

		/// <summary>
		/// Changes to files in the folder and subfolders will be mirrored in the topology.
		/// </summary>
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
		[Text(TextPosition.AfterField, 16, "You can add default script templates to be used for files found, by adding string-valued meta-data tags to the node, where the meta-data key names correspond to file extensions.")]
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
				await this.OnRenamed(e.OldFullPath, e.FullPath);
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
				await this.OnDeleted(e.FullPath);
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
				await this.OnCreated(e.FullPath);
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
					await this.OnChanged(e.FullPath, null);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal async Task SynchFolder()
		{
			await this.SynchFolder(this.synchronizationOptions, this.fileFilter, null);
		}

		internal async Task SynchFolder(SynchronizationOptions Options, string Filter, SynchronizationStatistics Statistics)
		{
			if (Options != SynchronizationOptions.NoSynchronization)
			{
				Log.Informational("Starting synchronizing folder.",
					new KeyValuePair<string, object>("Folder", this.folderPath),
					new KeyValuePair<string, object>("Node ID", this.NodeId));

				if (!(Statistics is null))
					await Statistics.Start();
				try
				{
					string[] Files = Directory.GetFiles(this.folderPath,
						string.IsNullOrEmpty(Filter) ? "*.*" : this.FileFilter,
						Options == SynchronizationOptions.IncludeSubfolders ?
						SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

					foreach (string File in Files)
					{
						try
						{
							await this.OnChanged(File, Statistics);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
				}
				catch (Exception ex)
				{
					if (!(Statistics is null))
						await Statistics.Error(ex);
					else
					{
						Log.Critical(ex,
							new KeyValuePair<string, object>("Folder", this.folderPath),
							new KeyValuePair<string, object>("Node ID", this.NodeId));
					}
				}
				finally
				{
					if (!(Statistics is null))
						await Statistics.Done();
				}

				Log.Informational("Synchronization of folder complete.",
					new KeyValuePair<string, object>("Folder", this.folderPath),
					new KeyValuePair<string, object>("Node ID", this.NodeId));
			}
		}

		private async Task<INode> FindNode(string Path, bool CreateIfNecessary, SynchronizationStatistics Statistics)
		{
			if (!Path.StartsWith(this.folderPath, StringComparison.InvariantCultureIgnoreCase))
				return null;

			Path = Path.Substring(this.folderPath.Length);

			if (Path.StartsWith(directorySeparator))
				Path = Path.Substring(1);

			Dictionary<string, string> DefaultTemplates = new Dictionary<string, string>();
			INode Parent = this;
			string SubPath = this.folderPath;
			string s, s2;
			int i;
			bool Found;

			if (!(this.MetaData is null))
			{
				foreach (MetaDataValue Tag in this.MetaData)
				{
					if (Tag.Value is string s3)
						DefaultTemplates[Tag.Name] = s3;
				}
			}

			while (!string.IsNullOrEmpty(Path))
			{
				i = Path.IndexOf(System.IO.Path.DirectorySeparatorChar);

				if (i < 0)
				{
					s = Path;
					Path = string.Empty;
				}
				else
				{
					s = Path.Substring(0, i);
					Path = Path.Substring(i + 1);
				}

				s2 = System.IO.Path.Combine(SubPath, s);
				Found = false;

				foreach (INode Child in await Parent.ChildNodes)
				{
					if (Child is SubFolderNode SubFolderNode)
					{
						if (string.Compare(s2, SubFolderNode.FolderPath, true) == 0)
						{
							if (!(SubFolderNode.MetaData is null))
							{
								foreach (MetaDataValue Tag in SubFolderNode.MetaData)
								{
									if (Tag.Value is string s3)
										DefaultTemplates[Tag.Name] = s3;
								}
							}

							if (!(Statistics is null))
								await Statistics.FolderFound(SubPath, s2);

							Parent = Child;
							Found = true;
							break;
						}
					}
					else if (Child is FileNode FileNode)
					{
						if (string.Compare(s2, FileNode.FolderPath, true) == 0)
						{
							if (!string.IsNullOrEmpty(Path))
								return null;

							if (!(Statistics is null))
								await Statistics.FileFound(SubPath, s2);

							Parent = Child;
							Found = true;
							break;
						}
					}
				}

				if (!Found)
				{
					if (!CreateIfNecessary)
						return null;

					if (string.IsNullOrEmpty(Path) && File.Exists(s2))
					{
						string FileExtension = System.IO.Path.GetExtension(s2);
						if (!string.IsNullOrEmpty(FileExtension) && FileExtension[0] == '.')
							FileExtension = FileExtension.Substring(1);

						if (!DefaultTemplates.TryGetValue(FileExtension, out string Template))
							Template = string.Empty;

						FileNode Node = new FileNode()
						{
							NodeId = await GetUniqueNodeId(s2),
							FolderPath = s2,
							ScriptNodeId = Template
						};

						await Parent.AddAsync(Node);

						if (!(Statistics is null))
							await Statistics.FileAdded(SubPath, s2);

						Log.Informational("File node added.",
							new KeyValuePair<string, object>("Folder", Node.FolderPath),
							new KeyValuePair<string, object>("Node ID", Node.NodeId),
							new KeyValuePair<string, object>("Script Node ID", Node.ScriptNodeId));

						Parent = Node;
					}
					else
					{
						SubFolderNode Node = new SubFolderNode()
						{
							NodeId = await GetUniqueNodeId(s2),
							FolderPath = s2
						};

						await Parent.AddAsync(Node);

						if (!(Statistics is null))
							await Statistics.FolderAdded(SubPath, s2);

						Log.Informational("Folder node added.",
							new KeyValuePair<string, object>("Folder", Node.FolderPath),
							new KeyValuePair<string, object>("Node ID", Node.NodeId));

						Parent = Node;
					}
				}

				SubPath = s2;
			}

			LinkedList<KeyValuePair<string, INode>> ToCheck = new LinkedList<KeyValuePair<string, INode>>();
			ToCheck.AddLast(new KeyValuePair<string, INode>(null, this));

			while (!(ToCheck.First is null))
			{
				KeyValuePair<string, INode> P = ToCheck.First.Value;
				string ParentPath = P.Key;
				INode Node = P.Value;

				ToCheck.RemoveFirst();

				if (Node is FileNode FileNode)
				{
					if (!File.Exists(FileNode.FolderPath))
					{
						await FileNode.DestroyAsync();

						if (!(Statistics is null))
							await Statistics.FileDeleted(ParentPath, FileNode.FolderPath);
					}
				}
				else if (Node is SubFolderNode SubFolderNode)
				{
					if (Directory.Exists(SubFolderNode.FolderPath))
					{
						foreach (INode Child in await SubFolderNode.ChildNodes)
							ToCheck.AddLast(new KeyValuePair<string, INode>(SubFolderNode.FolderPath, Child));
					}
					else
					{
						await SubFolderNode.DestroyAsync();

						if (!(Statistics is null))
							await Statistics.FolderDeleted(ParentPath, SubFolderNode.FolderPath);
					}
				}
				else if (Node is FolderNode FolderNode)
				{
					foreach (INode Child in await FolderNode.ChildNodes)
						ToCheck.AddLast(new KeyValuePair<string, INode>(FolderNode.FolderPath, Child));
				}
			}

			return Parent;
		}

		internal static string GetLocalName(string Path)
		{
			string[] Parts = Path.Split(System.IO.Path.DirectorySeparatorChar);
			int c = Parts.Length;
			string s;

			if (--c < 0)
				return Path;

			if (!string.IsNullOrEmpty(s = Parts[c]))
				return s;

			if (--c < 0)
				return Path;

			if (!string.IsNullOrEmpty(s = Parts[c]))
				return s;
			else
				return Path;
		}

		private static readonly string directorySeparator = new string(Path.DirectorySeparatorChar, 1);

		private async Task OnCreated(string Path)
		{
			INode Node = await this.FindNode(Path, true, null);
			await this.ExecuteAssociatedScript(Node);
		}

		private async Task OnChanged(string Path, SynchronizationStatistics Statistics)
		{
			INode Node = await this.FindNode(Path, true, Statistics);
			await this.ExecuteAssociatedScript(Node);
		}

		private async Task ExecuteAssociatedScript(INode Node)
		{
			if (Node is ScriptReferenceNode ScriptReferenceNode && !string.IsNullOrEmpty(ScriptReferenceNode.ScriptNodeId))
			{
				InternalReadoutRequest InternalReadout = new InternalReadoutRequest(Node.LogId, null,
					SensorData.FieldType.Momentary, null, DateTime.MinValue, DateTime.MaxValue,
					(sender, e) =>
					{
						ScriptReferenceNode.NewMomentaryValues(e.Fields);
						return Task.CompletedTask;
					},
					async (sender, e) =>
					{
						foreach (ThingError Error in e.Errors)
							await ScriptReferenceNode.LogErrorAsync(Error.ErrorMessage);
					}, null);

				await ScriptReferenceNode.StartReadout(InternalReadout);
			}
		}

		private async Task OnRenamed(string OldPath, string NewPath)
		{
			INode OldNode = await this.FindNode(OldPath, false, null);
			INode NewNode = await this.FindNode(NewPath, true, null);

			if (OldNode is null || OldNode.NodeId == NewNode.NodeId)
				return;

			if (NewNode is ScriptReferenceNode NewScriptReferenceNode && OldNode is ScriptReferenceNode OldScriptReferenceNode)
				NewScriptReferenceNode.ScriptNodeId = OldScriptReferenceNode.ScriptNodeId;

			if (NewNode is VirtualNode NewVirtualNode && OldNode is VirtualNode OldVirtualNode)
				NewVirtualNode.MetaData = OldVirtualNode.MetaData;

			if (NewNode is ProvisionedMeteringNode NewProvisionedMeteringNode && OldNode is ProvisionedMeteringNode OldProvisionedMeteringNode)
			{
				NewProvisionedMeteringNode.OwnerAddress = OldProvisionedMeteringNode.OwnerAddress;
				NewProvisionedMeteringNode.Public = OldProvisionedMeteringNode.Public;
				NewProvisionedMeteringNode.Provisioned = OldProvisionedMeteringNode.Provisioned;
			}

			if (NewNode is MetaMeteringNode NewMetaMeteringNode && OldNode is MetaMeteringNode OldMetaMeteringNode)
			{
				NewMetaMeteringNode.Name = OldMetaMeteringNode.Name;
				NewMetaMeteringNode.Class = OldMetaMeteringNode.Class;
				NewMetaMeteringNode.SerialNumber = OldMetaMeteringNode.SerialNumber;
				NewMetaMeteringNode.MeterNumber = OldMetaMeteringNode.MeterNumber;
				NewMetaMeteringNode.MeterLocation = OldMetaMeteringNode.MeterLocation;
				NewMetaMeteringNode.ManufacturerDomain = OldMetaMeteringNode.ManufacturerDomain;
				NewMetaMeteringNode.Model = OldMetaMeteringNode.Model;
				NewMetaMeteringNode.Version = OldMetaMeteringNode.Version;
				NewMetaMeteringNode.ProductUrl = OldMetaMeteringNode.ProductUrl;
				NewMetaMeteringNode.Country = OldMetaMeteringNode.Country;
				NewMetaMeteringNode.Region = OldMetaMeteringNode.Region;
				NewMetaMeteringNode.City = OldMetaMeteringNode.City;
				NewMetaMeteringNode.Street = OldMetaMeteringNode.Street;
				NewMetaMeteringNode.StreetNr = OldMetaMeteringNode.StreetNr;
				NewMetaMeteringNode.Building = OldMetaMeteringNode.Building;
				NewMetaMeteringNode.Apartment = OldMetaMeteringNode.Apartment;
				NewMetaMeteringNode.Room = OldMetaMeteringNode.Room;
				NewMetaMeteringNode.Latitude = OldMetaMeteringNode.Latitude;
				NewMetaMeteringNode.Longitude = OldMetaMeteringNode.Longitude;
				NewMetaMeteringNode.Altitude = OldMetaMeteringNode.Altitude;
			}

			await NewNode.UpdateAsync();
			await OldNode.DestroyAsync();

			Log.Informational("File Node renamed.",
				new KeyValuePair<string, object>("Old Node ID", OldNode.NodeId),
				new KeyValuePair<string, object>("New Node ID", NewNode.NodeId));
		}

		private async Task OnDeleted(string Path)
		{
			INode Node = await this.FindNode(Path, false, null);
			if (Node is null)
				return;

			await Node.DestroyAsync();

			Log.Informational("File Node deleted.",
				new KeyValuePair<string, object>("Node ID", Node.NodeId));
		}

		public override Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		private async Task<IEnumerable<ICommand>> GetCommands()
		{
			List<ICommand> Commands = new List<ICommand>();
			Commands.AddRange(await base.Commands);

			Commands.Add(new SynchronizeFolder(this));

			return Commands.ToArray();
		}

	}
}
