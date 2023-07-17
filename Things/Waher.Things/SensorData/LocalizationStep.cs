using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;

namespace Waher.Things.SensorData
{
	/// <summary>
	/// Represents a localization step, as defined in XEP-323:
	/// http://xmpp.org/extensions/xep-0323.html#localization
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	public class LocalizationStep
	{
		private int stringId;
		private string module;
		private string seed;

		/// <summary>
		/// Represents a localization step, as defined in XEP-323:
		/// http://xmpp.org/extensions/xep-0323.html#localization
		/// </summary>
		public LocalizationStep()
		{
			this.stringId = 0;
			this.module = null;
			this.seed = null;
		}

		/// <summary>
		/// Represents a localization step, as defined in XEP-323:
		/// http://xmpp.org/extensions/xep-0323.html#localization
		/// </summary>
		/// <param name="StringId">String ID</param>
		public LocalizationStep(int StringId)
		{
			this.stringId = StringId;
			this.module = null;
			this.seed = null;
		}

		/// <summary>
		/// Represents a localization step, as defined in XEP-323:
		/// http://xmpp.org/extensions/xep-0323.html#localization
		/// </summary>
		/// <param name="StringId">String ID</param>
		/// <param name="Module">Optional Language Module</param>
		public LocalizationStep(int StringId, string Module)
		{
			this.stringId = StringId;
			this.module = Module;
			this.seed = null;
		}

		/// <summary>
		/// Represents a localization step, as defined in XEP-323:
		/// http://xmpp.org/extensions/xep-0323.html#localization
		/// </summary>
		/// <param name="StringId">String ID</param>
		/// <param name="Module">Optional Language Module</param>
		/// <param name="Seed">Optional Seed value.</param>
		public LocalizationStep(int StringId, string Module, string Seed)
		{
			this.stringId = StringId;
			this.module = Module;
			this.seed = Seed;
		}

		/// <summary>
		/// String ID
		/// </summary>
		[ShortName("i")]
		public int StringId
		{
			get => this.stringId;
			set => this.stringId = value;
		}

		/// <summary>
		/// Optional language module, if different from the base module.
		/// </summary>
		[DefaultValueNull]
		[ShortName("m")]
		public string Module
		{
			get => this.module;
			set => this.module = value;
		}

		/// <summary>
		/// Optional localization seed.
		/// </summary>
		[DefaultValueNull]
		[ShortName("s")]
		public string Seed
		{
			get => this.seed;
			set => this.seed = value;
		}

		/// <summary>
		/// Tries to get the localization of a string, given a sequence of localization steps.
		/// </summary>
		/// <param name="Language">Language</param>
		/// <param name="BaseModule">Base module to use, if another module is not specified.</param>
		/// <param name="Steps">Sequence of steps.</param>
		/// <returns>Localized result, if successful, null if not.</returns>
		public static async Task<string> TryGetLocalization(Language Language, Namespace BaseModule, params LocalizationStep[] Steps)
		{
			if (Steps is null || Steps.Length == 0)
				return null;

			string Result = Steps[0].seed ?? string.Empty;

			Namespace Namespace;

			foreach (LocalizationStep Step in Steps)
			{
				if (!string.IsNullOrEmpty(Step.module))
					Namespace = await Language.GetNamespaceAsync(Step.module);
				else
					Namespace = BaseModule;

				if (Namespace is null)
					return null;

				LanguageString String = await BaseModule.GetStringAsync(Step.stringId);
				if (String is null)
					return null;

				Result = String.Value.Replace("%0%", Result);

				if (!string.IsNullOrEmpty(Step.seed))
					Result = Result.Replace("%1%", Step.seed);
			}

			return Result;
		}
	}
}
