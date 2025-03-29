using System;

namespace Waher.Persistence
{
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
