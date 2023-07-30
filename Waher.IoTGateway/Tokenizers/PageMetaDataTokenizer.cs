using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Html;
using Waher.Content.Html.OpenGraph;
using Waher.Persistence.FullTextSearch;
using Waher.Persistence.FullTextSearch.Tokenizers;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.Tokenizers
{
	/// <summary>
	/// Tokenizes meta-data information about some Internet Content.
	/// </summary>
	public class PageMetaDataTokenizer : ITokenizer
	{
		/// <summary>
		/// Tokenizes meta-data information about some Internet Content.
		/// </summary>
		public PageMetaDataTokenizer()
		{
		}

		/// <summary>
		/// If the interface understands objects such as <paramref name="Type"/>.
		/// </summary>
		/// <param name="Type">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Type)
		{
			if (Type == typeof(PageMetaData))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Value">Object to tokenize.</param>
		/// <param name="Process">Current tokenization process.</param>
		public async Task Tokenize(object Value, TokenizationProcess Process)
		{
			if (Value is PageMetaData Data)
				await Tokenize(Data, Process);
		}

		/// <summary>
		/// Tokenizes an HTML document.
		/// </summary>
		/// <param name="Data">Page Meta-data.</param>
		/// <param name="Process">Current tokenization process.</param>
		public static Task Tokenize(PageMetaData Data, TokenizationProcess Process)
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			Append(sb, ref First, Data.Determiner);
			Append(sb, ref First, Data.Title);
			Append(sb, ref First, Data.Description);
			Append(sb, ref First, Data.SiteName);
			Append(sb, ref First, Data.Locale);
			Append(sb, ref First, Data.LocaleAlternate);

			if (!(Data.Images is null))
			{
				foreach (ImageInformation Info in Data.Images)
					Append(sb, ref First, Info.Description);
			}

			StringTokenizer.Tokenize(sb.ToString(), Process);

			return Task.CompletedTask;
		}

		private static void Append(StringBuilder Text, ref bool First, string[] Value)
		{
			if (!(Value is null))
			{
				foreach (string s in Value)
					Append(Text, ref First, s);
			}
		}

		private static void Append(StringBuilder Text, ref bool First, string Value)
		{
			if (!string.IsNullOrEmpty(Value))
			{
				if (First)
					First = false;
				else
					Text.Append(' ');

				Text.Append(Value);
			}
		}
	}
}
