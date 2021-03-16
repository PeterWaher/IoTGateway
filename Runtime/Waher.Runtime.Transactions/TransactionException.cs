using System;
using Waher.Events;

namespace Waher.Runtime.Transactions
{
	/// <summary>
	/// Exception object for transaction exceptions.
	/// </summary>
	public class TransactionException : Exception, IEventObject
	{
		private readonly ITransaction transaction;

		/// <summary>
		/// Exception object for transaction exceptions.
		/// </summary>
		/// <param name="Transaction">Reference to transaction object.</param>
		/// <param name="Message">Exception message.</param>
		public TransactionException(ITransaction Transaction, string Message)
			: base(Message)
		{
			this.transaction = Transaction;
		}

		/// <summary>
		/// Exception object for transaction exceptions.
		/// </summary>
		/// <param name="Transaction">Reference to transaction object.</param>
		/// <param name="Message">Exception message.</param>
		/// <param name="InnerException">Inner exception.</param>
		public TransactionException(ITransaction Transaction, string Message, Exception InnerException)
			: base(Message, InnerException)
		{
			this.transaction = Transaction;
		}

		/// <summary>
		/// Reference to transaction object.
		/// </summary>
		public ITransaction Transaction => this.transaction;

		/// <summary>
		/// Object identifier related to the object.
		/// </summary>
		public string Object => this.transaction.Id.ToString();
	}
}
