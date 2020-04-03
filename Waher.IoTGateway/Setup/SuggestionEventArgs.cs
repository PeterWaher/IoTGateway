using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Delegate for suggestion event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void SuggesstionsEventHandler(object Sender, SuggestionEventArgs e);

	/// <summary>
	/// Event arguments for suggestion event handlers.
	/// </summary>
	public class SuggestionEventArgs : EventArgs
	{
		private readonly SortedDictionary<string, bool> suggestions;
		private readonly string startsWith;

		/// <summary>
		/// Event arguments for suggestion event handlers.
		/// </summary>
		public SuggestionEventArgs(string StartsWith)
		{
			this.suggestions = new SortedDictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
			this.startsWith = StartsWith;
		}

		/// <summary>
		/// Only suggestions starting with this string will be returned.
		/// </summary>
		public string StartsWith => this.startsWith;

		/// <summary>
		/// Adds a suggestion.
		/// </summary>
		/// <param name="Suggestion">Group name.</param>
		public void AddSuggestion(string Suggestion)
		{
			if (Suggestion.StartsWith(this.startsWith, StringComparison.InvariantCultureIgnoreCase) &&
				!this.suggestions.ContainsKey(Suggestion))
			{
				this.suggestions[Suggestion] = true;
			}
		}

		/// <summary>
		/// Returns suggestions, as an array of strings.
		/// </summary>
		/// <returns>Suggestions.</returns>
		public string[] ToArray()
		{
			string[] Result = new string[this.suggestions.Count];
			this.suggestions.Keys.CopyTo(Result, 0);
			return Result;
		}
	}
}
