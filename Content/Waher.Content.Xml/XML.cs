using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Waher.Events;

namespace Waher.Content.Xml
{
    /// <summary>
    /// Helps with common XML-related tasks.
    /// </summary>
    public static class XML
    {
        #region Encoding/Decoding

        /// <summary>
        /// Encodes a string for use in XML.
        /// </summary>
        /// <param name="s">String</param>
        /// <returns>XML-encoded string.</returns>
        public static string Encode(string s)
        {
            if (s == null || s.IndexOfAny(specialCharacters) < 0)
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
            if (s == null || s.IndexOfAny(specialAttributeCharacters) < 0)
                return s;

            return s.
                Replace("&", "&amp;").
                Replace("<", "&lt;").
                Replace(">", "&gt;").
                Replace("\"", "&quot;");
        }

        /// <summary>
        /// Differs from <see cref="Encode(String)"/>, in that it does not encode the aposotrophe or the quote.
        /// </summary>
        /// <param name="s">String to encode.</param>
        /// <returns>Encoded string</returns>
        public static string HtmlValueEncode(string s)
        {
            if (s == null || s.IndexOfAny(specialValueCharacters) < 0)
                return s;

            return s.
                Replace("&", "&amp;").
                Replace("<", "&lt;").
                Replace(">", "&gt;");
        }

        private static readonly char[] specialAttributeCharacters = new char[] { '<', '>', '&', '"' };
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

                if (i == 10)
                {
                    TimeSpan Offset;
                    char ch;

                    s = s.Substring(11);
                    i = s.IndexOfAny(plusMinusZ);

                    if (i < 0)
                    {
                        Value = new DateTime(Year, Month, Day);
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
                Settings.NewLineHandling = NewLineHandling.Entitize;
            }
            else
                Settings.Indent = false;

            Settings.NewLineOnAttributes = false;
            Settings.OmitXmlDeclaration = OmitXmlDeclaration;

            return Settings;
        }

        #endregion

    }
}
