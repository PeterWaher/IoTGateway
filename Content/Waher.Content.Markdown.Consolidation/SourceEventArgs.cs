using System;
using System.Threading.Tasks;

namespace Waher.Content.Markdown.Consolidation
{
	/// <summary>
	/// Delegate for source events.
	/// </summary>
	/// <param name="Sender">Sender of events.</param>
	/// <param name="e">Event arguments</param>
	public delegate void SourceEventHandler(object Sender, SourceEventArgs e);

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
