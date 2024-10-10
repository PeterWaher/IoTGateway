namespace Waher.Persistence
{
	/// <summary>
	/// Enumerator of ledger entries
	/// </summary>
	/// <typeparam name="T">Type of objects being processed.</typeparam>
	public interface ILedgerEnumerator<T> : IAsyncEnumerator<ILedgerEntry<T>>
	{
	}
}
