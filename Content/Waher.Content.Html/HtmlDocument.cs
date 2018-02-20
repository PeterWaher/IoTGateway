using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content.Html.Elements;

namespace Waher.Content.Html
{
	/// <summary>
	/// HTML document.
	/// </summary>
	public class HtmlDocument
	{
		private string html;
		private HtmlElement root = null;

		/// <summary>
		/// HTML document.
		/// </summary>
		/// <param name="Html">HTML text.</param>
		public HtmlDocument(string Html)
		{
			this.html = Html;
		}

		/// <summary>
		/// HTML text.
		/// </summary>
		public string Html
		{
			get { return this.html; }
		}

		/// <summary>
		/// Root element.
		/// </summary>
		public HtmlElement Root
		{
			get
			{
				if (this.root == null)
					this.Parse();

				return this.root;
			}
		}

		private void Parse()
		{
			LinkedList<HtmlElement> Stack = new LinkedList<HtmlElement>();
			StringBuilder sb = new StringBuilder();
			HtmlElement CurrentElement = null;
			HtmlAttribute CurrentAttribute = null;
			string Name = string.Empty;
			string s;
			int State = 0;
			char EndChar = '\x00';
			bool Empty = true;

			foreach (char ch in this.html)
			{
				switch (State)
				{
					case 0:     // Waiting for <
						if (ch == '<')
						{
							if (!Empty)
							{
								CurrentElement?.Add(new HtmlText(CurrentElement, sb.ToString()));

								sb.Clear();
								Empty = true;
							}

							State++;
						}
						else if (ch == '&')
						{
							if (!Empty)
							{
								CurrentElement?.Add(new HtmlText(CurrentElement, sb.ToString()));

								sb.Clear();
								Empty = true;
							}

							State = 10;
						}
						else
						{
							sb.Append(ch);
							Empty = false;
						}
						break;

					case 1:     // Waiting for !, /, attributes or >
						if (ch == '!' && this.root == null)
							State++;
						else if (ch == '>')
						{
							if (Empty)
							{
								sb.Append('<');
								sb.Append(ch);
								Empty = false;
								State = 0;
							}
							else
							{
								CurrentElement = this.CreateElement(CurrentElement, sb.ToString());

								sb.Clear();
								Empty = true;

								State = 0;
							}
						}
						else if (ch == '/')
						{
							if (Empty)  // Closing tag
								State = 4;
							else
								State = 3;
						}
						else if (ch <= ' ')
						{
							if (Empty)
							{
								sb.Append('<');
								sb.Append(ch);
								Empty = false;
								State = 0;
							}
							else
							{
								CurrentElement = this.CreateElement(CurrentElement, sb.ToString());

								sb.Clear();
								Empty = true;

								State = 5;
							}
						}
						else if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
						{
							sb.Append(ch);
							Empty = false;
						}
						else
						{
							sb.Insert(0, '<');
							sb.Append(ch);
							Empty = false;
							State = 0;
						}
						break;

					case 2: // Skip DTD
						if (ch == '>')
						{
							if (!Empty)
							{
								sb.Clear();
								Empty = true;
							}

							State = 0;
						}
						break;

					case 3: // Wait for > at end of empty element.
						if (ch == '>')
						{
							this.CreateElement(CurrentElement, sb.ToString());

							sb.Clear();
							Empty = true;

							State = 0;
						}
						else
						{
							sb.Append('/');
							sb.Append(ch);
							Empty = false;
							State = 1;
						}
						break;

					case 4:     // Closing tag
						if (ch == '>')
						{
							s = sb.ToString().ToUpper();
							sb.Clear();
							Empty = true;

							if (CurrentElement != null)
							{
								if (CurrentElement.Name == s)
									CurrentElement = CurrentElement.Parent as HtmlElement;
								else
								{
									HtmlElement Loop = CurrentElement.Parent as HtmlElement;

									while (Loop != null && Loop.Name != s)
										Loop = Loop.Parent as HtmlElement;

									if (Loop != null)
										CurrentElement = Loop.Parent as HtmlElement;
								}
							}

							State = 0;
						}
						else
						{
							sb.Append(ch);
							Empty = false;
						}
						break;

					case 5: // Waiting for attribute
						if (ch == '>')
							State = 0;
						else if (ch == '/')
							State = 9;
						else if (ch == '=')
						{
							Name = string.Empty;
							State = 7;
						}
						else if (ch > ' ')
						{
							if (IsNameCharacter(ch))
							{
								sb.Append(ch);
								Empty = false;
								State++;
							}
							else
							{
								sb.Clear();
								Empty = true;

								State = 13;
							}
						}
						break;

					case 6: // Attribute name
						if (ch == '>')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(CurrentElement, sb.ToString(), string.Empty));

							sb.Clear();
							Empty = true;

							State = 0;
						}
						else if (ch == '/')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(CurrentElement, sb.ToString(), string.Empty));

