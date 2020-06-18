using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for localized string array responses callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task LocalizedStringsResponseEventHandler(object Sender, LocalizedStringsResponseEventArgs e);

	/// <summary>
	/// Represents a localized string
	/// </summary>
	public struct LocalizedString
	{
		/// <summary>
		/// Unlocalized string.
		/// </summary>
		public string Unlocalized;

		/// <summary>
		/// Localized (human readable) string.
		/// </summary>
		public string Localized;
	}

	/// <summary>
	/// Event arguments for localized string array responses responsess.
	/// </summary>
	public class LocalizedStringsResponseEventArgs : IqResultEventArgs
	{
		private readonly LocalizedString[] result;

		internal LocalizedStringsResponseEventArgs(LocalizedString[] Result, IqResultEventArgs Responses)
			: base(Responses)
		{
			this.result = Result;
		}

		/// <summary>
		/// Result of operation.
		/// </summary>
		public LocalizedString[] Result
		{
			get { return this.result; }
		}
	}
}
