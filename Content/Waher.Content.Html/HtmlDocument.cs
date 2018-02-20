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
		private string htmlText;
		private HtmlElement root = null;
		private Elements.Html html = null;
		private Title title = null;
		private Body body = null;
		private Article article = null;
		private Main main = null;
		private Head head = null;
		private Header header = null;
		private Footer footer = null;
		private Details details = null;
		private Summary summary = null;
		private LinkedList<DtdInstruction> dtd = null;
		private LinkedList<ProcessingInstruction> processingInstructions = null;
		private LinkedList<Link> link = null;
		private LinkedList<Meta> meta = null;
		private LinkedList<Style> style = null;
		private LinkedList<Address> address = null;
		private LinkedList<Aside> aside = null;
		private LinkedList<Nav> nav = null;
		private LinkedList<Section> section = null;
		private LinkedList<Dialog> dialog = null;
		private LinkedList<Figure> figure = null;
		private LinkedList<Elements.Audio> audio = null;
		private LinkedList<Elements.Video> video = null;
		private LinkedList<Img> img = null;
		private LinkedList<Picture> picture = null;
		private LinkedList<Cite> cite = null;
		private LinkedList<Data> data = null;
		private LinkedList<Time> time = null;
		private LinkedList<Elements.Script> script = null;
		private LinkedList<Form> form = null;

		/// <summary>
		/// HTML document.
		/// </summary>
		/// <param name="Html">HTML text.</param>
		public HtmlDocument(string Html)
		{
			this.htmlText = Html;
		}

		/// <summary>
		/// HTML text.
		/// </summary>
		public string HtmlText
		{
			get { return this.htmlText; }
		}

		private void AssertParsed()
		{
			if (this.root == null)
				this.Parse();
		}

		/// <summary>
		/// Root element.
		/// </summary>
		public HtmlElement Root
		{
			get
			{
				this.AssertParsed();
				return this.root;
			}
		}

		/// <summary>
		/// First HTML element of document, if found, null otherwise.
		/// </summary>
		public Elements.Html Html
		{
			get
			{
				this.AssertParsed();
				return this.html;
			}
		}

		/// <summary>
		/// First TITLE element of document, if found, null otherwise.
		/// </summary>
		public Title Title
		{
			get
			{
				this.AssertParsed();
				return this.title;
			}
		}

		/// <summary>
		/// First HEAD element of document, if found, null otherwise.
		/// </summary>
		public Head Head
		{
			get
			{
				this.AssertParsed();
				return this.head;
			}
		}

		/// <summary>
		/// First BODY element of document, if found, null otherwise.
		/// </summary>
		public Body Body
		{
			get
			{
				this.AssertParsed();
				return this.body;
			}
		}

		/// <summary>
		/// First ARTICLE element of document, if found, null otherwise.
		/// </summary>
		public Article Article
		{
			get
			{
				this.AssertParsed();
				return this.article;
			}
		}

		/// <summary>
		/// First MAIN element of document, if found, null otherwise.
		/// </summary>
		public Main Main
		{
			get
			{
				this.AssertParsed();
				return this.main;
			}
		}

		/// <summary>
		/// First HEADER element of document, if found, null otherwise.
		/// </summary>
		public Header Header
		{
			get
			{
				this.AssertParsed();
				return this.header;
			}
		}

		/// <summary>
		/// First FOOTER element of document, if found, null otherwise.
		/// </summary>
		public Footer Footer
		{
			get
			{
				this.AssertParsed();
				return this.footer;
			}
		}

		/// <summary>
		/// First DETAILS element of document, if found, null otherwise.
		/// </summary>
		public Details Details
		{
			get
			{
				this.AssertParsed();
				return this.details;
			}
		}

		/// <summary>
		/// First SUMMARY element of document, if found, null otherwise.
		/// </summary>
		public Summary Summary
		{
			get
			{
				this.AssertParsed();
				return this.summary;
			}
		}

		/// <summary>
		/// LINK elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Link> Link
		{
			get
			{
				this.AssertParsed();
				return this.link;
			}
		}

		/// <summary>
		/// META elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Meta> Meta
		{
			get
			{
				this.AssertParsed();
				return this.meta;
			}
		}

		/// <summary>
		/// STYLE elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Style> Style
		{
			get
			{
				this.AssertParsed();
				return this.style;
			}
		}

		/// <summary>
		/// ADDRESS elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Address> Address
		{
			get
			{
				this.AssertParsed();
				return this.address;
			}
		}

		/// <summary>
		/// ASIDE elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Aside> Aside
		{
			get
			{
				this.AssertParsed();
				return this.aside;
			}
		}

		/// <summary>
		/// NAV elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Nav> Nav
		{
			get
			{
				this.AssertParsed();
				return this.nav;
			}
		}

		/// <summary>
		/// SECTION elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Section> Section
		{
			get
			{
				this.AssertParsed();
				return this.section;
			}
		}

		/// <summary>
		/// DIALOG elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Dialog> Dialog
		{
			get
			{
				this.AssertParsed();
				return this.dialog;
			}
		}

		/// <summary>
		/// FIGURE elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Figure> Figure
		{
			get
			{
				this.AssertParsed();
				return this.figure;
			}
		}

		/// <summary>
		/// AUDIO elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Elements.Audio> Audio
		{
			get
			{
				this.AssertParsed();
				return this.audio;
			}
		}

		/// <summary>
		/// VIDEO elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Elements.Video> Video
		{
			get
			{
				this.AssertParsed();
				return this.video;
			}
		}

		/// <summary>
		/// IMG elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Img> Img
		{
			get
			{
				this.AssertParsed();
				return this.img;
			}
		}

		/// <summary>
		/// PICTURE elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Picture> Picture
		{
			get
			{
				this.AssertParsed();
				return this.picture;
			}
		}

		/// <summary>
		/// CITE elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Cite> Cite
		{
			get
			{
				this.AssertParsed();
				return this.cite;
			}
		}

		/// <summary>
		/// DATA elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Data> Data
		{
			get
			{
				this.AssertParsed();
				return this.data;
			}
		}

		/// <summary>
		/// TIME elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Time> Time
		{
			get
			{
				this.AssertParsed();
				return this.time;
			}
		}

		/// <summary>
		/// SCRIPT elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Elements.Script> Script
		{
			get
			{
				this.AssertParsed();
				return this.script;
			}
		}

		/// <summary>
		/// FORM elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Form> Form
		{
			get
			{
				this.AssertParsed();
				return this.form;
			}
		}

		/// <summary>
		/// DTD instructions found in document, or null if none found.
		/// </summary>
		public IEnumerable<DtdInstruction> Dtd
		{
			get
			{
				this.AssertParsed();
				return this.dtd;
			}
		}

		/// <summary>
		/// Processing instructions found in document, or null if none found.
		/// </summary>
		public IEnumerable<ProcessingInstruction> ProcessingInstructions
		{
			get
			{
				this.AssertParsed();
				return this.processingInstructions;
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
			bool CurrentElementIsScript = false;

			foreach (char ch in this.htmlText)
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

					case 1:     // Waiting for ?, !, /, attributes or >
						if (ch == '/')
						{
							if (Empty)  // Closing tag
								State = 4;
							else if (CurrentElementIsScript)
							{
								sb.Insert(0, '<');
								sb.Append('/');
								State = 0;
							}
							else
								State = 3;
						}
						else if (ch == '!' && Empty)
							State++;
						else if (CurrentElementIsScript)
						{
							sb.Insert(0, '<');
							sb.Append(ch);
							State = 0;
						}
						else if (ch == '>')
						{
							if (Empty)
							{
								sb.Append("<>");
								Empty = false;
								State = 0;
							}
							else
							{
								CurrentElement = this.CreateElement(CurrentElement, sb.ToString());
								CurrentElementIsScript = CurrentElement is Elements.Script;

								sb.Clear();
								Empty = true;

								State = 0;
							}
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
							else if (CurrentElementIsScript)
							{
								sb.Insert(0, '<');
								sb.Append(ch);
								State = 0;
							}
							else
							{
								CurrentElement = this.CreateElement(CurrentElement, sb.ToString());
								CurrentElementIsScript = CurrentElement is Elements.Script;

								sb.Clear();
								Empty = true;

								State = 5;
							}
						}
						else if (ch == '?')
							State = 19;
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

					case 2: // DTD, comment or CDATA?
						if (ch == '[')
							State = 21;     // CDATA?
						else if (CurrentElementIsScript)
						{
							sb.Append("<!");
							sb.Append(ch);
							Empty = false;
							State = 0;
						}
						else if (ch == '>')
						{
							if (this.dtd == null)
								this.dtd = new LinkedList<DtdInstruction>();

							DtdInstruction Dtd = new DtdInstruction(CurrentElement, string.Empty);

							CurrentElement?.Add(Dtd);
							this.dtd.AddLast(Dtd);

							State = 0;
						}
						else if (ch == '-')
							State = 15;     // Comment?
						else
						{
							sb.Append(ch);
							Empty = false;

							State = 14;     // DTD
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
								{
									CurrentElement = CurrentElement.Parent as HtmlElement;
									CurrentElementIsScript = CurrentElement is Elements.Script;
								}
								else
								{
									HtmlElement Loop = CurrentElement.Parent as HtmlElement;

									while (Loop != null && Loop.Name != s)
										Loop = Loop.Parent as HtmlElement;

									if (Loop != null)
									{
										CurrentElement = Loop.Parent as HtmlElement;
										CurrentElementIsScript = CurrentElement is Elements.Script;
									}
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
							CurrentElementIsScript = CurrentElement is Elements.Script;
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

					case 14: // Skip DTD
						if (ch == '>')
						{
							if (this.dtd == null)
								this.dtd = new LinkedList<DtdInstruction>();

							DtdInstruction Dtd = new DtdInstruction(CurrentElement, sb.ToString());

							CurrentElement?.Add(Dtd);
							this.dtd.AddLast(Dtd);

							sb.Clear();
							Empty = true;

							State = 0;
						}
						else
						{
							sb.Append(ch);
							Empty = false;
						}
						break;

					case 15:    // Second hyphen in start of comment?
						if (ch == '-')
							State++;
						else
						{
							sb.Append("<!-");
							sb.Append(ch);
							Empty = false;

							State = 0;
						}
						break;

					case 16:    // In comment
						if (ch == '-')
							State++;
						else
						{
							sb.Append(ch);
							Empty = false;
						}
						break;

					case 17:    // Second hyphen?
						if (ch == '-')
							State++;
						else
						{
							sb.Append('-');
							sb.Append(ch);
							Empty = false;

							State--;
						}
						break;

					case 18:    // End of comment
						if (ch == '>')
						{
							CurrentElement?.Add(new Comment(CurrentElement, sb.ToString()));

							sb.Clear();
							Empty = true;

							State = 0;
						}
						else
						{
							sb.Append("--");
							sb.Append(ch);
							Empty = false;

							State -= 2;
						}
						break;

					case 19:    // In processing instruction
						if (ch == '?')
							State++;
						else
						{
							sb.Append(ch);
							Empty = false;
						}
						break;

					case 20:    // End of processing instruction?
						if (ch == '>')
						{
							if (this.processingInstructions == null)
								this.processingInstructions = new LinkedList<ProcessingInstruction>();

							ProcessingInstruction PI = new ProcessingInstruction(CurrentElement, sb.ToString());

							CurrentElement?.Add(PI);
							this.processingInstructions.AddLast(PI);

							sb.Clear();
							Empty = true;

							State = 0;
						}
						else
						{
							sb.Append('?');
							sb.Append(ch);
							Empty = false;

							State--;
						}
						break;

					case 21:    // <![ received
						if (ch == 'C')
							State++;
						else
						{
							sb.Append("<![");
							sb.Append(ch);
							Empty = false;
							State = 0;
						}
						break;

					case 22:    // <![C received
						if (ch == 'D')
							State++;
						else
						{
							sb.Append("<![C");
							sb.Append(ch);
							Empty = false;
							State = 0;
						}
						break;

					case 23:    // <![CD received
						if (ch == 'A')
							State++;
						else
						{
							sb.Append("<![CD");
							sb.Append(ch);
							Empty = false;
							State = 0;
						}
						break;

					case 24:    // <![CDA received
						if (ch == 'T')
							State++;
						else
						{
							sb.Append("<![CDA");
							sb.Append(ch);
							Empty = false;
							State = 0;
						}
						break;

					case 25:    // <![CDAT received
						if (ch == 'A')
							State++;
						else
						{
							sb.Append("<![CDAT");
							sb.Append(ch);
							Empty = false;
							State = 0;
						}
						break;

					case 26:    // <![CDATA received
						if (ch == '[')
							State++;
						else
						{
							sb.Append("<![CDATA");
							sb.Append(ch);
							Empty = false;
							State = 0;
						}
						break;

					case 27:    // In CDATA
						if (ch == ']')
							State++;
						else
						{
							sb.Append(ch);
							Empty = false;
						}
						break;

					case 28:
						if (ch == ']')
							State++;
						else
						{
							sb.Append('[');
							sb.Append(ch);
							Empty = false;
							State--;
						}
						break;

					case 29:
						if (ch == '>')
						{
							CurrentElement?.Add(new CDATA(CurrentElement, sb.ToString()));

							sb.Clear();
							Empty = true;

							State = 0;
						}
						else
						{
							sb.Append("[[");
							sb.Append(ch);
							Empty = false;
							State -= 2;
						}
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
				case "ADDRESS":
					Address Address = new Address(Parent);
					Result = Address;
					if (this.address == null)
						this.address = new LinkedList<Address>();
					this.address.AddLast(Address);
					break;

				case "APPLET": Result = new Applet(Parent); break;
				case "AREA": Result = new Area(Parent); break;
				case "ARTICLE":
					Article Article = new Article(Parent);
					Result = Article;
					if (this.article == null)
						this.article = Article;
					break;

				case "ASIDE":
					Aside Aside = new Aside(Parent);
					Result = Aside;
					if (this.aside == null)
						this.aside = new LinkedList<Aside>();
					this.aside.AddLast(Aside);
					break;

				case "AUDIO":
					Elements.Audio Audio = new Elements.Audio(Parent);
					Result = Audio;
					if (this.audio == null)
						this.audio = new LinkedList<Elements.Audio>();
					this.audio.AddLast(Audio);
					break;

				case "B": Result = new B(Parent); break;
				case "BASE": Result = new Base(Parent); break;
				case "BASEFONT": Result = new BaseFont(Parent); break;
				case "BDI": Result = new Bdi(Parent); break;
				case "BDO": Result = new Bdo(Parent); break;
				case "BGSOUND": Result = new BgSound(Parent); break;
				case "BIG": Result = new Big(Parent); break;
				case "BLINK": Result = new BLink(Parent); break;
				case "BLOCKQUOTE": Result = new BlockQuote(Parent); break;
				case "BODY":
					Body Body = new Body(Parent);
					Result = Body;
					if (this.body == null)
						this.body = Body;
					break;

				case "BR": Result = new Br(Parent); break;
				case "BUTTON": Result = new Button(Parent); break;
				case "CANVAS": Result = new Canvas(Parent); break;
				case "CAPTION": Result = new Caption(Parent); break;
				case "CENTER": Result = new Center(Parent); break;
				case "CITE":
					Cite Cite = new Cite(Parent);
					Result = Cite;
					if (this.cite == null)
						this.cite = new LinkedList<Cite>();
					this.cite.AddLast(Cite);
					break;

				case "CODE": Result = new Code(Parent); break;
				case "COL": Result = new Col(Parent); break;
				case "COLGROUP": Result = new ColGroup(Parent); break;
				case "COMMAND": Result = new Command(Parent); break;
				case "CONTENT": Result = new Elements.Content(Parent); break;
				case "DATA":
					Data Data = new Data(Parent);
					Result = Data;
					if (this.data == null)
						this.data = new LinkedList<Data>();
					this.data.AddLast(Data);
					break;

				case "DATALIST": Result = new DataList(Parent); break;
				case "DD": Result = new Dd(Parent); break;
				case "DEL": Result = new Del(Parent); break;
				case "DETAILS":
					Details Details = new Details(Parent);
					Result = Details;
					if (this.details == null)
						this.details = Details;
					break;

				case "DFN": Result = new Dfn(Parent); break;
				case "DIALOG":
					Dialog Dialog = new Dialog(Parent);
					Result = Dialog;
					if (this.dialog == null)
						this.dialog = new LinkedList<Dialog>();
					this.dialog.AddLast(Dialog);
					break;

				case "DIR": Result = new Dir(Parent); break;
				case "DIV": Result = new Div(Parent); break;
				case "DL": Result = new Dl(Parent); break;
				case "DT": Result = new Dt(Parent); break;
				case "ELEMENT": Result = new Element(Parent); break;
				case "EM": Result = new Em(Parent); break;
				case "EMBED": Result = new Embed(Parent); break;
				case "FIELDSET": Result = new FieldSet(Parent); break;
				case "FIGCAPTION": Result = new FigCaption(Parent); break;
				case "FIGURE":
					Figure Figure = new Figure(Parent);
					Result = Figure;
					if (this.figure == null)
						this.figure = new LinkedList<Figure>();
					this.figure.AddLast(Figure);
					break;

				case "FONT": Result = new Font(Parent); break;
				case "FOOTER":
					Footer Footer = new Footer(Parent);
					Result = Footer;
					if (this.footer == null)
						this.footer = Footer;
					break;

				case "FORM":
					Form Form = new Form(Parent);
					Result = Form;
					if (this.form == null)
						this.form = new LinkedList<Form>();
					this.form.AddLast(Form);
					break;

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
				case "HEAD":
					Head Head = new Head(Parent);
					Result = Head;
					if (this.head == null)
						this.head = Head;
					break;

				case "HEADER":
					Header Header = new Header(Parent);
					Result = Header;
					if (this.header == null)
						this.header = Header;
					break;

				case "HGROUP": Result = new HGroup(Parent); break;
				case "HR": Result = new Hr(Parent); break;
				case "HTML":
					Elements.Html Html = new Elements.Html(Parent);
					Result = Html;
					if (this.html == null)
						this.html = Html;
					break;

				case "I": Result = new I(Parent); break;
				case "IFRAME": Result = new IFrame(Parent); break;
				case "IMAGE": Result = new Image(Parent); break;
				case "IMG":
					Img Img = new Img(Parent);
					Result = Img;
					if (this.img == null)
						this.img = new LinkedList<Img>();
					this.img.AddLast(Img);
					break;

				case "INPUT": Result = new Input(Parent); break;
				case "INS": Result = new Ins(Parent); break;
				case "ISINDEX": Result = new IsIndex(Parent); break;
				case "KBD": Result = new Kbd(Parent); break;
				case "KEYGEN": Result = new Keygen(Parent); break;
				case "LABEL": Result = new Label(Parent); break;
				case "LEGEND": Result = new Legend(Parent); break;
				case "LI": Result = new Li(Parent); break;
				case "LINK":
					Link Link = new Link(Parent);
					Result = Link;
					if (this.link == null)
						this.link = new LinkedList<Link>();
					this.link.AddLast(Link);
					break;

				case "LISTING": Result = new Listing(Parent); break;
				case "MAIN":
					Main Main = new Main(Parent);
					Result = Main;
					if (this.main == null)
						this.main = Main;
					break;

				case "MAP": Result = new Map(Parent); break;
				case "MARK": Result = new Mark(Parent); break;
				case "MARQUEE": Result = new Marquee(Parent); break;
				case "MENU": Result = new Menu(Parent); break;
				case "MENUITEM": Result = new MenuItem(Parent); break;
				case "META":
					Meta Meta = new Meta(Parent);
					Result = Meta;
					if (this.meta == null)
						this.meta = new LinkedList<Meta>();
					this.meta.AddLast(Meta);
					break;

				case "METER": Result = new Meter(Parent); break;
				case "MULTICOL": Result = new MultiCol(Parent); break;
				case "NAV":
					Nav Nav = new Nav(Parent);
					Result = Nav;
					if (this.nav == null)
						this.nav = new LinkedList<Nav>();
					this.nav.AddLast(Nav);
					break;

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
				case "PICTURE":
					Picture Picture = new Picture(Parent);
					Result = Picture;
					if (this.picture == null)
						this.picture = new LinkedList<Picture>();
					this.picture.AddLast(Picture);
					break;

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
				case "SCRIPT":
					Elements.Script Script = new Elements.Script(Parent);
					Result = Script;
					if (this.script == null)
						this.script = new LinkedList<Elements.Script>();
					this.script.AddLast(Script);
					break;

				case "SECTION":
					Section Section = new Section(Parent);
					Result = Section;
					if (this.section == null)
						this.section = new LinkedList<Section>();
					this.section.AddLast(Section);
					break;

				case "SELECT": Result = new Select(Parent); break;
				case "SHADOW": Result = new Shadow(Parent); break;
				case "SLOT": Result = new Slot(Parent); break;
				case "SMALL": Result = new Small(Parent); break;
				case "SOURCE": Result = new Source(Parent); break;
				case "SPACER": Result = new Spacer(Parent); break;
				case "SPAN": Result = new Span(Parent); break;
				case "STRIKE": Result = new Strike(Parent); break;
				case "STRONG": Result = new Strong(Parent); break;
				case "STYLE":
					Style Style = new Style(Parent);
					Result = Style;
					if (this.style == null)
						this.style = new LinkedList<Style>();
					this.style.AddLast(Style);
					break;

				case "SUB": Result = new Sub(Parent); break;
				case "SUMMARY":
					Summary Summary = new Summary(Parent);
					Result = Summary;
					if (this.summary == null)
						this.summary = Summary;
					break;

				case "SUP": Result = new Sup(Parent); break;
				case "TABLE": Result = new Table(Parent); break;
				case "TBODY": Result = new TBody(Parent); break;
				case "TD": Result = new Td(Parent); break;
				case "TEMPLATE": Result = new Template(Parent); break;
				case "TEXTAREA": Result = new TextArea(Parent); break;
				case "TFOOT": Result = new TFoot(Parent); break;
				case "TH": Result = new Th(Parent); break;
				case "THEAD": Result = new THead(Parent); break;
				case "TIME":
					Time Time = new Time(Parent);
					Result = Time;
					if (this.time == null)
						this.time = new LinkedList<Time>();
					this.time.AddLast(Time);
					break;

				case "TITLE":
					Title Title = new Title(Parent);
					Result = Title;
					if (this.title == null)
						this.title = Title;
					break;

				case "TR": Result = new Tr(Parent); break;
				case "TRACK": Result = new Track(Parent); break;
				case "TT": Result = new Tt(Parent); break;
				case "U": Result = new U(Parent); break;
				case "UL": Result = new Ul(Parent); break;
				case "VAR": Result = new Var(Parent); break;
				case "VIDEO":
					Elements.Video Video = new Elements.Video(Parent);
					Result = Video;
					if (this.video == null)
						this.video = new LinkedList<Elements.Video>();
					this.video.AddLast(Video);
					break;

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
