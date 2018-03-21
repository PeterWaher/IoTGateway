using System;
using System.Collections.Generic;
using System.Text;
using Waher.Things;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Information about a command on a node.
	/// </summary>
    public class NodeCommand
    {
		private string command;
		private string name;
		private CommandType type;
		private string successString;
		private string failureString;
		private string confirmationString;
		private string sortCategory;
		private string sortKey;

		/// <summary>
		/// Information about a command on a node.
		/// </summary>
		/// <param name="Command">Command ID</param>
		/// <param name="Name">Localized name of command.</param>
		/// <param name="Type">Type of command.</param>
		/// <param name="SuccessString">Optional localized string to display to the user, if command executes successfully.</param>
		/// <param name="FailureString">Optional localized string to display to the user, if command fails.</param>
		/// <param name="ConfirmationString">Optional localized string to display to the user before executing the command.</param>
		/// <param name="SortCategory">Sort category.</param>
		/// <param name="SortKey">Sort key (within category).</param>
		public NodeCommand(string Command, string Name, CommandType Type, string SuccessString, string FailureString, string ConfirmationString,
			string SortCategory, string SortKey)
		{
			this.command = Command;
			this.name = Name;
			this.type = Type;
			this.successString = SuccessString;
			this.failureString = FailureString;
			this.confirmationString = ConfirmationString;
			this.sortCategory = SortCategory;
			this.sortKey = SortKey;
		}

		/// <summary>
		/// Command ID
		/// </summary>
		public string Command => this.command;

		/// <summary>
		/// Localized name of command.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Type of command.
		/// </summary>
		public CommandType Type => this.type;

		/// <summary>
		/// Optional localized string to display to the user, if command executes successfully.
		/// </summary>
		public string SuccessString => this.successString;

		/// <summary>
		/// Optional localized string to display to the user, if command fails.
		/// </summary>
		public string FailureString => this.failureString;

		/// <summary>
		/// Optional localized string to display to the user before executing the command.
		/// </summary>
		public string ConfirmationString => this.confirmationString;

		/// <summary>
		/// Sort category.
		/// </summary>
		public string SortCategory => this.sortCategory;

		/// <summary>
		/// Sort key (within category).
		/// </summary>
		public string SortKey => this.sortKey;
	}
}
