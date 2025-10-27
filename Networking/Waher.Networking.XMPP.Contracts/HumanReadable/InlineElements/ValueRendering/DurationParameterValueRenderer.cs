using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements.ValueRendering
{
	/// <summary>
	/// Converts a Duration parameter value to a human-readable string.
	/// </summary>
	public class DurationParameterValueRenderer : ParameterValueRenderer
	{
		/// <summary>
		/// Converts a Duration parameter value to a human-readable string.
		/// </summary>
		public DurationParameterValueRenderer()
			: base()
		{
		}

		/// <summary>
		/// How well a parameter value is supported.
		/// </summary>
		/// <param name="Value">Parameter value.</param>
		/// <returns>How well values of this type are supported.</returns>
		public override Grade Supports(object Value)
		{
			return Value is Duration ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Generates a Markdown string from the parameter value.
		/// </summary>
		/// <param name="Value">Parameter value.</param>
		/// <param name="Language">Desired language.</param>
		/// <param name="Settings">Markdown settings.</param>
		/// <returns>Markdown string.</returns>
		public override async Task<string> ToString(object Value, string Language, 
			MarkdownSettings Settings)
		{
			if (!(Value is Duration D))
				return await base.ToString(Value, Language, Settings);

			Language Language2 = await Translator.GetLanguageAsync(Settings.Contract.DefaultLanguage);
			Namespace Namespace = Language2 is null ? null : await Language2.GetNamespaceAsync(typeof(ContractsClient).Namespace);

			ChunkedList<string> Parts = new ChunkedList<string>();

			if (D.Years != 0)
				Parts.Add(await ToString(D.Years, 1, "year", 2, "years", Namespace));

			if (D.Months != 0)
				Parts.Add(await ToString(D.Months, 3, "month", 4, "months", Namespace));

			if (D.Days != 0)
				Parts.Add(await ToString(D.Days, 5, "day", 6, "days", Namespace));

			if (D.Hours != 0)
				Parts.Add(await ToString(D.Hours, 7, "hour", 8, "hours", Namespace));

			if (D.Minutes != 0)
				Parts.Add(await ToString(D.Minutes, 9, "minute", 10, "minutes", Namespace));

			if (D.Seconds != 0)
				Parts.Add(await ToString(D.Seconds, 11, "second", 12, "seconds", Namespace));

			if (Parts.Count == 0)
				Parts.Add(await ToString(0, 11, "second", 12, "seconds", Namespace));

			int i, c = Parts.Count;
			StringBuilder sb = new StringBuilder();

			if (D.Negation)
				sb.Append('-');

			for (i = 0; i < c; i++)
			{
				if (i > 0)
				{
					if (i == c - 1)
						sb.Append(" and ");
					else
						sb.Append(", ");
				}

				sb.Append(Parts[i]);
			}

			return sb.ToString();
		}

		private static async Task<string> ToString(int Value, int SingularId, string SingularUnit,
			int PluralId, string PluralUnit, Namespace Namespace)
		{
			if (Namespace is null)
			{
				if (Value == 1)
					return "1 " + SingularUnit;
				else
					return Value.ToString() + " " + PluralUnit;
			}
			else
			{
				if (Value == 1)
					return "1 " + await Namespace.GetStringAsync(SingularId, SingularUnit);
				else
					return Value.ToString() + " " + await Namespace.GetStringAsync(PluralId, PluralUnit);
			}
		}

		private static async Task<string> ToString(double Value, int SingularId, string SingularUnit,
			int PluralId, string PluralUnit, Namespace Namespace)
		{
			if (Namespace is null)
			{
				if (Value == 1)
					return "1 " + SingularUnit;
				else
					return Value.ToString() + " " + PluralUnit;
			}
			else
			{
				if (Value == 1)
					return "1 " + await Namespace.GetStringAsync(SingularId, SingularUnit);
				else
					return Value.ToString() + " " + await Namespace.GetStringAsync(PluralId, PluralUnit);
			}
		}
	}
}
