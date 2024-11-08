using System;

namespace Waher.Content.Markdown.Consolidation
{
	/// <summary>
	/// Event arguments for source events.
	/// </summary>
	public class SourceEventArgs : EventArgs
	{
		private readonly string source;

		/// <summary>
		/// Event arguments for source events.
		/// </summary>
		/// <param name="Source">Source causing the event to be raised.</param>
		public SourceEventArgs(string Source)
			: base()
		{
			this.source = Source;
		}

		/// <summary>
		/// Source causing the event to be raised.
		/// </summary>
		public string Source => this.source;
	}
}
