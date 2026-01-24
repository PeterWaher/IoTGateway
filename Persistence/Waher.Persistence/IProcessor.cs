using System.Threading.Tasks;

namespace Waher.Persistence
{
	/// <summary>
	/// Interface for processors of objects.
	/// </summary>
	public interface IProcessor<T>
	{
		/// <summary>
		/// If the processor operates asynchronously.
		/// </summary>
		bool IsAsynchronous { get; }

		/// <summary>
		/// Processes an object synchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		bool Process(T Object);

		/// <summary>
		/// Processes an object asynchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		Task<bool> ProcessAsync(T Object);
	}
}
