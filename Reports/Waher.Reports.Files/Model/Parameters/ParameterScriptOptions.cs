using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Runtime.Collections;
using Waher.Script;
using Waher.Script.Abstraction.Elements;

namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Represents a string-valued option.
	/// </summary>
	public class ParameterScriptOptions : IParameterOptions
	{
		private readonly string script;
		private readonly Expression parsed;

		/// <summary>
		/// Represents a string-valued option.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public ParameterScriptOptions(XmlElement Xml)
			: base()
		{
			this.script = Xml.InnerText;
			this.parsed = new Expression(this.script);
		}

		/// <summary>
		/// Gets the options for the parameter.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		/// <returns>Array of options.</returns>
		public async Task<KeyValuePair<string, string>[]> GetOptions(Variables Variables)
		{
			ChunkedList<KeyValuePair<string, string>> Result = new ChunkedList<KeyValuePair<string, string>>();
			object Obj = await this.parsed.EvaluateAsync(Variables);
			string s;

			if (Obj is IEnumerable<KeyValuePair<string, object>> Options)
			{
				foreach (KeyValuePair<string, object> P in Options)
					Result.Add(new KeyValuePair<string, string>(P.Key, P.Value?.ToString() ?? string.Empty));
			}
			else if (Obj is IEnumerable<KeyValuePair<string, IElement>> Options2)
			{
				foreach (KeyValuePair<string, IElement> P in Options2)
					Result.Add(new KeyValuePair<string, string>(P.Key, P.Value.AssociatedObjectValue?.ToString() ?? string.Empty));
			}
			else if (Obj is IEnumerable Enumerable)
			{
				IEnumerator e = Enumerable.GetEnumerator();

				while (e.MoveNext())
				{
					object Item = e.Current;
					s = Item?.ToString() ?? string.Empty;
					Result.Add(new KeyValuePair<string, string>(s, s));
				}
			}
			else
			{
				s = Obj?.ToString() ?? string.Empty;
				Result.Add(new KeyValuePair<string, string>(s, s));
			}

			return Result.ToArray();
		}

	}
}