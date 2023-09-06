using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Queries;

namespace Waher.Things.Virtual.Commands
{
	/// <summary>
	/// Adds a Duration-valued meta-data tag on a virtual node.
	/// </summary>
	internal class AddMetaDataDuration : ICommand
	{
		private readonly VirtualNode node;

		/// <summary>
		/// Adds a Duration-valued meta-data tag on a virtual node.
		/// </summary>
		/// <param name="Node">Metering node.</param>
		public AddMetaDataDuration(VirtualNode Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Tag Name
		/// </summary>
		[Page(2, "Meta-data")]
		[Header(4, "Tag Name:")]
		[ToolTip(5, "The name of the meta-data tag to add.")]
		[Required]
		public string TagName { get; set; }

		/// <summary>
		/// Tag Name
		/// </summary>
		[Page(2, "Meta-data")]
		[Header(6, "Tag Value:")]
		[ToolTip(7, "The value of the meta-data tag to add.")]
		[Required]
		public Duration TagValue { get; set; }

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "AddMetaDataDuration";

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Parametrized;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "MetaData";

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		public string SortKey => "AddDuration";

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
			return new AddMetaDataDuration(this.node);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public Task ExecuteCommandAsync()
		{
			return this.node.SetMetaData(this.TagName, this.TagValue);
		}

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(VirtualNode), 17, "Add Duration parameter...");
		}

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(VirtualNode), 9, "Parameter successfully added.");
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
			return Language.GetStringAsync(typeof(VirtualNode), 10, "Unable to add parameter.");
		}

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		public Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			return Task.CompletedTask;
		}
	}
}
