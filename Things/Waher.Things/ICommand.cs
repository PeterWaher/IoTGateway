using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things.Queries;
using Waher.Runtime.Language;

namespace Waher.Things
{
	/// <summary>
	/// Command type.
	/// </summary>
	public enum CommandType
	{
		/// <summary>
		/// Simple un-parametrized command.
		/// </summary>
		Simple,

		/// <summary>
		/// Parametrized command.
		/// </summary>
		Parametrized,

		/// <summary>
		/// Parametrized query.
		/// </summary>
		Query
	}

	/// <summary>
	/// Interface for commands.
	/// </summary>
	public interface ICommand
	{
		/// <summary>
		/// ID of command.
		/// </summary>
		string CommandID
		{
			get;
		}

		/// <summary>
		/// Type of command.
		/// </summary>
		CommandType Type
		{
			get;
		}

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		string SortCategory
		{
			get;
		}

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		string SortKey
		{
			get;
		}

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		Task<string> GetNameAsync(Language Language);

		/// <summary>
		/// Gets a confirmation string, if any, of the command. If no confirmation is necessary, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		Task<string> GetConfirmationStringAsync(Language Language);

		/// <summary>
		/// Gets a failure string, if any, of the command. If no specific failure string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		Task<string> GetFailureStringAsync(Language Language);

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		Task<string> GetSuccessStringAsync(Language Language);

		/// <summary>
		/// If the command can be executed by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the command can be executed by the caller.</returns>
		Task<bool> CanExecuteAsync(RequestOrigin Caller);

		/// <summary>
		/// Executes the command.
		/// </summary>
		Task ExecuteCommandAsync();

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		Task StartQueryExecutionAsync(Query Query);

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		ICommand Copy();
	}
}
