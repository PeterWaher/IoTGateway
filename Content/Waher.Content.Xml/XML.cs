using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects.Matrices;

namespace Waher.Content.Xml
{
	/// <summary>
	/// Helps with common XML-related tasks.
	/// </summary>
	public static class XML
	{
		#region Encoding

		/// <summary>
		/// Encodes a string for use in XML.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>XML-encoded string.</returns>
		public static string Encode(string s)
		{
			if (s is null || s.IndexOfAny(specialCharacters) < 0)
				return s;

			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;").
				Replace("\"", "&quot;").
				Replace("'", "&apos;");
		}

		/// <summary>
		/// Differs from <see cref="Encode(String)"/>, in that it does not encode the aposotrophe.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <returns>Encoded string</returns>
		public static string HtmlAttributeEncode(string s)
		{
			if (s is null || s.IndexOfAny(specialAttributeCharacters) < 0)
				return s;

			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;").
				Replace("\"", "&quot;").
				Replace("\t", "&#09;").
				Replace("\n", "&#10;").
				Replace("\r", "&#13;");
		}

		/// <summary>
		/// Differs from <see cref="Encode(String)"/>, in that it does not encode the aposotrophe or the quote.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <returns>Encoded string</returns>
		public static string HtmlValueEncode(string s)
		{
			if (s is null || s.IndexOfAny(specialValueCharacters) < 0)
				return s;

			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;");
		}

		private static readonly char[] specialAttributeCharacters = new char[] { '<', '>', '&', '"', '\t', '\r', '\n' };
		private static readonly char[] specialValueCharacters = new char[] { '<', '>', '&' };
		private static readonly char[] specialCharacters = new char[] { '<', '>', '&', '"', '\'' };

		/// <summary>
		/// Encodes a <see cref="DateTime"/> for use in XML.
		/// </summary>
		/// <param name="DT">Value to encode.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(DateTime DT)
		{
			return Encode(DT, false);
		}

		/// <summary>
		/// Encodes a <see cref="DateTime"/> for use in XML.
		/// </summary>
		/// <param name="DT">Value to encode.</param>
		/// <param name="DateOnly">If only the date should be encoded (true), or both date and time (false).</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(DateTime DT, bool DateOnly)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(DT.Year.ToString("D4"));
			sb.Append('-');
			sb.Append(DT.Month.ToString("D2"));
			sb.Append('-');
			sb.Append(DT.Day.ToString("D2"));

			if (!DateOnly)
			{
				sb.Append('T');
				sb.Append(DT.Hour.ToString("D2"));
				sb.Append(':');
				sb.Append(DT.Minute.ToString("D2"));
				sb.Append(':');
				sb.Append(DT.Second.ToString("D2"));
				sb.Append('.');
				sb.Append(DT.Millisecond.ToString("D3"));

				if (DT.Kind == DateTimeKind.Utc)
					sb.Append("Z");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Encodes a <see cref="DateTimeOffset"/> for use in XML.
		/// </summary>
		/// <param name="DTO">Value to encode.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(DateTimeOffset DTO)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(DTO.Year.ToString("D4"));
			sb.Append('-');
			sb.Append(DTO.Month.ToString("D2"));
			sb.Append('-');
			sb.Append(DTO.Day.ToString("D2"));
			sb.Append('T');
			sb.Append(DTO.Hour.ToString("D2"));
			sb.Append(':');
			sb.Append(DTO.Minute.ToString("D2"));
			sb.Append(':');
			sb.Append(DTO.Second.ToString("D2"));
			sb.Append('.');
			sb.Append(DTO.Millisecond.ToString("D3"));

			TimeSpan TS = DTO.Offset;
			if (TS < TimeSpan.Zero)
			{
				sb.Append('-');
				TS = -TS;
			}
			else
				sb.Append('+');

			sb.Append(TS.Hours.ToString("D2"));
			sb.Append(':');
			sb.Append(TS.Minutes.ToString("D2"));

			return sb.ToString();
		}

