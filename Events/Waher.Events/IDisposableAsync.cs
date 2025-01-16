using System;
using System.Threading.Tasks;

namespace Waher.Events
{
	/// <summary>
	/// Interface for asynchronously disposable objects.
	/// </summary>
	public interface IDisposableAsync : IDisposable
	{
		/// <summary>
		/// Disposes the connection
		/// </summary>
		[Obsolete("Use DisposeAsync instead.")]
		new void Dispose();

		/// <summary>
		/// Disposes of the object, asynchronously.
		/// </summary>
		Task DisposeAsync();
	}
}
