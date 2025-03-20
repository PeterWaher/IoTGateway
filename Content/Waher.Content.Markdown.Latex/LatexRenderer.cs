using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Images;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.BlockElements;
using Waher.Content.Markdown.Model.Multimedia;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Markdown.Rendering;
using Waher.Events;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Script.Objects.Matrices;
using Waher.Script.Operators.Matrices;

namespace Waher.Content.Markdown.Latex
{
	/// <summary>
	/// Renders LaTeX from a Markdown document.
	/// </summary>
	public class LatexRenderer : Renderer
	{
		/// <summary>
		/// LaTeX settings.
		/// </summary>
		public readonly LaTeXSettings LatexSettings;

		private bool inAbstract = false;
		private bool abstractOutput = false;

		/// <summary>
		/// Renders LaTeX from a Markdown document.
		/// </summary>
		/// <param name="LatexSettings">LaTeX settings.</param>
		public LatexRenderer(LaTeXSettings LatexSettings)
			: base()
		{
			this.LatexSettings = LatexSettings;
		}

		/// <summary>
		/// Renders LaTeX from a Markdown document.
		/// </summary>
		/// <param name="Output">LaTeX output.</param>
		/// <param name="LatexSettings">LaTeX settings.</param>
		public LatexRenderer(StringBuilder Output, LaTeXSettings LatexSettings)
			: base(Output)
		{
			this.LatexSettings = LatexSettings;
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentHeader()
		{
			bool MakeTitle = false;

			this.Output.Append("\\documentclass[");
			this.Output.Append(this.LatexSettings.DefaultFontSize.ToString());
			this.Output.Append("pt, ");

			switch (this.LatexSettings.PaperFormat)
			{
				case LaTeXPaper.A4:
				default:
					this.Output.Append("a4paper");
					break;

				case LaTeXPaper.Letter:
					this.Output.Append("letterpaper");
					break;
			}

			this.Output.Append("]{");
			switch (this.LatexSettings.DocumentClass)
			{
				case LaTeXDocumentClass.Article:
				default:
					this.Output.Append("article");
					break;

				case LaTeXDocumentClass.Report:
					this.Output.Append("report");
					break;

				case LaTeXDocumentClass.Book:
					this.Output.Append("book");
					break;

				case LaTeXDocumentClass.Standalone:
					this.Output.Append("standalone");
					break;
			}
			this.Output.AppendLine("}");

			// Strike-out cf. https://tex.stackexchange.com/questions/546884/strikethrough-command-in-latex
			this.Output.AppendLine("\\newlength{\\wdth}");
			this.Output.AppendLine("\\newcommand{\\strike}[1]{\\settowidth{\\wdth}{#1}\\rlap{\\rule[.5ex]{\\wdth}{.4pt}}#1}");

			if (this.Document.TryGetMetaData("TITLE", out KeyValuePair<string, bool>[] Values))
			{
				MakeTitle = true;

				foreach (KeyValuePair<string, bool> P in Values)
				{
					this.Output.Append("\\title{");
					this.Output.Append(P.Key);
					this.Output.AppendLine("}");
				}

				if (this.Document.TryGetMetaData("SUBTITLE", out Values))
				{
					foreach (KeyValuePair<string, bool> P2 in Values)
					{
						this.Output.Append("\\subtitle{");
						this.Output.Append(P2.Key);
						this.Output.AppendLine("}");
					}
				}
			}

			if (this.Document.TryGetMetaData("AUTHOR", out Values))
			{
				MakeTitle = true;

				bool First = true;

				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (First)
					{
						this.Output.AppendLine("\\author{");
						First = false;
					}
					else
						this.Output.AppendLine("\\and");

					this.Output.AppendLine(P.Key);
				}

				this.Output.AppendLine("}");
			}

			if (this.Document.TryGetMetaData("DATE", out Values))
			{
				MakeTitle = true;

				foreach (KeyValuePair<string, bool> P in Values)
				{
					this.Output.Append("\\date{");
					this.Output.Append(P.Key);
					this.Output.AppendLine("}");
				}
			}

			// Todo-lists in LaTeX, cf. https://tex.stackexchange.com/questions/247681/how-to-create-checkbox-todo-list

			this.Output.AppendLine("\\usepackage[utf8]{inputenc}");
			this.Output.AppendLine("\\usepackage[T1]{fontenc}");
			this.Output.AppendLine("\\usepackage{enumitem}");
			this.Output.AppendLine("\\usepackage{amsmath}");
			this.Output.AppendLine("\\usepackage{amssymb}");

			this.Output.Append("\\usepackage");
			if (!string.IsNullOrEmpty(this.LatexSettings.Language))
			{
				this.Output.Append("[");
				this.Output.Append(EscapeLaTeX(this.LatexSettings.Language));
				this.Output.Append("]");
			}
			this.Output.AppendLine("{babel}");

			this.Output.AppendLine("\\usepackage{graphicx}");
			this.Output.AppendLine("\\usepackage{pifont}");
			this.Output.AppendLine("\\usepackage{multirow}");
			this.Output.AppendLine("\\usepackage{ragged2e}");
			this.Output.AppendLine("\\usepackage{textcomp}");
			this.Output.AppendLine("\\newlist{tasklist}{itemize}{2}");
			this.Output.AppendLine("\\setlist[tasklist]{label=$\\square$}");
			this.Output.AppendLine("\\newcommand{\\checkmarksymbol}{\\ding{51}}");
			this.Output.AppendLine("\\newcommand{\\checked}{\\rlap{$\\square$}{\\raisebox{2pt}{\\large\\hspace{1pt}\\checkmarksymbol}}\\hspace{-2.5pt}}");
			this.Output.AppendLine("\\begin{document}");

			if (MakeTitle)
				this.Output.AppendLine("\\maketitle");

			this.inAbstract = false;
			this.abstractOutput = false;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders the document header.
		/// </summary>
		public override Task RenderDocumentFooter()
		{
			if (this.inAbstract)
			{
				this.Output.AppendLine("\\end{abstract}");
				this.inAbstract = false;
			}

			this.Output.AppendLine("\\end{document}");

			return Task.CompletedTask;
		}

		/// <summary>
		/// Escapes text for output in a LaTeX document.
		/// </summary>
		/// <param name="s">String to escape.</param>
		/// <returns>Escaped string.</returns>
		public static string EscapeLaTeX(string s)
		{
			return CommonTypes.Escape(s, latexCharactersToEscape, latexCharacterEscapes);
		}

		private static readonly char[] latexCharactersToEscape = new char[]
		{
			'\\',
			'#',
			'$',
			'%',
			'&',
			'{',
			'}',
			'_',
			'~',
			'^'
		};

		private static readonly string[] latexCharacterEscapes = new string[]
		{
			"\\textbackslash",
			"\\#",
			"\\$",
			"\\%",
			"\\&",
			"\\{",
			"\\}",
			"\\_",
			"\\textasciitilde",
			"\\textasciicircum"
		};

		#region Span Elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Abbreviation Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkMail Element)
		{
			this.Output.AppendLine(EscapeLaTeX(Element.EMail));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(AutomaticLinkUrl Element)
		{
			this.Output.AppendLine(EscapeLaTeX(Element.URL));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Delete Element)
		{
			this.Output.Append("\\strike{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DetailsReference Element)
		{
			if (!(this.Document.Detail is null))
				return this.RenderDocument(this.Document.Detail, true);
			else
				return this.Render((MetaReference)Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(EmojiReference Element)
		{
			this.Output.AppendLine(EscapeLaTeX(Element.Emoji.Unicode));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Emphasize Element)
		{
			this.Output.Append("\\emph{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(FootnoteReference Element)
		{
			if (this.Document?.TryGetFootnote(Element.Key, out Footnote Footnote) ?? false)
			{
				if (Element.AutoExpand)
					await this.Render(Footnote);
				else
				{
					this.Output.Append("\\footnote");

					if (this.Document?.TryGetFootnoteNumber(Element.Key, out int Nr) ?? false)
					{
						this.Output.Append('[');
						this.Output.Append(Nr.ToString());
						this.Output.Append(']');
					}

					this.Output.Append('{');
					await this.Render(Footnote);
					this.Output.Append('}');
				}
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HashTag Element)
		{
			this.Output.Append("\\#");
			this.Output.Append(Element.Tag);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntity Element)
		{
			string s = GetLaTeXEscapeCommand(Element.Entity);
			if (string.IsNullOrEmpty(s))
				this.Output.Append(EscapeLaTeX(Html.HtmlEntity.EntityToCharacter(Element.Entity)));
			else if (IsEquationCommand(s))
			{
				this.Output.Append('$');
				this.Output.Append(s);
				this.Output.Append('$');
			}
			else if (AllSymbols(s))
				this.Output.Append(s);
			else
			{
				this.Output.Append('{');
				this.Output.Append(s);
				this.Output.Append('}');
			}

			return Task.CompletedTask;
		}

		private static bool AllSymbols(string Command)
		{
			foreach (char ch in Command)
			{
				if (!char.IsLetter(ch) || char.IsDigit(ch))
					return false;
			}

			return true;
		}

		private static bool IsEquationCommand(string Command)
		{
			switch (Command)
			{
				// Relational Operators
				case "\\leq":
				case "\\geq":
				case "\\neq":
				case "\\approx":
				case "\\equiv":
				case "\\propto":
				case "\\sim":
				case "\\simeq":
				case "\\cong":
				case "\\asymp":
				case "\\doteq":
				case "\\bowtie":
				case "\\models":
				case "\\perp":
				case "\\parallel":
				case "\\nparallel":
				case "\\subset":
				case "\\supset":
				case "\\subseteq":
				case "\\supseteq":
				case "\\nsubseteq":
				case "\\nsupseteq":
				case "\\subsetneq":
				case "\\supsetneq":
				case "\\in":
				case "\\ni":
				case "\\notin":
				case "\\vdash":
				case "\\dashv":
				case "\\Vdash":
				case "\\vDash":
				case "\\nvdash":
				case "\\nvDash":
				case "\\nVdash":
				case "\\nVDash":

				// Arithmetic Operators
				case "\\pm":
				case "\\mp":
				case "\\times":
				case "\\div":
				case "\\cdot":
				case "\\ast":
				case "\\star":
				case "\\circ":
				case "\\bullet":
				case "\\oplus":
				case "\\ominus":
				case "\\otimes":
				case "\\oslash":
				case "\\odot":
				case "\\bigoplus":
				case "\\bigotimes":
				case "\\bigodot":
				case "\\sum":
				case "\\prod":
				case "\\coprod":
				case "\\int":
				case "\\iint":
				case "\\iiint":
				case "\\oint":
				case "\\bigcup":
				case "\\bigcap":
				case "\\bigsqcup":
				case "\\biguplus":
				case "\\bigvee":
				case "\\bigwedge":

				// Greek Letters
				case "\\alpha":
				case "\\beta":
				case "\\gamma":
				case "\\delta":
				case "\\epsilon":
				case "\\zeta":
				case "\\eta":
				case "\\theta":
				case "\\iota":
				case "\\kappa":
				case "\\lambda":
				case "\\mu":
				case "\\nu":
				case "\\xi":
				case "\\pi":
				case "\\rho":
				case "\\sigma":
				case "\\tau":
				case "\\upsilon":
				case "\\phi":
				case "\\chi":
				case "\\psi":
				case "\\omega":
				case "\\Gamma":
				case "\\Delta":
				case "\\Theta":
				case "\\Lambda":
				case "\\Xi":
				case "\\Pi":
				case "\\Sigma":
				case "\\Upsilon":
				case "\\Phi":
				case "\\Psi":
				case "\\Omega":

				// Miscellaneous Symbols
				case "\\infty":
				case "\\partial":
				case "\\nabla":
				case "\\forall":
				case "\\exists":
				case "\\neg":
				case "\\land":
				case "\\lor":
				case "\\Rightarrow":
				case "\\Leftarrow":
				case "\\Leftrightarrow":
				case "\\rightarrow":
				case "\\leftarrow":
				case "\\leftrightarrow":
				case "\\uparrow":
				case "\\downarrow":
				case "\\updownarrow":
				case "\\Uparrow":
				case "\\Downarrow":
				case "\\Updownarrow":
				case "\\square":
				case "\\blacksquare":
				case "\\triangle":
				case "\\blacktriangle":
				case "\\diamond":
				case "\\blacklozenge":
				case "\\lozenge":
				case "\\angle":
				case "\\measuredangle":
				case "\\sphericalangle":
				case "\\perthousand":
				case "\\degree":
				case "\\prime":
				case "\\doubleprime":
				case "\\backslash":
				case "\\setminus":
				case "\\cup":
				case "\\cap":
				case "\\uplus":
				case "\\sqcup":
				case "\\sqcap":
				case "\\vee":
				case "\\wedge":
				case "\\cdots":
				case "\\vdots":
				case "\\ddots":
				case "\\aleph":
				case "\\beth":
				case "\\gimel":
				case "\\daleth":
				case "\\hbar":
				case "\\imath":
				case "\\jmath":
				case "\\ell":
				case "\\wp":
				case "\\Re":
				case "\\Im":
				case "\\mho":
				case "\\eth":
				case "\\textmu":
				case "\\textpm":
				case "\\textdiv":
				case "\\texttimes":
				case "\\textdegree":
				case "\\textasciicircum":
				case "\\textasciitilde":
				case "\\textbackslash":
				case "\\textbar":
				case "\\textbraceleft":
				case "\\textbraceright":
				case "\\textbullet":
				case "\\textdagger":
				case "\\textdaggerdbl":
				case "\\textellipsis":
				case "\\textemdash":
				case "\\textendash":
				case "\\textgreater":
				case "\\textless":
				case "\\textquotedblleft":
				case "\\textquotedblright":
				case "\\textquoteleft":
				case "\\textquoteright":
				case "\\textsection":
				case "\\textsterling":
				case "\\textunderscore":
				case "\\textvisiblespace":
				case "\\textcent":
				case "\\textcurrency":
				case "\\textyen":
				case "\\textnumero":
				case "\\textcircledP":
				case "\\textregistered":
				case "\\texttrademark":
				case "\\textcopyright":
				case "\\textordfeminine":
				case "\\textordmasculine":
				case "\\textparagraph":
					return true;

				default:
					return false;
			}
		}

		private static string GetLaTeXEscapeCommand(string Entity)
		{
			// Ref (with alterations): https://gist.github.com/adityam/1431606

			switch (Entity)
			{
				case "cups": return "\\cup";
				case "boxplus": return "\\boxplus";
				case "DoubleLongLeftRightArrow": return "\\Longleftrightarrow";
				case "szlig": return "\\ssharp";
				case "xoplus": return "\\bigoplus";
				case "ensp": return "\\space";
				case "upharpoonleft": return "\\upharpoonleft";
				case "ClockwiseContourIntegral": return "\\ointclockwise";
				case "THgr": return "\\greekTheta";
				case "nbsp": return "\\breakspace";
				case "YUcy": return "\\  cyrillicYU";
				case "excl": return "\\ot defined";
				case "GreaterLess": return "\\gtrless";
				case "PHgr": return "\\greekPhi";
				case "OHgr": return "\\  greekOmega";
				case "KHgr": return "\\  greekChi";
				case "NotExists": return "\\nexists";
				case "lne": return "\\lneq";
				case "prnE": return "\\precneqq";
				case "eqsim": return "\\eqsim";
				case "bne": return "\\eq";
				case "prE": return "\\preceqq";
				case "gne": return "\\rneq";
				case "Vdash": return "\\Vdash";
				case "plusmn": return "\\textpm";
				case "nexist": return "\\nexists";
				case "succneqq": return "\\succneqq";
				case "nsubseteq": return "\\nsubseteq";
				case "leftarrow": return "\\leftarrow";
				case "nges": return "\\geqslant";
				case "TScy": return "\\cyrillicC";
				case "NotLeftTriangleEqual": return "\\ntrianglelefteq";
				case "lowast": return "\\ast";
				case "gneqq": return "\\gneqq";
				case "rang": return "\\rangle";
				case "plusdo": return "\\dotplus";
				case "lneqq": return "\\lneqq";
				case "nang": return "\\angle";
				case "lang": return "\\langle";
				case "NotLessGreater": return "\\nlessgtr";
				case "ngeqslant": return "\\geqslant";
				case "Leftarrow": return "\\Leftarrow";
				case "DoubleLongRightArrow": return "\\Longrightarrow";
				case "gtreqqless": return "\\gtreqqless";
				case "npre": return "\\preceq";
				case "Rang": return "\\rrangle";
				case "DScy": return "\\  cyrillicDZE";
				case "Lang": return "\\llangle";
				case "emsp": return "\\space";
				case "infin": return "\\infty";
				case "leqq": return "\\leqq";
				case "cent": return "\\textcent";
				case "PlusMinus": return "\\textpm";
				case "nleqq": return "\\leqq";
				case "dotplus": return "\\dotplus";
				case "PrecedesSlantEqual": return "\\preccurlyeq";
				case "gtrsim": return "\\gtrsim";
				case "lvnE": return "\\lneqq";
				case "TRADE": return "\\trademark";
				case "brvbar": return "\\textbrokenbar";
				case "rightsquigarrow": return "\\rightwavearrow";
				case "DownArrow": return "\\downarrow";
				case "nVDash": return "\\nVDash";
				case "Hat": return "\\textasciicircum";
				case "sup3": return "\\threesuperior";
				case "EEgr": return "\\greekEta";
				case "bepsi": return "\\backepsilon";
				case "gnsim": return "\\gnsim";
				case "upharpoonright": return "\\upharpoonright";
				case "lnsim": return "\\lnsim";
				case "Sum": return "\\sum";
				case "gvnE": return "\\gneqq";
				case "prnsim": return "\\precnsim";
				case "fork": return "\\pitchfork";
				case "Vee": return "\\bigvee";
				case "NotPrecedes": return "\\nprec";
				case "Wedge": return "\\bigwedge";
				case "rightharpoondown": return "\\rightharpoondown";
				case "ETH": return "\\Eth";
				case "eqslantgtr": return "\\eqslantgtr";
				case "vee": return "\\vee";
				case "supsetneqq": return "\\supsetneqq";
				case "PrecedesTilde": return "\\precsim";
				case "UpperRightArrow": return "\\nearrow";
				case "ngE": return "\\geqq";
				case "Downarrow": return "\\Downarrow";
				case "NegativeVeryThinSpace": return "\\zerowidthspace";
				case "isinv": return "\\in";
				case "Sqrt": return "\\surd";
				case "xuplus": return "\\biguplus";
				case "sum": return "\\sum";
				case "ngeqq": return "\\geqq";
				case "lAarr": return "\\Lleftarrow";
				case "Phi": return "\\greekPhi";
				case "downarrow": return "\\downarrow";
				case "prsim": return "\\precsim";
				case "cap": return "\\cap";
				case "rAarr": return "\\Rrightarrow";
				case "ring": return "\\textring";
				case "map": return "\\mapsto";
				case "nap": return "\\napprox";
				case "lap": return "\\lessapprox";
				case "vdash": return "\\vdash";
				case "gap": return "\\gtrapprox";
				case "Chi": return "\\greekChi";
				case "hksearow": return "\\lhooksearrow";
				case "Idigr": return "\\greekIotadialytika";
				case "trade": return "\\trademark";
				case "mdash": return "\\emdash";
				case "ndash": return "\\endash";
				case "odash": return "\\circleddash";
				case "mapstoleft": return "\\mapsfrom";
				case "DD": return "\\differentialD";
				case "Bumpeq": return "\\Bumpeq";
				case "Cap": return "\\Cap";
				case "curlyvee": return "\\curlyvee";
				case "coloneq": return "\\colonequals";
				case "kappa": return "\\  greekkappa";
				case "mapsto": return "\\mapsto";
				case "chi": return "\\greekchi";
				case "nlE": return "\\leqq";
				case "nlsim": return "\\nlesssim";
				case "inodot": return "\\dotlessi";
				case "sharp": return "\\sharp";
				case "Omicron": return "\\  greekOmicron";
				case "therefore": return "\\therefore";
				case "udigr": return "\\greekupsilondiaeresis";
				case "mnplus": return "\\mp";
				case "wedge": return "\\wedge";
				case "Kappa": return "\\greekKappa";
				case "supset": return "\\supset";
				case "lessdot": return "\\lessdot";
				case "CounterClockwiseContourIntegral": return "\\ointctrclockwise";
				case "bigwedge": return "\\bigwedge";
				case "eeacgr": return "\\greeketatonos";
				case "NotSubsetEqual": return "\\nsubseteq";
				case "lE": return "\\leqq";
				case "Therefore": return "\\therefore";
				case "Udigr": return "\\greekUpsilondialytika";
				case "gE": return "\\geqq";
				case "gnE": return "\\gneqq";
				case "vnsup": return "\\supset";
				case "Supset": return "\\Supset";
				case "lnE": return "\\lneqq";
				case "idigr": return "\\greekiotadialytika";
				case "hkswarow": return "\\rhookswarrow";
				case "Copf": return "\\  complexes";
				case "Nopf": return "\\  naturalnumbers";
				case "Ropf": return "\\  reals";
				case "Popf": return "\\  primes";
				case "Qopf": return "\\rationals";
				case "Otilde": return "\\  Otilde";
				case "oS": return "\\circledS";
				case "para": return "\\paragraphmark";
				case "utilde": return "\\  utilde";
				case "NotPrecedesSlantEqual": return "\\npreccurlyeq";
				case "bigotimes": return "\\bigotimes";
				case "LeftVector": return "\\leftharpoonup";
				case "geq": return "\\geq";
				case "checkmark": return "\\checkmark";
				case "leq": return "\\leq";
				case "scnE": return "\\succneqq";
				case "Conint": return "\\oiint";
				case "RightVector": return "\\rightharpoonup";
				case "ffllig": return "\\fflligature";
				case "bowtie": return "\\bowtie";
				case "nrightarrow": return "\\nrightarrow";
				case "UpArrowDownArrow": return "\\updownarrows";
				case "veeeq": return "\\veeeq";
				case "Zopf": return "\\integers";
				case "Itilde": return "\\Itilde";
				case "nearhk": return "\\rhooknearrow";
				case "PartialD": return "\\partial";
				case "Atilde": return "\\Atilde";
				case "Rrightarrow": return "\\Rrightarrow";
				case "Succeeds": return "\\succ";
				case "Auml": return "\\Adiaeresis";
				case "nhArr": return "\\nLeftrightarrow";
				case "upsi": return "\\greekupsilon";
				case "xhArr": return "\\Longleftrightarrow";
				case "Ouml": return "\\Odiaeresis";
				case "quot": return "\\quotedbl";
				case "epsi": return "\\greekepsilon";
				case "Iuml": return "\\Idiaeresis";
				case "Euml": return "\\Ediaeresis";
				case "precsim": return "\\precsim";
				case "notni": return "\\nni";
				case "nvdash": return "\\nvdash";
				case "notinva": return "\\nin";
				case "backepsilon": return "\\backepsilon";
				case "Rightarrow": return "\\Rightarrow";
				case "LeftTeeArrow": return "\\mapsfrom";
				case "OverParenthesis": return "\\overparent";
				case "NotCongruent": return "\\nequiv";
				case "ohacgr": return "\\greekomegatonos";
				case "Assign": return "\\colonequals";
				case "rhov": return "\\greekrhoalt";
				case "yuml": return "\\ydiaeresis";
				case "cuepr": return "\\curlyeqprec";
				case "uuml": return "\\udiaeresis";
				case "lsqb": return "\\lbrack";
				case "rsqb": return "\\rbrack";
				case "rbrack": return "\\rbrack";
				case "lbrack": return "\\lbrack";
				case "efDot": return "\\fallingdotseq";
				case "pre": return "\\preceq";
				case "auml": return "\\adiaeresis";
				case "Intersection": return "\\bigcap";
				case "DoubleRightArrow": return "\\Rightarrow";
				case "Yuml": return "\\Ydiaeresis";
				case "Uuml": return "\\Udiaeresis";
				case "ouml": return "\\odiaeresis";
				case "leftrightharpoons": return "\\leftrightharpoons";
				case "iuml": return "\\idiaeresis";
				case "euml": return "\\ediaeresis";
				case "LongLeftRightArrow": return "\\longleftrightarrow";
				case "DoubleVerticalBar": return "\\parallel";
				case "vsubnE": return "\\subsetneqq";
				case "abreve": return "\\abreve";
				case "NotLessLess": return "\\ll";
				case "eta": return "\\greeketa";
				case "gvertneqq": return "\\gneqq";
				case "UpArrow": return "\\uparrow";
				case "nge": return "\\ngeq";
				case "NotSucceeds": return "\\nsucc";
				case "Ubreve": return "\\Ubreve";
				case "VerticalBar": return "\\divides";
				case "Updownarrow": return "\\Updownarrow";
				case "gbreve": return "\\gbreve";
				case "Abreve": return "\\Abreve";
				case "part": return "\\partial";
				case "oint": return "\\oint";
				case "Eta": return "\\greekEta";
				case "RightTeeArrow": return "\\mapsto";
				case "NotRightTriangle": return "\\ntriangleleft";
				case "updownarrow": return "\\updownarrow";
				case "Gbreve": return "\\Gbreve";
				case "LeftFloor": return "\\lfloor";
				case "sqcup": return "\\sqcup";
				case "zwnj": return "\\zwnj";
				case "subseteqq": return "\\subseteqq";
				case "circledast": return "\\circledast";
				case "dblac": return "\\texthungarumlaut";
				case "VeryThinSpace	 ": return "\\irspace";
				case "thicksim": return "\\sim";
				case "SquareUnion": return "\\sqcup";
				case "ubreve": return "\\ubreve";
				case "zigrarr": return "\\rightsquigarrow";
				case "NonBreakingSpace	": return "\\breakspace";
				case "gEl": return "\\gtreqqless";
				case "npart": return "\\partial";
				case "ShortRightArrow": return "\\rightarrow";
				case "nLl": return "\\lll";
				case "npr": return "\\nprec";
				case "ulcorn": return "\\ulcorner";
				case "period": return "\\mathperiod";
				case "SuchThat": return "\\ni";
				case "MinusPlus": return "\\mp";
				case "NotLessTilde": return "\\nlesssim";
				case "Element": return "\\in";
				case "precapprox": return "\\precapprox";
				case "ShortDownArrow": return "\\downarrow";
				case "THORN": return "\\Thorn";
				case "omacr": return "\\omacron";
				case "LT": return "\\lt";
				case "vartheta": return "\\greekthetaalt";
				case "nle": return "\\nleq";
				case "PSgr": return "\\greekPsi";
				case "GT": return "\\gt";
				case "dlcorn": return "\\llcorner";
				case "DoubleLongLeftArrow": return "\\Longleftarrow";
				case "CircleTimes": return "\\otimes";
				case "QUOT": return "\\quotedbl";
				case "Imacr": return "\\Imacron";
				case "Omacr": return "\\Omacron";
				case "DDotrahd": return "\\dottedrightarrow";
				case "angle": return "\\angle";
				case "Umacr": return "\\Umacron";
				case "xvee": return "\\bigvee";
				case "die": return "\\textdiaeresis";
				case "Vert": return "\\Vert";
				case "emacr": return "\\emacron";
				case "nparallel": return "\\nparallel";
				case "nsccue": return "\\nsucccurlyeq";
				case "Amacr": return "\\Amacron";
				case "Emacr": return "\\Emacron";
				case "ThickSpace	  ": return "\\dspace";
				case "Iota": return "\\greekIota";
				case "blacktriangleright": return "\\blacktriangleleft";
				case "scnsim": return "\\succnsim";
				case "nlArr": return "\\nLeftarrow";
				case "circeq": return "\\circeq";
				case "acute": return "\\textacute";
				case "Backslash": return "\\setminus";
				case "ncong": return "\\approxnEq";
				case "leftleftarrows": return "\\leftleftarrows";
				case "varkappa": return "\\varkappa";
				case "LongRightArrow": return "\\longrightarrow";
				case "plusb": return "\\boxplus";
				case "longmapsto": return "\\longmapsto";
				case "circlearrowleft": return "\\circlearrowright";
				case "CloseCurlyQuote": return "\\textquoteright";
				case "bnequiv": return "\\equiv";
				case "iota": return "\\greekiota";
				case "compfn": return "\\circ";
				case "nrarrw": return "\\rightwavearrow";
				case "and": return "\\wedge";
				case "ntriangleleft": return "\\ntriangleright";
				case "varsubsetneq": return "\\subsetneq";
				case "Gdot": return "\\Gdotaccent";
				case "middot": return "\\periodcentered";
				case "Idot": return "\\Idotaccent";
				case "lEg": return "\\lesseqqgtr";
				case "bigcirc": return "\\bigcirc";
				case "Cdot": return "\\Cdotaccent";
				case "CloseCurlyDoubleQuote": return "\\quotedblright";
				case "Edot": return "\\Edotaccent";
				case "langle": return "\\langle";
				case "pitchfork": return "\\pitchfork";
				case "rationals": return "\\rationals";
				case "numsp	 ": return "\\gurespace";
				case "mapstoup": return "\\mapsup";
				case "Square": return "\\square";
				case "omega": return "\\greekomega";
				case "rangle": return "\\rangle";
				case "cdot": return "\\cdotaccent";
				case "edot": return "\\edotaccent";
				case "zdot": return "\\zdotaccent";
				case "odot": return "\\odot";
				case "eqslantless": return "\\eqslantless";
				case "DiacriticalTilde": return "\\texttilde";
				case "Omega": return "\\greekOmega";
				case "TildeEqual": return "\\simeq";
				case "bullet": return "\\textbullet";
				case "ubrcy": return "\\cyrillicushrt";
				case "PrecedesEqual": return "\\preceq";
				case "equals": return "\\eq";
				case "nsqsube": return "\\nsqsubseteq";
				case "VerticalTilde": return "\\wr";
				case "nvDash": return "\\nvDash";
				case "Leftrightarrow": return "\\Leftrightarrow";
				case "planck": return "\\hslash";
				case "comma": return "\\textcomma";
				case "GreaterSlantEqual": return "\\geqslant";
				case "permil": return "\\perthousand";
				case "Ubrcy": return "\\cyrillicUSHRT";
				case "DoubleUpArrow": return "\\Uparrow";
				case "NotGreaterGreater": return "\\gg";
				case "harrw": return "\\leftrightsquigarrow";
				case "bigoplus": return "\\bigoplus";
				case "rarrw": return "\\rightwavearrow";
				case "nshortparallel": return "\\nparallel";
				case "SmallCircle": return "\\circ";
				case "bigcup": return "\\bigcup";
				case "Integral": return "\\intop";
				case "ntrianglerighteq": return "\\ntrianglerighteq";
				case "Exists": return "\\exists";
				case "angsph": return "\\sphericalangle";
				case "NotLessSlantEqual": return "\\leqslant";
				case "Xi": return "\\greekXi";
				case "COPY": return "\\copyright";
				case "oplus": return "\\oplus";
				case "nsupseteq": return "\\nsupseteq";
				case "Jsercy": return "\\cyrillicJE";
				case "pi": return "\\greekpi";
				case "Psi": return "\\greekPsi";
				case "uplus": return "\\uplus";
				case "ii": return "\\imaginaryi";
				case "lesseqqgtr": return "\\lesseqqgtr";
				case "approx": return "\\approx";
				case "NegativeThinSpace": return "\\zerowidthspace";
				case "Superset": return "\\supset";
				case "nwArr": return "\\Nwarrow";
				case "daleth": return "\\daleth";
				case "NotGreaterTilde": return "\\ngtrsim";
				case "NotReverseElement": return "\\nni";
				case "jsercy": return "\\cyrillicje";
				case "OpenCurlyDoubleQuote": return "\\quotedblleft";
				case "euro": return "\\texteuro";
				case "leftharpoondown": return "\\leftharpoondown";
				case "ZeroWidthSpace": return "\\zerowidthspace";
				case "frown": return "\\frown";
				case "Dcaron": return "\\Dcaron";
				case "Ecaron": return "\\Ecaron";
				case "Ccaron": return "\\Ccaron";
				case "Ncaron": return "\\Ncaron";
				case "Tcaron": return "\\Tcaron";
				case "gg": return "\\gg";
				case "Rcaron": return "\\Rcaron";
				case "Scaron": return "\\Scaron";
				case "Lcaron": return "\\Lcaron";
				case "leftrightarrows": return "\\leftrightarrows";
				case "flat": return "\\flat";
				case "kgreen": return "\\kkra";
				case "nLt": return "\\ll";
				case "af": return "\\relax";
				case "psi": return "\\greekpsi";
				case "Gg": return "\\ggg";
				case "cire": return "\\circeq";
				case "EEacgr": return "\\greekEtatonos";
				case "simeq": return "\\simeq";
				case "nGg": return "\\ggg";
				case "ne": return "\\neq";
				case "nsubseteqq": return "\\subseteqq";
				case "le": return "\\leq";
				case "succcurlyeq": return "\\succcurlyeq";
				case "ge": return "\\geq";
				case "xrArr": return "\\Longrightarrow";
				case "vDash": return "\\vDash";
				case "iexcl": return "\\exclamdown";
				case "dcaron": return "\\dcaron";
				case "UpTeeArrow": return "\\mapsup";
				case "ccaron": return "\\ccaron";
				case "lambda": return "\\greeklambda";
				case "EqualTilde": return "\\eqsim";
				case "Zcaron": return "\\Zcaron";
				case "RightDoubleBracket": return "\\rrbracket";
				case "ntriangleright": return "\\ntriangleleft";
				case "NotTildeFullEqual": return "\\approxnEq";
				case "commat": return "\\textat";
				case "Re": return "\\Re";
				case "VDash": return "\\VDash";
				case "frac23": return "\\twothirds";
				case "Breve": return "\\textbreve";
				case "NotSucceedsEqual": return "\\succeq";
				case "iukcy": return "\\cyrillicii";
				case "Lambda": return "\\greekLambda";
				case "CHcy": return "\\cyrillicCH";
				case "jukcy": return "\\cyrillicie";
				case "Dstrok": return "\\Dstroke";
				case "Gamma": return "\\greekGamma";
				case "Lstrok": return "\\Lstroke";
				case "Hstrok": return "\\Hstroke";
				case "Tstrok": return "\\Tstroke";
				case "breve": return "\\textbreve";
				case "Iukcy": return "\\cyrillicII";
				case "napprox": return "\\napprox";
				case "downharpoonleft": return "\\downharpoonleft";
				case "Jukcy": return "\\cyrillicIE";
				case "UpTee": return "\\bot";
				case "dstrok": return "\\dstroke";
				case "gamma": return "\\greekgamma";
				case "lstrok": return "\\lstroke";
				case "supsetneq": return "\\supsetneq";
				case "nsucc": return "\\nsucc";
				case "NotGreaterSlantEqual": return "\\geqslant";
				case "nvap": return "\\asymp";
				case "upuparrows": return "\\upuparrows";
				case "wp": return "\\wp";
				case "IEcy": return "\\cyrillicE";
				case "reals": return "\\reals";
				case "prcue": return "\\preccurlyeq";
				case "Uogon": return "\\Uogonek";
				case "apos": return "\\quotesingle";
				case "rightrightarrows": return "\\rightrightarrows";
				case "Aogon": return "\\Aogonek";
				case "Eogon": return "\\Eogonek";
				case "LessTilde": return "\\lesssim";
				case "Iogon": return "\\Iogonek";
				case "ap": return "\\approx";
				case "RightArrow": return "\\rightarrow";
				case "rbrace": return "\\textbraceright";
				case "DownTee": return "\\top";
				case "amalg": return "\\amalg";
				case "Ll": return "\\lll";
				case "rthree": return "\\rightthreetimes";
				case "check": return "\\checkmark";
				case "DownArrowUpArrow": return "\\downuparrows";
				case "xi": return "\\greekxi";
				case "ntlg": return "\\nlessgtr";
				case "empty": return "\\emptyset";
				case "SubsetEqual": return "\\subseteq";
				case "emptyset": return "\\emptyset";
				case "in": return "\\in";
				case "frac14": return "\\onequarter";
				case "LeftDoubleBracket": return "\\llbracket";
				case "YAcy": return "\\cyrillicYA";
				case "tstrok": return "\\tstroke";
				case "scE": return "\\succeqq";
				case "urcorn": return "\\urcorner";
				case "CircleMinus": return "\\ominus";
				case "OHacgr": return "\\greekOmegatonos";
				case "DoubleUpDownArrow": return "\\Updownarrow";
				case "clubs": return "\\clubsuit";
				case "pm": return "\\textpm";
				case "Lmidot": return "\\Ldotmiddle";
				case "drcorn": return "\\lrcorner";
				case "divonx": return "\\divideontimes";
				case "ZHcy": return "\\cyrillicZH";
				case "Cconint": return "\\oiiint";
				case "GreaterTilde": return "\\gtrsim";
				case "squ": return "\\square";
				case "straightepsilon": return "\\greekepsilonalt";
				case "KHcy": return "\\cyrillicH";
				case "lmidot": return "\\ldotmiddle";
				case "ll": return "\\ll";
				case "Im": return "\\Im";
				case "SHcy": return "\\cyrillicSH";
				case "napos": return "\\napostrophe";
				case "circlearrowright": return "\\circlearrowleft";
				case "IOcy": return "\\cyrillicYO";
				case "longleftrightarrow": return "\\longleftrightarrow";
				case "ngt": return "\\ngtr";
				case "larrlp": return "\\looparrowleft";
				case "nleq": return "\\nleq";
				case "SOFTcy": return "\\cyrillicSFTSN";
				case "curlyeqprec": return "\\curlyeqprec";
				case "midast": return "\\ast";
				case "NotEqualTilde": return "\\eqsim";
				case "sfrown": return "\\frown";
				case "geqq": return "\\geqq";
				case "Longleftrightarrow": return "\\Longleftrightarrow";
				case "NotGreater": return "\\ngtr";
				case "UpDownArrow": return "\\updownarrow";
				case "NotSuperset": return "\\supset";
				case "angmsd": return "\\measuredangle";
				case "ReverseEquilibrium": return "\\leftrightharpoons";
				case "gnap": return "\\gnapprox";
				case "gel": return "\\gtreqless";
				case "ImaginaryI": return "\\imaginaryi";
				case "lnap": return "\\lnapprox";
				case "nrtri": return "\\ntriangleleft";
				case "thickapprox": return "\\approx";
				case "igrave": return "\\igrave";
				case "dzigrarr": return "\\longrightsquigarrow";
				case "egrave": return "\\egrave";
				case "rmoust": return "\\rmoustache";
				case "nsupe": return "\\nsupseteq";
				case "there4": return "\\therefore";
				case "ugrave": return "\\ugrave";
				case "lmoust": return "\\lmoustache";
				case "wedgeq": return "\\wedgeeq";
				case "Ograve": return "\\Ograve";
				case "Igrave": return "\\Igrave";
				case "Egrave": return "\\Egrave";
				case "agrave": return "\\agrave";
				case "supseteqq": return "\\supseteqq";
				case "Ugrave": return "\\Ugrave";
				case "Eacute": return "\\Eacute";
				case "Cacute": return "\\Cacute";
				case "Aacute": return "\\Aacute";
				case "DoubleDot": return "\\textdiaeresis";
				case "maltese": return "\\maltese";
				case "Nacute": return "\\Nacute";
				case "Lacute": return "\\Lacute";
				case "Iacute": return "\\Iacute";
				case "succnsim": return "\\succnsim";
				case "Lt": return "\\ll";
				case "Uacute": return "\\Uacute";
				case "Tilde": return "\\sim";
				case "Agrave": return "\\Agrave";
				case "Racute": return "\\Racute";
				case "Oacute": return "\\Oacute";
				case "NotCupCap": return "\\nasymp";
				case "Yacute": return "\\Yacute";
				case "Zacute": return "\\Zacute";
				case "eacute": return "\\eacute";
				case "ell": return "\\ell";
				case "cacute": return "\\cacute";
				case "NJcy": return "\\cyrillicNJE";
				case "nacute": return "\\nacute";
				case "LJcy": return "\\cyrillicLJE";
				case "KJcy": return "\\cyrillicKJE";
				case "iacute": return "\\iacute";
				case "GJcy": return "\\cyrillicGJE";
				case "rarrb": return "\\rightarrowbar";
				case "SucceedsEqual": return "\\succeq";
				case "DifferentialD": return "\\differentiald";
				case "racute": return "\\racute";
				case "GreaterEqualLess": return "\\gtreqless";
				case "lharu": return "\\leftharpoonup";
				case "ApplyFunction": return "\\relax";
				case "LessSlantEqual": return "\\leqslant";
				case "smid": return "\\divides";
				case "mid": return "\\divides";
				case "nu": return "\\greeknu";
				case "mu": return "\\greekmu";
				case "xutri": return "\\triangle";
				case "DJcy": return "\\cyrillicDJE";
				case "delta": return "\\greekdelta";
				case "YIcy": return "\\cyrillicYI";
				case "vprop": return "\\propto";
				case "lneq": return "\\lneq";
				case "prap": return "\\precapprox";
				case "twoheadleftarrow": return "\\twoheadleftarrow";
				case "backsim": return "\\backsim";
				case "comp": return "\\complement";
				case "frac35": return "\\threefifths";
				case "DoubleLeftRightArrow": return "\\Leftrightarrow";
				case "Mu": return "\\greekMu";
				case "osol": return "\\oslash";
				case "diams": return "\\blacklozenge";
				case "geqslant": return "\\geqslant";
				case "bsol": return "\\textbackslash";
				case "Delta": return "\\greekDelta";
				case "Longleftarrow": return "\\Longleftarrow";
				case "forall": return "\\forall";
				case "leqslant": return "\\leqslant";
				case "frac78": return "\\seveneighths";
				case "frac58": return "\\fiveeighths";
				case "prod": return "\\prod";
				case "Alpha": return "\\greekAlpha";
				case "sstarf": return "\\star";
				case "CircleDot": return "\\odot";
				case "CupCap": return "\\asymp";
				case "ReverseElement": return "\\ni";
				case "NotSquareSuperset": return "\\sqsupset";
				case "asymp": return "\\approx";
				case "ldsh": return "\\Ldsh";
				case "gtreqless": return "\\gtreqless";
				case "alpha": return "\\greekalpha";
				case "bump": return "\\Bumpeq";
				case "realpart": return "\\Re";
				case "natural": return "\\natural";
				case "ordf": return "\\ordfeminine";
				case "ast": return "\\ast";
				case "copysr": return "\\textcircledP";
				case "circledR": return "\\registered";
				case "subset": return "\\subset";
				case "rarrhk": return "\\hookrightarrow";
				case "TSHcy": return "\\cyrillicTSHE";
				case "larrhk": return "\\hookleftarrow";
				case "frac34": return "\\threequarter";
				case "straightphi": return "\\greekphialt";
				case "eDot": return "\\doteqdot";
				case "Verbar": return "\\Vert";
				case "rhard": return "\\rightharpoondown";
				case "VerticalLine": return "\\textbar";
				case "nltri": return "\\ntriangleright";
				case "LeftArrow": return "\\leftarrow";
				case "dashv": return "\\dashv";
				case "NotSucceedsSlantEqual": return "\\nsucccurlyeq";
				case "TripleDot": return "\\dddot";
				case "intcal": return "\\intercal";
				case "Congruent": return "\\equiv";
				case "biguplus": return "\\biguplus";
				case "uharl": return "\\upharpoonleft";
				case "rmoustache": return "\\rmoustache";
				case "gnapprox": return "\\gnapprox";
				case "deg": return "\\textdegree";
				case "lnapprox": return "\\lnapprox";
				case "rho": return "\\greekrho";
				case "mho": return "\\textmho";
				case "DoubleDownArrow": return "\\Downarrow";
				case "xmap": return "\\longmapsto";
				case "sbquo": return "\\quotesinglebase";
				case "micro": return "\\textmu";
				case "rcub": return "\\textbraceright";
				case "lcub": return "\\textbraceleft";
				case "Theta": return "\\greekTheta";
				case "leg": return "\\lesseqgtr";
				case "reg": return "\\registered";
				case "nleftarrow": return "\\nleftarrow";
				case "lhard": return "\\leftharpoondown";
				case "ForAll": return "\\forall";
				case "curlywedge": return "\\curlywedge";
				case "looparrowleft": return "\\looparrowleft";
				case "cuvee": return "\\curlyvee";
				case "copy": return "\\copyright";
				case "vsubne": return "\\subsetneq";
				case "NotRightTriangleEqual": return "\\ntrianglerighteq";
				case "Lleftarrow": return "\\Lleftarrow";
				case "lesseqgtr": return "\\lesseqgtr";
				case "NotLess": return "\\nless";
				case "gtrdot": return "\\gtrdot";
				case "shortmid": return "\\divides";
				case "cup": return "\\cup";
				case "setmn": return "\\setminus";
				case "thetav": return "\\greekthetaalt";
				case "DiacriticalAcute": return "\\textacute";
				case "Sup": return "\\Supset";
				case "ocir": return "\\circledcirc";
				case "nlt": return "\\nless";
				case "bigodot": return "\\bigodot";
				case "ecir": return "\\eqcirc";
				case "LeftArrowRightArrow": return "\\leftrightarrows";
				case "race": return "\\backsim";
				case "ltimes": return "\\ltimes";
				case "sup": return "\\supset";
				case "rtimes": return "\\rtimes";
				case "otimes": return "\\otimes";
				case "supne": return "\\supsetneq";
				case "SquareSupersetEqual": return "\\sqsupseteq";
				case "ShortUpArrow": return "\\uparrow";
				case "preceq": return "\\preceq";
				case "Uacgr": return "\\greekUpsilontonos";
				case "aacgr": return "\\greekalphatonos";
				case "Prime": return "\\doubleprime";
				case "Iacgr": return "\\greekIotatonos";
				case "Eacgr": return "\\greekEpsilontonos";
				case "Oacgr": return "\\greekOmicrontonos";
				case "ntrianglelefteq": return "\\ntrianglelefteq";
				case "cedil": return "\\textcedilla";
				case "Cup": return "\\Cup";
				case "idiagr": return "\\greekiotadialytikatonos";
				case "nsmid": return "\\ndivides";
				case "xcap": return "\\bigcap";
				case "Aacgr": return "\\greekAlphatonos";
				case "Colon": return "\\squaredots";
				case "scap": return "\\succapprox";
				case "between": return "\\between";
				case "bigcap": return "\\bigcap";
				case "diamondsuit": return "\\blacklozenge";
				case "shchcy": return "\\cyrillicshch";
				case "crarr": return "\\carriagereturn";
				case "ominus": return "\\ominus";
				case "rdsh": return "\\Rdsh";
				case "nsime": return "\\nsimeq";
				case "nrarr": return "\\nrightarrow";
				case "NotHumpDownHump": return "\\Bumpeq";
				case "srarr": return "\\rightarrow";
				case "plankv": return "\\hslash";
				case "phiv": return "\\greekphialt";
				case "angrt": return "\\rightangle";
				case "xdtri": return "\\bigtriangledown";
				case "NegativeMediumSpace": return "\\zerowidthspace";
				case "par": return "\\parallel";
				case "diam": return "\\diamond";
				case "laquo": return "\\leftguillemot";
				case "sigmav": return "\\greekfinalsigma";
				case "supe": return "\\supseteq";
				case "SucceedsTilde": return "\\succsim";
				case "ffilig": return "\\ffiligature";
				case "IJlig": return "\\IJligature";
				case "Rsh": return "\\Rsh";
				case "lsh": return "\\Lsh";
				case "DiacriticalGrave": return "\\textgrave";
				case "Equilibrium": return "\\rightleftharpoons";
				case "downdownarrows": return "\\downdownarrows";
				case "egs": return "\\eqslantgtr";
				case "cwint": return "\\intclockwise";
				case "Dot": return "\\textdiaeresis";
				case "Beta": return "\\greekBeta";
				case "malt": return "\\maltese";
				case "ges": return "\\geqslant";
				case "lobrk": return "\\llbracket";
				case "Product": return "\\prod";
				case "trie": return "\\triangleq";
				case "sqsupset": return "\\sqsupset";
				case "aleph": return "\\aleph";
				case "DiacriticalDoubleAcute": return "\\texthungarumlaut";
				case "supE": return "\\supseteqq";
				case "uacgr": return "\\greekupsilontonos";
				case "approxeq": return "\\approxeq";
				case "robrk": return "\\rrbracket";
				case "UnionPlus": return "\\uplus";
				case "Lsh": return "\\Lsh";
				case "iacgr": return "\\greekiotatonos";
				case "eth": return "\\eth";
				case "eacgr": return "\\greekepsilontonos";
				case "intercal": return "\\intercal";
				case "imagpart": return "\\Im";
				case "ctdot": return "\\cdots";
				case "dtdot": return "\\ddots";
				case "gtdot": return "\\gtrdot";
				case "lessapprox": return "\\lessapprox";
				case "AElig": return "\\AEligature";
				case "colone": return "\\colonequals";
				case "circleddash": return "\\circleddash";
				case "models": return "\\models";
				case "NotDoubleVerticalBar": return "\\nparallel";
				case "uml": return "\\textdiaeresis";
				case "udblac": return "\\uhungarumlaut";
				case "SucceedsSlantEqual": return "\\succcurlyeq";
				case "utdot": return "\\udots";
				case "LeftCeiling": return "\\lceiling";
				case "top": return "\\top";
				case "ltdot": return "\\lessdot";
				case "LowerRightArrow": return "\\searrow";
				case "gtrapprox": return "\\gtrapprox";
				case "Udblac": return "\\Uhungarumlaut";
				case "LessEqualGreater": return "\\lesseqgtr";
				case "Int": return "\\iintop";
				case "Odblac": return "\\Ohungarumlaut";
				case "nsupset": return "\\supset";
				case "nwarrow": return "\\nwarrow";
				case "nsube": return "\\nsubseteq";
				case "swarrow": return "\\swarrow";
				case "int": return "\\intop";
				case "sol": return "\\textslash";
				case "REG": return "\\registered";
				case "ShortLeftArrow": return "\\leftarrow";
				case "alefsym": return "\\aleph";
				case "Uarr": return "\\twoheaduparrow";
				case "Rarr": return "\\twoheadrightarrow";
				case "darr": return "\\downarrow";
				case "percnt": return "\\percent";
				case "harr": return "\\leftrightarrow";
				case "RightArrowBar": return "\\rightarrowbar";
				case "uarr": return "\\uparrow";
				case "Uparrow": return "\\Uparrow";
				case "eqcolon": return "\\equalscolon";
				case "succapprox": return "\\succapprox";
				case "notin": return "\\nin";
				case "varphi": return "\\greekphialt";
				case "Rho": return "\\greekRho";
				case "bottom": return "\\bot";
				case "curarr": return "\\curvearrowright";
				case "exponentiale": return "\\exponentiale";
				case "nrtrie": return "\\ntrianglerighteq";
				case "SquareSuperset": return "\\sqsupset";
				case "lmoustache": return "\\lmoustache";
				case "propto": return "\\propto";
				case "nsupseteqq": return "\\supseteqq";
				case "dharl": return "\\downharpoonleft";
				case "varnothing": return "\\emptyset";
				case "GreaterFullEqual": return "\\geqq";
				case "nLtv": return "\\ll";
				case "fallingdotseq": return "\\fallingdotseq";
				case "smile": return "\\smile";
				case "bigsqcup": return "\\bigsqcup";
				case "sime": return "\\simeq";
				case "NotSucceedsTilde": return "\\succsim";
				case "Darr": return "\\twoheaddownarrow";
				case "Larr": return "\\twoheadleftarrow";
				case "ggg": return "\\ggg";
				case "SquareSubset": return "\\sqsubset";
				case "Subset": return "\\Subset";
				case "rightleftarrows": return "\\rightleftarrows";
				case "dzcy": return "\\cyrillicdzhe";
				case "HumpDownHump": return "\\Bumpeq";
				case "hookrightarrow": return "\\hookrightarrow";
				case "beta": return "\\greekbeta";
				case "Zeta": return "\\greekZeta";
				case "RightUpVector": return "\\upharpoonright";
				case "HARDcy": return "\\cyrillicHRDSN";
				case "MediumSpace	 ": return "\\dspace";
				case "RightFloor": return "\\rfloor";
				case "uparrow": return "\\uparrow";
				case "image": return "\\Im";
				case "dotsquare": return "\\boxdot";
				case "scnap": return "\\succnapprox";
				case "ssetmn": return "\\setminus";
				case "CenterDot": return "\\periodcentered";
				case "Coproduct": return "\\coprod";
				case "eng": return "\\neng";
				case "supseteq": return "\\supseteq";
				case "NotGreaterFullEqual": return "\\geqq";
				case "softcy": return "\\cyrillicsftsn";
				case "iiota": return "\\turnediota";
				case "NotVerticalBar": return "\\ndivides";
				case "puncsp	 ": return "\\nctuationspace";
				case "ENG": return "\\Neng";
				case "ang": return "\\angle";
				case "RightDownVector": return "\\downharpoonright";
				case "natur": return "\\natural";
				case "boxminus": return "\\boxminus";
				case "Tab": return "\\t defined";
				case "ogon": return "\\textogonek";
				case "ddarr": return "\\downdownarrows";
				case "beth": return "\\beth";
				case "NotLeftTriangle": return "\\ntriangleright";
				case "khgr": return "\\greekchi";
				case "starf": return "\\bigstar";
				case "thgr": return "\\greektheta";
				case "imath": return "\\dotlessi";
				case "jmath": return "\\dotlessj";
				case "nvsim": return "\\sim";
				case "rbarr": return "\\dashedrightarrow";
				case "isin": return "\\in";
				case "iocy": return "\\cyrillicyo";
				case "circledcirc": return "\\circledcirc";
				case "DoubleContourIntegral": return "\\oiint";
				case "minusd": return "\\dotminus";
				case "nearrow": return "\\nearrow";
				case "CapitalDifferentialD": return "\\differentialD";
				case "NestedLessLess": return "\\ll";
				case "erDot": return "\\risingdotseq";
				case "cdots": return "\\textellipsis";
				case "minusplus": return "\\mp";
				case "plusminus": return "\\textpm";
				case "real": return "\\Re";
				case "searr": return "\\searrow";
				case "nltrie": return "\\ntrianglelefteq";
				case "zgr": return "\\greekzeta";
				case "ograve": return "\\ograve";
				case "zeta": return "\\greekzeta";
				case "xrarr": return "\\longrightarrow";
				case "Bcy": return "\\cyrillicB";
				case "Acy": return "\\cyrillicA";
				case "zcaron": return "\\zcaron";
				case "Gcy": return "\\cyrillicG";
				case "Fcy": return "\\cyrillicF";
				case "Ecy": return "\\cyrillicEREV";
				case "Dcy": return "\\cyrillicD";
				case "Kcy": return "\\cyrillicK";
				case "Jcy": return "\\cyrillicISHRT";
				case "Icy": return "\\cyrillicI";
				case "zacute": return "\\zacute";
				case "Ocy": return "\\cyrillicO";
				case "Ncy": return "\\cyrillicN";
				case "Mcy": return "\\cyrillicM";
				case "Lcy": return "\\cyrillicL";
				case "Scy": return "\\cyrillicS";
				case "Rcy": return "\\cyrillicR";
				case "yucy": return "\\cyrillicyu";
				case "Pcy": return "\\cyrillicP";
				case "Vcy": return "\\cyrillicV";
				case "Ucy": return "\\cyrillicU";
				case "Tcy": return "\\cyrillicT";
				case "sacute": return "\\sacute";
				case "Zcy": return "\\cyrillicZ";
				case "Ycy": return "\\cyrillicERY";
				case "yen": return "\\textyen";
				case "xotime": return "\\bigotimes";
				case "Longrightarrow": return "\\Longrightarrow";
				case "bcy": return "\\cyrillicb";
				case "acy": return "\\cyrillica";
				case "nharr": return "\\nleftrightarrow";
				case "gcy": return "\\cyrillicg";
				case "bsim": return "\\backsim";
				case "ecy": return "\\cyrillicerev";
				case "dcy": return "\\cyrillicd";
				case "kcy": return "\\cyrillick";
				case "jcy": return "\\cyrillicishrt";
				case "icy": return "\\cyrillici";
				case "nprec": return "\\nprec";
				case "ocy": return "\\cyrillico";
				case "lcy": return "\\cyrillicl";
				case "scy": return "\\cyrillics";
				case "rcy": return "\\cyrillicr";
				case "yacute": return "\\yacute";
				case "xwedge": return "\\bigwedge";
				case "vcy": return "\\cyrillicv";
				case "ucy": return "\\cyrillicu";
				case "tcy": return "\\cyrillict";
				case "xsqcup": return "\\bigsqcup";
				case "zcy": return "\\cyrillicz";
				case "ycy": return "\\cyrillicery";
				case "LeftAngleBracket": return "\\langle";
				case "bkarow": return "\\dashedrightarrow";
				case "xodot": return "\\bigodot";
				case "xlarr": return "\\longleftarrow";
				case "xlArr": return "\\Longleftarrow";
				case "ugr": return "\\greekupsilon";
				case "xgr": return "\\greekxi";
				case "omicron": return "\\greekomicron";
				case "xcup": return "\\bigcup";
				case "xcirc": return "\\bigcirc";
				case "wr": return "\\wr";
				case "weierp": return "\\wp";
				case "primes": return "\\primes";
				case "shy": return "\\softhyphen";
				case "amacr": return "\\amacron";
				case "Tau": return "\\greekTau";
				case "vsupne": return "\\supsetneq";
				case "vsupnE": return "\\supsetneqq";
				case "nsim": return "\\nsim";
				case "vnsub": return "\\subset";
				case "Cedilla": return "\\textcedilla";
				case "lsim": return "\\lesssim";
				case "vert": return "\\textbar";
				case "NotElement": return "\\nin";
				case "egr": return "\\greekepsilon";
				case "dscy": return "\\cyrillicdze";
				case "vellip": return "\\vdots";
				case "parallel": return "\\parallel";
				case "nequiv": return "\\nequiv";
				case "varsupsetneqq": return "\\supsetneqq";
				case "varsupsetneq": return "\\supsetneq";
				case "eegr": return "\\greeketa";
				case "square": return "\\square";
				case "varsigma": return "\\greekfinalsigma";
				case "varr": return "\\updownarrow";
				case "varpropto": return "\\propto";
				case "varpi": return "\\greekpialt";
				case "tau": return "\\greektau";
				case "sfgr": return "\\greekfinalsigma";
				case "iff": return "\\Leftrightarrow";
				case "vArr": return "\\Updownarrow";
				case "dotminus": return "\\dotminus";
				case "uuarr": return "\\upuparrows";
				case "NotSquareSupersetEqual": return "\\nsqsupseteq";
				case "lessgtr": return "\\lessgtr";
				case "gesl": return "\\gtreqless";
				case "OpenCurlyQuote": return "\\textquoteleft";
				case "sqsubseteq": return "\\sqsubseteq";
				case "lfloor": return "\\lfloor";
				case "urcorner": return "\\urcorner";
				case "NotSubset": return "\\subset";
				case "LeftArrowBar": return "\\barleftarrow";
				case "uogon": return "\\uogonek";
				case "umacr": return "\\umacron";
				case "TildeFullEqual": return "\\approxEq";
				case "ulcorner": return "\\ulcorner";
				case "dharr": return "\\downharpoonright";
				case "naturals": return "\\naturalnumbers";
				case "xharr": return "\\longleftrightarrow";
				case "uharr": return "\\upharpoonright";
				case "udiagr": return "\\greekupsilondialytikatonos";
				case "NegativeThickSpace": return "\\zerowidthspace";
				case "udarr": return "\\updownarrows";
				case "ucirc": return "\\ucircumflex";
				case "uacute": return "\\uacute";
				case "uArr": return "\\Uparrow";
				case "twoheadrightarrow": return "\\twoheadrightarrow";
				case "twixt": return "\\between";
				case "hardcy": return "\\cyrillichrdsn";
				case "tshcy": return "\\cyrillictshe";
				case "tscy": return "\\cyrillicc";
				case "triangleq": return "\\triangleq";
				case "tprime": return "\\tripleprime";
				case "curren": return "\\textcurrency";
				case "tint": return "\\iiintop";
				case "timesb": return "\\boxtimes";
				case "times": return "\\textmultiply";
				case "RightArrowLeftArrow": return "\\rightleftarrows";
				case "tilde": return "\\texttilde";
				case "thorn": return "\\thorn";
				case "thksim": return "\\sim";
				case "kgr": return "\\greekkappa";
				case "iecy": return "\\cyrillice";
				case "thinsp	 ": return "\\eakablethinspace";
				case "thetasym": return "\\greekthetaalt";
				case "theta": return "\\greektheta";
				case "tgr": return "\\greektau";
				case "iiint": return "\\iiintop";
				case "tdot": return "\\dddot";
				case "lesssim": return "\\lesssim";
				case "tcaron": return "\\tcaron";
				case "nGt": return "\\gg";
				case "swarhk": return "\\rhookswarrow";
				case "swArr": return "\\Swarrow";
				case "supnE": return "\\supsetneqq";
				case "divideontimes": return "\\divideontimes";
				case "awconint": return "\\ointctrclockwise";
				case "conint": return "\\oint";
				case "sup2": return "\\twosuperior";
				case "succsim": return "\\succsim";
				case "NotLessEqual": return "\\nleq";
				case "succnapprox": return "\\succnapprox";
				case "succeq": return "\\succeq";
				case "succ": return "\\succ";
				case "subsetneqq": return "\\subsetneqq";
				case "subsetneq": return "\\subsetneq";
				case "LongLeftArrow": return "\\longleftarrow";
				case "subseteq": return "\\subseteq";
				case "subne": return "\\subsetneq";
				case "subnE": return "\\subsetneqq";
				case "sube": return "\\subseteq";
				case "subE": return "\\subseteqq";
				case "DownTeeArrow": return "\\mapsdown";
				case "coprod": return "\\coprod";
				case "Ntilde": return "\\Ntilde";
				case "ssmile": return "\\smile";
				case "varsubsetneqq": return "\\subsetneqq";
				case "circ": return "\\textcircumflex";
				case "sqsupseteq": return "\\sqsupseteq";
				case "sqsupe": return "\\sqsupseteq";
				case "sqsup": return "\\sqsupset";
				case "sqsubset": return "\\sqsubset";
				case "LeftRightArrow": return "\\leftrightarrow";
				case "DiacriticalDot": return "\\textdotaccent";
				case "sqsube": return "\\sqsubseteq";
				case "sqsub": return "\\sqsubset";
				case "ngr": return "\\greeknu";
				case "sqcaps": return "\\sqcap";
				case "sqcap": return "\\sqcap";
				case "ee": return "\\exponentiale";
				case "leftthreetimes": return "\\leftthreetimes";
				case "Precedes": return "\\prec";
				case "spadesuit": return "\\spadesuit";
				case "divide": return "\\textdiv";
				case "spades": return "\\spadesuit";
				case "boxtimes": return "\\boxtimes";
				case "smallsetminus": return "\\setminus";
				case "slarr": return "\\leftarrow";
				case "zhcy": return "\\cyrilliczh";
				case "simne": return "\\napproxEq";
				case "sim": return "\\sim";
				case "sigmaf": return "\\greekfinalsigma";
				case "shortparallel": return "\\parallel";
				case "chcy": return "\\cyrillicch";
				case "SHCHcy": return "\\cyrillicSHCH";
				case "shcy": return "\\cyrillicsh";
				case "sgr": return "\\greeksigma";
				case "otilde": return "\\otilde";
				case "bigstar": return "\\bigstar";
				case "khcy": return "\\cyrillich";
				case "sect": return "\\sectionmark";
				case "searrow": return "\\searrow";
				case "searhk": return "\\lhooksearrow";
				case "seArr": return "\\Searrow";
				case "sdot": return "\\cdot";
				case "DoubleRightTee": return "\\vDash";
				case "scsim": return "\\succsim";
				case "Proportion": return "\\squaredots";
				case "hellip": return "\\textellipsis";
				case "Zdot": return "\\Zdotaccent";
				case "ExponentialE": return "\\exponentiale";
				case "sce": return "\\succeq";
				case "sccue": return "\\succcurlyeq";
				case "scaron": return "\\scaron";
				case "angst": return "\\Aring";
				case "sc": return "\\succ";
				case "yicy": return "\\cyrillicyi";
				case "rtrif": return "\\blacktriangleleft";
				case "notniva": return "\\nni";
				case "rsquo": return "'";
				case "rsh": return "\\Rsh";
				case "ordm": return "\\ordmasculine";
				case "OverBrace": return "\\overbrace";
				case "rsaquo": return "\\guilsingleright";
				case "rrarr": return "\\rightrightarrows";
				case "rpar": return "\\rparent";
				case "ngtr": return "\\ngtr";
				case "ljcy": return "\\cyrilliclje";
				case "kjcy": return "\\cyrillickje";
				case "roarr": return "\\rightarrowtriangle";
				case "gjcy": return "\\cyrillicgje";
				case "rceil": return "\\rceiling";
				case "djcy": return "\\cyrillicdje";
				case "eogon": return "\\eogonek";
				case "rlarr": return "\\rightleftarrows";
				case "ecolon": return "\\equalscolon";
				case "risingdotseq": return "\\risingdotseq";
				case "rightthreetimes": return "\\rightthreetimes";
				case "rightleftharpoons": return "\\rightleftharpoons";
				case "nGtv": return "\\gg";
				case "lceil": return "\\lceiling";
				case "rightharpoonup": return "\\rightharpoonup";
				case "rightarrowtail": return "\\rightarrowtail";
				case "rightarrow": return "\\rightarrow";
				case "wreath": return "\\wr";
				case "rharu": return "\\rightharpoonup";
				case "cuesc": return "\\curlyeqsucc";
				case "bigtriangledown": return "\\bigtriangledown";
				case "njcy": return "\\cyrillicnje";
				case "rgr": return "\\greekrho";
				case "oslash": return "\\ostroke";
				case "rfloor": return "\\rfloor";
				case "rdquo": return "\"";
				case "lsquor": return "\\quotesinglebase";
				case "veebar": return "\\veebar";
				case "rcedil": return "\\rcommaaccent";
				case "rcaron": return "\\rcaron";
				case "rsquor": return "\\textquoteright";
				case "prec": return "\\prec";
				case "mgr": return "\\greekmu";
				case "rarrtl": return "\\rightarrowtail";
				case "rarrlp": return "\\looparrowright";
				case "rarr": return "\\rightarrow";
				case "becaus": return "\\because";
				case "raquo": return "\\rightguillemot";
				case "radic": return "\\surd";
				case "sup1": return "\\onesuperior";
				case "rArr": return "\\Rightarrow";
				case "nles": return "\\leqslant";
				case "ncy": return "\\cyrillicn";
				case "leftharpoonup": return "\\leftharpoonup";
				case "larr": return "\\leftarrow";
				case "UnderBrace": return "\\underbrace";
				case "prnap": return "\\precnapprox";
				case "setminus": return "\\setminus";
				case "prime": return "\\prime";
				case "NotSquareSubset": return "\\sqsubset";
				case "SquareIntersection": return "\\sqcap";
				case "LeftDownVector": return "\\downharpoonleft";
				case "precneqq": return "\\precneqq";
				case "precnapprox": return "\\precnapprox";
				case "pr": return "\\prec";
				case "pound": return "\\textsterling";
				case "planckh": return "\\Plankconst";
				case "piv": return "\\greekpialt";
				case "phi": return "\\greekphi";
				case "phgr": return "\\greekphi";
				case "pgr": return "\\greekpi";
				case "NotSupersetEqual": return "\\nsupseteq";
				case "perp": return "\\bot";
				case "pcy": return "\\cyrillicp";
				case "nLeftrightarrow": return "\\nLeftrightarrow";
				case "ijlig": return "\\ijligature";
				case "NotPrecedesEqual": return "\\preceq";
				case "Because": return "\\because";
				case "ogr": return "\\greekomicron";
				case "orarr": return "\\circlearrowleft";
				case "or": return "\\vee";
				case "Rarrtl": return "\\twoheadrightarrowtail";
				case "ohm": return "\\greekOmega";
				case "ohgr": return "\\greekomega";
				case "Rfr": return "\\Re";
				case "Ifr": return "\\Im";
				case "TildeTilde": return "\\approx";
				case "Aring": return "\\Aring";
				case "complexes": return "\\complexes";
				case "Pgr": return "\\greekPi";
				case "odblac": return "\\ohungarumlaut";
				case "Ngr": return "\\greekNu";
				case "Ogr": return "\\greekOmicron";
				case "Tgr": return "\\greekTau";
				case "Ugr": return "\\greekUpsilon";
				case "Rgr": return "\\greekRho";
				case "Sgr": return "\\greekSigma";
				case "Igr": return "\\greekIota";
				case "Ggr": return "\\greekGamma";
				case "Lgr": return "\\greekLambda";
				case "Mgr": return "\\greekMu";
				case "oast": return "\\circledast";
				case "Kgr": return "\\greekKappa";
				case "agr": return "\\greekalpha";
				case "oacute": return "\\oacute";
				case "oacgr": return "\\greekomicrontonos";
				case "Xgr": return "\\greekXi";
				case "epsilon": return "\\greekepsilon";
				case "mapstodown": return "\\mapsdown";
				case "mumap": return "\\multimap";
				case "nleqslant": return "\\leqslant";
				case "nwarhk": return "\\lhooknwarrow";
				case "Zgr": return "\\greekZeta";
				case "bdquo": return "\\quotedblbase";
				case "hslash": return "\\hslash";
				case "llcorner": return "\\llcorner";
				case "nvgt": return "\\gt";
				case "nvge": return "\\geq";
				case "frac13": return "\\onethird";
				case "frac25": return "\\twofifths";
				case "numero": return "\\textnumero";
				case "ntilde": return "\\ntilde";
				case "equest": return "\\questionedeq";
				case "ntgl": return "\\ngtrless";
				case "nsupE": return "\\supseteqq";
				case "leftarrowtail": return "\\leftarrowtail";
				case "nsucceq": return "\\succeq";
				case "Agr": return "\\greekAlpha";
				case "lacute": return "\\lacute";
				case "nsubE": return "\\subseteqq";
				case "Dgr": return "\\greekDelta";
				case "Egr": return "\\greekEpsilon";
				case "Bgr": return "\\greekBeta";
				case "iquest": return "\\questiondown";
				case "nsub": return "\\nsubset";
				case "nsqsupe": return "\\nsqsupseteq";
				case "nspar": return "\\nparallel";
				case "Oslash": return "\\Ostroke";
				case "asympeq": return "\\asymp";
				case "nRightarrow": return "\\nRightarrow";
				case "nsimeq": return "\\nsimeq";
				case "nshortmid": return "\\ndivides";
				case "frac38": return "\\threeeighths";
				case "cong": return "\\approxEq";
				case "nsce": return "\\succeq";
				case "nsc": return "\\nsucc";
				case "nrArr": return "\\nRightarrow";
				case "npreceq": return "\\preceq";
				case "yacy": return "\\cyrillicya";
				case "dagger": return "\\textdag";
				case "nprcue": return "\\npreccurlyeq";
				case "Gammad": return "\\digamma";
				case "dollar": return "\\textdollar";
				case "NotGreaterEqual": return "\\ngeq";
				case "not": return "\\textlognot";
				case "nmid": return "\\ndivides";
				case "nless": return "\\nless";
				case "nleftrightarrow": return "\\nleftrightarrow";
				case "DownLeftVector": return "\\leftharpoondown";
				case "ni": return "\\ni";
				case "ngsim": return "\\ngtrsim";
				case "sqcups": return "\\sqcup";
				case "ngeq": return "\\ngeq";
				case "frac45": return "\\fourfifths";
				case "nexists": return "\\nexists";
				case "nesim": return "\\eqsim";
				case "nearr": return "\\nearrow";
				case "fnof": return "\\fhook";
				case "neArr": return "\\Nearrow";
				case "questeq": return "\\questionedeq";
				case "ncaron": return "\\ncaron";
				case "frasl": return "\\textfraction";
				case "nbump": return "\\Bumpeq";
				case "nVdash": return "\\nVdash";
				case "nLeftarrow": return "\\nLeftarrow";
				case "swarr": return "\\swarrow";
				case "nwarr": return "\\nwarrow";
				case "multimap": return "\\multimap";
				case "gimel": return "\\gimel";
				case "mp": return "\\mp";
				case "mldr": return "\\textellipsis";
				case "because": return "\\because";
				case "ratio": return "\\colon";
				case "measuredangle": return "\\measuredangle";
				case "mcy": return "\\cyrillicm";
				case "cwconint": return "\\ointclockwise";
				case "Utilde": return "\\Utilde";
				case "Uring": return "\\Uring";
				case "lvertneqq": return "\\lneqq";
				case "UnderParenthesis": return "\\underparent";
				case "nsubset": return "\\subset";
				case "lthree": return "\\leftthreetimes";
				case "lt": return "\\lt";
				case "lsquo": return "'";
				case "strns": return "\\textmacron";
				case "DownRightVector": return "\\rightharpoondown";
				case "Sub": return "\\Subset";
				case "lrarr": return "\\leftrightarrows";
				case "lpar": return "\\lparent";
				case "lozenge": return "\\lozenge";
				case "loz": return "\\lozenge";
				case "aring": return "\\aring";
				case "lowbar": return "\\textunderscore";
				case "looparrowright": return "\\looparrowright";
				case "longrightarrow": return "\\longrightarrow";
				case "longleftarrow": return "\\longleftarrow";
				case "llarr": return "\\leftleftarrows";
				case "lgr": return "\\greeklambda";
				case "lg": return "\\lessgtr";
				case "uring": return "\\uring";
				case "lesg": return "\\lesseqgtr";
				case "curlyeqsucc": return "\\curlyeqsucc";
				case "sigma": return "\\greeksigma";
				case "les": return "\\leqslant";
				case "leftrightsquigarrow": return "\\leftrightsquigarrow";
				case "bull": return "\\textbullet";
				case "Dagger": return "\\textddag";
				case "leftrightarrow": return "\\leftrightarrow";
				case "gammad": return "\\greekdigamma";
				case "nsup": return "\\nsupset";
				case "sub": return "\\subset";
				case "ldquo": return "\"";
				case "aacute": return "\\aacute";
				case "lcaron": return "\\lcaron";
				case "LessFullEqual": return "\\leqq";
				case "ContourIntegral": return "\\oint";
				case "lbrace": return "\\textbraceleft";
				case "lbarr": return "\\dashedleftarrow";
				case "larrtl": return "\\leftarrowtail";
				case "bigtriangleup": return "\\triangle";
				case "varepsilon": return "\\greekepsilonalt";
				case "lArr": return "\\Leftarrow";
				case "larrb": return "\\barleftarrow";
				case "sdotb": return "\\boxdot";
				case "hArr": return "\\Leftrightarrow";
				case "nlarr": return "\\nleftarrow";
				case "olarr": return "\\circlearrowright";
				case "thkap": return "\\approx";
				case "Star": return "\\star";
				case "aogon": return "\\aogonek";
				case "CirclePlus": return "\\oplus";
				case "kappav": return "\\varkappa";
				case "itilde": return "\\itilde";
				case "iogon": return "\\iogonek";
				case "Ccedil": return "\\Ccedilla";
				case "imped": return "\\Zstroke";
				case "imacr": return "\\imacron";
				case "Gcedil": return "\\Gcommaaccent";
				case "gneq": return "\\rneq";
				case "igr": return "\\greekiota";
				case "icirc": return "\\icircumflex";
				case "hstrok": return "\\hstroke";
				case "lrhar": return "\\leftrightharpoons";
				case "lrcorner": return "\\lrcorner";
				case "niv": return "\\ni";
				case "hoarr": return "\\leftrightarrowtriangle";
				case "hcirc": return "\\hcircumflex";
				case "hbar": return "\\hslash";
				case "half": return "\\onehalf";
				case "hairsp	 ": return "\\irspace";
				case "ThinSpace	 ": return "\\eakablethinspace";
				case "gtrless": return "\\gtrless";
				case "gt": return "\\gt";
				case "gsim": return "\\gtrsim";
				case "grave": return "\\textgrave";
				case "gl": return "\\gtrless";
				case "caron": return "\\textcaron";
				case "ggr": return "\\greekgamma";
				case "dArr": return "\\Downarrow";
				case "preccurlyeq": return "\\preccurlyeq";
				case "gdot": return "\\gdotaccent";
				case "frac15": return "\\onefifth";
				case "gacute": return "\\gacute";
				case "frac56": return "\\fivesixths";
				case "frac18": return "\\oneeighth";
				case "ecirc": return "\\ecircumflex";
				case "frac16": return "\\onesixth";
				case "gcirc": return "\\gcircumflex";
				case "LessGreater": return "\\lessgtr";
				case "jcirc": return "\\jcircumflex";
				case "frac12": return "\\onehalf";
				case "fllig": return "\\flligature";
				case "filig": return "\\filigature";
				case "ocirc": return "\\ocircumflex";
				case "fcy": return "\\cyrillicf";
				case "exist": return "\\exists";
				case "esim": return "\\eqsim";
				case "scirc": return "\\scircumflex";
				case "Hacek": return "\\textcaron";
				case "Ucirc": return "\\Ucircumflex";
				case "Wcirc": return "\\Wcircumflex";
				case "eqcirc": return "\\eqcirc";
				case "Ycirc": return "\\Ycircumflex";
				case "epsiv": return "\\greekepsilonalt";
				case "emsp14	 ": return "\\urperemspace";
				case "rlhar": return "\\rightleftharpoons";
				case "acirc": return "\\acircumflex";
				case "emsp13	 ": return "\\reeperemspace";
				case "ccirc": return "\\ccircumflex";
				case "emptyv": return "\\emptyset";
				case "Ecirc": return "\\Ecircumflex";
				case "Gcirc": return "\\Gcircumflex";
				case "Hcirc": return "\\Hcircumflex";
				case "Icirc": return "\\Icircumflex";
				case "Jcirc": return "\\Jcircumflex";
				case "NotTildeEqual": return "\\nsimeq";
				case "verbar": return "\\textbar";
				case "spar": return "\\parallel";
				case "LeftTee": return "\\dashv";
				case "ecaron": return "\\ecaron";
				case "duarr": return "\\downuparrows";
				case "Scirc": return "\\Scircumflex";
				case "els": return "\\eqslantless";
				case "downharpoonright": return "\\downharpoonright";
				case "doteqdot": return "\\doteqdot";
				case "dot": return "\\textdotaccent";
				case "npar": return "\\nparallel";
				case "div": return "\\textdiv";
				case "digamma": return "\\greekdigamma";
				case "UpperLeftArrow": return "\\nwarrow";
				case "Acirc": return "\\Acircumflex";
				case "dgr": return "\\greekdelta";
				case "Ccirc": return "\\Ccircumflex";
				case "ddagger": return "\\textddag";
				case "dd": return "\\differentiald";
				case "cuwed": return "\\curlywedge";
				case "curvearrowright": return "\\curvearrowright";
				case "cularr": return "\\curvearrowleft";
				case "varrho": return "\\greekrhoalt";
				case "ldquor": return "\\quotedblbase";
				case "complement": return "\\complement";
				case "clubsuit": return "\\clubsuit";
				case "DZcy": return "\\cyrillicDZHE";
				case "rdquor": return "\\quotedblright";
				case "centerdot": return "\\periodcentered";
				case "fflig": return "\\ffligature";
				case "ccedil": return "\\ccedilla";
				case "caps": return "\\cap";
				case "circledS": return "\\circledS";
				case "oelig": return "\\oeligature";
				case "bot": return "\\bot";
				case "bigvee": return "\\bigvee";
				case "prop": return "\\propto";
				case "bgr": return "\\greekbeta";
				case "minusb": return "\\boxminus";
				case "nvle": return "\\leq";
				case "loarr": return "\\leftarrowtriangle";
				case "horbar": return "\\texthorizontalbar";
				case "Diamond": return "\\diamond";
				case "wcirc": return "\\wcircumflex";
				case "aelig": return "\\aeligature";
				case "ycirc": return "\\ycircumflex";
				case "SupersetEqual": return "\\supseteq";
				case "curvearrowleft": return "\\curvearrowleft";
				case "zwj": return "\\zwj";
				case "psgr": return "\\greekpsi";
				case "atilde": return "\\atilde";
				case "ape": return "\\approxeq";
				case "NotSquareSubsetEqual": return "\\nsqsubseteq";
				case "kcedil": return "\\kcommaaccent";
				case "upsilon": return "\\greekupsilon";
				case "NotTildeTilde": return "\\napprox";
				case "ncedil": return "\\ncommaaccent";
				case "lcedil": return "\\lcommaaccent";
				case "scedil": return "\\scedilla";
				case "equiv": return "\\equiv";
				case "Vvdash": return "\\Vvdash";
				case "DoubleLeftArrow": return "\\Leftarrow";
				case "macr": return "\\textmacron";
				case "tcedil": return "\\tcedilla";
				case "RightCeiling": return "\\rceiling";
				case "diamond": return "\\diamond";
				case "UnderBar": return "\\textunderscore";
				case "Tcedil": return "\\Tcedilla";
				case "Nu": return "\\greekNu";
				case "precnsim": return "\\precnsim";
				case "Sigma": return "\\greekSigma";
				case "Proportional": return "\\propto";
				case "Sacute": return "\\Sacute";
				case "integers": return "\\integers";
				case "RightTee": return "\\vdash";
				case "Kcedil": return "\\Kcommaaccent";
				case "Upsilon": return "\\greekUpsilon";
				case "RightAngleBracket": return "\\rangle";
				case "Pi": return "\\greekPi";
				case "Ncedil": return "\\Ncommaaccent";
				case "Lcedil": return "\\Lcommaaccent";
				case "Scedil": return "\\Scedilla";
				case "Rcedil": return "\\Rcommaaccent";
				case "hookleftarrow": return "\\hookleftarrow";
				case "NotGreaterLess": return "\\ngtrless";
				case "Ocirc": return "\\Ocircumflex";
				case "OElig": return "\\OEligature";
				case "SquareSubsetEqual": return "\\sqsubseteq";
				case "Epsilon": return "\\greekEpsilon";
				case "lsaquo": return "\\guilsingleleft";
				case "NotTilde": return "\\nsim";
				case "LeftUpVector": return "\\upharpoonleft";
				case "NestedGreaterGreater": return "\\gg";
				case "Union": return "\\bigcup";
				case "LowerLeftArrow": return "\\swarrow";
				case "NotEqual": return "\\neq";
				case "Implies": return "\\Rightarrow";
				case "Gt": return "\\gg";
				case "GreaterEqual": return "\\geq";
			}

			return null;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HtmlEntityUnicode Element)
		{
			this.Output.Append(EscapeLaTeX(new string((char)Element.Code, 1)));

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineCode Element)
		{
			this.Output.Append("\\texttt{");
			this.Output.Append(EscapeLaTeX(Element.Code));
			this.Output.Append('}');

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineHTML Element)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(InlineScript Element)
		{
			object Result = await Element.EvaluateExpression();

			await this.RenderObject(Result, Element.AloneInParagraph, Element.Variables);
		}

		/// <summary>
		/// Generates HTML from Script output.
		/// </summary>
		/// <param name="Result">Script output.</param>
		/// <param name="AloneInParagraph">If the script output is to be presented alone in a paragraph.</param>
		/// <param name="Variables">Current variables.</param>
		public async Task RenderObject(object Result, bool AloneInParagraph, Variables Variables)
		{
			if (Result is null)
				return;

			if (Result is XmlDocument Xml)
				Result = await MarkdownDocument.TransformXml(Xml, Variables);
			else if (Result is IToMatrix ToMatrix)
				Result = ToMatrix.ToMatrix();

			if (Result is Graph G)
			{
				PixelInformation Pixels = G.CreatePixels(out GraphSettings GraphSettings);
				byte[] Bin = Pixels.EncodeAsPng();
				string FileName = await ImageContent.GetTemporaryFile(Bin, ImageCodec.FileExtensionPng);

				if (AloneInParagraph)
				{
					this.Output.AppendLine("\\begin{figure}[!hb]");
					this.Output.AppendLine("\\centering");
				}

				this.Output.Append("\\fbox{\\includegraphics[width=");
				this.Output.Append(((GraphSettings.Width * 3) / 4).ToString());
				this.Output.Append("pt, height=");
				this.Output.Append(((GraphSettings.Height * 3) / 4).ToString());
				this.Output.Append("pt]{");
				this.Output.Append(FileName.Replace('\\', '/'));
				this.Output.Append("}}");

				if (AloneInParagraph)
				{
					this.Output.AppendLine();
					this.Output.AppendLine("\\end{figure}");
					this.Output.AppendLine();
				}
			}
			else if (Result is PixelInformation Pixels)
			{
				byte[] Bin = Pixels.EncodeAsPng();
				string FileName = await ImageContent.GetTemporaryFile(Bin, ImageCodec.FileExtensionPng);

				if (AloneInParagraph)
				{
					this.Output.AppendLine("\\begin{figure}[!hb]");
					this.Output.AppendLine("\\centering");
				}

				this.Output.Append("\\fbox{\\includegraphics[width=");
				this.Output.Append(((Pixels.Width * 3) / 4).ToString());
				this.Output.Append("pt, height=");
				this.Output.Append(((Pixels.Height * 3) / 4).ToString());
				this.Output.Append("pt]{");
				this.Output.Append(FileName.Replace('\\', '/'));
				this.Output.Append("}}");

				if (AloneInParagraph)
				{
					this.Output.AppendLine();
					this.Output.AppendLine("\\end{figure}");
					this.Output.AppendLine();
				}
			}
			else if (Result is SKImage Img)
			{
				using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
				{
					byte[] Bin = Data.ToArray();
					string FileName = await ImageContent.GetTemporaryFile(Bin, ImageCodec.FileExtensionPng);

					if (AloneInParagraph)
					{
						this.Output.AppendLine("\\begin{figure}[!hb]");
						this.Output.AppendLine("\\centering");
					}

					this.Output.Append("\\fbox{\\includegraphics[width=");
					this.Output.Append(((Img.Width * 3) / 4).ToString());
					this.Output.Append("pt, height=");
					this.Output.Append(((Img.Height * 3) / 4).ToString());
					this.Output.Append("pt]{");
					this.Output.Append(FileName.Replace('\\', '/'));
					this.Output.Append("}}");

					if (AloneInParagraph)
					{
						this.Output.AppendLine();
						this.Output.AppendLine("\\end{figure}");
						this.Output.AppendLine();
					}
				}
			}
			else if (Result is MarkdownDocument Doc)
			{
				await this.RenderDocument(Doc, true);   // Does not call ProcessAsyncTasks()
				Doc.ProcessAsyncTasks();
			}
			else if (Result is MarkdownContent Markdown)
			{
				Doc = await MarkdownDocument.CreateAsync(Markdown.Markdown, Markdown.Settings ?? new MarkdownSettings());
				await this.RenderDocument(Doc, true);   // Does not call ProcessAsyncTasks()
				Doc.ProcessAsyncTasks();
			}
			else if (Result is Exception ex)
			{
				bool First = true;

				ex = Log.UnnestException(ex);

				this.Output.AppendLine("\\texttt{\\color{red}");

				if (ex is AggregateException ex2)
				{
					foreach (Exception ex3 in ex2.InnerExceptions)
					{
						foreach (string Row in ex3.Message.Replace("\r\n", "\n").
							Replace('\r', '\n').Split('\n'))
						{
							if (First)
								First = false;
							else
								this.Output.AppendLine("\\\\");

							this.Output.Append(EscapeLaTeX(Row));
						}
					}
				}
				else
				{
					foreach (string Row in ex.Message.Replace("\r\n", "\n").
						Replace('\r', '\n').Split('\n'))
					{
						if (First)
							First = false;
						else
							this.Output.AppendLine("\\\\");

						this.Output.Append(EscapeLaTeX(Row));
					}
				}

				this.Output.AppendLine("}");

				if (AloneInParagraph)
				{
					this.Output.AppendLine();
					this.Output.AppendLine();
				}
			}
			else if (Result is ObjectMatrix M && !(M.ColumnNames is null))
			{
				this.Output.AppendLine("\\begin{table}[!hb]");
				this.Output.AppendLine("\\centering");
				this.Output.Append("\\begin{tabular}{");
				foreach (string _ in M.ColumnNames)
					this.Output.Append("|c");

				this.Output.AppendLine("|}");
				this.Output.AppendLine("\\hline");

				bool First = true;

				foreach (string Name in M.ColumnNames)
				{
					if (First)
						First = false;
					else
						this.Output.Append(" & ");

					this.Output.Append(EscapeLaTeX(Name));
				}

				this.Output.AppendLine("\\\\");
				this.Output.AppendLine("\\hline");

				int x, y;

				for (y = 0; y < M.Rows; y++)
				{
					for (x = 0; x < M.Columns; x++)
					{
						if (x > 0)
							this.Output.Append(" & ");

						object Item = M.GetElement(x, y).AssociatedObjectValue;
						if (!(Item is null))
						{
							if (Item is string s2)
								this.Output.Append(EscapeLaTeX(s2));
							else if (Item is MarkdownElement Element)
							{
								this.Output.Append('{');
								await Element.Render(this);
								this.Output.Append('}');
							}
							else
							{
								this.Output.Append('{');
								this.Output.Append(EscapeLaTeX(Expression.ToString(Item)));
								this.Output.Append('}');
							}
						}

						this.Output.Append("</td>");
					}

					this.Output.AppendLine("\\\\");
				}

				this.Output.AppendLine("\\hline");
				this.Output.AppendLine("\\end{tabular}");
				this.Output.AppendLine("\\end{table}");
				this.Output.AppendLine();
			}
			else if (Result is Array A)
			{
				foreach (object Item in A)
					await this.RenderObject(Item, false, Variables);
			}
			else
				this.Output.Append(EscapeLaTeX(Result?.ToString() ?? string.Empty));

			if (AloneInParagraph)
			{
				this.Output.AppendLine();
				this.Output.AppendLine();
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InlineText Element)
		{
			this.Output.Append(EscapeLaTeX(Element.Value));
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Insert Element)
		{
			this.Output.Append("\\emph{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LineBreak Element)
		{
			this.Output.AppendLine("\\newline");
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Link Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(LinkReference Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(MetaReference Element)
		{
			bool FirstOnRow = true;

			if (Element.TryGetMetaData(out KeyValuePair<string, bool>[] Values))
			{
				foreach (KeyValuePair<string, bool> P in Values)
				{
					if (FirstOnRow)
						FirstOnRow = false;
					else
						this.Output.Append(' ');

					this.Output.Append(EscapeLaTeX(P.Key));

					if (P.Value)
					{
						this.Output.AppendLine();
						FirstOnRow = true;
					}
				}
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Model.SpanElements.Multimedia Element)
		{
			IMultimediaLatexRenderer Renderer = Element.MultimediaHandler<IMultimediaLatexRenderer>();
			if (Renderer is null)
				return this.RenderChildren(Element);
			else
				return Renderer.RenderLatex(this, Element.Items, Element.Children, Element.AloneInParagraph, Element.Document);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(MultimediaReference Element)
		{
			Model.SpanElements.Multimedia Multimedia = Element.Document.GetReference(Element.Label);

			if (!(Multimedia is null))
			{
				IMultimediaLatexRenderer Renderer = Multimedia.MultimediaHandler<IMultimediaLatexRenderer>();
				if (!(Renderer is null))
					return Renderer.RenderLatex(this, Multimedia.Items, Element.Children, Element.AloneInParagraph, Element.Document);
			}

			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(StrikeThrough Element)
		{
			this.Output.Append("\\strike{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Strong Element)
		{
			this.Output.Append("\\textbf{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SubScript Element)
		{
			this.Output.Append("\\textsubscript{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(SuperScript Element)
		{
			this.Output.Append("\\textsuperscript{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Underline Element)
		{
			this.Output.Append("\\underline{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		#endregion

		#region Block elements

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BlockQuote Element)
		{
			this.Output.AppendLine("\\begin{quote}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{quote}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(BulletList Element)
		{
			this.Output.AppendLine("\\begin{itemize}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{itemize}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CenterAligned Element)
		{
			this.Output.AppendLine("\\begin{center}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{center}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(CodeBlock Element)
		{
			ICodeContentLatexRenderer Renderer = Element.CodeContentHandler<ICodeContentLatexRenderer>();

			if (!(Renderer is null))
			{
				try
				{
					if (await Renderer.RenderLatex(this, Element.Rows, Element.Language, Element.Indent, Element.Document))
						return;
				}
				catch (Exception ex)
				{
					bool First = true;

					ex = Log.UnnestException(ex);

					this.Output.AppendLine("\\texttt{\\color{red}");

					if (ex is AggregateException ex2)
					{
						foreach (Exception ex3 in ex2.InnerExceptions)
						{
							foreach (string Row in ex3.Message.Replace("\r\n", "\n").
								Replace('\r', '\n').Split('\n'))
							{
								if (First)
									First = false;
								else
									this.Output.AppendLine("\\\\");

								this.Output.Append(EscapeLaTeX(Row));
							}
						}
					}
					else
					{
						foreach (string Row in ex.Message.Replace("\r\n", "\n").
							Replace('\r', '\n').Split('\n'))
						{
							if (First)
								First = false;
							else
								this.Output.AppendLine("\\\\");

							this.Output.Append(EscapeLaTeX(Row));
						}
					}

					this.Output.AppendLine("}");
					this.Output.AppendLine();
				}
			}

			this.Output.Append("\\texttt{");

			int i;

			for (i = Element.Start; i <= Element.End; i++)
			{
				this.Output.Append(Element.IndentString);
				this.Output.AppendLine(Element.Rows[i]);
			}

			this.Output.AppendLine("}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(CommentBlock Element)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DefinitionDescriptions Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DefinitionList Element)
		{
			int State = 0;

			this.Output.AppendLine("\\begin{description}");

			foreach (MarkdownElement E in Element.Children)
			{
				if (E is DefinitionTerms Terms)
				{
					switch (State)
					{
						case 0:
							this.Output.Append("\\item[");
							await Terms.Render(this);
							State++;
							break;

						case 1:
							this.Output.Append(", ");
							await Terms.Render(this);
							break;

						case 2:
							this.Output.AppendLine("}");
							this.Output.Append("\\item[");
							State--;
							await Terms.Render(this);
							break;
					}
				}
				else if (E is DefinitionDescriptions Descriptions)
				{
					switch (State)
					{
						case 0:
							this.Output.Append("\\item{");
							await Descriptions.Render(this);
							State += 2;
							break;

						case 1:
							this.Output.Append("]{");
							await Descriptions.Render(this);
							State++;
							break;

						case 2:
							this.Output.AppendLine();
							await Descriptions.Render(this);
							break;
					}
				}
			}

			switch (State)
			{
				case 1:
					this.Output.AppendLine("]{}");
					break;

				case 2:
					this.Output.AppendLine("}");
					break;
			}

			this.Output.AppendLine("\\end{description}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(DefinitionTerms Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(DeleteBlocks Element)
		{
			this.Output.Append("\\strike{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Footnote Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Header Element)
		{
			string Command;

			if (!this.abstractOutput)
			{
				if (Element.HasOneChild &&
					Element.FirstChild is InlineText Text &&
					string.Compare(Text.Value, "abstract", true) == 0)
				{
					this.Output.AppendLine("\\begin{abstract}");
					this.abstractOutput = true;
					this.inAbstract = true;
					return;
				}
				else
				{
					if (this.Document.TryGetMetaData("DESCRIPTION", out KeyValuePair<string, bool>[] Values))
					{
						this.Output.AppendLine("\\begin{abstract}");

						foreach (KeyValuePair<string, bool> P in Values)
							this.Output.AppendLine(EscapeLaTeX(P.Key));

						this.Output.AppendLine("\\end{abstract}");
						this.abstractOutput = true;
					}
				}
			}

			switch (this.LatexSettings.DocumentClass)
			{
				case LaTeXDocumentClass.Book:
				case LaTeXDocumentClass.Report:
					switch (Element.Level)
					{
						case 1:
							Command = "part";
							break;

						case 2:
							Command = "chapter";
							break;

						case 3:
							Command = "section";
							break;

						case 4:
							Command = "subsection";
							break;

						case 5:
							Command = "subsubsection";
							break;

						case 6:
							Command = "paragraph";
							break;

						case 7:
						default:
							Command = "subparagraph";
							break;
					}
					break;

				case LaTeXDocumentClass.Article:
				case LaTeXDocumentClass.Standalone:
				default:
					switch (Element.Level)
					{
						case 1:
							Command = "section";
							break;

						case 2:
							Command = "subsection";
							break;

						case 3:
							Command = "subsubsection";
							break;

						case 4:
							Command = "paragraph";
							break;

						case 5:
						default:
							Command = "subparagraph";
							break;
					}
					break;
			}

			if (this.inAbstract)
			{
				this.Output.AppendLine("\\end{abstract}");
				this.inAbstract = false;
			}

			this.Output.Append('\\');
			this.Output.Append(Command);
			this.Output.Append("*{");

			await this.RenderChildren(Element);

			this.Output.AppendLine("}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(HorizontalRule Element)
		{
			this.Output.AppendLine("\\hrulefill");
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(HtmlBlock Element)
		{
			await this.RenderChildren(Element);

			this.Output.AppendLine();
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(InsertBlocks Element)
		{
			this.Output.Append("\\emph{");
			await this.RenderChildren(Element);
			this.Output.Append('}');
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(InvisibleBreak Element)
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(LeftAligned Element)
		{
			this.Output.AppendLine("\\begin{flushleft}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{flushleft}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(MarginAligned Element)
		{
			this.Output.AppendLine("\\begin{justify}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{justify}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(NestedBlock Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedItem Element)
		{
			if (Element.NumberExplicit)
			{
				this.Output.Append("\\item[");
				this.Output.Append(Element.Number.ToString());
				this.Output.Append("]{");
			}
			else
				this.Output.Append("\\item{");

			await this.RenderChild(Element);

			this.Output.AppendLine("}");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(NumberedList Element)
		{
			this.Output.AppendLine("\\begin{enumerate}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{enumerate}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Paragraph Element)
		{
			await this.RenderChildren(Element);

			this.Output.AppendLine();
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(RightAligned Element)
		{
			this.Output.AppendLine("\\begin{flushright}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{flushright}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(Sections Element)
		{
			return this.RenderChildren(Element);
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override Task Render(SectionSeparator Element)
		{
			this.Output.AppendLine();
			this.Output.AppendLine("\\newpage");
			this.Output.AppendLine();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(Table Element)
		{
			MarkdownElement E;
			MarkdownElement[] Row;
			TextAlignment?[] CellAlignments;
			int NrRows, RowIndex;
			int NrColumns = Element.Columns;
			string s;
			int i, j, k;

			this.Output.AppendLine("\\begin{table}[!hb]");
			this.Output.AppendLine("\\centering");
			this.Output.Append("\\begin{tabular}{");
			foreach (TextAlignment Alignment in Element.ColumnAlignments)
			{
				this.Output.Append('|');
				this.RenderAlignment(Alignment);
			}

			this.Output.AppendLine("|}");
			this.Output.AppendLine("\\hline");

			NrRows = Element.Headers.Length;
			for (RowIndex = 0; RowIndex < NrRows; RowIndex++)
			{
				Row = Element.Headers[RowIndex];
				CellAlignments = Element.HeaderCellAlignments[RowIndex];

				for (i = 0; i < NrColumns; i++)
				{
					if (i > 0)
						this.Output.Append(" & ");

					k = 1;
					j = i + 1;
					while (j < NrColumns && Row[j++] is null)
						k++;

					if (k > 1)
					{
						this.Output.Append("\\multicolumn{");
						this.Output.Append(k.ToString());
						this.Output.Append("}{|");
						this.RenderAlignment(CellAlignments[i] ?? Element.ColumnAlignments[i]);
						this.Output.Append("|}{");
					}

					E = Row[i];
					if (!(E is null))
						await E.Render(this);

					if (k > 1)
					{
						this.Output.Append('}');
						i += k - 1;
					}
				}

				this.Output.AppendLine("\\\\");
			}

			this.Output.AppendLine("\\hline");

			NrRows = Element.Rows.Length;
			for (RowIndex = 0; RowIndex < NrRows; RowIndex++)
			{
				Row = Element.Rows[RowIndex];
				CellAlignments = Element.RowCellAlignments[RowIndex];

				for (i = 0; i < NrColumns; i++)
				{
					if (i > 0)
						this.Output.Append(" & ");

					k = 1;
					j = i + 1;
					while (j < NrColumns && Row[j++] is null)
						k++;

					if (k > 1)
					{
						this.Output.Append("\\multicolumn{");
						this.Output.Append(k.ToString());
						this.Output.Append("}{|");
						this.RenderAlignment(CellAlignments[i] ?? Element.ColumnAlignments[i]);
						this.Output.Append("|}{");
					}

					E = Row[i];
					if (!(E is null))
						await E.Render(this);

					if (k > 1)
						this.Output.Append('}');
				}

				this.Output.AppendLine("\\\\");
			}

			this.Output.AppendLine("\\hline");
			this.Output.AppendLine("\\end{tabular}");

			if (!string.IsNullOrEmpty(Element.Id))
			{
				this.Output.Append("\\caption{");

				s = string.IsNullOrEmpty(Element.Caption) ? Element.Id : Element.Caption;

				this.Output.Append(EscapeLaTeX(s));

				this.Output.AppendLine("}");
				this.Output.Append("\\label{");

				this.Output.Append(EscapeLaTeX(Element.Id));

				this.Output.AppendLine("}");
			}

			this.Output.AppendLine("\\end{table}");
			this.Output.AppendLine();
		}

		private void RenderAlignment(TextAlignment Alignment)
		{
			switch (Alignment)
			{
				case TextAlignment.Left:
				default:
					this.Output.Append('l');
					break;

				case TextAlignment.Center:
					this.Output.Append('c');
					break;

				case TextAlignment.Right:
					this.Output.Append('r');
					break;
			}
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskItem Element)
		{
			this.Output.Append("\\item");

			if (Element.IsChecked)
				this.Output.Append("[\\checked]");

			this.Output.Append('{');
			await this.RenderChild(Element);
			this.Output.AppendLine("}");
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(TaskList Element)
		{
			this.Output.AppendLine("\\begin{tasklist}");

			await this.RenderChildren(Element);

			this.Output.AppendLine("\\end{tasklist}");
			this.Output.AppendLine();
		}

		/// <summary>
		/// Renders <paramref name="Element"/>.
		/// </summary>
		/// <param name="Element">Element to render</param>
		public override async Task Render(UnnumberedItem Element)
		{
			this.Output.Append("\\item{");
			await this.RenderChild(Element);
			this.Output.AppendLine("}");
		}

		#endregion

	}
}