		/// <summary>
		/// Encodes a named dictionary as XML.
		/// </summary>
		/// <param name="Object">Object.</param>
		public static string Encode(NamedDictionary<string, object> Object)
		{
			return Encode(Object, null);
		}

		/// <summary>
		/// Encodes a named dictionary as XML.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If XML should be indented.</param>
		public static string Encode(NamedDictionary<string, object> Object, int? Indent)
		{
			StringBuilder sb = new StringBuilder();
			Encode(Object, Indent, sb);
			return sb.ToString();
		}

		/// <summary>
		/// Encodes a named dictionary as XML.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Indent">If XML should be indented.</param>
		/// <param name="Xml">XML Output.</param>
		public static void Encode(NamedDictionary<string, object> Object, int? Indent, StringBuilder Xml)
		{
			Encode(Object, Object.LocalName, Object.Namespace, Indent, Xml);
		}

		/// <summary>
		/// Encodes a named dictionary as XML.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="LocalName">Local Name of XML Element to encode the object.</param>
		/// <param name="Namespace">Namespace of XML Element to encode the object.</param>
		/// <param name="Indent">If XML should be indented.</param>
		/// <param name="Xml">XML Output.</param>
		private static void Encode(IEnumerable<KeyValuePair<string, object>> Object,
			string LocalName, string Namespace, int? Indent, StringBuilder Xml)
		{
			Xml.Append('<');
			Xml.Append(LocalName);

			if (!string.IsNullOrEmpty(Namespace))
			{
				Xml.Append(" xmlns='");
				Xml.Append(Namespace);
				Xml.Append("'");
			}

			bool HasChildren = false;

			foreach (KeyValuePair<string, object> Member in Object)
			{
				if (!EncodeAttribute(Member.Key, Member.Value, Xml))
					HasChildren = true;
			}

			if (HasChildren)
			{
				Xml.Append('>');

				if (Indent.HasValue)
					Indent++;

				foreach (KeyValuePair<string, object> Member in Object)
					EncodeChildElement(Member.Key, Member.Value, Indent, Xml);

				if (Indent.HasValue)
				{
					Indent--;
					Xml.AppendLine();
					Xml.Append(new string('\t', Indent.Value));
				}

				Xml.Append("</");
				Xml.Append(LocalName);
				Xml.Append('>');
			}
			else
				Xml.Append("/>");
		}

		/// <summary>
		/// Encodes a property as an XML attribute.
		/// </summary>
		/// <param name="Key">Parameter key.</param>
		/// <param name="Value">Parameter value.</param>
		/// <param name="Xml">XML Output.</param>
		/// <returns>If value was encoded.</returns>
		private static bool EncodeAttribute(string Key, object Value, StringBuilder Xml)
		{
			Type T = Value.GetType();
			TypeInfo TI = T.GetTypeInfo();
			string ValueString;

			if (TI.IsValueType)
			{
				if (Value is bool b)
					ValueString = CommonTypes.Encode(b);
				else if (Value is char ch)
					ValueString = Encode(new string(ch, 1));
				else if (Value is double dbl)
					ValueString = CommonTypes.Encode(dbl);
				else if (Value is float fl)
					ValueString = CommonTypes.Encode(fl);
				else if (Value is decimal dec)
					ValueString = CommonTypes.Encode(dec);
				else if (TI.IsEnum)
					ValueString = Encode(Value.ToString());
				else if (Value is DateTime TP)
					ValueString = XML.Encode(TP);
				else if (Value is DateTimeOffset TPO)
					ValueString = XML.Encode(TPO);
				else if (Value is int || Value is long || Value is short || Value is byte ||
					Value is uint || Value is ulong || Value is ushort || Value is sbyte)
				{
					ValueString = Value.ToString();
				}
				else
					ValueString = Encode(Value.ToString());
			}
			else if (Value is string s)
				ValueString = XML.Encode(s);
			else
				return false;

			Xml.Append(' ');
			Xml.Append(Key);
			Xml.Append("='");
			Xml.Append(ValueString);
			Xml.Append('\'');

			return true;
		}

