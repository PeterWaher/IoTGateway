using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Waher.Persistence
{
	/// <summary>
	/// Event handler for collection repaired events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void CollectionRepairedEventHandler(object Sender, CollectionRepairedEventArgs e);

	/// <summary>
	/// Event arguments for collection repaired events.
	/// </summary>
	public class CollectionRepairedEventArgs : CollectionEventArgs
	{
		private readonly FlagSource[] flagged;

		/// <summary>
		/// Event arguments for collection repaired events.
		/// </summary>
		/// <param name="Collection">Collection</param>
		/// <param name="Flagged">If the collection have been flagged as corrupt, and from what stack traces. Is null, if collection not flagged.</param>
		public CollectionRepairedEventArgs(string Collection, FlagSource[] Flagged)
			: base(Collection)
		{
			this.flagged = Flagged;
		}

		/// <summary>
		/// If the collection have been flagged as corrupt, and from what stack traces. Is null, if collection not flagged.
		/// </summary>
		public FlagSource[] Flagged => this.flagged;
	}
}