							sb.Clear();
							Empty = true;

							State = 9;
						}
						else if (ch == '=')
						{
							Name = sb.ToString();

							sb.Clear();
							Empty = true;

							State = 7;
						}
						else
						{
							if (ch <= ' ')
							{
								CurrentElement.AddAttribute(new HtmlAttribute(CurrentElement, sb.ToString(), string.Empty));

								sb.Clear();
								Empty = true;

								State--;
							}
							else if (IsNameCharacter(ch))
							{
								sb.Append(ch);
								Empty = false;
							}
							else
							{
								sb.Clear();
								Empty = true;

								State = 13;
							}
						}
						break;

					case 7: // Wait for value.
						if (ch == '"' || ch == '\'')
						{
							CurrentAttribute = new HtmlAttribute(CurrentElement, Name);
							CurrentElement.AddAttribute(CurrentAttribute);

							EndChar = ch;
							State = 11;
						}
						else if (ch == '>')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(CurrentElement, Name, string.Empty));
							State = 0;
						}
						else if (ch == '/')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(CurrentElement, Name, string.Empty));
							State = 9;
						}
						else if (ch > ' ')
						{
							sb.Append(ch);
							Empty = false;
							State++;
						}
						break;

					case 8: // Non-encapsulated attribute value
						if (ch <= ' ')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(CurrentElement, Name, sb.ToString()));

							sb.Clear();
							Empty = true;

							State = 5;
						}
						else if (ch == '>')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(CurrentElement, Name, sb.ToString()));

							sb.Clear();
							Empty = true;

							State = 0;
						}
						else if (ch == '/')
							State = 9;
						else
						{
							sb.Append(ch);
							Empty = false;
						}
						break;

					case 9: // Waiting for > at end of empty element
						if (ch == '>')
						{
							CurrentElement = CurrentElement.Parent as HtmlElement;
							State = 0;
						}
						break;

					case 10: // Entity
						if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
						{
							sb.Append(ch);
							Empty = false;
						}
						else if (ch == ';')
						{
							CurrentElement?.Add(new HtmlEntity(CurrentElement, sb.ToString()));

							sb.Clear();
							Empty = true;

							State = 0;
						}
						else
						{
							sb.Insert(0, '&');
							sb.Append(ch);
							Empty = false;
							State = 0;
						}
						break;

					case 11:    // Encapsulated attribute value
						if (ch == EndChar)
						{
							if (!Empty)
							{
								s = sb.ToString();

								if (CurrentAttribute.HasSegments)
									CurrentAttribute.Add(new HtmlText(CurrentAttribute, s));
								else
									CurrentAttribute.Value = s;

								sb.Clear();
								Empty = true;
							}

							State = 5;
						}
						else if (ch == '&')
						{
							if (!Empty)
							{
								CurrentAttribute.Add(new HtmlText(CurrentAttribute, sb.ToString()));

								sb.Clear();
								Empty = true;
							}

							State = 12;
						}
						else
						{
							sb.Append(ch);
							Empty = false;
						}
						break;

					case 12: // Entity in attribute value
						if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
						{
							sb.Append(ch);
							Empty = false;
						}
						else if (ch == ';')
						{
							CurrentAttribute.Add(new HtmlEntity(CurrentAttribute, sb.ToString()));

							sb.Clear();
							Empty = true;

							State = 11;
						}
						else
						{
							sb.Insert(0, '&');
							sb.Append(ch);
							Empty = false;
							State = 11;
						}
						break;

					case 13:    // Ignore everything until end of tag.
						if (ch == '>')
						{
							sb.Clear();
							Empty = true;

							State = 0;
						}
						else if (ch == '/')
							State = 9;
						break;

					default:
						throw new Exception("Internal error: Unrecognized state.");
				}
			}

			if (!Empty)
			{
				switch (State)
				{
					case 0:     // Waiting for <
					case 1:     // Waiting for !, /, attributes or >
					case 3: // Wait for > at end of empty element.
						CurrentElement?.Add(new HtmlText(CurrentElement, sb.ToString()));
						break;

					case 6: // Attribute name
						CurrentElement.AddAttribute(new HtmlAttribute(CurrentElement, sb.ToString(), string.Empty));
						break;

					case 8: // Non-encapsulated attribute value
						CurrentElement.AddAttribute(new HtmlAttribute(CurrentElement, Name, sb.ToString()));
						break;

					case 10: // Entity
						sb.Insert(0, '&');
						CurrentElement?.Add(new HtmlText(CurrentElement, sb.ToString()));
						break;

					case 11:    // Encapsulated attribute value
						s = sb.ToString();

						if (CurrentAttribute.HasSegments)
							CurrentAttribute.Add(new HtmlText(CurrentAttribute, s));
						else
							CurrentAttribute.Value = s;
						break;

					case 12: // Entity in attribute value
						sb.Insert(0, '&');
						CurrentAttribute.Add(new HtmlText(CurrentElement, sb.ToString()));
						break;
				}
			}

			// TODO: Elements that cannot take children (BR, HR)
		}

		private static bool IsNameCharacter(char ch)
		{
			if (ch == '-' || ch == '.')
				return true;

			if (ch < '0')
				return false;

			if (ch <= '9')
				return true;

			if (ch == ':')
				return true;

			if (ch < 'A')
				return false;

			if (ch <= 'Z')
				return true;

			if (ch == '_')
				return true;

			if (ch < 'a')
				return false;

			if (ch <= 'z')
				return true;

			if (ch == '\xb7')
				return true;

			if (ch < '\xc0')
				return false;

			if (ch == '\xd7' || ch == '\xf7')
				return false;

			if (ch == '\x037e')
				return false;

			if (ch <= '\x1fff')
				return true;

			if (ch == '\x200c' || ch == '\x200d' || ch == '\x203f' || ch == '\x2040')
				return true;

			if (ch < '\x2070')
				return false;

			if (ch <= '\x218f')
				return true;

			if (ch < '\x2c00')
				return false;

			if (ch <= '\x2fef')
				return true;

			if (ch < '\x3001')
				return false;

			if (ch <= '\xd7ff')
				return true;

			if (ch < '\xf900')
				return false;

			if (ch <= '\xfdcf')
				return true;

			if (ch < '\xfdf0')
				return false;

			if (ch <= '\xfffd')
				return true;

			return false;
		}

		private HtmlElement CreateElement(HtmlElement Parent, string TagName)
		{
			HtmlElement Result;

			if (Parent != null && Parent.IsEmptyElement)
				Parent = Parent.Parent as HtmlElement;

			TagName = TagName.ToUpper();

			switch (TagName)
			{
				case "A": Result = new A(Parent); break;
				case "ABBR": Result = new Abbr(Parent); break;
				case "ACRONYM": Result = new Acronym(Parent); break;
				case "ADDRESS": Result = new Address(Parent); break;
				case "APPLET": Result = new Applet(Parent); break;
				case "AREA": Result = new Area(Parent); break;
				case "ARTICLE": Result = new Article(Parent); break;
				case "ASIDE": Result = new Aside(Parent); break;
				case "AUDIO": Result = new Elements.Audio(Parent); break;
				case "B": Result = new B(Parent); break;
				case "BASE": Result = new Base(Parent); break;
				case "BASEFONT": Result = new BaseFont(Parent); break;
				case "BDI": Result = new Bdi(Parent); break;
				case "BDO": Result = new Bdo(Parent); break;
				case "BGSOUND": Result = new BgSound(Parent); break;
				case "BIG": Result = new Big(Parent); break;
				case "BLINK": Result = new BLink(Parent); break;
				case "BLOCKQUOTE": Result = new BlockQuote(Parent); break;
				case "BODY": Result = new Body(Parent); break;
				case "BR": Result = new Br(Parent); break;
				case "BUTTON": Result = new Button(Parent); break;
				case "CANVAS": Result = new Canvas(Parent); break;
				case "CAPTION": Result = new Caption(Parent); break;
				case "CENTER": Result = new Center(Parent); break;
				case "CITE": Result = new Cite(Parent); break;
				case "CODE": Result = new Code(Parent); break;
				case "COL": Result = new Col(Parent); break;
				case "COLGROUP": Result = new ColGroup(Parent); break;
				case "COMMAND": Result = new Command(Parent); break;
				case "CONTENT": Result = new Elements.Content(Parent); break;
				case "DATA": Result = new Data(Parent); break;
				case "DATALIST": Result = new DataList(Parent); break;
				case "DD": Result = new Dd(Parent); break;
				case "DEL": Result = new Del(Parent); break;
				case "DETAILS": Result = new Details(Parent); break;
				case "DFN": Result = new Dfn(Parent); break;
				case "DIALOG": Result = new Dialog(Parent); break;
				case "DIR": Result = new Dir(Parent); break;
				case "DIV": Result = new Div(Parent); break;
				case "DL": Result = new Dl(Parent); break;
				case "DT": Result = new Dt(Parent); break;
				case "ELEMENT": Result = new Element(Parent); break;
				case "EM": Result = new Em(Parent); break;
				case "EMBED": Result = new Embed(Parent); break;
				case "FIELDSET": Result = new FieldSet(Parent); break;
				case "FIGCAPTION": Result = new FigCaption(Parent); break;
				case "FIGURE": Result = new FigCaption(Parent); break;
				case "FONT": Result = new Font(Parent); break;
				case "FOOTER": Result = new Footer(Parent); break;
				case "FORM": Result = new Form(Parent); break;
				case "FRAME": Result = new Frame(Parent); break;
				case "FRAMESET": Result = new FrameSet(Parent); break;
				case "H1": Result = new Hn(Parent, 1); break;
				case "H2": Result = new Hn(Parent, 2); break;
				case "H3": Result = new Hn(Parent, 3); break;
				case "H4": Result = new Hn(Parent, 4); break;
				case "H5": Result = new Hn(Parent, 5); break;
				case "H6": Result = new Hn(Parent, 6); break;
				case "H7": Result = new Hn(Parent, 7); break;
				case "H8": Result = new Hn(Parent, 8); break;
				case "H9": Result = new Hn(Parent, 9); break;
				case "HEAD": Result = new Head(Parent); break;
				case "HEADER": Result = new Header(Parent); break;
				case "HGROUP": Result = new HGroup(Parent); break;
				case "HR": Result = new Hr(Parent); break;
				case "HTML": Result = new Elements.Html(Parent); break;
				case "I": Result = new I(Parent); break;
				case "IFRAME": Result = new IFrame(Parent); break;
				case "IMAGE": Result = new Image(Parent); break;
				case "IMG": Result = new Img(Parent); break;
				case "INPUT": Result = new Input(Parent); break;
				case "INS": Result = new Ins(Parent); break;
				case "ISINDEX": Result = new IsIndex(Parent); break;
				case "KBD": Result = new Kbd(Parent); break;
				case "KEYGEN": Result = new Keygen(Parent); break;
				case "LABEL": Result = new Label(Parent); break;
				case "LEGEND": Result = new Legend(Parent); break;
				case "LI": Result = new Li(Parent); break;
				case "LINK": Result = new Link(Parent); break;
				case "LISTING": Result = new Listing(Parent); break;
				case "MAIN": Result = new Main(Parent); break;
				case "MAP": Result = new Map(Parent); break;
				case "MARK": Result = new Mark(Parent); break;
				case "MARQUEE": Result = new Marquee(Parent); break;
				case "MENU": Result = new Menu(Parent); break;
				case "MENUITEM": Result = new MenuItem(Parent); break;
				case "META": Result = new Meta(Parent); break;
				case "METER": Result = new Meter(Parent); break;
				case "MULTICOL": Result = new MultiCol(Parent); break;
				case "NAV": Result = new Nav(Parent); break;
				case "NEXTID": Result = new NextId(Parent); break;
				case "NOBR": Result = new NoBr(Parent); break;
				case "NOEMBED": Result = new NoEmbed(Parent); break;
				case "NOFRAMES": Result = new NoFrames(Parent); break;
				case "NOSCRIPT": Result = new NoScript(Parent); break;
				case "OBJECT": Result = new Elements.Object(Parent); break;
				case "OL": Result = new Ol(Parent); break;
				case "OPTGROUP": Result = new OptGroup(Parent); break;
				case "OPTION": Result = new Option(Parent); break;
				case "OUTPUT": Result = new Output(Parent); break;
				case "P": Result = new P(Parent); break;
				case "PARAM": Result = new Param(Parent); break;
				case "PICTURE": Result = new Picture(Parent); break;
				case "PLAINTEXT": Result = new PlainText(Parent); break;
				case "PRE": Result = new Pre(Parent); break;
				case "PROGRESS": Result = new Progress(Parent); break;
				case "Q": Result = new Q(Parent); break;
				case "RP": Result = new Rp(Parent); break;
				case "RT": Result = new Rt(Parent); break;
				case "RTC": Result = new Rtc(Parent); break;
				case "RUBY": Result = new Ruby(Parent); break;
				case "S": Result = new S(Parent); break;
				case "SAMP": Result = new Samp(Parent); break;
				case "SCRIPT": Result = new Elements.Script(Parent); break;
				case "SECTION": Result = new Section(Parent); break;
				case "SELECT": Result = new Select(Parent); break;
				case "SHADOW": Result = new Shadow(Parent); break;
				case "SLOT": Result = new Slot(Parent); break;
				case "SMALL": Result = new Small(Parent); break;
				case "SOURCE": Result = new Source(Parent); break;
				case "SPACER": Result = new Spacer(Parent); break;
				case "SPAN": Result = new Span(Parent); break;
				case "STRIKE": Result = new Strike(Parent); break;
				case "STRONG": Result = new Strong(Parent); break;
				case "STYLE": Result = new Style(Parent); break;
				case "SUB": Result = new Sub(Parent); break;
				case "SUMMARY": Result = new Summary(Parent); break;
				case "SUP": Result = new Sup(Parent); break;
				case "TABLE": Result = new Table(Parent); break;
				case "TBODY": Result = new TBody(Parent); break;
				case "TD": Result = new Td(Parent); break;
				case "TEMPLATE": Result = new Template(Parent); break;
				case "TEXTAREA": Result = new TextArea(Parent); break;
				case "TFOOT": Result = new TFoot(Parent); break;
				case "TH": Result = new Th(Parent); break;
				case "THEAD": Result = new THead(Parent); break;
				case "TIME": Result = new Time(Parent); break;
				case "TITLE": Result = new Title(Parent); break;
				case "TR": Result = new Tr(Parent); break;
				case "TRACK": Result = new Track(Parent); break;
				case "TT": Result = new Tt(Parent); break;
				case "U": Result = new U(Parent); break;
				case "UL": Result = new Ul(Parent); break;
				case "VAR": Result = new Var(Parent); break;
				case "VIDEO": Result = new Elements.Video(Parent); break;
				case "WBR": Result = new Wbr(Parent); break;
				case "XMP": Result = new Xmp(Parent); break;
				default: Result = new HtmlElement(Parent, TagName); break;
			}

			if (Parent == null)
			{
				if (this.root == null)
					this.root = Result;
			}
			else
				Parent?.Add(Result);

			return Result;
		}

		/// <summary>
		/// Exports the HTML document to XML.
		/// </summary>
		/// <param name="Output">XML Output</param>
		public void Export(XmlWriter Output)
		{
			this.root?.Export(Output);
		}

	}
}
