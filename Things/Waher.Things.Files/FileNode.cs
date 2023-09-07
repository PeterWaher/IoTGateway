using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Script;

namespace Waher.Things.Files
{
	/// <summary>
	/// Represents a file in the file system.
	/// </summary>
	public class FileNode : ScriptReferenceNode
	{
		/// <summary>
		/// Represents a file in the file system.
		/// </summary>
		public FileNode()
		{
		}

		/// <summary>
		/// Full path to folder.
		/// </summary>
		[Page(2, "File System", 100)]
		[Header(13, "File:")]
		[ToolTip(14, "Full path to file (on host).")]
		public string FolderPath { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(FolderNode), 15, "File reference");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is FolderNode || Parent is SubFolderNode);
		}
	}
}
