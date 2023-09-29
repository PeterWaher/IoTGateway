namespace Waher.Networking.XMPP.Contracts.HumanReadable
{
	/// <summary>
	/// Type of Markdown to generate
	/// </summary>
	public enum MarkdownType
	{
		/// <summary>
		/// Markdown used for rendering
		/// </summary>
		ForRendering,

		/// <summary>
		/// Markdown used for editing
		/// </summary>
		ForEditing
	}

	/// <summary>
	/// Settings used for Markdown generation of human-readable text.
	/// </summary>
	public class MarkdownSettings
	{
		/// <summary>
		/// Settings used for Markdown generation of human-readable text.
		/// </summary>
		/// <param name="Contract">Contract containing the human-readable text.</param>
		/// <param name="Type">Type of Markdown being generated.</param>
		public MarkdownSettings(Contract Contract, MarkdownType Type)
		{
			this.Contract = Contract;
			this.Type = Type;
			this.SimpleEscape = Type == MarkdownType.ForEditing;
		}

		/// <summary>
		/// Contract containing the human-readable text.
		/// </summary>
		public Contract Contract { get; }

		/// <summary>
		/// Type of Markdown being generated.
		/// </summary>
		public MarkdownType Type { get; }

		/// <summary>
		/// If simple Markdown escaping rules apply.
		/// </summary>
		public bool SimpleEscape { get; }
	}
}
