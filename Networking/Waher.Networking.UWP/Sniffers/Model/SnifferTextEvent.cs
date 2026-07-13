using System;
using System.Text;

namespace Waher.Networking.Sniffers.Model
{
	/// <summary>
	/// Base class for text-based sniffer events.
	/// </summary>
	public abstract class SnifferTextEvent : SnifferEvent
	{
		private static readonly string[] sensitiveWords = new string[]
		{
			"password",
			"secret",
			"key",
			"authorization",
			"access_token",
			"refresh_token"
		};
		private static readonly int nrSensitiveWords = sensitiveWords.Length;

		private readonly string text;

		/// <summary>
		/// Base class for text-based sniffer events.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text.</param>
		/// <param name="Processor">Sniff event processor</param>
		public SnifferTextEvent(DateTime Timestamp, string Text, ISniffEventProcessor Processor)
			: base(Timestamp, Processor)
		{
			string s = Text?.ToLower() ?? string.Empty;
			int i;

			for (i = 0; i < nrSensitiveWords; i++)
			{
				if (s.Contains(sensitiveWords[i]))
					break;
			}

			if (i < nrSensitiveWords)
			{
				StringBuilder sb = new StringBuilder();
				string[] Rows = Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
				int j, k, d = Rows.Length;
				string Row;

				for (k = 0; k < d; k++)
				{
					Row = Rows[k];
					s = Row.ToLower();

					for (j = i; j < nrSensitiveWords; j++)
					{
						if (s.Contains(sensitiveWords[j]))
							break;
					}

					if (k > 0)
						sb.AppendLine();

					if (j < nrSensitiveWords)
						sb.Append("******** MASKED ********");
					else
						sb.Append(Row);
				}

				Text = sb.ToString();
			}

			this.text = Text;
		}

		/// <summary>
		/// Text
		/// </summary>
		public string Text => this.text;
	}
}
