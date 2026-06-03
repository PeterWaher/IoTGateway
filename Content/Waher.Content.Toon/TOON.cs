using System;
using System.Collections.Generic;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Toon
{
	/// <summary>
	/// Helps with common TOON-related tasks.
	/// </summary>
	public static class TOON
	{
		private static readonly Dictionary<Type, IToonEncoder> encoders;

		static TOON()
		{
			encoders = new Dictionary<Type, IToonEncoder>();
			Types.OnInvalidated += Types_OnInvalidated;
		}

		private static void Types_OnInvalidated(object Sender, EventArgs e)
		{
			lock (encoders)
			{
				encoders.Clear();
			}
		}

		#region Encoding

		/// <summary>
		/// Encodes a string for inclusion in TOON.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <returns>Encoded string.</returns>
		public static string Encode(string s)
		{
			switch (s)
			{
				case null:
				case "":
					return "\"\"";

				case "true":
					return "\"true\"";

				case "false":
					return "\"false\"";

				case "null":
					return "\"null\"";

				default:
					if (CommonTypes.TryParse(s, out double _))
						return "\"" + s + "\"";

					if (JSON.ContainsEscapeCharacters(s))
						return "\"" + JSON.Encode(s) + "\"";

					return CommonTypes.Escape(s, toonCharactersToEscape, toonCharacterEscapes);
			}
		}

		private static readonly char[] toonCharactersToEscape = new char[]
		{
			'\\',
			'"',
			'\x00',
			'\x01',
			'\x02',
			'\x03',
			'\x04',
			'\x05',
			'\x06',
			'\a',	// 7  - 0x07
			'\b',	// 8  - 0x08
			'\t',	// 9  - 0x09
			'\n',	// 10 - 0x0a
			'\v',	// 11 - 0x0b
			'\f',	// 12 - 0x0c
			'\r',	// 13 - 0x0d
			'\x0e',
			'\x0f',
			'\x10',
			'\x11',
			'\x12',
			'\x13',
			'\x14',
			'\x15',
			'\x16',
			'\x17',
			'\x18',
			'\x19',
			'\x1a',
			'\x1b',
			'\x1c',
			'\x1d',
			'\x1e',
			'\x1f'
		};
		private static readonly string[] toonCharacterEscapes = new string[]
		{
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" ",
			" "
		};

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <returns>TOON string.</returns>
		public static string Encode(object Object, bool Indent)
		{
			StringBuilder sb = new StringBuilder();
			Encode(Object, Indent, sb);
			return sb.ToString();
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Toon">TOON Output.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		public static void Encode(object Object, bool Indent, StringBuilder Toon)
		{
			Encode(Object, Indent ? (int?)0 : null, Toon);
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <param name="AdditionalProperties">Optional additional properties.</param>
		/// <returns>Encoded object.</returns>
		public static string Encode(IEnumerable<KeyValuePair<string, object>> Object, int? Indent,
			params KeyValuePair<string, object>[] AdditionalProperties)
		{
			StringBuilder Toon = new StringBuilder();
			Encode(Object, Indent, Toon, AdditionalProperties);
			return Toon.ToString();
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <param name="Toon">TOON Output.</param>
		/// <param name="AdditionalProperties">Optional additional properties.</param>
		public static void Encode(IEnumerable<KeyValuePair<string, object>> Object, int? Indent, StringBuilder Toon,
			params KeyValuePair<string, object>[] AdditionalProperties)
		{
			bool First = true;

			if (Indent.HasValue)
				Indent++;

			if (!(Object is null))
			{
				foreach (KeyValuePair<string, object> Member in Object)
				{
					if (First)
						First = false;
					else
						Toon.Append(',');

					if (Indent.HasValue)
					{
						Toon.Append('\n');
						Toon.Append(new string('\t', Indent.Value));
					}

					Toon.Append(Encode(Member.Key));
					Toon.Append(": ");

					Encode(Member.Value, Indent, Toon);
				}
			}

			if (!(AdditionalProperties is null))
			{
				foreach (KeyValuePair<string, object> Member in AdditionalProperties)
				{
					if (First)
						First = false;
					else
						Toon.Append(',');

					if (Indent.HasValue)
					{
						Toon.Append('\n');
						Toon.Append(new string('\t', Indent.Value));
					}

					Toon.Append(Encode(Member.Key));
					Toon.Append(": ");

					Encode(Member.Value, Indent, Toon);
				}
			}

			if (Indent.HasValue)
			{
				Indent--;

				if (!First)
				{
					Toon.Append('\n');
					Toon.Append(new string('\t', Indent.Value));
				}
			}
		}

		/// <summary>
		/// Encodes an object as TOON.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If TOON should be indented.</param>
		/// <param name="Toon">TOON Output.</param>
		public static void Encode(IEnumerable<KeyValuePair<string, IElement>> Object, int? Indent, StringBuilder Toon)
		{
			bool First = true;

			Toon.Append('{');

			if (Indent.HasValue)
				Indent++;

			if (!(Object is null))
			{
				foreach (KeyValuePair<string, IElement> Member in Object)
				{
					if (First)
						First = false;
					else
						Toon.Append(',');

					if (Indent.HasValue)
					{
						Toon.Append('\n');
						Toon.Append(new string('\t', Indent.Value));
					}

					Toon.Append('"');
					Toon.Append(Encode(Member.Key));
					Toon.Append("\": ");

					if (Indent.HasValue)
						Toon.Append(' ');

					Encode(Member.Value.AssociatedObjectValue, Indent, Toon);
				}
			}

			if (Indent.HasValue)
			{
				Indent--;

				if (!First)
				{
					Toon.Append('\n');
					Toon.Append(new string('\t', Indent.Value));
				}
			}

			Toon.Append('}');
		}

		/// <summary>
		/// Extensible encoding of object <paramref name="Object"/>, by using
		/// best available <see cref="IToonEncoder"/> for the corresponding type.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Optional indentation.</param>
		/// <param name="Toon">TOON output.</param>
		public static void Encode(object Object, int? Indent, StringBuilder Toon)
		{
			if (Object is null)
				Toon.Append("null");
			else
			{
				Type T = Object.GetType();
				IToonEncoder Encoder;

				lock (encoders)
				{
					if (!encoders.TryGetValue(T, out Encoder))
					{
						Encoder = Types.FindBest<IToonEncoder, Type>(T)
							?? throw new ArgumentException("Unable to encode objects of type " + T.FullName, nameof(Object));

						encoders[T] = Encoder;
					}
				}

				Encoder.Encode(Object, Indent, Toon);
			}
		}

		#endregion
	}
}