		/// <summary>
		/// Encodes a property as an XML attribute.
		/// </summary>
		/// <param name="Key">Parameter key.</param>
		/// <param name="Value">Parameter value.</param>
		/// <param name="Indent">If XML should be indented.</param>
		/// <param name="Xml">XML Output.</param>
		/// <returns>If value was encoded.</returns>
		private static bool EncodeChildElement(string Key, object Value,
			int? Indent, StringBuilder Xml)
		{
			Type T = Value.GetType();
			TypeInfo TI = T.GetTypeInfo();

			if (TI.IsValueType || Value is string)
				return false;

			if (Indent.HasValue)
			{
				Xml.AppendLine();
				Xml.Append(new string('\t', Indent.Value));
			}

			if (Value is byte[] ByteArray)
			{
				Xml.Append('<');
				Xml.Append(Key);
				Xml.Append('>');
				Xml.Append(Convert.ToBase64String(ByteArray));
				Xml.Append("</");
				Xml.Append(Key);
				Xml.Append('>');
			}
			else if (Value is IEnumerable<KeyValuePair<string, object>> Obj)
				Encode(Obj, Key, null, Indent, Xml);
			else if (Value is IEnumerable<KeyValuePair<string, IElement>> Obj2)
				Encode(NamedDictionary<string, object>.ToNamedDictionary(Obj2), Key, null, Indent, Xml);
			else if (Value is ObjectMatrix M && !(M.ColumnNames is null))
			{
				string[] Names = M.ColumnNames;
				int Rows = M.Rows;
				int Columns = M.Columns;
				int x, y;

				Xml.Append('<');
				Xml.Append(Key);
				Xml.Append('>');

				if (Indent.HasValue)
					Indent++;

				for (y = 0; y < Rows; y++)
				{
					if (Indent.HasValue)
					{
						Xml.AppendLine();
						Xml.Append(new string('\t', Indent.Value));
						Indent++;
					}

					Xml.Append("<Record>");

					for (x = 0; x < Columns; x++)
					{
						if (Indent.HasValue)
						{
							Xml.AppendLine();
							Xml.Append(new string('\t', Indent.Value));
						}

						EncodeChildElement(Encode(Names[x]), M.GetElement(x, y).AssociatedObjectValue, Indent, Xml);
					}

					if (Indent.HasValue)
					{
						Indent--;
						Xml.AppendLine();
						Xml.Append(new string('\t', Indent.Value));
					}

					Xml.Append("</Record>");
				}

				if (Indent.HasValue)
				{
					Indent--;

					if (Rows > 0 && Columns > 0)
					{
						Xml.AppendLine();
						Xml.Append(new string('\t', Indent.Value));
					}
				}

				Xml.Append("</");
				Xml.Append(Key);
				Xml.Append('>');
			}
			else if (Value is IVector V)
			{
				bool HashItems = false;

				Xml.Append('<');
				Xml.Append(Key);
				Xml.Append('>');

				if (Indent.HasValue)
					Indent++;

				foreach (IElement Element in V.VectorElements)
				{
					HashItems = true;

					if (Indent.HasValue)
					{
						Xml.AppendLine();
						Xml.Append(new string('\t', Indent.Value));
					}

					object Obj3 = Element.AssociatedObjectValue;

					if (Obj3 is NamedDictionary<string, object> Named)
						Encode(Named, Indent, Xml);
					else
						EncodeChildElement("Item", Obj3, Indent, Xml);
				}

				if (HashItems && Indent.HasValue)
				{
					Indent--;
					Xml.AppendLine();
					Xml.Append(new string('\t', Indent.Value));
				}

				Xml.Append("</");
				Xml.Append(Key);
				Xml.Append('>');
			}
			else if (Value is IDictionary Dictionary)
			{
				LinkedList<KeyValuePair<string, object>> Properties = new LinkedList<KeyValuePair<string, object>>();

				foreach (object Key2 in Dictionary.Keys)
					Properties.AddLast(new KeyValuePair<string, object>(Key2.ToString(), Dictionary[Key]));

				Encode(Properties, Key, null, Indent, Xml);
			}
			else if (Value is XmlDocument Doc)
			{
				if (!(Doc.DocumentElement is null))
					Xml.Append(Doc.DocumentElement.OuterXml);
			}
			else if (Value is XmlElement E2)
				Xml.Append(E2.OuterXml);
			else if (Value is IEnumerable E)
			{
				IEnumerator e = E.GetEnumerator();
				bool HashItems = false;

				Xml.Append('<');
				Xml.Append(Key);
				Xml.Append('>');

				if (Indent.HasValue)
					Indent++;

				while (e.MoveNext())
				{
					HashItems = true;

					if (Indent.HasValue)
					{
						Xml.AppendLine();
						Xml.Append(new string('\t', Indent.Value));
					}

					object Obj3 = e.Current;

					if (Obj3 is NamedDictionary<string, object> Named)
						Encode(Named, Indent, Xml);
					else
						EncodeChildElement("Item", Obj3, Indent, Xml);
				}

				if (HashItems && Indent.HasValue)
				{
					Indent--;
					Xml.AppendLine();
					Xml.Append(new string('\t', Indent.Value));
				}

				Xml.Append("</");
				Xml.Append(Key);
				Xml.Append('>');
			}
			else
			{
				LinkedList<KeyValuePair<string, object>> Properties = new LinkedList<KeyValuePair<string, object>>();

				foreach (FieldInfo FI in T.GetRuntimeFields())
				{
					if (FI.IsPublic && !FI.IsStatic)
						Properties.AddLast(new KeyValuePair<string, object>(FI.Name, FI.GetValue(Value)));
				}

				foreach (PropertyInfo PI in T.GetRuntimeProperties())
				{
					if (PI.CanRead && PI.GetMethod.IsPublic && PI.GetIndexParameters().Length == 0)
						Properties.AddLast(new KeyValuePair<string, object>(PI.Name, PI.GetValue(Value, null)));
				}

				Encode(Properties, Key, null, Indent, Xml);
			}

			return true;
		}

