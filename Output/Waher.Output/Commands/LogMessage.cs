using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Queries;

namespace Waher.Output.Commands
{
	/// <summary>
	/// Logs a message on an output.
	/// </summary>
	public class LogMessage : ICommand
	{
		private readonly OutputNode node;
		private MessageType messageType = MessageType.Information;
		private string messageBody = string.Empty;

		/// <summary>
		/// Logs a message on an output.
		/// </summary>
		/// <param name="Node">Job node.</param>
		public LogMessage(OutputNode Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Type of message.
		/// </summary>
		[Page(5, "Message")]
		[Header(6, "Type:")]
		[ToolTip(7, "Type of message to log.")]
		[Required]
		public MessageType MessageType
		{
			get => this.messageType;
			set => this.messageType = value;
		}

		/// <summary>
		/// Message body
		/// </summary>
		[Page(5, "Message")]
		[Header(8, "Body:")]
		[ToolTip(9, "Text of the message to log.")]
		[Required]
		public string MessageBody
		{
			get => this.messageBody;
			set => this.messageBody = value;
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => nameof(LogMessage);

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Parametrized;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "Output";

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		public string SortKey => nameof(LogMessage);

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
			return new LogMessage(this.node);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public Task ExecuteCommandAsync()
		{
			return this.node.LogMessageAsync(this.messageType, this.messageBody);
		}

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(LogMessage), 10, "Log message...");
		}

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(LogMessage), 11, "Message successfully logged.");
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
			return Language.GetStringAsync(typeof(LogMessage), 12, "Unable to log message.");
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
