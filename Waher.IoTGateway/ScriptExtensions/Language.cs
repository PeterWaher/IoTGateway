using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions
{
	/// <summary>
	/// Points to the language object for the current session.
	/// </summary>
	public class Language : IConstant
	{
		/// <summary>
		/// Points to the language object for the current session.
		/// </summary>
		public Language()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName => "Language";

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases => new string[0];

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			if (!Variables.TryGetVariable("Language", out Variable v) ||
				!(v.ValueObject is Runtime.Language.Language Language))
			{
				Language = GetLanguageAsync(Variables).Result;
				Variables["Language"] = Language;
			}

			return new ObjectValue(Language);
		}

		/// <summary>
		/// Gets the current language for the session.
		/// </summary>
		/// <param name="Session">Session variables.</param>
		/// <returns>Language object</returns>
		public static async Task<Runtime.Language.Language> GetLanguageAsync(Variables Session)
		{
			if (Session.TryGetVariable("Language", out Variable v))
			{
				if (v.ValueObject is Runtime.Language.Language Language)
					return Language;
				else if (v.ValueObject is string LanguageCode)
					return await Translator.GetLanguageAsync(LanguageCode);
			}

			if (Session.TryGetVariable("Request", out v) &&
				v.ValueObject is HttpRequest Request)
			{
				if (!(Request.Header.AcceptLanguage is null))
				{
					List<string> Alternatives = new List<string>();

					foreach (Runtime.Language.Language Language in await Translator.GetLanguagesAsync())
						Alternatives.Add(Language.Code);

					string Best = Request.Header.AcceptLanguage.GetBestAlternative(Alternatives.ToArray());
					if (!string.IsNullOrEmpty(Best))
						return await Translator.GetLanguageAsync(Best);
				}
			}

			return await Translator.GetDefaultLanguageAsync();
		}

	}
}
