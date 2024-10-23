﻿using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Runtime.Language;
using Waher.Things.Queries;

namespace Waher.Things.Xmpp.Commands
{
	/// <summary>
	/// Abstract base class for concentrator commands.
	/// </summary>
	public abstract class ConcentratorCommand : ICommand
	{
		private readonly ConcentratorDevice concentrator;
		private readonly string sortKey;

		/// <summary>
		/// Scans a concentrator node for its root sources.
		/// </summary>
		/// <param name="Concentrator">Concentrator node.</param>
		/// <param name="SortKey">Sort key</param>
		public ConcentratorCommand(ConcentratorDevice Concentrator, string SortKey)
		{
			this.concentrator = Concentrator;
			this.sortKey = SortKey;
		}

		/// <summary>
		/// Reference to the concentrator node.
		/// </summary>
		public ConcentratorDevice Concentrator => this.concentrator;

		/// <summary>
		/// ID of command.
		/// </summary>
		public abstract string CommandID { get; }

		/// <summary>
		/// Type of command.
		/// </summary>
		public virtual CommandType Type => CommandType.Simple;

		/// <summary>
		/// Sort Category, if available.
		/// </summary>
		public string SortCategory => "Concentrator";

		/// <summary>
		/// Sort Key, if available.
		/// </summary>
		public string SortKey => this.sortKey;

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public abstract Task<string> GetNameAsync(Language Language);

		/// <summary>
		/// Gets a confirmation string, if any, of the command. If no confirmation is necessary, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public virtual Task<string> GetConfirmationStringAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// Gets a failure string, if any, of the command. If no specific failure string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public virtual Task<string> GetFailureStringAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// Gets a success string, if any, of the command. If no specific success string is available, null, or the empty string can be returned.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public virtual Task<string> GetSuccessStringAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// If the command can be executed by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the command can be executed by the caller.</returns>
		public virtual Task<bool> CanExecuteAsync(RequestOrigin Caller)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		public virtual Task ExecuteCommandAsync() => Task.CompletedTask;

		/// <summary>
		/// Starts the execution of a query.
		/// </summary>
		/// <param name="Query">Query data receptor.</param>
		/// <param name="Language">Language to use.</param>
		public virtual Task StartQueryExecutionAsync(Query Query, Language Language) => Task.CompletedTask;

		/// <summary>
		/// Creates a copy of the command object.
		/// </summary>
		/// <returns>Copy of command object.</returns>
		public abstract ICommand Copy();

		/// <summary>
		/// Gets the concentrator client, if it exists.
		/// </summary>
		/// <returns>Reference to concentrator client.</returns>
		/// <exception cref="Exception">If the client is not found.</exception>
		public async Task<ConcentratorClient> GetClient()
		{
			ConcentratorClient Client = await this.concentrator.GetConcentratorClient()
				?? throw new Exception("Concentrator client not found.");

			return Client;
		}

		/// <summary>
		/// Gets the Full JID of the remote concentrator.
		/// </summary>
		/// <param name="Client">Concentrator client.</param>
		/// <returns>Full JID, if found.</returns>
		/// <exception cref="Exception">If unable to get the Full JID.</exception>
		public string GetRemoteFullJid(ConcentratorClient Client)
		{
			RosterItem Contact = Client.Client[this.concentrator.JID]
				?? throw new Exception(this.concentrator.JID + " is not in the list of contacts.");

			if (!(Contact.State == SubscriptionState.Both ||
				Contact.State == SubscriptionState.To))
			{
				throw new Exception("Not subscribed to the precense of " + this.concentrator.JID);
			}

			string FullJid = Contact.LastPresenceFullJid;
			if (!string.IsNullOrEmpty(FullJid))
				throw new Exception(this.concentrator.JID + " is not online.");

			return FullJid;
		}
	}
}
