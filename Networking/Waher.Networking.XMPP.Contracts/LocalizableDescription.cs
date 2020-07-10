using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Abstract base class for contractual parameters
	/// </summary>
	public abstract class LocalizableDescription
	{
		private HumanReadableText[] descriptions = null;

		/// <summary>
		/// Discriptions of the object, in different languages.
		/// </summary>
		public HumanReadableText[] Descriptions
		{
			get => this.descriptions;
			set => this.descriptions = value;
		}

		/// <summary>
		/// Creates a human-readable Markdown document for the contract.
		/// </summary>
		/// <param name="Language">Desired language</param>
		/// <param name="Contract">Contract hosting the object.</param>
		/// <returns>Markdown</returns>
		public string ToMarkdown(string Language, Contract Contract)
		{
			return Contract.ToMarkdown(this.descriptions, Language);
		}

		/// <summary>
		/// Creates a human-readable HTML document for the contract.
		/// </summary>
		/// <param name="Language">Desired language</param>
		/// <param name="Contract">Contract hosting the object.</param>
		/// <returns>Markdown</returns>
		public string ToHTML(string Language, Contract Contract)
		{
			return Contract.ToHTML(this.descriptions, Language);
		}

		/// <summary>
		/// Creates a human-readable Plain Trext document for the contract.
		/// </summary>
		/// <param name="Language">Desired language</param>
		/// <param name="Contract">Contract hosting the object.</param>
		/// <returns>Markdown</returns>
		public string ToPlainText(string Language, Contract Contract)
		{
			return Contract.ToPlainText(this.descriptions, Language);
		}

		/// <summary>
		/// Creates a human-readable WPF XAML document for the contract.
		/// </summary>
		/// <param name="Language">Desired language</param>
		/// <param name="Contract">Contract hosting the object.</param>
		/// <returns>Markdown</returns>
		public string ToXAML(string Language, Contract Contract)
		{
			return Contract.ToXAML(this.descriptions, Language);
		}

		/// <summary>
		/// Creates a human-readable Xamarin.Forms XAML document for the contract.
		/// </summary>
		/// <param name="Language">Desired language</param>
		/// <param name="Contract">Contract hosting the object.</param>
		/// <returns>Markdown</returns>
		public string ToXamarinForms(string Language, Contract Contract)
		{
			return Contract.ToXamarinForms(this.descriptions, Language);
		}
	}
}
