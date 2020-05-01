using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Test.Encodings
{
	public class Windows1252 : Encoding
	{
		/// <summary>
		/// Source: http://www.unicode.org/Public/MAPPINGS/VENDORS/MICSFT/WINDOWS/CP1252.TXT
		/// </summary>
		private static readonly char?[] codeToUnicode = new char?[]
		{
			/* 0x00 */ (char)0x0000, // NULL
			/* 0x01 */ (char)0x0001, // START OF HEADING
			/* 0x02 */ (char)0x0002, // START OF TEXT
			/* 0x03 */ (char)0x0003, // END OF TEXT
			/* 0x04 */ (char)0x0004, // END OF TRANSMISSION
			/* 0x05 */ (char)0x0005, // ENQUIRY
			/* 0x06 */ (char)0x0006, // ACKNOWLEDGE
			/* 0x07 */ (char)0x0007, // BELL
			/* 0x08 */ (char)0x0008, // BACKSPACE
			/* 0x09 */ (char)0x0009, // HORIZONTAL TABULATION
			/* 0x0A */ (char)0x000A, // LINE FEED
			/* 0x0B */ (char)0x000B, // VERTICAL TABULATION
			/* 0x0C */ (char)0x000C, // FORM FEED
			/* 0x0D */ (char)0x000D, // CARRIAGE RETURN
			/* 0x0E */ (char)0x000E, // SHIFT OUT
			/* 0x0F */ (char)0x000F, // SHIFT IN
			/* 0x10 */ (char)0x0010, // DATA LINK ESCAPE
			/* 0x11 */ (char)0x0011, // DEVICE CONTROL ONE
			/* 0x12 */ (char)0x0012, // DEVICE CONTROL TWO
			/* 0x13 */ (char)0x0013, // DEVICE CONTROL THREE
			/* 0x14 */ (char)0x0014, // DEVICE CONTROL FOUR
			/* 0x15 */ (char)0x0015, // NEGATIVE ACKNOWLEDGE
			/* 0x16 */ (char)0x0016, // SYNCHRONOUS IDLE
			/* 0x17 */ (char)0x0017, // END OF TRANSMISSION BLOCK
			/* 0x18 */ (char)0x0018, // CANCEL
			/* 0x19 */ (char)0x0019, // END OF MEDIUM
			/* 0x1A */ (char)0x001A, // SUBSTITUTE
			/* 0x1B */ (char)0x001B, // ESCAPE
			/* 0x1C */ (char)0x001C, // FILE SEPARATOR
			/* 0x1D */ (char)0x001D, // GROUP SEPARATOR
			/* 0x1E */ (char)0x001E, // RECORD SEPARATOR
			/* 0x1F */ (char)0x001F, // UNIT SEPARATOR
			/* 0x20 */ (char)0x0020, // SPACE
			/* 0x21 */ (char)0x0021, // EXCLAMATION MARK
			/* 0x22 */ (char)0x0022, // QUOTATION MARK
			/* 0x23 */ (char)0x0023, // NUMBER SIGN
			/* 0x24 */ (char)0x0024, // DOLLAR SIGN
			/* 0x25 */ (char)0x0025, // PERCENT SIGN
			/* 0x26 */ (char)0x0026, // AMPERSAND
			/* 0x27 */ (char)0x0027, // APOSTROPHE
			/* 0x28 */ (char)0x0028, // LEFT PARENTHESIS
			/* 0x29 */ (char)0x0029, // RIGHT PARENTHESIS
			/* 0x2A */ (char)0x002A, // ASTERISK
			/* 0x2B */ (char)0x002B, // PLUS SIGN
			/* 0x2C */ (char)0x002C, // COMMA
			/* 0x2D */ (char)0x002D, // HYPHEN-MINUS
			/* 0x2E */ (char)0x002E, // FULL STOP
			/* 0x2F */ (char)0x002F, // SOLIDUS
			/* 0x30 */ (char)0x0030, // DIGIT ZERO
			/* 0x31 */ (char)0x0031, // DIGIT ONE
			/* 0x32 */ (char)0x0032, // DIGIT TWO
			/* 0x33 */ (char)0x0033, // DIGIT THREE
			/* 0x34 */ (char)0x0034, // DIGIT FOUR
			/* 0x35 */ (char)0x0035, // DIGIT FIVE
			/* 0x36 */ (char)0x0036, // DIGIT SIX
			/* 0x37 */ (char)0x0037, // DIGIT SEVEN
			/* 0x38 */ (char)0x0038, // DIGIT EIGHT
			/* 0x39 */ (char)0x0039, // DIGIT NINE
			/* 0x3A */ (char)0x003A, // COLON
			/* 0x3B */ (char)0x003B, // SEMICOLON
			/* 0x3C */ (char)0x003C, // LESS-THAN SIGN
			/* 0x3D */ (char)0x003D, // EQUALS SIGN
			/* 0x3E */ (char)0x003E, // GREATER-THAN SIGN
			/* 0x3F */ (char)0x003F, // QUESTION MARK
			/* 0x40 */ (char)0x0040, // COMMERCIAL AT
			/* 0x41 */ (char)0x0041, // LATIN CAPITAL LETTER A
			/* 0x42 */ (char)0x0042, // LATIN CAPITAL LETTER B
			/* 0x43 */ (char)0x0043, // LATIN CAPITAL LETTER C
			/* 0x44 */ (char)0x0044, // LATIN CAPITAL LETTER D
			/* 0x45 */ (char)0x0045, // LATIN CAPITAL LETTER E
			/* 0x46 */ (char)0x0046, // LATIN CAPITAL LETTER F
			/* 0x47 */ (char)0x0047, // LATIN CAPITAL LETTER G
			/* 0x48 */ (char)0x0048, // LATIN CAPITAL LETTER H
			/* 0x49 */ (char)0x0049, // LATIN CAPITAL LETTER I
			/* 0x4A */ (char)0x004A, // LATIN CAPITAL LETTER J
			/* 0x4B */ (char)0x004B, // LATIN CAPITAL LETTER K
			/* 0x4C */ (char)0x004C, // LATIN CAPITAL LETTER L
			/* 0x4D */ (char)0x004D, // LATIN CAPITAL LETTER M
			/* 0x4E */ (char)0x004E, // LATIN CAPITAL LETTER N
			/* 0x4F */ (char)0x004F, // LATIN CAPITAL LETTER O
			/* 0x50 */ (char)0x0050, // LATIN CAPITAL LETTER P
			/* 0x51 */ (char)0x0051, // LATIN CAPITAL LETTER Q
			/* 0x52 */ (char)0x0052, // LATIN CAPITAL LETTER R
			/* 0x53 */ (char)0x0053, // LATIN CAPITAL LETTER S
			/* 0x54 */ (char)0x0054, // LATIN CAPITAL LETTER T
			/* 0x55 */ (char)0x0055, // LATIN CAPITAL LETTER U
			/* 0x56 */ (char)0x0056, // LATIN CAPITAL LETTER V
			/* 0x57 */ (char)0x0057, // LATIN CAPITAL LETTER W
			/* 0x58 */ (char)0x0058, // LATIN CAPITAL LETTER X
			/* 0x59 */ (char)0x0059, // LATIN CAPITAL LETTER Y
			/* 0x5A */ (char)0x005A, // LATIN CAPITAL LETTER Z
			/* 0x5B */ (char)0x005B, // LEFT SQUARE BRACKET
			/* 0x5C */ (char)0x005C, // REVERSE SOLIDUS
			/* 0x5D */ (char)0x005D, // RIGHT SQUARE BRACKET
			/* 0x5E */ (char)0x005E, // CIRCUMFLEX ACCENT
			/* 0x5F */ (char)0x005F, // LOW LINE
			/* 0x60 */ (char)0x0060, // GRAVE ACCENT
			/* 0x61 */ (char)0x0061, // LATIN SMALL LETTER A
			/* 0x62 */ (char)0x0062, // LATIN SMALL LETTER B
			/* 0x63 */ (char)0x0063, // LATIN SMALL LETTER C
			/* 0x64 */ (char)0x0064, // LATIN SMALL LETTER D
			/* 0x65 */ (char)0x0065, // LATIN SMALL LETTER E
			/* 0x66 */ (char)0x0066, // LATIN SMALL LETTER F
			/* 0x67 */ (char)0x0067, // LATIN SMALL LETTER G
			/* 0x68 */ (char)0x0068, // LATIN SMALL LETTER H
			/* 0x69 */ (char)0x0069, // LATIN SMALL LETTER I
			/* 0x6A */ (char)0x006A, // LATIN SMALL LETTER J
			/* 0x6B */ (char)0x006B, // LATIN SMALL LETTER K
			/* 0x6C */ (char)0x006C, // LATIN SMALL LETTER L
			/* 0x6D */ (char)0x006D, // LATIN SMALL LETTER M
			/* 0x6E */ (char)0x006E, // LATIN SMALL LETTER N
			/* 0x6F */ (char)0x006F, // LATIN SMALL LETTER O
			/* 0x70 */ (char)0x0070, // LATIN SMALL LETTER P
			/* 0x71 */ (char)0x0071, // LATIN SMALL LETTER Q
			/* 0x72 */ (char)0x0072, // LATIN SMALL LETTER R
			/* 0x73 */ (char)0x0073, // LATIN SMALL LETTER S
			/* 0x74 */ (char)0x0074, // LATIN SMALL LETTER T
			/* 0x75 */ (char)0x0075, // LATIN SMALL LETTER U
			/* 0x76 */ (char)0x0076, // LATIN SMALL LETTER V
			/* 0x77 */ (char)0x0077, // LATIN SMALL LETTER W
			/* 0x78 */ (char)0x0078, // LATIN SMALL LETTER X
			/* 0x79 */ (char)0x0079, // LATIN SMALL LETTER Y
			/* 0x7A */ (char)0x007A, // LATIN SMALL LETTER Z
			/* 0x7B */ (char)0x007B, // LEFT CURLY BRACKET
			/* 0x7C */ (char)0x007C, // VERTICAL LINE
			/* 0x7D */ (char)0x007D, // RIGHT CURLY BRACKET
			/* 0x7E */ (char)0x007E, // TILDE
			/* 0x7F */ (char)0x007F, // DELETE
			/* 0x80 */ (char)0x20AC, // EURO SIGN
			/* 0x81 */ null        , // UNDEFINED
			/* 0x82 */ (char)0x201A, // SINGLE LOW-9 QUOTATION MARK
			/* 0x83 */ (char)0x0192, // LATIN SMALL LETTER F WITH HOOK
			/* 0x84 */ (char)0x201E, // DOUBLE LOW-9 QUOTATION MARK
			/* 0x85 */ (char)0x2026, // HORIZONTAL ELLIPSIS
			/* 0x86 */ (char)0x2020, // DAGGER
			/* 0x87 */ (char)0x2021, // DOUBLE DAGGER
			/* 0x88 */ (char)0x02C6, // MODIFIER LETTER CIRCUMFLEX ACCENT
			/* 0x89 */ (char)0x2030, // PER MILLE SIGN
			/* 0x8A */ (char)0x0160, // LATIN CAPITAL LETTER S WITH CARON
			/* 0x8B */ (char)0x2039, // SINGLE LEFT-POINTING ANGLE QUOTATION MARK
			/* 0x8C */ (char)0x0152, // LATIN CAPITAL LIGATURE OE
			/* 0x8D */ null        , // UNDEFINED
			/* 0x8E */ (char)0x017D, // LATIN CAPITAL LETTER Z WITH CARON
			/* 0x8F */ null        , // UNDEFINED
			/* 0x90 */ null        , // UNDEFINED
			/* 0x91 */ (char)0x2018, // LEFT SINGLE QUOTATION MARK
			/* 0x92 */ (char)0x2019, // RIGHT SINGLE QUOTATION MARK
			/* 0x93 */ (char)0x201C, // LEFT DOUBLE QUOTATION MARK
			/* 0x94 */ (char)0x201D, // RIGHT DOUBLE QUOTATION MARK
			/* 0x95 */ (char)0x2022, // BULLET
			/* 0x96 */ (char)0x2013, // EN DASH
			/* 0x97 */ (char)0x2014, // EM DASH
			/* 0x98 */ (char)0x02DC, // SMALL TILDE
			/* 0x99 */ (char)0x2122, // TRADE MARK SIGN
			/* 0x9A */ (char)0x0161, // LATIN SMALL LETTER S WITH CARON
			/* 0x9B */ (char)0x203A, // SINGLE RIGHT-POINTING ANGLE QUOTATION MARK
			/* 0x9C */ (char)0x0153, // LATIN SMALL LIGATURE OE
			/* 0x9D */ null        , // UNDEFINED
			/* 0x9E */ (char)0x017E, // LATIN SMALL LETTER Z WITH CARON
			/* 0x9F */ (char)0x0178, // LATIN CAPITAL LETTER Y WITH DIAERESIS
			/* 0xA0 */ (char)0x00A0, // NO-BREAK SPACE
			/* 0xA1 */ (char)0x00A1, // INVERTED EXCLAMATION MARK
			/* 0xA2 */ (char)0x00A2, // CENT SIGN
			/* 0xA3 */ (char)0x00A3, // POUND SIGN
			/* 0xA4 */ (char)0x00A4, // CURRENCY SIGN
			/* 0xA5 */ (char)0x00A5, // YEN SIGN
			/* 0xA6 */ (char)0x00A6, // BROKEN BAR
			/* 0xA7 */ (char)0x00A7, // SECTION SIGN
			/* 0xA8 */ (char)0x00A8, // DIAERESIS
			/* 0xA9 */ (char)0x00A9, // COPYRIGHT SIGN
			/* 0xAA */ (char)0x00AA, // FEMININE ORDINAL INDICATOR
			/* 0xAB */ (char)0x00AB, // LEFT-POINTING DOUBLE ANGLE QUOTATION MARK
			/* 0xAC */ (char)0x00AC, // NOT SIGN
			/* 0xAD */ (char)0x00AD, // SOFT HYPHEN
			/* 0xAE */ (char)0x00AE, // REGISTERED SIGN
			/* 0xAF */ (char)0x00AF, // MACRON
			/* 0xB0 */ (char)0x00B0, // DEGREE SIGN
			/* 0xB1 */ (char)0x00B1, // PLUS-MINUS SIGN
			/* 0xB2 */ (char)0x00B2, // SUPERSCRIPT TWO
			/* 0xB3 */ (char)0x00B3, // SUPERSCRIPT THREE
			/* 0xB4 */ (char)0x00B4, // ACUTE ACCENT
			/* 0xB5 */ (char)0x00B5, // MICRO SIGN
			/* 0xB6 */ (char)0x00B6, // PILCROW SIGN
			/* 0xB7 */ (char)0x00B7, // MIDDLE DOT
			/* 0xB8 */ (char)0x00B8, // CEDILLA
			/* 0xB9 */ (char)0x00B9, // SUPERSCRIPT ONE
			/* 0xBA */ (char)0x00BA, // MASCULINE ORDINAL INDICATOR
			/* 0xBB */ (char)0x00BB, // RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK
			/* 0xBC */ (char)0x00BC, // VULGAR FRACTION ONE QUARTER
			/* 0xBD */ (char)0x00BD, // VULGAR FRACTION ONE HALF
			/* 0xBE */ (char)0x00BE, // VULGAR FRACTION THREE QUARTERS
			/* 0xBF */ (char)0x00BF, // INVERTED QUESTION MARK
			/* 0xC0 */ (char)0x00C0, // LATIN CAPITAL LETTER A WITH GRAVE
			/* 0xC1 */ (char)0x00C1, // LATIN CAPITAL LETTER A WITH ACUTE
			/* 0xC2 */ (char)0x00C2, // LATIN CAPITAL LETTER A WITH CIRCUMFLEX
			/* 0xC3 */ (char)0x00C3, // LATIN CAPITAL LETTER A WITH TILDE
			/* 0xC4 */ (char)0x00C4, // LATIN CAPITAL LETTER A WITH DIAERESIS
			/* 0xC5 */ (char)0x00C5, // LATIN CAPITAL LETTER A WITH RING ABOVE
			/* 0xC6 */ (char)0x00C6, // LATIN CAPITAL LETTER AE
			/* 0xC7 */ (char)0x00C7, // LATIN CAPITAL LETTER C WITH CEDILLA
			/* 0xC8 */ (char)0x00C8, // LATIN CAPITAL LETTER E WITH GRAVE
			/* 0xC9 */ (char)0x00C9, // LATIN CAPITAL LETTER E WITH ACUTE
			/* 0xCA */ (char)0x00CA, // LATIN CAPITAL LETTER E WITH CIRCUMFLEX
			/* 0xCB */ (char)0x00CB, // LATIN CAPITAL LETTER E WITH DIAERESIS
			/* 0xCC */ (char)0x00CC, // LATIN CAPITAL LETTER I WITH GRAVE
			/* 0xCD */ (char)0x00CD, // LATIN CAPITAL LETTER I WITH ACUTE
			/* 0xCE */ (char)0x00CE, // LATIN CAPITAL LETTER I WITH CIRCUMFLEX
			/* 0xCF */ (char)0x00CF, // LATIN CAPITAL LETTER I WITH DIAERESIS
			/* 0xD0 */ (char)0x00D0, // LATIN CAPITAL LETTER ETH
			/* 0xD1 */ (char)0x00D1, // LATIN CAPITAL LETTER N WITH TILDE
			/* 0xD2 */ (char)0x00D2, // LATIN CAPITAL LETTER O WITH GRAVE
			/* 0xD3 */ (char)0x00D3, // LATIN CAPITAL LETTER O WITH ACUTE
			/* 0xD4 */ (char)0x00D4, // LATIN CAPITAL LETTER O WITH CIRCUMFLEX
			/* 0xD5 */ (char)0x00D5, // LATIN CAPITAL LETTER O WITH TILDE
			/* 0xD6 */ (char)0x00D6, // LATIN CAPITAL LETTER O WITH DIAERESIS
			/* 0xD7 */ (char)0x00D7, // MULTIPLICATION SIGN
			/* 0xD8 */ (char)0x00D8, // LATIN CAPITAL LETTER O WITH STROKE
			/* 0xD9 */ (char)0x00D9, // LATIN CAPITAL LETTER U WITH GRAVE
			/* 0xDA */ (char)0x00DA, // LATIN CAPITAL LETTER U WITH ACUTE
			/* 0xDB */ (char)0x00DB, // LATIN CAPITAL LETTER U WITH CIRCUMFLEX
			/* 0xDC */ (char)0x00DC, // LATIN CAPITAL LETTER U WITH DIAERESIS
			/* 0xDD */ (char)0x00DD, // LATIN CAPITAL LETTER Y WITH ACUTE
			/* 0xDE */ (char)0x00DE, // LATIN CAPITAL LETTER THORN
			/* 0xDF */ (char)0x00DF, // LATIN SMALL LETTER SHARP S
			/* 0xE0 */ (char)0x00E0, // LATIN SMALL LETTER A WITH GRAVE
			/* 0xE1 */ (char)0x00E1, // LATIN SMALL LETTER A WITH ACUTE
			/* 0xE2 */ (char)0x00E2, // LATIN SMALL LETTER A WITH CIRCUMFLEX
			/* 0xE3 */ (char)0x00E3, // LATIN SMALL LETTER A WITH TILDE
			/* 0xE4 */ (char)0x00E4, // LATIN SMALL LETTER A WITH DIAERESIS
			/* 0xE5 */ (char)0x00E5, // LATIN SMALL LETTER A WITH RING ABOVE
			/* 0xE6 */ (char)0x00E6, // LATIN SMALL LETTER AE
			/* 0xE7 */ (char)0x00E7, // LATIN SMALL LETTER C WITH CEDILLA
			/* 0xE8 */ (char)0x00E8, // LATIN SMALL LETTER E WITH GRAVE
			/* 0xE9 */ (char)0x00E9, // LATIN SMALL LETTER E WITH ACUTE
			/* 0xEA */ (char)0x00EA, // LATIN SMALL LETTER E WITH CIRCUMFLEX
			/* 0xEB */ (char)0x00EB, // LATIN SMALL LETTER E WITH DIAERESIS
			/* 0xEC */ (char)0x00EC, // LATIN SMALL LETTER I WITH GRAVE
			/* 0xED */ (char)0x00ED, // LATIN SMALL LETTER I WITH ACUTE
			/* 0xEE */ (char)0x00EE, // LATIN SMALL LETTER I WITH CIRCUMFLEX
			/* 0xEF */ (char)0x00EF, // LATIN SMALL LETTER I WITH DIAERESIS
			/* 0xF0 */ (char)0x00F0, // LATIN SMALL LETTER ETH
			/* 0xF1 */ (char)0x00F1, // LATIN SMALL LETTER N WITH TILDE
			/* 0xF2 */ (char)0x00F2, // LATIN SMALL LETTER O WITH GRAVE
			/* 0xF3 */ (char)0x00F3, // LATIN SMALL LETTER O WITH ACUTE
			/* 0xF4 */ (char)0x00F4, // LATIN SMALL LETTER O WITH CIRCUMFLEX
			/* 0xF5 */ (char)0x00F5, // LATIN SMALL LETTER O WITH TILDE
			/* 0xF6 */ (char)0x00F6, // LATIN SMALL LETTER O WITH DIAERESIS
			/* 0xF7 */ (char)0x00F7, // DIVISION SIGN
			/* 0xF8 */ (char)0x00F8, // LATIN SMALL LETTER O WITH STROKE
			/* 0xF9 */ (char)0x00F9, // LATIN SMALL LETTER U WITH GRAVE
			/* 0xFA */ (char)0x00FA, // LATIN SMALL LETTER U WITH ACUTE
			/* 0xFB */ (char)0x00FB, // LATIN SMALL LETTER U WITH CIRCUMFLEX
			/* 0xFC */ (char)0x00FC, // LATIN SMALL LETTER U WITH DIAERESIS
			/* 0xFD */ (char)0x00FD, // LATIN SMALL LETTER Y WITH ACUTE
			/* 0xFE */ (char)0x00FE, // LATIN SMALL LETTER THORN
			/* 0xFF */ (char)0x00FF // LATIN SMALL LETTER Y WITH DIAERESIS
		};

		private static readonly Dictionary<char, byte> unicodeToCode;

		static Windows1252()
		{
			byte i = 0;

			unicodeToCode = new Dictionary<char, byte>();

			foreach (char? ch in codeToUnicode)
			{
				if (ch.HasValue)
					unicodeToCode[ch.Value] = i;

				i++;
			}
		}

		public override int GetByteCount(char[] chars, int index, int count)
		{
			return count;
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			int c = charCount;

			while (c-- > 0)
			{
				if (unicodeToCode.TryGetValue(chars[charIndex++], out byte b))
					bytes[byteIndex++] = b;
				else
					bytes[byteIndex++] = (byte)'?';
			}

			return charCount;
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			return count;
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			int c = byteCount;
			char? ch;

			while (c-- > 0)
			{
				ch = codeToUnicode[bytes[byteIndex++]];
				if (ch.HasValue)
					chars[charIndex++] = ch.Value;
				else
					chars[charIndex++] = '?';
			}

			return byteCount;
		}

		public override int GetMaxByteCount(int charCount)
		{
			return charCount;
		}

		public override int GetMaxCharCount(int byteCount)
		{
			return byteCount;
		}
	}
}
