using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;

namespace Waher.Content.Html
{
	/// <summary>
	/// HTML Entity
	/// </summary>
    public class HtmlEntity : HtmlNode
    {
		private readonly string entityName;

		/// <summary>
		/// HTML Entity
		/// </summary>
		/// <param name="Document">HTML Document.</param>
		/// <param name="Parent">Parent element. Can be null for root elements.</param>
		/// <param name="StartPosition">Start position.</param>
		/// <param name="EndPosition">End position.</param>
		/// <param name="EntityName">Entity name.</param>
		public HtmlEntity(HtmlDocument Document, HtmlNode Parent, int StartPosition, int EndPosition, string EntityName)
			: base(Document, Parent, StartPosition, EndPosition)
		{
			this.entityName = EntityName;
		}

		/// <summary>
		/// Entity Name
		/// </summary>
		public string EntityName
		{
			get { return this.entityName; }
		}

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			string s = EntityToCharacter(this.entityName);
			if (!string.IsNullOrEmpty(s))
				return s;
			else
				return "&" + this.entityName + ";";
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public override void Export(XmlWriter Output)
		{
			Output.Flush();
			Output.WriteRaw(XML.Encode(this.ToString()));
		}

		/// <summary>
		/// Converts an HTML entity into a character.
		/// 
		/// Reference:
		/// http://dev.w3.org/html5/html-author/charref
		/// </summary>
		/// <param name="Entity">HTML entity (without the &amp; and the ;)</param>
		/// <returns>Character, if entity recognized, or null if not.</returns>
		public static string EntityToCharacter(string Entity)
		{
			// Assumed to be compiled into a dictionary.
			switch (Entity)
			{
				case "Tab": return "\t";
				case "NewLine": return "\n";
				case "excl": return "!";
				case "quot":
				case "QUOT": return "\"";
				case "num": return "#";
				case "dollar": return "$";
				case "percnt": return "%";
				case "amp":
				case "AMP": return "&";
				case "apos": return "'";
				case "lpar": return "(";
				case "rpar": return ")";
				case "ast":
				case "midast": return "*";
				case "plus": return "+";
				case "comma": return ",";
				case "period": return ".";
				case "sol": return "/";
				case "colon": return ":";
				case "semi": return ";";
				case "lt":
				case "LT": return "<";
				case "equals": return "=";
				case "gt":
				case "GT": return ">";
				case "quest": return "?";
				case "commat": return "@";
				case "lsqb":
				case "lbrack": return "[";
				case "bsol": return "\\";
				case "rsqb":
				case "rbrack": return "]";
				case "Hat": return "^";
				case "lowbar": return "_";
				case "grave":
				case "DiacriticalGrave": return "`";
				case "lcub":
				case "lbrace": return "{";
				case "verbar":
				case "vert":
				case "VerticalLine": return "|";
				case "rcub":
				case "rbrace": return "}";
				case "nbsp":
				case "NonBreakingSpace": return " ";
				case "iexcl": return "¡";
				case "cent": return "¢";
				case "pound": return "£";
				case "curren": return "¤";
				case "yen": return "¥";
				case "brvbar": return "¦";
				case "sect": return "§";
				case "Dot":
				case "die":
				case "DoubleDot":
				case "uml": return "¨";
				case "copy":
				case "COPY": return "©";
				case "ordf": return "ª";
				case "laquo": return "«";
				case "not": return "¬";
				case "shy": return " ";
				case "reg":
				case "circledR":
				case "REG": return "®";
				case "macr":
				case "OverBar":
				case "strns": return "¯";
				case "deg": return "°";
				case "plusmn":
				case "pm":
				case "PlusMinus": return "±";
				case "sup2": return "²";
				case "sup3": return "³";
				case "acute":
				case "DiacriticalAcute": return "´";
				case "micro": return "µ";
				case "para": return "¶";
				case "middot":
				case "centerdot":
				case "CenterDot": return "·";
				case "cedil":
				case "Cedilla": return "¸";
				case "sup1": return "¹";
				case "ordm": return "º";
				case "raquo": return "»";
				case "frac14": return "¼";
				case "frac12":
				case "half": return "½";
				case "frac34": return "¾";
				case "iquest": return "¿";
				case "Agrave": return "À";
				case "Aacute": return "Á";
				case "Acirc": return "Â";
				case "Atilde": return "Ã";
				case "Auml": return "Ä";
				case "Aring": return "Å";
				case "AElig": return "Æ";
				case "Ccedil": return "Ç";
				case "Egrave": return "È";
				case "Eacute": return "É";
				case "Ecirc": return "Ê";
				case "Euml": return "Ë";
				case "Igrave": return "Ì";
				case "Iacute": return "Í";
				case "Icirc": return "Î";
				case "Iuml": return "Ï";
				case "ETH": return "Ð";
				case "Ntilde": return "Ñ";
				case "Ograve": return "Ò";
				case "Oacute": return "Ó";
				case "Ocirc": return "Ô";
				case "Otilde": return "Õ";
				case "Ouml": return "Ö";
				case "times": return "×";
				case "Oslash": return "Ø";
				case "Ugrave": return "Ù";
				case "Uacute": return "Ú";
				case "Ucirc": return "Û";
				case "Uuml": return "Ü";
				case "Yacute": return "Ý";
				case "THORN": return "Þ";
				case "szlig": return "ß";
				case "agrave": return "à";
				case "aacute": return "á";
				case "acirc": return "â";
				case "atilde": return "ã";
				case "auml": return "ä";
				case "aring": return "å";
				case "aelig": return "æ";
				case "ccedil": return "ç";
				case "egrave": return "è";
				case "eacute": return "é";
				case "ecirc": return "ê";
				case "euml": return "ë";
				case "igrave": return "ì";
				case "iacute": return "í";
				case "icirc": return "î";
				case "iuml": return "ï";
				case "eth": return "ð";
				case "ntilde": return "ñ";
				case "ograve": return "ò";
				case "oacute": return "ó";
				case "ocirc": return "ô";
				case "otilde": return "õ";
				case "ouml": return "ö";
				case "divide":
				case "div": return "÷";
				case "oslash": return "ø";
				case "ugrave": return "ù";
				case "uacute": return "ú";
				case "ucirc": return "û";
				case "uuml": return "ü";
				case "yacute": return "ý";
				case "thorn": return "þ";
				case "yuml": return "ÿ";
				case "Amacr": return "Ā";
				case "amacr": return "ā";
				case "Abreve": return "Ă";
				case "abreve": return "ă";
				case "Aogon": return "Ą";
				case "aogon": return "ą";
				case "Cacute": return "Ć";
				case "cacute": return "ć";
				case "Ccirc": return "Ĉ";
				case "ccirc": return "ĉ";
				case "Cdot": return "Ċ";
				case "cdot": return "ċ";
				case "Ccaron": return "Č";
				case "ccaron": return "č";
				case "Dcaron": return "Ď";
				case "dcaron": return "ď";
				case "Dstrok": return "Đ";
				case "dstrok": return "đ";
				case "Emacr": return "Ē";
				case "emacr": return "ē";
				case "Edot": return "Ė";
				case "edot": return "ė";
				case "Eogon": return "Ę";
				case "eogon": return "ę";
				case "Ecaron": return "Ě";
				case "ecaron": return "ě";
				case "Gcirc": return "Ĝ";
				case "gcirc": return "ĝ";
				case "Gbreve": return "Ğ";
				case "gbreve": return "ğ";
				case "Gdot": return "Ġ";
				case "gdot": return "ġ";
				case "Gcedil": return "Ģ";
				case "Hcirc": return "Ĥ";
				case "hcirc": return "ĥ";
				case "Hstrok": return "Ħ";
				case "hstrok": return "ħ";
				case "Itilde": return "Ĩ";
				case "itilde": return "ĩ";
				case "Imacr": return "Ī";
				case "imacr": return "ī";
				case "Iogon": return "Į";
				case "iogon": return "į";
				case "Idot": return "İ";
				case "imath":
				case "inodot": return "ı";
				case "IJlig": return "Ĳ";
				case "ijlig": return "ĳ";
				case "Jcirc": return "Ĵ";
				case "jcirc": return "ĵ";
				case "Kcedil": return "Ķ";
				case "kcedil": return "ķ";
				case "kgreen": return "ĸ";
				case "Lacute": return "Ĺ";
				case "lacute": return "ĺ";
				case "Lcedil": return "Ļ";
				case "lcedil": return "ļ";
				case "Lcaron": return "Ľ";
				case "lcaron": return "ľ";
				case "Lmidot": return "Ŀ";
				case "lmidot": return "ŀ";
				case "Lstrok": return "Ł";
				case "lstrok": return "ł";
				case "Nacute": return "Ń";
				case "nacute": return "ń";
				case "Ncedil": return "Ņ";
				case "ncedil": return "ņ";
				case "Ncaron": return "Ň";
				case "ncaron": return "ň";
				case "napos": return "ŉ";
				case "ENG": return "Ŋ";
				case "eng": return "ŋ";
				case "Omacr": return "Ō";
				case "omacr": return "ō";
				case "Odblac": return "Ő";
				case "odblac": return "ő";
				case "OElig": return "Œ";
				case "oelig": return "œ";
				case "Racute": return "Ŕ";
				case "racute": return "ŕ";
				case "Rcedil": return "Ŗ";
				case "rcedil": return "ŗ";
				case "Rcaron": return "Ř";
				case "rcaron": return "ř";
				case "Sacute": return "Ś";
				case "sacute": return "ś";
				case "Scirc": return "Ŝ";
				case "scirc": return "ŝ";
				case "Scedil": return "Ş";
				case "scedil": return "ş";
				case "Scaron": return "Š";
				case "scaron": return "š";
				case "Tcedil": return "Ţ";
				case "tcedil": return "ţ";
				case "Tcaron": return "Ť";
				case "tcaron": return "ť";
				case "Tstrok": return "Ŧ";
				case "tstrok": return "ŧ";
				case "Utilde": return "Ũ";
				case "utilde": return "ũ";
				case "Umacr": return "Ū";
				case "umacr": return "ū";
				case "Ubreve": return "Ŭ";
				case "ubreve": return "ŭ";
				case "Uring": return "Ů";
				case "uring": return "ů";
				case "Udblac": return "Ű";
				case "udblac": return "ű";
				case "Uogon": return "Ų";
				case "uogon": return "ų";
				case "Wcirc": return "Ŵ";
				case "wcirc": return "ŵ";
				case "Ycirc": return "Ŷ";
				case "ycirc": return "ŷ";
				case "Yuml": return "Ÿ";
				case "Zacute": return "Ź";
				case "zacute": return "ź";
				case "Zdot": return "Ż";
				case "zdot": return "ż";
				case "Zcaron": return "Ž";
				case "zcaron": return "ž";
				case "fnof": return "ƒ";
				case "imped": return "Ƶ";
				case "gacute": return "ǵ";
				case "jmath": return "ȷ";
				case "circ": return "ˆ";
				case "caron":
				case "Hacek": return "ˇ";
				case "breve":
				case "Breve": return "˘";
				case "dot":
				case "DiacriticalDot": return "˙";
				case "ring": return "˚";
				case "ogon": return "˛";
				case "tilde":
				case "DiacriticalTilde": return "˜";
				case "dblac":
				case "DiacriticalDoubleAcute": return "˝";
				case "DownBreve": return new string((char)785, 1);
				case "UnderBar": return new string((char)818, 1);
				case "Alpha": return "Α";
				case "Beta": return "Β";
				case "Gamma": return "Γ";
				case "Delta": return "Δ";
				case "Epsilon": return "Ε";
				case "Zeta": return "Ζ";
				case "Eta": return "Η";
				case "Theta": return "Θ";
				case "Iota": return "Ι";
				case "Kappa": return "Κ";
				case "Lambda": return "Λ";
				case "Mu": return "Μ";
				case "Nu": return "Ν";
				case "Xi": return "Ξ";
				case "Omicron": return "Ο";
				case "Pi": return "Π";
				case "Rho": return "Ρ";
				case "Sigma": return "Σ";
				case "Tau": return "Τ";
				case "Upsilon": return "Υ";
				case "Phi": return "Φ";
				case "Chi": return "Χ";
				case "Psi": return "Ψ";
				case "Omega": return "Ω";
				case "alpha": return "α";
				case "beta": return "β";
				case "gamma": return "γ";
				case "delta": return "δ";
				case "epsiv":
				case "varepsilon":
				case "epsilon": return "ε";
				case "zeta": return "ζ";
				case "eta": return "η";
				case "theta": return "θ";
				case "iota": return "ι";
				case "kappa": return "κ";
				case "lambda": return "λ";
				case "mu": return "μ";
				case "nu": return "ν";
				case "xi": return "ξ";
				case "omicron": return "ο";
				case "pi": return "π";
				case "rho": return "ρ";
				case "sigmav":
				case "varsigma":
				case "sigmaf": return "ς";
				case "sigma": return "σ";
				case "tau": return "τ";
				case "upsi":
				case "upsilon": return "υ";
				case "phi":
				case "phiv":
				case "varphi": return "φ";
				case "chi": return "χ";
				case "psi": return "ψ";
				case "omega": return "ω";
				case "thetav":
				case "vartheta":
				case "thetasym": return "ϑ";
				case "Upsi":
				case "upsih": return "ϒ";
				case "straightphi": return "ϕ";
				case "piv":
				case "varpi": return "ϖ";
				case "Gammad": return "Ϝ";
				case "gammad":
				case "digamma": return "ϝ";
				case "kappav":
				case "varkappa": return "ϰ";
				case "rhov":
				case "varrho": return "ϱ";
				case "epsi":
				case "straightepsilon": return "ϵ";
				case "bepsi":
				case "backepsilon": return "϶";
				case "IOcy": return "Ё";
				case "DJcy": return "Ђ";
				case "GJcy": return "Ѓ";
				case "Jukcy": return "Є";
				case "DScy": return "Ѕ";
				case "Iukcy": return "І";
				case "YIcy": return "Ї";
				case "Jsercy": return "Ј";
				case "LJcy": return "Љ";
				case "NJcy": return "Њ";
				case "TSHcy": return "Ћ";
				case "KJcy": return "Ќ";
				case "Ubrcy": return "Ў";
				case "DZcy": return "Џ";
				case "Acy": return "А";
				case "Bcy": return "Б";
				case "Vcy": return "В";
				case "Gcy": return "Г";
				case "Dcy": return "Д";
				case "IEcy": return "Е";
				case "ZHcy": return "Ж";
				case "Zcy": return "З";
				case "Icy": return "И";
				case "Jcy": return "Й";
				case "Kcy": return "К";
				case "Lcy": return "Л";
				case "Mcy": return "М";
				case "Ncy": return "Н";
				case "Ocy": return "О";
				case "Pcy": return "П";
				case "Rcy": return "Р";
				case "Scy": return "С";
				case "Tcy": return "Т";
				case "Ucy": return "У";
				case "Fcy": return "Ф";
				case "KHcy": return "Х";
				case "TScy": return "Ц";
				case "CHcy": return "Ч";
				case "SHcy": return "Ш";
				case "SHCHcy": return "Щ";
				case "HARDcy": return "Ъ";
				case "Ycy": return "Ы";
				case "SOFTcy": return "Ь";
				case "Ecy": return "Э";
				case "YUcy": return "Ю";
				case "YAcy": return "Я";
				case "acy": return "а";
				case "bcy": return "б";
				case "vcy": return "в";
				case "gcy": return "г";
				case "dcy": return "д";
				case "iecy": return "е";
				case "zhcy": return "ж";
				case "zcy": return "з";
				case "icy": return "и";
				case "jcy": return "й";
				case "kcy": return "к";
				case "lcy": return "л";
				case "mcy": return "м";
				case "ncy": return "н";
				case "ocy": return "о";
				case "pcy": return "п";
				case "rcy": return "р";
				case "scy": return "с";
				case "tcy": return "т";
				case "ucy": return "у";
				case "fcy": return "ф";
				case "khcy": return "х";
				case "tscy": return "ц";
				case "chcy": return "ч";
				case "shcy": return "ш";
				case "shchcy": return "щ";
				case "hardcy": return "ъ";
				case "ycy": return "ы";
				case "softcy": return "ь";
				case "ecy": return "э";
				case "yucy": return "ю";
				case "yacy": return "я";
				case "iocy": return "ё";
				case "djcy": return "ђ";
				case "gjcy": return "ѓ";
				case "jukcy": return "є";
				case "dscy": return "ѕ";
				case "iukcy": return "і";
				case "yicy": return "ї";
				case "jsercy": return "ј";
				case "ljcy": return "љ";
				case "njcy": return "њ";
				case "tshcy": return "ћ";
				case "kjcy": return "ќ";
				case "ubrcy": return "ў";
				case "dzcy": return "џ";
				case "ensp": return new string((char)8194, 1);
				case "emsp": return new string((char)8195, 1);
				case "emsp13": return new string((char)8196, 1);
				case "emsp14": return new string((char)8197, 1);
				case "numsp": return new string((char)8199, 1);
				case "puncsp": return new string((char)8200, 1);
				case "thinsp":
				case "ThinSpace": return new string((char)8201, 1);
				case "hairsp":
				case "VeryThinSpace": return new string((char)8202, 1);
				case "ZeroWidthSpace":
				case "NegativeVeryThinSpace":
				case "NegativeThinSpace":
				case "NegativeMediumSpace":
				case "NegativeThickSpace": return new string((char)8203, 1);
				case "zwnj": return new string((char)8204, 1);
				case "zwj": return new string((char)8205, 1);
				case "lrm": return new string((char)8206, 1);
				case "rlm": return new string((char)8207, 1);
				case "hyphen":
				case "dash": return "‐";
				case "ndash": return "–";
				case "mdash": return "—";
				case "horbar": return "―";
				case "Verbar":
				case "Vert": return "‖";
				case "lsquo":
				case "OpenCurlyQuote": return "‘";
				case "rsquo":
				case "rsquor":
				case "CloseCurlyQuote": return "’";
				case "lsquor":
				case "sbquo": return "‚";
				case "ldquo":
				case "OpenCurlyDoubleQuote": return "“";
				case "rdquo":
				case "rdquor":
				case "CloseCurlyDoubleQuote": return "”";
				case "ldquor":
				case "bdquo": return "„";
				case "dagger": return "†";
				case "Dagger":
				case "ddagger": return "‡";
				case "bull":
				case "bullet": return "•";
				case "nldr": return "‥";
				case "hellip":
				case "mldr": return "…";
				case "permil": return "‰";
				case "pertenk": return "‱";
				case "prime": return "′";
				case "Prime": return "″";
				case "tprime": return "‴";
				case "bprime":
				case "backprime": return "‵";
				case "lsaquo": return "‹";
				case "rsaquo": return "›";
				case "oline": return "‾";
				case "caret": return "⁁";
				case "hybull": return "⁃";
				case "frasl": return "⁄";
				case "bsemi": return "⁏";
				case "qprime": return "⁗";
				case "MediumSpace; ": return new string((char)8287, 1);
				case "NoBreak": return new string((char)8288, 1);
				case "ApplyFunction":
				case "af": return new string((char)8289, 1);
				case "InvisibleTimes":
				case "it": return new string((char)8290, 1);
				case "InvisibleComma":
				case "ic": return new string((char)8291, 1);
				case "euro": return "€";
				case "tdot":
				case "TripleDot": return new string((char)8411, 1);
				case "DotDot": return new string((char)8412, 1);
				case "Copf":
				case "complexes": return "ℂ";
				case "incare": return "℅";
				case "gscr": return "ℊ";
				case "hamilt":
				case "HilbertSpace":
				case "Hscr": return "ℋ";
				case "Hfr":
				case "Poincareplane": return "ℌ";
				case "quaternions":
				case "Hopf": return "ℍ";
				case "planckh": return "ℎ";
				case "planck":
				case "hbar":
				case "plankv":
				case "hslash": return "ℏ";
				case "Iscr":
				case "imagline": return "ℐ";
				case "image":
				case "Im":
				case "imagpart":
				case "Ifr": return "ℑ";
				case "Lscr":
				case "lagran":
				case "Laplacetrf": return "ℒ";
				case "ell": return "ℓ";
				case "Nopf":
				case "naturals": return "ℕ";
				case "numero": return "№";
				case "copysr": return "℗";
				case "weierp":
				case "wp": return "℘";
				case "Popf":
				case "primes": return "ℙ";
				case "rationals":
				case "Qopf": return "ℚ";
				case "Rscr":
				case "realine": return "ℛ";
				case "real":
				case "Re":
				case "realpart":
				case "Rfr": return "ℜ";
				case "reals":
				case "Ropf": return "ℝ";
				case "rx": return "℞";
				case "trade":
				case "TRADE": return "™";
				case "integers":
				case "Zopf": return "ℤ";
				case "ohm": return "Ω";
				case "mho": return "℧";
				case "Zfr":
				case "zeetrf": return "ℨ";
				case "iiota": return "℩";
				case "angst": return "Å";
				case "bernou":
				case "Bernoullis":
				case "Bscr": return "ℬ";
				case "Cfr":
				case "Cayleys": return "ℭ";
				case "escr": return "ℯ";
				case "Escr":
				case "expectation": return "ℰ";
				case "Fscr":
				case "Fouriertrf": return "ℱ";
				case "phmmat":
				case "Mellintrf":
				case "Mscr": return "ℳ";
				case "order":
				case "orderof":
				case "oscr": return "ℴ";
				case "alefsym":
				case "aleph": return "ℵ";
				case "beth": return "ℶ";
				case "gimel": return "ℷ";
				case "daleth": return "ℸ";
				case "CapitalDifferentialD":
				case "DD": return "ⅅ";
				case "DifferentialD":
				case "dd": return "ⅆ";
				case "ExponentialE":
				case "exponentiale":
				case "ee": return "ⅇ";
				case "ImaginaryI":
				case "ii": return "ⅈ";
				case "frac13": return "⅓";
				case "frac23": return "⅔";
				case "frac15": return "⅕";
				case "frac25": return "⅖";
				case "frac35": return "⅗";
				case "frac45": return "⅘";
				case "frac16": return "⅙";
				case "frac56": return "⅚";
				case "frac18": return "⅛";
				case "frac38": return "⅜";
				case "frac58": return "⅝";
				case "frac78": return "⅞";
				case "larr":
				case "leftarrow":
				case "LeftArrow":
				case "slarr":
				case "ShortLeftArrow": return "←";
				case "uarr":
				case "uparrow":
				case "UpArrow":
				case "ShortUpArrow": return "↑";
				case "rarr":
				case "rightarrow":
				case "RightArrow":
				case "srarr":
				case "ShortRightArrow": return "→";
				case "darr":
				case "downarrow":
				case "DownArrow":
				case "ShortDownArrow": return "↓";
				case "harr":
				case "leftrightarrow":
				case "LeftRightArrow": return "↔";
				case "varr":
				case "updownarrow":
				case "UpDownArrow": return "↕";
				case "nwarr":
				case "UpperLeftArrow":
				case "nwarrow": return "↖";
				case "nearr":
				case "UpperRightArrow":
				case "nearrow": return "↗";
				case "searr":
				case "searrow":
				case "LowerRightArrow": return "↘";
				case "swarr":
				case "swarrow":
				case "LowerLeftArrow": return "↙";
				case "nlarr":
				case "nleftarrow": return "↚";
				case "nrarr":
				case "nrightarrow": return "↛";
				case "rarrw":
				case "rightsquigarrow": return "↝";
				case "Larr":
				case "twoheadleftarrow": return "↞";
				case "Uarr": return "↟";
				case "Rarr":
				case "twoheadrightarrow": return "↠";
				case "Darr": return "↡";
				case "larrtl":
				case "leftarrowtail": return "↢";
				case "rarrtl":
				case "rightarrowtail": return "↣";
				case "LeftTeeArrow":
				case "mapstoleft": return "↤";
				case "UpTeeArrow":
				case "mapstoup": return "↥";
				case "map":
				case "RightTeeArrow":
				case "mapsto": return "↦";
				case "DownTeeArrow":
				case "mapstodown": return "↧";
				case "larrhk":
				case "hookleftarrow": return "↩";
				case "rarrhk":
				case "hookrightarrow": return "↪";
				case "larrlp":
				case "looparrowleft": return "↫";
				case "rarrlp":
				case "looparrowright": return "↬";
				case "harrw":
				case "leftrightsquigarrow": return "↭";
				case "nharr":
				case "nleftrightarrow": return "↮";
				case "lsh":
				case "Lsh": return "↰";
				case "rsh":
				case "Rsh": return "↱";
				case "ldsh": return "↲";
				case "rdsh": return "↳";
				case "crarr": return "↵";
				case "cularr":
				case "curvearrowleft": return "↶";
				case "curarr":
				case "curvearrowright": return "↷";
				case "olarr":
				case "circlearrowleft": return "↺";
				case "orarr":
				case "circlearrowright": return "↻";
				case "lharu":
				case "LeftVector":
				case "leftharpoonup": return "↼";
				case "lhard":
				case "leftharpoondown":
				case "DownLeftVector": return "↽";
				case "uharr":
				case "upharpoonright":
				case "RightUpVector": return "↾";
				case "uharl":
				case "upharpoonleft":
				case "LeftUpVector": return "↿";
				case "rharu":
				case "RightVector":
				case "rightharpoonup": return "⇀";
				case "rhard":
				case "rightharpoondown":
				case "DownRightVector": return "⇁";
				case "dharr":
				case "RightDownVector":
				case "downharpoonright": return "⇂";
				case "dharl":
				case "LeftDownVector":
				case "downharpoonleft": return "⇃";
				case "rlarr":
				case "rightleftarrows":
				case "RightArrowLeftArrow": return "⇄";
				case "udarr":
				case "UpArrowDownArrow": return "⇅";
				case "lrarr":
				case "leftrightarrows":
				case "LeftArrowRightArrow": return "⇆";
				case "llarr":
				case "leftleftarrows": return "⇇";
				case "uuarr":
				case "upuparrows": return "⇈";
				case "rrarr":
				case "rightrightarrows": return "⇉";
				case "ddarr":
				case "downdownarrows": return "⇊";
				case "lrhar":
				case "ReverseEquilibrium":
				case "leftrightharpoons": return "⇋";
				case "rlhar":
				case "rightleftharpoons":
				case "Equilibrium": return "⇌";
				case "nlArr":
				case "nLeftarrow": return "⇍";
				case "nhArr":
				case "nLeftrightarrow": return "⇎";
				case "nrArr":
				case "nRightarrow": return "⇏";
				case "lArr":
				case "Leftarrow":
				case "DoubleLeftArrow": return "⇐";
				case "uArr":
				case "Uparrow":
				case "DoubleUpArrow": return "⇑";
				case "rArr":
				case "Rightarrow":
				case "Implies":
				case "DoubleRightArrow": return "⇒";
				case "dArr":
				case "Downarrow":
				case "DoubleDownArrow": return "⇓";
				case "hArr":
				case "Leftrightarrow":
				case "DoubleLeftRightArrow":
				case "iff": return "⇔";
				case "vArr":
				case "Updownarrow":
				case "DoubleUpDownArrow": return "⇕";
				case "nwArr": return "⇖";
				case "neArr": return "⇗";
				case "seArr": return "⇘";
				case "swArr": return "⇙";
				case "lAarr":
				case "Lleftarrow": return "⇚";
				case "rAarr":
				case "Rrightarrow": return "⇛";
				case "zigrarr": return "⇝";
				case "larrb":
				case "LeftArrowBar": return "⇤";
				case "rarrb":
				case "RightArrowBar": return "⇥";
				case "duarr":
				case "DownArrowUpArrow": return "⇵";
				case "loarr": return "⇽";
				case "roarr": return "⇾";
				case "hoarr": return "⇿";
				case "forall":
				case "ForAll": return "∀";
				case "comp":
				case "complement": return "∁";
				case "part":
				case "PartialD": return "∂";
				case "exist":
				case "Exists": return "∃";
				case "nexist":
				case "NotExists":
				case "nexists": return "∄";
				case "empty":
				case "emptyset":
				case "emptyv":
				case "varnothing": return "∅";
				case "nabla":
				case "Del": return "∇";
				case "isin":
				case "isinv":
				case "Element":
				case "in": return "∈";
				case "notin":
				case "NotElement":
				case "notinva": return "∉";
				case "niv":
				case "ReverseElement":
				case "ni":
				case "SuchThat": return "∋";
				case "notni":
				case "notniva":
				case "NotReverseElement": return "∌";
				case "prod":
				case "Product": return "∏";
				case "coprod":
				case "Coproduct": return "∐";
				case "sum":
				case "Sum": return "∑";
				case "minus": return "−";
				case "mnplus":
				case "mp":
				case "MinusPlus": return "∓";
				case "plusdo":
				case "dotplus": return "∔";
				case "setmn":
				case "setminus":
				case "Backslash":
				case "ssetmn":
				case "smallsetminus": return "∖";
				case "lowast": return "∗";
				case "compfn":
				case "SmallCircle": return "∘";
				case "radic":
				case "Sqrt": return "√";
				case "prop":
				case "propto":
				case "Proportional":
				case "vprop":
				case "varpropto": return "∝";
				case "infin": return "∞";
				case "angrt": return "∟";
				case "ang":
				case "angle": return "∠";
				case "angmsd":
				case "measuredangle": return "∡";
				case "angsph": return "∢";
				case "mid":
				case "VerticalBar":
				case "smid":
				case "shortmid": return "∣";
				case "nmid":
				case "NotVerticalBar":
				case "nsmid":
				case "nshortmid": return "∤";
				case "par":
				case "parallel":
				case "DoubleVerticalBar":
				case "spar":
				case "shortparallel": return "∥";
				case "npar":
				case "nparallel":
				case "NotDoubleVerticalBar":
				case "nspar":
				case "nshortparallel": return "∦";
				case "and":
				case "wedge": return "∧";
				case "or":
				case "vee": return "∨";
				case "cap": return "∩";
				case "cup": return "∪";
				case "int":
				case "Integral": return "∫";
				case "Int": return "∬";
				case "tint":
				case "iiint": return "∭";
				case "conint":
				case "oint":
				case "ContourIntegral": return "∮";
				case "Conint":
				case "DoubleContourIntegral": return "∯";
				case "Cconint": return "∰";
				case "cwint": return "∱";
				case "cwconint":
				case "ClockwiseContourIntegral": return "∲";
				case "awconint":
				case "CounterClockwiseContourIntegral": return "∳";
				case "there4":
				case "therefore":
				case "Therefore": return "∴";
				case "becaus":
				case "because":
				case "Because": return "∵";
				case "ratio": return "∶";
				case "Colon":
				case "Proportion": return "∷";
				case "minusd":
				case "dotminus": return "∸";
				case "mDDot": return "∺";
				case "homtht": return "∻";
				case "sim":
				case "Tilde":
				case "thksim":
				case "thicksim": return "∼";
				case "bsim":
				case "backsim": return "∽";
				case "ac":
				case "mstpos": return "∾";
				case "acd": return "∿";
				case "wreath":
				case "VerticalTilde":
				case "wr": return "≀";
				case "nsim":
				case "NotTilde": return "≁";
				case "esim":
				case "EqualTilde":
				case "eqsim": return "≂";
				case "sime":
				case "TildeEqual":
				case "simeq": return "≃";
				case "nsime":
				case "nsimeq":
				case "NotTildeEqual": return "≄";
				case "cong":
				case "TildeFullEqual": return "≅";
				case "simne": return "≆";
				case "ncong":
				case "NotTildeFullEqual": return "≇";
				case "asymp":
				case "ap":
				case "TildeTilde":
				case "approx":
				case "thkap":
				case "thickapprox": return "≈";
				case "nap":
				case "NotTildeTilde":
				case "napprox": return "≉";
				case "ape":
				case "approxeq": return "≊";
				case "apid": return "≋";
				case "bcong":
				case "backcong": return "≌";
				case "asympeq":
				case "CupCap": return "≍";
				case "bump":
				case "HumpDownHump":
				case "Bumpeq": return "≎";
				case "bumpe":
				case "HumpEqual":
				case "bumpeq": return "≏";
				case "esdot":
				case "DotEqual":
				case "doteq": return "≐";
				case "eDot":
				case "doteqdot": return "≑";
				case "efDot":
				case "fallingdotseq": return "≒";
				case "erDot":
				case "risingdotseq": return "≓";
				case "colone":
				case "coloneq":
				case "Assign": return "≔";
				case "ecolon":
				case "eqcolon": return "≕";
				case "ecir":
				case "eqcirc": return "≖";
				case "cire":
				case "circeq": return "≗";
				case "wedgeq": return "≙";
				case "veeeq": return "≚";
				case "trie":
				case "triangleq": return "≜";
				case "equest":
				case "questeq": return "≟";
				case "ne":
				case "NotEqual": return "≠";
				case "equiv":
				case "Congruent": return "≡";
				case "nequiv":
				case "NotCongruent": return "≢";
				case "le":
				case "leq": return "≤";
				case "ge":
				case "GreaterEqual":
				case "geq": return "≥";
				case "lE":
				case "LessFullEqual":
				case "leqq": return "≦";
				case "gE":
				case "GreaterFullEqual":
				case "geqq": return "≧";
				case "lnE":
				case "lneqq": return "≨";
				case "gnE":
				case "gneqq": return "≩";
				case "Lt":
				case "NestedLessLess":
				case "ll": return "≪";
				case "Gt":
				case "NestedGreaterGreater":
				case "gg": return "≫";
				case "twixt":
				case "between": return "≬";
				case "NotCupCap": return "≭";
				case "nlt":
				case "NotLess":
				case "nless": return "≮";
				case "ngt":
				case "NotGreater":
				case "ngtr": return "≯";
				case "nle":
				case "NotLessEqual":
				case "nleq": return "≰";
				case "nge":
				case "NotGreaterEqual":
				case "ngeq": return "≱";
				case "lsim":
				case "LessTilde":
				case "lesssim": return "≲";
				case "gsim":
				case "gtrsim":
				case "GreaterTilde": return "≳";
				case "nlsim":
				case "NotLessTilde": return "≴";
				case "ngsim":
				case "NotGreaterTilde": return "≵";
				case "lg":
				case "lessgtr":
				case "LessGreater": return "≶";
				case "gl":
				case "gtrless":
				case "GreaterLess": return "≷";
				case "ntlg":
				case "NotLessGreater": return "≸";
				case "ntgl":
				case "NotGreaterLess": return "≹";
				case "pr":
				case "Precedes":
				case "prec": return "≺";
				case "sc":
				case "Succeeds":
				case "succ": return "≻";
				case "prcue":
				case "PrecedesSlantEqual":
				case "preccurlyeq": return "≼";
				case "sccue":
				case "SucceedsSlantEqual":
				case "succcurlyeq": return "≽";
				case "prsim":
				case "precsim":
				case "PrecedesTilde": return "≾";
				case "scsim":
				case "succsim":
				case "SucceedsTilde": return "≿";
				case "npr":
				case "nprec":
				case "NotPrecedes": return "⊀";
				case "nsc":
				case "nsucc":
				case "NotSucceeds": return "⊁";
				case "sub":
				case "subset": return "⊂";
				case "sup":
				case "supset":
				case "Superset": return "⊃";
				case "nsub": return "⊄";
				case "nsup": return "⊅";
				case "sube":
				case "SubsetEqual":
				case "subseteq": return "⊆";
				case "supe":
				case "supseteq":
				case "SupersetEqual": return "⊇";
				case "nsube":
				case "nsubseteq":
				case "NotSubsetEqual": return "⊈";
				case "nsupe":
				case "nsupseteq":
				case "NotSupersetEqual": return "⊉";
				case "subne":
				case "subsetneq": return "⊊";
				case "supne":
				case "supsetneq": return "⊋";
				case "cupdot": return "⊍";
				case "uplus":
				case "UnionPlus": return "⊎";
				case "sqsub":
				case "SquareSubset":
				case "sqsubset": return "⊏";
				case "sqsup":
				case "SquareSuperset":
				case "sqsupset": return "⊐";
				case "sqsube":
				case "SquareSubsetEqual":
				case "sqsubseteq": return "⊑";
				case "sqsupe":
				case "SquareSupersetEqual":
				case "sqsupseteq": return "⊒";
				case "sqcap":
				case "SquareIntersection": return "⊓";
				case "sqcup":
				case "SquareUnion": return "⊔";
				case "oplus":
				case "CirclePlus": return "⊕";
				case "ominus":
				case "CircleMinus": return "⊖";
				case "otimes":
				case "CircleTimes": return "⊗";
				case "osol": return "⊘";
				case "odot":
				case "CircleDot": return "⊙";
				case "ocir":
				case "circledcirc": return "⊚";
				case "oast":
				case "circledast": return "⊛";
				case "odash":
				case "circleddash": return "⊝";
				case "plusb":
				case "boxplus": return "⊞";
				case "minusb":
				case "boxminus": return "⊟";
				case "timesb":
				case "boxtimes": return "⊠";
				case "sdotb":
				case "dotsquare": return "⊡";
				case "vdash":
				case "RightTee": return "⊢";
				case "dashv":
				case "LeftTee": return "⊣";
				case "top":
				case "DownTee": return "⊤";
				case "bottom":
				case "bot":
				case "perp":
				case "UpTee": return "⊥";
				case "models": return "⊧";
				case "vDash":
				case "DoubleRightTee": return "⊨";
				case "Vdash": return "⊩";
				case "Vvdash": return "⊪";
				case "VDash": return "⊫";
				case "nvdash": return "⊬";
				case "nvDash": return "⊭";
				case "nVdash": return "⊮";
				case "nVDash": return "⊯";
				case "prurel": return "⊰";
				case "vltri":
				case "vartriangleleft":
				case "LeftTriangle": return "⊲";
				case "vrtri":
				case "vartriangleright":
				case "RightTriangle": return "⊳";
				case "ltrie":
				case "trianglelefteq":
				case "LeftTriangleEqual": return "⊴";
				case "rtrie":
				case "trianglerighteq":
				case "RightTriangleEqual": return "⊵";
				case "origof": return "⊶";
				case "imof": return "⊷";
				case "mumap":
				case "multimap": return "⊸";
				case "hercon": return "⊹";
				case "intcal":
				case "intercal": return "⊺";
				case "veebar": return "⊻";
				case "barvee": return "⊽";
				case "angrtvb": return "⊾";
				case "lrtri": return "⊿";
				case "xwedge":
				case "Wedge":
				case "bigwedge": return "⋀";
				case "xvee":
				case "Vee":
				case "bigvee": return "⋁";
				case "xcap":
				case "Intersection":
				case "bigcap": return "⋂";
				case "xcup":
				case "Union":
				case "bigcup": return "⋃";
				case "diam":
				case "diamond":
				case "Diamond": return "⋄";
				case "sdot": return "⋅";
				case "sstarf":
				case "Star": return "⋆";
				case "divonx":
				case "divideontimes": return "⋇";
				case "bowtie": return "⋈";
				case "ltimes": return "⋉";
				case "rtimes": return "⋊";
				case "lthree":
				case "leftthreetimes": return "⋋";
				case "rthree":
				case "rightthreetimes": return "⋌";
				case "bsime":
				case "backsimeq": return "⋍";
				case "cuvee":
				case "curlyvee": return "⋎";
				case "cuwed":
				case "curlywedge": return "⋏";
				case "Sub":
				case "Subset": return "⋐";
				case "Sup":
				case "Supset": return "⋑";
				case "Cap": return "⋒";
				case "Cup": return "⋓";
				case "fork":
				case "pitchfork": return "⋔";
				case "epar": return "⋕";
				case "ltdot":
				case "lessdot": return "⋖";
				case "gtdot":
				case "gtrdot": return "⋗";
				case "Ll": return "⋘";
				case "Gg":
				case "ggg": return "⋙";
				case "leg":
				case "LessEqualGreater":
				case "lesseqgtr": return "⋚";
				case "gel":
				case "gtreqless":
				case "GreaterEqualLess": return "⋛";
				case "cuepr":
				case "curlyeqprec": return "⋞";
				case "cuesc":
				case "curlyeqsucc": return "⋟";
				case "nprcue":
				case "NotPrecedesSlantEqual": return "⋠";
				case "nsccue":
				case "NotSucceedsSlantEqual": return "⋡";
				case "nsqsube":
				case "NotSquareSubsetEqual": return "⋢";
				case "nsqsupe":
				case "NotSquareSupersetEqual": return "⋣";
				case "lnsim": return "⋦";
				case "gnsim": return "⋧";
				case "prnsim":
				case "precnsim": return "⋨";
				case "scnsim":
				case "succnsim": return "⋩";
				case "nltri":
				case "ntriangleleft":
				case "NotLeftTriangle": return "⋪";
				case "nrtri":
				case "ntriangleright":
				case "NotRightTriangle": return "⋫";
				case "nltrie":
				case "ntrianglelefteq":
				case "NotLeftTriangleEqual": return "⋬";
				case "nrtrie":
				case "ntrianglerighteq":
				case "NotRightTriangleEqual": return "⋭";
				case "vellip": return "⋮";
				case "ctdot": return "⋯";
				case "utdot": return "⋰";
				case "dtdot": return "⋱";
				case "disin": return "⋲";
				case "isinsv": return "⋳";
				case "isins": return "⋴";
				case "isindot": return "⋵";
				case "notinvc": return "⋶";
				case "notinvb": return "⋷";
				case "isinE": return "⋹";
				case "nisd": return "⋺";
				case "xnis": return "⋻";
				case "nis": return "⋼";
				case "notnivc": return "⋽";
				case "notnivb": return "⋾";
				case "barwed":
				case "barwedge": return "⌅";
				case "Barwed":
				case "doublebarwedge": return "⌆";
				case "lceil":
				case "LeftCeiling": return "⌈";
				case "rceil":
				case "RightCeiling": return "⌉";
				case "lfloor":
				case "LeftFloor": return "⌊";
				case "rfloor":
				case "RightFloor": return "⌋";
				case "drcrop": return "⌌";
				case "dlcrop": return "⌍";
				case "urcrop": return "⌎";
				case "ulcrop": return "⌏";
				case "bnot": return "⌐";
				case "profline": return "⌒";
				case "profsurf": return "⌓";
				case "telrec": return "⌕";
				case "target": return "⌖";
				case "ulcorn":
				case "ulcorner": return "⌜";
				case "urcorn":
				case "urcorner": return "⌝";
				case "dlcorn":
				case "llcorner": return "⌞";
				case "drcorn":
				case "lrcorner": return "⌟";
				case "frown":
				case "sfrown": return "⌢";
				case "smile":
				case "ssmile": return "⌣";
				case "cylcty": return "⌭";
				case "profalar": return "⌮";
				case "topbot": return "⌶";
				case "ovbar": return "⌽";
				case "solbar": return "⌿";
				case "angzarr": return "⍼";
				case "lmoust":
				case "lmoustache": return "⎰";
				case "rmoust":
				case "rmoustache": return "⎱";
				case "tbrk":
				case "OverBracket": return "⎴";
				case "bbrk":
				case "UnderBracket": return "⎵";
				case "bbrktbrk": return "⎶";
				case "OverParenthesis": return "⏜";
				case "UnderParenthesis": return "⏝";
				case "OverBrace": return "⏞";
				case "UnderBrace": return "⏟";
				case "trpezium": return "⏢";
				case "elinters": return "⏧";
				case "blank": return "␣";
				case "oS":
				case "circledS": return "Ⓢ";
				case "boxh":
				case "HorizontalLine": return "─";
				case "boxv": return "│";
				case "boxdr": return "┌";
				case "boxdl": return "┐";
				case "boxur": return "└";
				case "boxul": return "┘";
				case "boxvr": return "├";
				case "boxvl": return "┤";
				case "boxhd": return "┬";
				case "boxhu": return "┴";
				case "boxvh": return "┼";
				case "boxH": return "═";
				case "boxV": return "║";
				case "boxdR": return "╒";
				case "boxDr": return "╓";
				case "boxDR": return "╔";
				case "boxdL": return "╕";
				case "boxDl": return "╖";
				case "boxDL": return "╗";
				case "boxuR": return "╘";
				case "boxUr": return "╙";
				case "boxUR": return "╚";
				case "boxuL": return "╛";
				case "boxUl": return "╜";
				case "boxUL": return "╝";
				case "boxvR": return "╞";
				case "boxVr": return "╟";
				case "boxVR": return "╠";
				case "boxvL": return "╡";
				case "boxVl": return "╢";
				case "boxVL": return "╣";
				case "boxHd": return "╤";
				case "boxhD": return "╥";
				case "boxHD": return "╦";
				case "boxHu": return "╧";
				case "boxhU": return "╨";
				case "boxHU": return "╩";
				case "boxvH": return "╪";
				case "boxVh": return "╫";
				case "boxVH": return "╬";
				case "uhblk": return "▀";
				case "lhblk": return "▄";
				case "block": return "█";
				case "blk14": return "░";
				case "blk12": return "▒";
				case "blk34": return "▓";
				case "squ":
				case "square":
				case "Square": return "□";
				case "squf":
				case "squarf":
				case "blacksquare":
				case "FilledVerySmallSquare": return "▪";
				case "EmptyVerySmallSquare": return "▫";
				case "rect": return "▭";
				case "marker": return "▮";
				case "fltns": return "▱";
				case "xutri":
				case "bigtriangleup": return "△";
				case "utrif":
				case "blacktriangle": return "▴";
				case "utri":
				case "triangle": return "▵";
				case "rtrif":
				case "blacktriangleright": return "▸";
				case "rtri":
				case "triangleright": return "▹";
				case "xdtri":
				case "bigtriangledown": return "▽";
				case "dtrif":
				case "blacktriangledown": return "▾";
				case "dtri":
				case "triangledown": return "▿";
				case "ltrif":
				case "blacktriangleleft": return "◂";
				case "ltri":
				case "triangleleft": return "◃";
				case "loz":
				case "lozenge": return "◊";
				case "cir": return "○";
				case "tridot": return "◬";
				case "xcirc":
				case "bigcirc": return "◯";
				case "ultri": return "◸";
				case "urtri": return "◹";
				case "lltri": return "◺";
				case "EmptySmallSquare": return "◻";
				case "FilledSmallSquare": return "◼";
				case "starf":
				case "bigstar": return "★";
				case "star": return "☆";
				case "phone": return "☎";
				case "female": return "♀";
				case "male": return "♂";
				case "spades":
				case "spadesuit": return "♠";
				case "clubs":
				case "clubsuit": return "♣";
				case "hearts":
				case "heartsuit": return "♥";
				case "diams":
				case "diamondsuit": return "♦";
				case "sung": return "♪";
				case "flat": return "♭";
				case "natur":
				case "natural": return "♮";
				case "sharp": return "♯";
				case "check":
				case "checkmark": return "✓";
				case "cross": return "✗";
				case "malt":
				case "maltese": return "✠";
				case "sext": return "✶";
				case "VerticalSeparator": return "❘";
				case "lbbrk": return "❲";
				case "rbbrk": return "❳";
				case "lobrk":
				case "LeftDoubleBracket": return "⟦";
				case "robrk":
				case "RightDoubleBracket": return "⟧";
				case "lang":
				case "LeftAngleBracket":
				case "langle": return "⟨";
				case "rang":
				case "RightAngleBracket":
				case "rangle": return "⟩";
				case "Lang": return "⟪";
				case "Rang": return "⟫";
				case "loang": return "⟬";
				case "roang": return "⟭";
				case "xlarr":
				case "longleftarrow":
				case "LongLeftArrow": return "⟵";
				case "xrarr":
				case "longrightarrow":
				case "LongRightArrow": return "⟶";
				case "xharr":
				case "longleftrightarrow":
				case "LongLeftRightArrow": return "⟷";
				case "xlArr":
				case "Longleftarrow":
				case "DoubleLongLeftArrow": return "⟸";
				case "xrArr":
				case "Longrightarrow":
				case "DoubleLongRightArrow": return "⟹";
				case "xhArr":
				case "Longleftrightarrow":
				case "DoubleLongLeftRightArrow": return "⟺";
				case "xmap":
				case "longmapsto": return "⟼";
				case "dzigrarr": return "⟿";
				case "nvlArr": return "⤂";
				case "nvrArr": return "⤃";
				case "nvHarr": return "⤄";
				case "Map": return "⤅";
				case "lbarr": return "⤌";
				case "rbarr":
				case "bkarow": return "⤍";
				case "lBarr": return "⤎";
				case "rBarr":
				case "dbkarow": return "⤏";
				case "RBarr":
				case "drbkarow": return "⤐";
				case "DDotrahd": return "⤑";
				case "UpArrowBar": return "⤒";
				case "DownArrowBar": return "⤓";
				case "Rarrtl": return "⤖";
				case "latail": return "⤙";
				case "ratail": return "⤚";
				case "lAtail": return "⤛";
				case "rAtail": return "⤜";
				case "larrfs": return "⤝";
				case "rarrfs": return "⤞";
				case "larrbfs": return "⤟";
				case "rarrbfs": return "⤠";
				case "nwarhk": return "⤣";
				case "nearhk": return "⤤";
				case "searhk":
				case "hksearow": return "⤥";
				case "swarhk":
				case "hkswarow": return "⤦";
				case "nwnear": return "⤧";
				case "nesear":
				case "toea": return "⤨";
				case "seswar":
				case "tosa": return "⤩";
				case "swnwar": return "⤪";
				case "rarrc": return "⤳";
				case "cudarrr": return "⤵";
				case "ldca": return "⤶";
				case "rdca": return "⤷";
				case "cudarrl": return "⤸";
				case "larrpl": return "⤹";
				case "curarrm": return "⤼";
				case "cularrp": return "⤽";
				case "rarrpl": return "⥅";
				case "harrcir": return "⥈";
				case "Uarrocir": return "⥉";
				case "lurdshar": return "⥊";
				case "ldrushar": return "⥋";
				case "LeftRightVector": return "⥎";
				case "RightUpDownVector": return "⥏";
				case "DownLeftRightVector": return "⥐";
				case "LeftUpDownVector": return "⥑";
				case "LeftVectorBar": return "⥒";
				case "RightVectorBar": return "⥓";
				case "RightUpVectorBar": return "⥔";
				case "RightDownVectorBar": return "⥕";
				case "DownLeftVectorBar": return "⥖";
				case "DownRightVectorBar": return "⥗";
				case "LeftUpVectorBar": return "⥘";
				case "LeftDownVectorBar": return "⥙";
				case "LeftTeeVector": return "⥚";
				case "RightTeeVector": return "⥛";
				case "RightUpTeeVector": return "⥜";
				case "RightDownTeeVector": return "⥝";
				case "DownLeftTeeVector": return "⥞";
				case "DownRightTeeVector": return "⥟";
				case "LeftUpTeeVector": return "⥠";
				case "LeftDownTeeVector": return "⥡";
				case "lHar": return "⥢";
				case "uHar": return "⥣";
				case "rHar": return "⥤";
				case "dHar": return "⥥";
				case "luruhar": return "⥦";
				case "ldrdhar": return "⥧";
				case "ruluhar": return "⥨";
				case "rdldhar": return "⥩";
				case "lharul": return "⥪";
				case "llhard": return "⥫";
				case "rharul": return "⥬";
				case "lrhard": return "⥭";
				case "udhar":
				case "UpEquilibrium": return "⥮";
				case "duhar":
				case "ReverseUpEquilibrium": return "⥯";
				case "RoundImplies": return "⥰";
				case "erarr": return "⥱";
				case "simrarr": return "⥲";
				case "larrsim": return "⥳";
				case "rarrsim": return "⥴";
				case "rarrap": return "⥵";
				case "ltlarr": return "⥶";
				case "gtrarr": return "⥸";
				case "subrarr": return "⥹";
				case "suplarr": return "⥻";
				case "lfisht": return "⥼";
				case "rfisht": return "⥽";
				case "ufisht": return "⥾";
				case "dfisht": return "⥿";
				case "lopar": return "⦅";
				case "ropar": return "⦆";
				case "lbrke": return "⦋";
				case "rbrke": return "⦌";
				case "lbrkslu": return "⦍";
				case "rbrksld": return "⦎";
				case "lbrksld": return "⦏";
				case "rbrkslu": return "⦐";
				case "langd": return "⦑";
				case "rangd": return "⦒";
				case "lparlt": return "⦓";
				case "rpargt": return "⦔";
				case "gtlPar": return "⦕";
				case "ltrPar": return "⦖";
				case "vzigzag": return "⦚";
				case "vangrt": return "⦜";
				case "angrtvbd": return "⦝";
				case "ange": return "⦤";
				case "range": return "⦥";
				case "dwangle": return "⦦";
				case "uwangle": return "⦧";
				case "angmsdaa": return "⦨";
				case "angmsdab": return "⦩";
				case "angmsdac": return "⦪";
				case "angmsdad": return "⦫";
				case "angmsdae": return "⦬";
				case "angmsdaf": return "⦭";
				case "angmsdag": return "⦮";
				case "angmsdah": return "⦯";
				case "bemptyv": return "⦰";
				case "demptyv": return "⦱";
				case "cemptyv": return "⦲";
				case "raemptyv": return "⦳";
				case "laemptyv": return "⦴";
				case "ohbar": return "⦵";
				case "omid": return "⦶";
				case "opar": return "⦷";
				case "operp": return "⦹";
				case "olcross": return "⦻";
				case "odsold": return "⦼";
				case "olcir": return "⦾";
				case "ofcir": return "⦿";
				case "olt": return "⧀";
				case "ogt": return "⧁";
				case "cirscir": return "⧂";
				case "cirE": return "⧃";
				case "solb": return "⧄";
				case "bsolb": return "⧅";
				case "boxbox": return "⧉";
				case "trisb": return "⧍";
				case "rtriltri": return "⧎";
				case "LeftTriangleBar": return "⧏";
				case "RightTriangleBar": return "⧐";
				case "race": return "⧚";
				case "iinfin": return "⧜";
				case "infintie": return "⧝";
				case "nvinfin": return "⧞";
				case "eparsl": return "⧣";
				case "smeparsl": return "⧤";
				case "eqvparsl": return "⧥";
				case "lozf":
				case "blacklozenge": return "⧫";
				case "RuleDelayed": return "⧴";
				case "dsol": return "⧶";
				case "xodot":
				case "bigodot": return "⨀";
				case "xoplus":
				case "bigoplus": return "⨁";
				case "xotime":
				case "bigotimes": return "⨂";
				case "xuplus":
				case "biguplus": return "⨄";
				case "xsqcup":
				case "bigsqcup": return "⨆";
				case "qint":
				case "iiiint": return "⨌";
				case "fpartint": return "⨍";
				case "cirfnint": return "⨐";
				case "awint": return "⨑";
				case "rppolint": return "⨒";
				case "scpolint": return "⨓";
				case "npolint": return "⨔";
				case "pointint": return "⨕";
				case "quatint": return "⨖";
				case "intlarhk": return "⨗";
				case "pluscir": return "⨢";
				case "plusacir": return "⨣";
				case "simplus": return "⨤";
				case "plusdu": return "⨥";
				case "plussim": return "⨦";
				case "plustwo": return "⨧";
				case "mcomma": return "⨩";
				case "minusdu": return "⨪";
				case "loplus": return "⨭";
				case "roplus": return "⨮";
				case "Cross": return "⨯";
				case "timesd": return "⨰";
				case "timesbar": return "⨱";
				case "smashp": return "⨳";
				case "lotimes": return "⨴";
				case "rotimes": return "⨵";
				case "otimesas": return "⨶";
				case "Otimes": return "⨷";
				case "odiv": return "⨸";
				case "triplus": return "⨹";
				case "triminus": return "⨺";
				case "tritime": return "⨻";
				case "iprod":
				case "intprod": return "⨼";
				case "amalg": return "⨿";
				case "capdot": return "⩀";
				case "ncup": return "⩂";
				case "ncap": return "⩃";
				case "capand": return "⩄";
				case "cupor": return "⩅";
				case "cupcap": return "⩆";
				case "capcup": return "⩇";
				case "cupbrcap": return "⩈";
				case "capbrcup": return "⩉";
				case "cupcup": return "⩊";
				case "capcap": return "⩋";
				case "ccups": return "⩌";
				case "ccaps": return "⩍";
				case "ccupssm": return "⩐";
				case "And": return "⩓";
				case "Or": return "⩔";
				case "andand": return "⩕";
				case "oror": return "⩖";
				case "orslope": return "⩗";
				case "andslope": return "⩘";
				case "andv": return "⩚";
				case "orv": return "⩛";
				case "andd": return "⩜";
				case "ord": return "⩝";
				case "wedbar": return "⩟";
				case "sdote": return "⩦";
				case "simdot": return "⩪";
				case "congdot": return "⩭";
				case "easter": return "⩮";
				case "apacir": return "⩯";
				case "apE": return "⩰";
				case "eplus": return "⩱";
				case "pluse": return "⩲";
				case "Esim": return "⩳";
				case "Colone": return "⩴";
				case "Equal": return "⩵";
				case "eDDot":
				case "ddotseq": return "⩷";
				case "equivDD": return "⩸";
				case "ltcir": return "⩹";
				case "gtcir": return "⩺";
				case "ltquest": return "⩻";
				case "gtquest": return "⩼";
				case "les":
				case "LessSlantEqual":
				case "leqslant": return "⩽";
				case "ges":
				case "GreaterSlantEqual":
				case "geqslant": return "⩾";
				case "lesdot": return "⩿";
				case "gesdot": return "⪀";
				case "lesdoto": return "⪁";
				case "gesdoto": return "⪂";
				case "lesdotor": return "⪃";
				case "gesdotol": return "⪄";
				case "lap":
				case "lessapprox": return "⪅";
				case "gap":
				case "gtrapprox": return "⪆";
				case "lne":
				case "lneq": return "⪇";
				case "gne":
				case "gneq": return "⪈";
				case "lnap":
				case "lnapprox": return "⪉";
				case "gnap":
				case "gnapprox": return "⪊";
				case "lEg":
				case "lesseqqgtr": return "⪋";
				case "gEl":
				case "gtreqqless": return "⪌";
				case "lsime": return "⪍";
				case "gsime": return "⪎";
				case "lsimg": return "⪏";
				case "gsiml": return "⪐";
				case "lgE": return "⪑";
				case "glE": return "⪒";
				case "lesges": return "⪓";
				case "gesles": return "⪔";
				case "els":
				case "eqslantless": return "⪕";
				case "egs":
				case "eqslantgtr": return "⪖";
				case "elsdot": return "⪗";
				case "egsdot": return "⪘";
				case "el": return "⪙";
				case "eg": return "⪚";
				case "siml": return "⪝";
				case "simg": return "⪞";
				case "simlE": return "⪟";
				case "simgE": return "⪠";
				case "LessLess": return "⪡";
				case "GreaterGreater": return "⪢";
				case "glj": return "⪤";
				case "gla": return "⪥";
				case "ltcc": return "⪦";
				case "gtcc": return "⪧";
				case "lescc": return "⪨";
				case "gescc": return "⪩";
				case "smt": return "⪪";
				case "lat": return "⪫";
				case "smte": return "⪬";
				case "late": return "⪭";
				case "bumpE": return "⪮";
				case "pre":
				case "preceq":
				case "PrecedesEqual": return "⪯";
				case "sce":
				case "succeq":
				case "SucceedsEqual": return "⪰";
				case "prE": return "⪳";
				case "scE": return "⪴";
				case "prnE":
				case "precneqq": return "⪵";
				case "scnE":
				case "succneqq": return "⪶";
				case "prap":
				case "precapprox": return "⪷";
				case "scap":
				case "succapprox": return "⪸";
				case "prnap":
				case "precnapprox": return "⪹";
				case "scnap":
				case "succnapprox": return "⪺";
				case "Pr": return "⪻";
				case "Sc": return "⪼";
				case "subdot": return "⪽";
				case "supdot": return "⪾";
				case "subplus": return "⪿";
				case "supplus": return "⫀";
				case "submult": return "⫁";
				case "supmult": return "⫂";
				case "subedot": return "⫃";
				case "supedot": return "⫄";
				case "subE":
				case "subseteqq": return "⫅";
				case "supE":
				case "supseteqq": return "⫆";
				case "subsim": return "⫇";
				case "supsim": return "⫈";
				case "subnE":
				case "subsetneqq": return "⫋";
				case "supnE":
				case "supsetneqq": return "⫌";
				case "csub": return "⫏";
				case "csup": return "⫐";
				case "csube": return "⫑";
				case "csupe": return "⫒";
				case "subsup": return "⫓";
				case "supsub": return "⫔";
				case "subsub": return "⫕";
				case "supsup": return "⫖";
				case "suphsub": return "⫗";
				case "supdsub": return "⫘";
				case "forkv": return "⫙";
				case "topfork": return "⫚";
				case "mlcp": return "⫛";
				case "Dashv":
				case "DoubleLeftTee": return "⫤";
				case "Vdashl": return "⫦";
				case "Barv": return "⫧";
				case "vBar": return "⫨";
				case "vBarv": return "⫩";
				case "Vbar": return "⫫";
				case "Not": return "⫬";
				case "bNot": return "⫭";
				case "rnmid": return "⫮";
				case "cirmid": return "⫯";
				case "midcir": return "⫰";
				case "topcir": return "⫱";
				case "nhpar": return "⫲";
				case "parsim": return "⫳";
				case "parsl": return "⫽";
				case "fflig": return "ﬀ";
				case "filig": return "ﬁ";
				case "fllig": return "ﬂ";
				case "ffilig": return "ﬃ";
				case "ffllig": return "ﬄ";
				case "Ascr": return "𝒜";
				case "Cscr": return "𝒞";
				case "Dscr": return "𝒟";
				case "Gscr": return "𝒢";
				case "Jscr": return "𝒥";
				case "Kscr": return "𝒦";
				case "Nscr": return "𝒩";
				case "Oscr": return "𝒪";
				case "Pscr": return "𝒫";
				case "Qscr": return "𝒬";
				case "Sscr": return "𝒮";
				case "Tscr": return "𝒯";
				case "Uscr": return "𝒰";
				case "Vscr": return "𝒱";
				case "Wscr": return "𝒲";
				case "Xscr": return "𝒳";
				case "Yscr": return "𝒴";
				case "Zscr": return "𝒵";
				case "ascr": return "𝒶";
				case "bscr": return "𝒷";
				case "cscr": return "𝒸";
				case "dscr": return "𝒹";
				case "fscr": return "𝒻";
				case "hscr": return "𝒽";
				case "iscr": return "𝒾";
				case "jscr": return "𝒿";
				case "kscr": return "𝓀";
				case "lscr": return "𝓁";
				case "mscr": return "𝓂";
				case "nscr": return "𝓃";
				case "pscr": return "𝓅";
				case "qscr": return "𝓆";
				case "rscr": return "𝓇";
				case "sscr": return "𝓈";
				case "tscr": return "𝓉";
				case "uscr": return "𝓊";
				case "vscr": return "𝓋";
				case "wscr": return "𝓌";
				case "xscr": return "𝓍";
				case "yscr": return "𝓎";
				case "zscr": return "𝓏";
				case "Afr": return "𝔄";
				case "Bfr": return "𝔅";
				case "Dfr": return "𝔇";
				case "Efr": return "𝔈";
				case "Ffr": return "𝔉";
				case "Gfr": return "𝔊";
				case "Jfr": return "𝔍";
				case "Kfr": return "𝔎";
				case "Lfr": return "𝔏";
				case "Mfr": return "𝔐";
				case "Nfr": return "𝔑";
				case "Ofr": return "𝔒";
				case "Pfr": return "𝔓";
				case "Qfr": return "𝔔";
				case "Sfr": return "𝔖";
				case "Tfr": return "𝔗";
				case "Ufr": return "𝔘";
				case "Vfr": return "𝔙";
				case "Wfr": return "𝔚";
				case "Xfr": return "𝔛";
				case "Yfr": return "𝔜";
				case "afr": return "𝔞";
				case "bfr": return "𝔟";
				case "cfr": return "𝔠";
				case "dfr": return "𝔡";
				case "efr": return "𝔢";
				case "ffr": return "𝔣";
				case "gfr": return "𝔤";
				case "hfr": return "𝔥";
				case "ifr": return "𝔦";
				case "jfr": return "𝔧";
				case "kfr": return "𝔨";
				case "lfr": return "𝔩";
				case "mfr": return "𝔪";
				case "nfr": return "𝔫";
				case "ofr": return "𝔬";
				case "pfr": return "𝔭";
				case "qfr": return "𝔮";
				case "rfr": return "𝔯";
				case "sfr": return "𝔰";
				case "tfr": return "𝔱";
				case "ufr": return "𝔲";
				case "vfr": return "𝔳";
				case "wfr": return "𝔴";
				case "xfr": return "𝔵";
				case "yfr": return "𝔶";
				case "zfr": return "𝔷";
				case "Aopf": return "𝔸";
				case "Bopf": return "𝔹";
				case "Dopf": return "𝔻";
				case "Eopf": return "𝔼";
				case "Fopf": return "𝔽";
				case "Gopf": return "𝔾";
				case "Iopf": return "𝕀";
				case "Jopf": return "𝕁";
				case "Kopf": return "𝕂";
				case "Lopf": return "𝕃";
				case "Mopf": return "𝕄";
				case "Oopf": return "𝕆";
				case "Sopf": return "𝕊";
				case "Topf": return "𝕋";
				case "Uopf": return "𝕌";
				case "Vopf": return "𝕍";
				case "Wopf": return "𝕎";
				case "Xopf": return "𝕏";
				case "Yopf": return "𝕐";
				case "aopf": return "𝕒";
				case "bopf": return "𝕓";
				case "copf": return "𝕔";
				case "dopf": return "𝕕";
				case "eopf": return "𝕖";
				case "fopf": return "𝕗";
				case "gopf": return "𝕘";
				case "hopf": return "𝕙";
				case "iopf": return "𝕚";
				case "jopf": return "𝕛";
				case "kopf": return "𝕜";
				case "lopf": return "𝕝";
				case "mopf": return "𝕞";
				case "nopf": return "𝕟";
				case "oopf": return "𝕠";
				case "popf": return "𝕡";
				case "qopf": return "𝕢";
				case "ropf": return "𝕣";
				case "sopf": return "𝕤";
				case "topf": return "𝕥";
				case "uopf": return "𝕦";
				case "vopf": return "𝕧";
				case "wopf": return "𝕨";
				case "xopf": return "𝕩";
				case "yopf": return "𝕪";
				case "zopf": return "𝕫";
				default: return null;
			}
		}

	}
}