		#endregion

		#region Parsing

		/// <summary>
		/// Decodes a string used in XML.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>XML-decoded string.</returns>
		public static string DecodeString(string s)
		{
			if (s.IndexOf('&') < 0)
				return s;

			return s.
				Replace("&apos;", "'").
				Replace("&qout;", "\"").
				Replace("&lt;", "<").
				Replace("&gt;", ">").
				Replace("&amp;", "&");
		}

		/// <summary>
		/// Tries to decode a string encoded DateTime.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParse(string s, out DateTime Value)
		{
			int i = s.IndexOf('T');

			if (i == 10 || s.Length == 10)
			{
				if (!int.TryParse(s.Substring(0, 4), out int Year) ||
					!int.TryParse(s.Substring(5, 2), out int Month) ||
					!int.TryParse(s.Substring(8, 2), out int Day))
				{
					Value = DateTime.MinValue;
					return false;
				}

				if (Year < 1 || Year > 9999 || Month < 1 || Month > 12 || Day < 1 || Day > DateTime.DaysInMonth(Year, Month))
				{
					Value = DateTime.MinValue;
					return false;
				}

				if (i == 10)
				{
					char ch;

					s = s.Substring(11);
					i = s.IndexOfAny(plusMinusZ);

					if (i < 0)
						Value = new DateTime(Year, Month, Day);
					else
					{
						TimeSpan Offset;

						ch = s[i];
						if (ch == 'z' || ch == 'Z')
							Offset = TimeSpan.Zero;
						else if (!TimeSpan.TryParse(s.Substring(i + 1), out Offset))
						{
							Value = new DateTime(Year, Month, Day);
							return false;
						}

						if (ch == '-')
							Offset = -Offset;

						s = s.Substring(0, i);

						Value = new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Utc);
						Value -= Offset;
					}

					if (TimeSpan.TryParse(s, out TimeSpan TS))
						Value += TS;

					return true;
				}
				else
				{
					Value = new DateTime(Year, Month, Day);
					return true;
				}
			}

