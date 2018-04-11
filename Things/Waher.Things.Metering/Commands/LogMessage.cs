using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Queries;
using Waher.Things.SourceEvents;
using Waher.Persistence;
using Waher.Persistence.Filters;

namespace Waher.Things.Metering.Commands
{
	/// <summary>
	/// Logs a message on a node.
	/// </summary>
	public class LogMessage : ICommand
	{
		private MeteringNode node;
		private MessageType messageType = MessageType.Information;
		private string messaageBody = string.Empty;

		/// <summary>
		/// Logs a message on a node.
		/// </summary>
		/// <param name="Node">Metering node.</param>
		public LogMessage(MeteringNode Node)
		{
			this.node = Node;
		}

		/// <summary>
		/// Type of message.
		/// </summary>
		[Page(100, "Message")]
		[Header(101, "Type:")]
		[ToolTip(102, "Type of message to log.")]
		[Required]
		public MessageType MessageType
		{
			get { return this.messageType; }
			set { this.messageType = value; }
		}

		/// <summary>
		/// Message body
		/// </summary>
		[Page(100, "Message")]
		[Header(103, "Body:")]
		[ToolTip(104, "Text of the message to log.")]
		[Required]
		public string MessageBody
		{
			get { return this.messaageBody; }
			set { this.messaageBody = value; }
		}

		/// <summary>
		/// ID of command.
		/// </summary>
		public string CommandID => "LogMessage";

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => CommandType.Parametrized;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "Node";

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		public string SortKey => "LogMessage";

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
			return this.node.LogMessageAsync(this.messageType, this.messaageBody);
		}

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MeteringTopology), 105, "Log message...");
		}

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetSuccessStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MeteringTopology), 106, "Message successfully logged.");
		}

		/// <summary>
		/// Gets a confirmation string, if any, of the command. If no confirmation is necessary, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetConfirmationStringAsync(Language Language)
		{
			return Task.FromResult<string>(string.Empty);
		}

		/// <summary>
		/// Gets a failure string, if any, of the command. If no specific failure string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MeteringTopology), 107, "Unable to log message.");
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
