﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Constants
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
		public string ConstantName => nameof(Language);

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases => Array.Empty<string>();

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			if (!Variables.TryGetVariable("Language", out Variable v) ||
				!(v.ValueObject is Waher.Runtime.Language.Language Language))
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
		public static async Task<Waher.Runtime.Language.Language> GetLanguageAsync(Variables Session)
		{
			if (Session.TryGetVariable("Language", out Variable v))
			{
				if (v.ValueObject is Waher.Runtime.Language.Language Language)
					return Language;
				else if (v.ValueObject is string LanguageCode && !string.IsNullOrEmpty(LanguageCode))
					return await Translator.GetLanguageAsync(LanguageCode);
			}

			if (Session is SessionVariables SessionVariables &&
				!(SessionVariables.CurrentRequest is null))
			{
				if (!(SessionVariables.CurrentRequest.Header.AcceptLanguage is null))
				{
					List<string> Alternatives = new List<string>();

					foreach (Waher.Runtime.Language.Language Language in await Translator.GetLanguagesAsync())
						Alternatives.Add(Language.Code);

					string Best = SessionVariables.CurrentRequest.Header.AcceptLanguage.GetBestAlternative(Alternatives.ToArray());
					if (!string.IsNullOrEmpty(Best))
						return await Translator.GetLanguageAsync(Best);
				}
			}

			return await Translator.GetDefaultLanguageAsync();
		}

	}
}