			Value = DateTime.MinValue;
			return false;
		}

		/// <summary>
		/// Tries to decode a string encoded DateTimeOffset.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParse(string s, out DateTimeOffset Value)
		{
			int i = s.IndexOf('T');

			if (i == 10 || s.Length == 10)
			{
				if (!int.TryParse(s.Substring(0, 4), out int Year) ||
					!int.TryParse(s.Substring(5, 2), out int Month) ||
					!int.TryParse(s.Substring(8, 2), out int Day))
				{
					Value = DateTime.MinValue;
					return false;
				}

				if (Year < 1 || Year > 9999 || Month < 1 || Month > 12 || Day < 1 || Day > DateTime.DaysInMonth(Year, Month))
				{
					Value = DateTimeOffset.MinValue;
					return false;
				}

				if (i == 10)
				{
					DateTime DT;
					TimeSpan Offset;
					char ch;

					s = s.Substring(11);
					i = s.IndexOfAny(plusMinusZ);

					if (i < 0)
					{
						DT = new DateTime(Year, Month, Day);
						Offset = TimeSpan.Zero;
					}
					else
					{
						ch = s[i];
						if (ch == 'z' || ch == 'Z')
							Offset = TimeSpan.Zero;
						else if (!TimeSpan.TryParse(s.Substring(i + 1), out Offset))
						{
							Value = new DateTime(Year, Month, Day);
							return false;
						}

						if (ch == '-')
							Offset = -Offset;

						s = s.Substring(0, i);

						DT = new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Utc);
					}

					if (TimeSpan.TryParse(s, out TimeSpan TS))
						DT += TS;

					Value = new DateTimeOffset(DT, Offset);

					return true;
				}
				else
				{
					Value = new DateTime(Year, Month, Day);
					return true;
				}
			}

