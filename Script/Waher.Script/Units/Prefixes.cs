using System;
using Waher.Script.Exceptions;

namespace Waher.Script.Units
{
	/// <summary>
	/// SI prefixes.
	/// http://physics.nist.gov/cuu/Units/prefixes.html
	/// </summary>
	public enum Prefix
	{
		/// <summary>
		/// 10^24 yotta Y 
		/// </summary>
		Yotta = 24,

		/// <summary>
		/// 10^21 zetta Z 
		/// </summary>
		Zetta = 21,

		/// <summary>
		/// 10^18 exa E 
		/// </summary>
		Exa = 18,

		/// <summary>
		/// 10^15 peta P 
		/// </summary>
		Peta = 15,

		/// <summary>
		/// 10^12 tera T 
		/// </summary>
		Tera = 12,

		/// <summary>
		/// 10^9 giga G 
		/// </summary>
		Giga = 9,

		/// <summary>
		/// 10^6 mega M 
		/// </summary>
		Mega = 6,

		/// <summary>
		/// 10^3 kilo k 
		/// </summary>
		Kilo = 3,

		/// <summary>
		/// 10^2 hecto h 
		/// </summary>
		Hecto = 2,

		/// <summary>
		/// 10^1 deka da
		/// </summary>
		Deka = 1,

		/// <summary>
		/// No prefix
		/// </summary>
		None = 0,

		/// <summary>
		/// 10^-1 deci d 
		/// </summary>
		Deci = -1,

		/// <summary>
		/// 10^-2 centi c 
		/// </summary>
		Centi = -2,

		/// <summary>
		/// 10^-3 milli m 
		/// </summary>
		Milli = -3,

		/// <summary>
		/// 10^-6 micro µ 
		/// </summary>
		Micro = -6,

		/// <summary>
		/// 10^-9 nano n 
		/// </summary>
		Nano = -9,

		/// <summary>
		/// 10^-12 pico p 
		/// </summary>
		Pico = -12,

		/// <summary>
		/// 10^-15 femto f 
		/// </summary>
		Femto = -15,

		/// <summary>
		/// 10^-18 atto a 
		/// </summary>
		Atto = -18,

		/// <summary>
		/// 10^-21 zepto z 
		/// </summary>
		Zepto = -21,

		/// <summary>
		/// 10^-24 yocto y
		/// </summary>
		Yocto = -24
	}

	/// <summary>
	/// Static class managing units.
	/// </summary>
	public static class Prefixes
	{
		/// <summary>
		/// Conerts a prefix to a multiplicative factor.
		/// </summary>
		/// <param name="Prefix">Prefix</param>
		/// <returns>Multiplicative factor.</returns>
		public static double PrefixToFactor(Prefix Prefix)
		{
			return Math.Pow(10, (int)Prefix);
		}

		/// <summary>
		/// Conerts a prefix to an exponent.
		/// </summary>
		/// <param name="Prefix">Prefix</param>
		/// <returns>Exponent.</returns>
		public static int PrefixToExponent(Prefix Prefix)
		{
			return (int)Prefix;
		}

