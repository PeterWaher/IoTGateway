using System;
using System.Threading.Tasks;

namespace Waher.Persistence
{
	/// <summary>
	/// Event handler for collection events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task CollectionEventHandler(object Sender, CollectionEventArgs e);

	/// <summary>
	/// Event arguments for collection events.
	/// </summary>
	public class CollectionEventArgs : EventArgs
	{
		private readonly string collection;

		/// <summary>
		/// Event arguments for collection events.
		/// </summary>
		/// <param name="Collection">Collection</param>
		public CollectionEventArgs(string Collection)
		{
			this.collection = Collection;
		}

		/// <summary>
		/// Collection
		/// </summary>
		public string Collection => this.collection;
	}
}
