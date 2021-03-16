using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Transactions
{
	/// <summary>
	/// Interface for collections of transactions that can be monitored by <see cref="TransactionModule"/>.
	/// </summary>
	public interface ITransactions : IDisposable
	{
		/// <summary>
		/// Gets pending transactions.
		/// </summary>
		Task<ITransaction[]> GetTransactions();
	}
}