			Value = DateTime.MinValue;
			return false;
		}

		private static readonly char[] plusMinusZ = new char[] { '+', '-', 'z', 'Z' };

		#endregion

		#region XML Attributes

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <returns>Value of attribute, if found, or the empty string, if not found.</returns>
		public static string Attribute(XmlElement E, string Name)
		{
			if (E.HasAttribute(Name))
				return E.GetAttribute(Name);
			else
				return string.Empty;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static string Attribute(XmlElement E, string Name, string DefaultValue)
		{
			if (E.HasAttribute(Name))
				return E.GetAttribute(Name);
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static bool Attribute(XmlElement E, string Name, bool DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				if (CommonTypes.TryParse(E.GetAttribute(Name), out bool Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static int Attribute(XmlElement E, string Name, int DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				if (int.TryParse(E.GetAttribute(Name), out int Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static long Attribute(XmlElement E, string Name, long DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				if (long.TryParse(E.GetAttribute(Name), out long Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static double Attribute(XmlElement E, string Name, double DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				if (CommonTypes.TryParse(E.GetAttribute(Name), out double Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static decimal Attribute(XmlElement E, string Name, decimal DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				if (CommonTypes.TryParse(E.GetAttribute(Name), out decimal Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static DateTime Attribute(XmlElement E, string Name, DateTime DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				if (TryParse(E.GetAttribute(Name), out DateTime Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static DateTimeOffset Attribute(XmlElement E, string Name, DateTimeOffset DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				if (TryParse(E.GetAttribute(Name), out DateTimeOffset Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		[Obsolete("Use generic overload of this method: Attribute<T>")]
		public static Enum Attribute(XmlElement E, string Name, Enum DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				try
				{
					return (Enum)Enum.Parse(DefaultValue.GetType(), E.GetAttribute(Name));
				}
				catch (Exception)
				{
					return DefaultValue;
				}
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static T Attribute<T>(XmlElement E, string Name, T DefaultValue)
			where T : Enum
		{
			if (E.HasAttribute(Name))
			{
				try
				{
					return (T)Enum.Parse(DefaultValue.GetType(), E.GetAttribute(Name));
				}
				catch (Exception)
				{
					return DefaultValue;
				}
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static Duration Attribute(XmlElement E, string Name, Duration DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				if (Duration.TryParse(E.GetAttribute(Name), out Duration Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static TimeSpan Attribute(XmlElement E, string Name, TimeSpan DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				if (TimeSpan.TryParse(E.GetAttribute(Name), out TimeSpan Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		#endregion

		#region Settings

		/// <summary>
		/// Gets an XML writer settings object.
		/// </summary>
		/// <param name="Indent">If output should be indented.</param>
		/// <param name="OmitXmlDeclaration">If the XML declaration should be omitted.</param>
		/// <returns>Settings object.</returns>
		public static XmlWriterSettings WriterSettings(bool Indent, bool OmitXmlDeclaration)
		{
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				CloseOutput = false,
				ConformanceLevel = ConformanceLevel.Document,
				Encoding = System.Text.Encoding.UTF8
			};

			if (Indent)
			{
				Settings.Indent = true;
				Settings.IndentChars = "\t";
				Settings.NewLineChars = "\r\n";
				Settings.NewLineHandling = NewLineHandling.Replace;
			}
			else
				Settings.Indent = false;

			Settings.NewLineOnAttributes = false;
			Settings.OmitXmlDeclaration = OmitXmlDeclaration;

			return Settings;
		}

		#endregion

		#region Validation

		/// <summary>
		/// Checks if a string is valid XML
		/// </summary>
		/// <param name="Xml">String with possible XML.</param>
		/// <returns>If <paramref name="Xml"/> is valid XML.</returns>
		public static bool IsValidXml(string Xml)
		{
			return IsValidXml(Xml, true, true, false, false, false, false);
		}

		/// <summary>
		/// Checks if a string is valid XML
		/// </summary>
		/// <param name="Xml">String with possible XML.</param>
		/// <param name="Comments">If comments are allowed. (Default=true)</param>
		/// <returns>If <paramref name="Xml"/> is valid XML.</returns>
		public static bool IsValidXml(string Xml, bool Comments)
		{
			return IsValidXml(Xml, Comments, true, false, false, false, false);
		}

		/// <summary>
		/// Checks if a string is valid XML
		/// </summary>
		/// <param name="Xml">String with possible XML.</param>
		/// <param name="Comments">If comments are allowed. (Default=true)</param>
		/// <param name="CDATA">If CDATA sections are allowed. (Default=true)</param>
		/// <returns>If <paramref name="Xml"/> is valid XML.</returns>
		public static bool IsValidXml(string Xml, bool Comments, bool CDATA)
		{
			return IsValidXml(Xml, Comments, CDATA, false, false, false, false);
		}

		/// <summary>
		/// Checks if a string is valid XML
		/// </summary>
		/// <param name="Xml">String with possible XML.</param>
		/// <param name="Comments">If comments are allowed. (Default=true)</param>
		/// <param name="CDATA">If CDATA sections are allowed. (Default=true)</param>
		/// <param name="Empty">If empty XML (or an empty string) is acceptable. (Default=false)</param>
		/// <returns>If <paramref name="Xml"/> is valid XML.</returns>
		public static bool IsValidXml(string Xml, bool Comments, bool CDATA, bool Empty)
		{
			return IsValidXml(Xml, Comments, CDATA, Empty, false, false, false);
		}

		/// <summary>
		/// Checks if a string is valid XML
		/// </summary>
		/// <param name="Xml">String with possible XML.</param>
		/// <param name="Comments">If comments are allowed. (Default=true)</param>
		/// <param name="CDATA">If CDATA sections are allowed. (Default=true)</param>
		/// <param name="Empty">If empty XML (or an empty string) is acceptable. (Default=false)</param>
		/// <param name="Fragment">If XML fragments are allowed. (Default=false)</param>
		/// <returns>If <paramref name="Xml"/> is valid XML.</returns>
		public static bool IsValidXml(string Xml, bool Comments, bool CDATA, bool Empty, bool Fragment)
		{
			return IsValidXml(Xml, Comments, CDATA, Empty, Fragment, false, false);
		}

		/// <summary>
		/// Checks if a string is valid XML
		/// </summary>
		/// <param name="Xml">String with possible XML.</param>
		/// <param name="Comments">If comments are allowed. (Default=true)</param>
		/// <param name="CDATA">If CDATA sections are allowed. (Default=true)</param>
		/// <param name="Empty">If empty XML (or an empty string) is acceptable. (Default=false)</param>
		/// <param name="Fragment">If XML fragments are allowed. (Default=false)</param>
		/// <param name="ProcessingInstructions">If processing instructions are allowed. (Default=false)</param>
		/// <returns>If <paramref name="Xml"/> is valid XML.</returns>
		public static bool IsValidXml(string Xml, bool Comments, bool CDATA, bool Empty, bool Fragment, bool ProcessingInstructions)
		{
			return IsValidXml(Xml, Comments, CDATA, Empty, Fragment, ProcessingInstructions, false);
		}

		/// <summary>
		/// Checks if a string is valid XML
		/// </summary>
		/// <param name="Xml">String with possible XML.</param>
		/// <param name="Comments">If comments are allowed. (Default=true)</param>
		/// <param name="CDATA">If CDATA sections are allowed. (Default=true)</param>
		/// <param name="Empty">If empty XML (or an empty string) is acceptable. (Default=false)</param>
		/// <param name="Fragment">If XML fragments are allowed. (Default=false)</param>
		/// <param name="ProcessingInstructions">If processing instructions are allowed. (Default=false)</param>
		/// <param name="DTD">If DTD processing is allowed. (Default=false)</param>
		/// <returns>If <paramref name="Xml"/> is valid XML.</returns>
		public static bool IsValidXml(string Xml, bool Comments, bool CDATA, bool Empty, bool Fragment, bool ProcessingInstructions, bool DTD)
		{
			try
			{
				if (string.IsNullOrEmpty(Xml.Trim()))
					return Empty;

				if (Fragment)
					Xml = "<Root>" + Xml + "</Root>";

				XmlDocument Doc = new XmlDocument();
				using (StringReader r = new StringReader(Xml))
				{
					XmlReaderSettings Settings = new XmlReaderSettings()
					{
						CheckCharacters = true,
						ConformanceLevel = ConformanceLevel.Document,
						DtdProcessing = DTD ? DtdProcessing.Ignore : DtdProcessing.Prohibit,
						IgnoreComments = false,
						IgnoreProcessingInstructions = false,
						IgnoreWhitespace = true
					};

					using (XmlReader xr = XmlReader.Create(r, Settings))
					{
						Doc.Load(xr);

						if (Doc.DocumentElement is null)
							return Empty;

						if (!Comments || !ProcessingInstructions)
						{
							LinkedList<XmlNode> Nodes = new LinkedList<XmlNode>();
							Nodes.AddLast(Doc.DocumentElement);

							while (!(Nodes.First is null))
							{
								XmlNode N = Nodes.First.Value;
								Nodes.RemoveFirst();

								if (N is XmlComment Comment)
								{
									if (!Comments)
										return false;
								}
								else if (N is XmlProcessingInstruction PI)
								{
									if (!ProcessingInstructions)
										return false;
								}
								else if (N is XmlCDataSection)
								{
									if (!CDATA)
										return false;
								}
								else if (N is XmlElement E)
								{
									foreach (XmlNode N2 in E.ChildNodes)
										Nodes.AddLast(N2);
								}
							}
						}
					}
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		#endregion

	}
}
