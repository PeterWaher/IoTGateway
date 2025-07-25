﻿using System;
using System.Collections.Generic;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
	/// <summary>
	/// Event arguments for identity validation responses
	/// </summary>
	public class ContractValidationEventArgs : EventArgs
	{
		private readonly KeyValuePair<string, object>[] tags;
		private readonly ContractStatus status;
		private readonly object state;

		/// <summary>
		/// Event arguments for identity validation responses
		/// </summary>
		/// <param name="Status">Validation status</param>
		/// <param name="State">State object</param>
		/// <param name="Tags">Associated tags with more information.</param>
		public ContractValidationEventArgs(ContractStatus Status, object State,
			params KeyValuePair<string, object>[] Tags)
			: base()
		{
			this.status = Status;
			this.state = State;
			this.tags = Tags;
		}

		/// <summary>
		/// Validation status of smart contract.
		/// </summary>
		public ContractStatus Status => this.status;

		/// <summary>
		/// Associated tags with more information.
		/// </summary>
		public KeyValuePair<string, object>[] Tags => this.tags;

		/// <summary>
		/// State object.
		/// </summary>
		public object State => this.state;
	}
}
