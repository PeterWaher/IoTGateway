using Waher.Runtime.Inventory;

namespace Waher.Runtime.Language
{
	/// <summary>
	/// Interface for language translators
	/// </summary>
	public interface ITranslator : IProcessingSupport<string>
	{
		/// <summary>
		/// If the translator can detect language or not.
		/// </summary>
		bool CanDetectLanguage
		{
			get;
		}

		/// <summary>
		/// What languages are supported by the translator.
		/// </summary>
		string[] SupportedLanguages
		{
			get;
		}

		/// <summary>
		/// Tries to detect the language used in the provided text.
		/// </summary>
		/// <param name="Text">Natural language text.</param>
		/// <returns>Language code of detected language, or empty string if not detected.</returns>
		string DetectLanguage(string Text);

		/// <summary>
		/// Translates natural language text, from one language to another.
		/// </summary>
		/// <param name="Text">Text to translate.</param>
		/// <param name="FromLanguage">From language</param>
		/// <param name="ToLanguage">To language</param>
		/// <returns>Translated text.</returns>
		string Translate(string Text, string FromLanguage, string ToLanguage);
	}
}