		/// <summary>
		/// Converts an exponent to a prefix.
		/// </summary>
		/// <param name="Exponent">Exponent.</param>
		/// <param name="ResidueExponent">Any exponential residue. If this value is 0, it means the exponent corresponds exactly 
		/// to the returned prefix.</param>
		/// <returns>Closest prefix.</returns>
		public static Prefix ExponentToPrefix(int Exponent, out int ResidueExponent)
		{
			switch (Exponent)
			{
				case 24:
					ResidueExponent = 0;
					return Prefix.Yotta;

				case 23:
					ResidueExponent = 2;
					return Prefix.Zetta;

				case 22:
					ResidueExponent = 1;
					return Prefix.Zetta;

				case 21:
					ResidueExponent = 0;
					return Prefix.Zetta;

				case 20:
					ResidueExponent = 2;
					return Prefix.Exa;

				case 19:
					ResidueExponent = 1;
					return Prefix.Exa;

				case 18:
					ResidueExponent = 0;
					return Prefix.Exa;

				case 17:
					ResidueExponent = 2;
					return Prefix.Peta;

				case 16:
					ResidueExponent = 1;
					return Prefix.Peta;

				case 15:
					ResidueExponent = 0;
					return Prefix.Peta;

				case 14:
					ResidueExponent = 2;
					return Prefix.Tera;

				case 13:
					ResidueExponent = 1;
					return Prefix.Tera;

				case 12:
					ResidueExponent = 0;
					return Prefix.Tera;

				case 11:
					ResidueExponent = 2;
					return Prefix.Giga;

				case 10:
					ResidueExponent = 1;
					return Prefix.Giga;

				case 9:
					ResidueExponent = 0;
					return Prefix.Giga;

				case 8:
					ResidueExponent = 2;
					return Prefix.Mega;

				case 7:
					ResidueExponent = 1;
					return Prefix.Mega;

				case 6:
					ResidueExponent = 0;
					return Prefix.Mega;

				case 5:
					ResidueExponent = 2;
					return Prefix.Kilo;

				case 4:
					ResidueExponent = 1;
					return Prefix.Kilo;

				case 3:
					ResidueExponent = 0;
					return Prefix.Kilo;

				case 2:
					ResidueExponent = 0;
					return Prefix.Hecto;

				case 1:
					ResidueExponent = 0;
					return Prefix.Deka;

				case 0:
					ResidueExponent = 0;
					return Prefix.None;

				case -1:
					ResidueExponent = 0;
					return Prefix.Deci;

				case -2:
					ResidueExponent = 0;
					return Prefix.Centi;

				case -3:
					ResidueExponent = 0;
					return Prefix.Milli;

				case -4:
					ResidueExponent = 2;
					return Prefix.Micro;

				case -5:
					ResidueExponent = 1;
					return Prefix.Micro;

				case -6:
					ResidueExponent = 0;
					return Prefix.Micro;

				case -7:
					ResidueExponent = 2;
					return Prefix.Nano;

				case -8:
					ResidueExponent = 1;
					return Prefix.Nano;

				case -9:
					ResidueExponent = 0;
					return Prefix.Nano;

				case -10:
					ResidueExponent = 2;
					return Prefix.Pico;

				case -11:
					ResidueExponent = 1;
					return Prefix.Pico;

				case -12:
					ResidueExponent = 0;
					return Prefix.Pico;

				case -13:
					ResidueExponent = 2;
					return Prefix.Femto;

				case -14:
					ResidueExponent = 1;
					return Prefix.Femto;

				case -15:
					ResidueExponent = 0;
					return Prefix.Femto;

				case -16:
					ResidueExponent = 2;
					return Prefix.Atto;

				case -17:
					ResidueExponent = 1;
					return Prefix.Atto;

				case -18:
					ResidueExponent = 0;
					return Prefix.Atto;

				case -19:
					ResidueExponent = 2;
					return Prefix.Zepto;

				case -20:
					ResidueExponent = 1;
					return Prefix.Zepto;

				case -21:
					ResidueExponent = 0;
					return Prefix.Zepto;

				case -22:
					ResidueExponent = 2;
					return Prefix.Yocto;

				case -23:
					ResidueExponent = 1;
					return Prefix.Yocto;

				case -24:
					ResidueExponent = 0;
					return Prefix.Yocto;

				default:
					if (Exponent > 24)
					{
						ResidueExponent = Exponent - 24;
						return Prefix.Yotta;
					}
					else
					{
						ResidueExponent = Exponent + 24;
						return Prefix.Yocto;
					}
			}
		}

		/// <summary>
		/// Tries to parse a character into a prefix.
		/// 
		/// NOTE: The only prefix the method does not recognize is "da"=<see cref="Prefix.Deka"/>, since it consists of two letters.
		/// This prefix must be handled manually by the caller.
		/// </summary>
		/// <param name="ch">Character.</param>
		/// <param name="Prefix">Prefix, if successful.</param>
		/// <returns>If the character was recognized as a prefix.</returns>
		public static bool TryParsePrefix(char ch, out Prefix Prefix)
		{
			switch (ch)
			{
				case 'Y': Prefix = Prefix.Yotta; break;
				case 'Z': Prefix = Prefix.Zetta; break;
				case 'E': Prefix = Prefix.Exa; break;
				case 'P': Prefix = Prefix.Peta; break;
				case 'T': Prefix = Prefix.Tera; break;
				case 'G': Prefix = Prefix.Giga; break;
				case 'M': Prefix = Prefix.Mega; break;
				case 'k': Prefix = Prefix.Kilo; break;
				case 'h': Prefix = Prefix.Hecto; break;
				//case 'da': Prefix = Prefix.Deka; break;
				case 'd': Prefix = Prefix.Deci; break;
				case 'c': Prefix = Prefix.Centi; break;
				case 'm': Prefix = Prefix.Milli; break;
				case 'u':
				case 'µ': Prefix = Prefix.Micro; break;
				case 'n': Prefix = Prefix.Nano; break;
				case 'p': Prefix = Prefix.Pico; break;
				case 'f': Prefix = Prefix.Femto; break;
				case 'a': Prefix = Prefix.Atto; break;
				case 'z': Prefix = Prefix.Zepto; break;
				case 'y': Prefix = Prefix.Yocto; break;
				default:
					Prefix = Prefix.None;
					return false;
			}

			return true;
		}

		/// <summary>
		/// Converts a prefix to its string representation.
		/// </summary>
		/// <param name="Prefix">Prefix.</param>
		/// <returns>String representation.</returns>
		public static string ToString(Prefix Prefix)
		{
			switch (Prefix)
			{
				case Prefix.Yotta: return "Y";
				case Prefix.Zetta: return "Z";
				case Prefix.Exa: return "E";
				case Prefix.Peta: return "P";
				case Prefix.Tera: return "T";
				case Prefix.Giga: return "G";
				case Prefix.Mega: return "M";
				case Prefix.Kilo: return "k";
				case Prefix.Hecto: return "h";
				case Prefix.Deka: return "da";
				case Prefix.None: return string.Empty;
				case Prefix.Deci: return "d";
				case Prefix.Centi: return "c";
				case Prefix.Milli: return "m";
				case Prefix.Micro: return "µ";
				case Prefix.Nano: return "n";
				case Prefix.Pico: return "p";
				case Prefix.Femto: return "f";
				case Prefix.Atto: return "a";
				case Prefix.Zepto: return "z";
				case Prefix.Yocto: return "y";
				default:
					throw new ScriptException("Invalid prefix.");
			}
		}

	}
}
