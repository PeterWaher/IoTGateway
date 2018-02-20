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
		private Head head = null;
		private LinkedList<Main> main = null;
		private LinkedList<Header> header = null;
		private LinkedList<Footer> footer = null;
		private LinkedList<Details> details = null;
		private LinkedList<Summary> summary = null;
		private LinkedList<Article> article = null;
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
		/// HEADER elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Main> Main
		{
			get
			{
				this.AssertParsed();
				return this.main;
			}
		}

		/// <summary>
		/// HEADER elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Header> Header
		{
			get
			{
				this.AssertParsed();
				return this.header;
			}
		}

		/// <summary>
		/// FOOTER elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Footer> Footer
		{
			get
			{
				this.AssertParsed();
				return this.footer;
			}
		}

		/// <summary>
		/// DETAILS elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Details> Details
		{
			get
			{
				this.AssertParsed();
				return this.details;
			}
		}

		/// <summary>
		/// SUMMARY elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Summary> Summary
		{
			get
			{
				this.AssertParsed();
				return this.summary;
			}
		}

		/// <summary>
		/// ARTICLE elements found in document, or null if none found.
		/// </summary>
		public IEnumerable<Article> Article
		{
			get
			{
				this.AssertParsed();
				return this.article;
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
			HtmlElement EmptyElement;
			HtmlAttribute CurrentAttribute = null;
			string Name = string.Empty;
			string s;
			int State = 0;
			char EndChar = '\x00';
			char ch;
			int Pos;
			int StartOfElement = 0;
			int StartOfText = 0;
			int StartOfAttribute = 0;
			int Len = this.htmlText.Length;
			bool Empty = true;
			bool CurrentElementIsScript = false;

			for (Pos = 0; Pos < Len; Pos++)
			{
				ch = this.htmlText[Pos];

				switch (State)
				{
					case 0:     // Waiting for <
						if (ch == '<')
						{
							if (!Empty)
							{
								if (CurrentElement != null && CurrentElement.IsEmptyElement)
									CurrentElement = CurrentElement.Parent as HtmlElement;

								CurrentElement?.Add(new HtmlText(this, CurrentElement, StartOfText, Pos - 1, sb.ToString()));

								sb.Clear();
								Empty = true;
							}

							StartOfElement = Pos;
							State++;
						}
						else if (ch == '&')
						{
							if (!Empty)
							{
								if (CurrentElement != null && CurrentElement.IsEmptyElement)
									CurrentElement = CurrentElement.Parent as HtmlElement;

								CurrentElement?.Add(new HtmlText(this, CurrentElement, StartOfText, Pos - 1, sb.ToString()));

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
								StartOfText = Pos + 1 - sb.Length;
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
							StartOfText = Pos + 1 - sb.Length;
							State = 0;
						}
						else if (ch == '>')
						{
							if (Empty)
							{
								sb.Append("<>");
								Empty = false;
								StartOfText = Pos + 1 - sb.Length;
								State = 0;
							}
							else
							{
								CurrentElement = this.CreateElement(CurrentElement, sb.ToString(), StartOfElement, Pos);
								CurrentElementIsScript = CurrentElement is Elements.Script;

								sb.Clear();
								Empty = true;

								StartOfText = Pos + 1;
								State = 0;
							}
						}
						else if (ch <= ' ' || ch == 160)
						{
							if (Empty)
							{
								sb.Append('<');
								sb.Append(ch);
								Empty = false;
								StartOfText = Pos + 1 - sb.Length;
								State = 0;
							}
							else if (CurrentElementIsScript)
							{
								sb.Insert(0, '<');
								sb.Append(ch);
								StartOfText = Pos + 1 - sb.Length;
								State = 0;
							}
							else
							{
								CurrentElement = this.CreateElement(CurrentElement, sb.ToString(), StartOfElement, Pos);
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
							StartOfText = Pos + 1 - sb.Length;
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
							StartOfText = Pos + 1 - sb.Length;
							State = 0;
						}
						else if (ch == '>')
						{
							if (this.dtd == null)
								this.dtd = new LinkedList<DtdInstruction>();

							DtdInstruction Dtd = new DtdInstruction(this, CurrentElement, Pos - 2, Pos, string.Empty);

							if (CurrentElement != null && CurrentElement.IsEmptyElement)
								CurrentElement = CurrentElement.Parent as HtmlElement;

							CurrentElement?.Add(Dtd);
							this.dtd.AddLast(Dtd);

							StartOfText = Pos + 1;
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
							EmptyElement = this.CreateElement(CurrentElement, sb.ToString(), StartOfElement, Pos);
							EmptyElement.EndPosition = Pos;

							sb.Clear();
							Empty = true;

							StartOfText = Pos + 1;
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
								if (CurrentElement.EndPosition < 0)
									CurrentElement.EndPosition = Pos;

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
										Loop = CurrentElement.Parent as HtmlElement;

										while (Loop != null && Loop.Name != s)
										{
											if (Loop.EndPosition < 0)
												Loop.EndPosition = Pos;

											Loop = Loop.Parent as HtmlElement;
										}

										if (Loop.EndPosition < 0)
											Loop.EndPosition = Pos;

										CurrentElement = Loop.Parent as HtmlElement;
										CurrentElementIsScript = CurrentElement is Elements.Script;
									}
								}
							}

							StartOfText = Pos + 1;
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
						{
							StartOfText = Pos + 1;
							State = 0;
						}
						else if (ch == '/')
							State = 9;
						else if (ch == '=')
						{
							StartOfAttribute = Pos;
							Name = string.Empty;
							State = 7;
						}
						else if (ch > ' ' && ch != 160)
						{
							if (IsNameCharacter(ch))
							{
								StartOfAttribute = Pos;
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
							CurrentElement.AddAttribute(new HtmlAttribute(this, CurrentElement, StartOfAttribute, Pos - 1, sb.ToString(), string.Empty));

							sb.Clear();
							Empty = true;

							StartOfText = Pos + 1;
							State = 0;
						}
						else if (ch == '/')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(this, CurrentElement, StartOfAttribute, Pos - 1, sb.ToString(), string.Empty));

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
							if (ch <= ' ' || ch == 160)
							{
								CurrentElement.AddAttribute(new HtmlAttribute(this, CurrentElement, StartOfAttribute, Pos - 1, sb.ToString(), string.Empty));

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
							CurrentAttribute = new HtmlAttribute(this, CurrentElement, StartOfAttribute, Name);
							CurrentElement.AddAttribute(CurrentAttribute);

							EndChar = ch;
							State = 11;
						}
						else if (ch == '>')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(this, CurrentElement, StartOfAttribute, Pos - 1, Name, string.Empty));
							StartOfText = Pos + 1;
							State = 0;
						}
						else if (ch == '/')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(this, CurrentElement, StartOfAttribute, Pos - 1, Name, string.Empty));
							State = 9;
						}
						else if (ch > ' ' && ch != 160)
						{
							sb.Append(ch);
							Empty = false;
							State++;
						}
						break;

					case 8: // Non-encapsulated attribute value
						if (ch <= ' ' || ch == 160)
						{
							CurrentElement.AddAttribute(new HtmlAttribute(this, CurrentElement, StartOfAttribute, Pos - 1, Name, sb.ToString()));

							sb.Clear();
							Empty = true;

							State = 5;
						}
						else if (ch == '>')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(this, CurrentElement, StartOfAttribute, Pos - 1, Name, sb.ToString()));

							sb.Clear();
							Empty = true;

							StartOfText = Pos + 1;
							State = 0;
						}
						else if (ch == '/')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(this, CurrentElement, StartOfAttribute, Pos - 1, Name, sb.ToString()));

							sb.Clear();
							Empty = true;

							State = 9;
						}
						else
						{
							sb.Append(ch);
							Empty = false;
						}
						break;

					case 9: // Waiting for > at end of empty element
						if (ch == '>')
						{
							CurrentElement.EndPosition = Pos;
							CurrentElement = CurrentElement.Parent as HtmlElement;
							CurrentElementIsScript = CurrentElement is Elements.Script;
							StartOfText = Pos + 1;
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
							if (CurrentElement != null && CurrentElement.IsEmptyElement)
								CurrentElement = CurrentElement.Parent as HtmlElement;

							s = sb.ToString();
							CurrentElement?.Add(new HtmlEntity(this, CurrentElement, Pos - s.Length - 1, Pos, s));

							sb.Clear();
							Empty = true;

							StartOfText = Pos + 1;
							State = 0;
						}
						else
						{
							sb.Insert(0, '&');
							sb.Append(ch);
							Empty = false;
							StartOfText = Pos + 1 - sb.Length;
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
									CurrentAttribute.Add(new HtmlText(this, CurrentAttribute, Pos - s.Length, Pos - 1, s));
								else
									CurrentAttribute.Value = s;

								CurrentAttribute.EndPosition = Pos;

								sb.Clear();
								Empty = true;
							}

							State = 5;
						}
						else if (ch == '&')
						{
							if (!Empty)
							{
								s = sb.ToString();

								CurrentAttribute.Add(new HtmlText(this, CurrentAttribute, Pos - s.Length, Pos - 1, s));

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
							s = sb.ToString();
							CurrentAttribute.Add(new HtmlEntity(this, CurrentAttribute, Pos - s.Length - 1, Pos, s));

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
							StartOfText = Pos + 1;
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

							s = sb.ToString();
							DtdInstruction Dtd = new DtdInstruction(this, CurrentElement, Pos - s.Length - 2, Pos, s);

							if (CurrentElement != null && CurrentElement.IsEmptyElement)
								CurrentElement = CurrentElement.Parent as HtmlElement;

							CurrentElement?.Add(Dtd);
							this.dtd.AddLast(Dtd);

							sb.Clear();
							Empty = true;

							StartOfText = Pos + 1;
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

							StartOfText = Pos + 1 - sb.Length;
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
							if (CurrentElement != null && CurrentElement.IsEmptyElement)
								CurrentElement = CurrentElement.Parent as HtmlElement;

							s = sb.ToString();
							CurrentElement?.Add(new Comment(this, CurrentElement, Pos - s.Length - 5, Pos, s));

							sb.Clear();
							Empty = true;

							StartOfText = Pos + 1;
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

							s = sb.ToString();
							ProcessingInstruction PI = new ProcessingInstruction(this, CurrentElement, Pos - s.Length - 3, Pos, s);

							if (CurrentElement != null && CurrentElement.IsEmptyElement)
								CurrentElement = CurrentElement.Parent as HtmlElement;

							CurrentElement?.Add(PI);
							this.processingInstructions.AddLast(PI);

							sb.Clear();
							Empty = true;

							StartOfText = Pos + 1;
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
							StartOfText = Pos + 1 - sb.Length;
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
							StartOfText = Pos + 1 - sb.Length;
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
							StartOfText = Pos + 1 - sb.Length;
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
							StartOfText = Pos + 1 - sb.Length;
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
							StartOfText = Pos + 1 - sb.Length;
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
							StartOfText = Pos + 1 - sb.Length;
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
							if (CurrentElement != null && CurrentElement.IsEmptyElement)
								CurrentElement = CurrentElement.Parent as HtmlElement;

							s = sb.ToString();
							CurrentElement?.Add(new CDATA(this, CurrentElement, Pos - s.Length - 10, Pos, s));

							sb.Clear();
							Empty = true;

							StartOfText = Pos + 1;
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
						if (CurrentElement != null && CurrentElement.IsEmptyElement)
							CurrentElement = CurrentElement.Parent as HtmlElement;

						s = sb.ToString();
						CurrentElement?.Add(new HtmlText(this, CurrentElement, Pos - s.Length, Pos - 1, s));
						break;

					case 6: // Attribute name
						CurrentElement.AddAttribute(new HtmlAttribute(this, CurrentElement, StartOfAttribute, Pos - 1, sb.ToString(), string.Empty));
						break;

					case 8: // Non-encapsulated attribute value
						CurrentElement.AddAttribute(new HtmlAttribute(this, CurrentElement, StartOfAttribute, Pos - 1, Name, sb.ToString()));
						break;

					case 10: // Entity
						if (CurrentElement != null && CurrentElement.IsEmptyElement)
							CurrentElement = CurrentElement.Parent as HtmlElement;

						sb.Insert(0, '&');
						s = sb.ToString();
						CurrentElement?.Add(new HtmlText(this, CurrentElement, Pos - s.Length, Pos - 1, s));
						break;

					case 11:    // Encapsulated attribute value
						s = sb.ToString();

						if (CurrentAttribute.HasSegments)
							CurrentAttribute.Add(new HtmlText(this, CurrentAttribute, Pos - s.Length, Pos - 1, s));
						else
							CurrentAttribute.Value = s;

						CurrentAttribute.EndPosition = Pos - 1;
						break;

					case 12: // Entity in attribute value
						sb.Insert(0, '&');
						s = sb.ToString();
						CurrentAttribute.Add(new HtmlText(this, CurrentElement, Pos - s.Length, Pos - 1, s));
						break;
				}
			}

			while (CurrentElement != null)
			{
				if (CurrentElement.EndPosition < 0)
					CurrentElement.EndPosition = Pos - 1;

				CurrentElement = CurrentElement.Parent as HtmlElement;
			}
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

		private HtmlElement CreateElement(HtmlElement Parent, string TagName, int Start, int Pos)
		{
			HtmlElement Result;

			if (Parent != null && Parent.IsEmptyElement)
				Parent = Parent.Parent as HtmlElement;

			TagName = TagName.ToUpper();

			switch (TagName)
			{
				case "A": Result = new A(this, Parent, Start); break;
				case "ABBR": Result = new Abbr(this, Parent, Start); break;
				case "ACRONYM": Result = new Acronym(this, Parent, Start); break;
				case "ADDRESS":
					Address Address = new Address(this, Parent, Start);
					Result = Address;
					if (this.address == null)
						this.address = new LinkedList<Address>();
					this.address.AddLast(Address);
					break;

				case "APPLET": Result = new Applet(this, Parent, Start); break;
				case "AREA": Result = new Area(this, Parent, Start); break;
				case "ARTICLE":
					Article Article = new Article(this, Parent, Start);
					Result = Article;
					if (this.article == null)
						this.article = new LinkedList<Article>();
					this.article.AddLast(Article);
					break;

				case "ASIDE":
					Aside Aside = new Aside(this, Parent, Start);
					Result = Aside;
					if (this.aside == null)
						this.aside = new LinkedList<Aside>();
					this.aside.AddLast(Aside);
					break;

				case "AUDIO":
					Elements.Audio Audio = new Elements.Audio(this, Parent, Start);
					Result = Audio;
					if (this.audio == null)
						this.audio = new LinkedList<Elements.Audio>();
					this.audio.AddLast(Audio);
					break;

				case "B": Result = new B(this, Parent, Start); break;
				case "BASE": Result = new Base(this, Parent, Start); break;
				case "BASEFONT": Result = new BaseFont(this, Parent, Start); break;
				case "BDI": Result = new Bdi(this, Parent, Start); break;
				case "BDO": Result = new Bdo(this, Parent, Start); break;
				case "BGSOUND": Result = new BgSound(this, Parent, Start); break;
				case "BIG": Result = new Big(this, Parent, Start); break;
				case "BLINK": Result = new BLink(this, Parent, Start); break;
				case "BLOCKQUOTE": Result = new BlockQuote(this, Parent, Start); break;
				case "BODY":
					Body Body = new Body(this, Parent, Start);
					Result = Body;
					if (this.body == null)
						this.body = Body;
					break;

				case "BR": Result = new Br(this, Parent, Start); break;
				case "BUTTON": Result = new Button(this, Parent, Start); break;
				case "CANVAS": Result = new Canvas(this, Parent, Start); break;
				case "CAPTION": Result = new Caption(this, Parent, Start); break;
				case "CENTER": Result = new Center(this, Parent, Start); break;
				case "CITE":
					Cite Cite = new Cite(this, Parent, Start);
					Result = Cite;
					if (this.cite == null)
						this.cite = new LinkedList<Cite>();
					this.cite.AddLast(Cite);
					break;

				case "CODE": Result = new Code(this, Parent, Start); break;
				case "COL": Result = new Col(this, Parent, Start); break;
				case "COLGROUP": Result = new ColGroup(this, Parent, Start); break;
				case "COMMAND": Result = new Command(this, Parent, Start); break;
				case "CONTENT": Result = new Elements.Content(this, Parent, Start); break;
				case "DATA":
					Data Data = new Data(this, Parent, Start);
					Result = Data;
					if (this.data == null)
						this.data = new LinkedList<Data>();
					this.data.AddLast(Data);
					break;

				case "DATALIST": Result = new DataList(this, Parent, Start); break;
				case "DD": Result = new Dd(this, Parent, Start); break;
				case "DEL": Result = new Del(this, Parent, Start); break;
				case "DETAILS":
					Details Details = new Details(this, Parent, Start);
					Result = Details;
					if (this.details == null)
						this.details = new LinkedList<Details>();
					this.details.AddLast(Details);
					break;

				case "DFN": Result = new Dfn(this, Parent, Start); break;
				case "DIALOG":
					Dialog Dialog = new Dialog(this, Parent, Start);
					Result = Dialog;
					if (this.dialog == null)
						this.dialog = new LinkedList<Dialog>();
					this.dialog.AddLast(Dialog);
					break;

				case "DIR": Result = new Dir(this, Parent, Start); break;
				case "DIV": Result = new Div(this, Parent, Start); break;
				case "DL": Result = new Dl(this, Parent, Start); break;
				case "DT": Result = new Dt(this, Parent, Start); break;
				case "ELEMENT": Result = new Element(this, Parent, Start); break;
				case "EM": Result = new Em(this, Parent, Start); break;
				case "EMBED": Result = new Embed(this, Parent, Start); break;
				case "FIELDSET": Result = new FieldSet(this, Parent, Start); break;
				case "FIGCAPTION": Result = new FigCaption(this, Parent, Start); break;
				case "FIGURE":
					Figure Figure = new Figure(this, Parent, Start);
					Result = Figure;
					if (this.figure == null)
						this.figure = new LinkedList<Figure>();
					this.figure.AddLast(Figure);
					break;

				case "FONT": Result = new Font(this, Parent, Start); break;
				case "FOOTER":
					Footer Footer = new Footer(this, Parent, Start);
					Result = Footer;
					if (this.footer == null)
						this.footer= new LinkedList<Footer>();
					this.footer.AddLast(Footer);
					break;

				case "FORM":
					Form Form = new Form(this, Parent, Start);
					Result = Form;
					if (this.form == null)
						this.form = new LinkedList<Form>();
					this.form.AddLast(Form);
					break;

				case "FRAME": Result = new Frame(this, Parent, Start); break;
				case "FRAMESET": Result = new FrameSet(this, Parent, Start); break;
				case "H1": Result = new Hn(this, Parent, Start, 1); break;
				case "H2": Result = new Hn(this, Parent, Start, 2); break;
				case "H3": Result = new Hn(this, Parent, Start, 3); break;
				case "H4": Result = new Hn(this, Parent, Start, 4); break;
				case "H5": Result = new Hn(this, Parent, Start, 5); break;
				case "H6": Result = new Hn(this, Parent, Start, 6); break;
				case "H7": Result = new Hn(this, Parent, Start, 7); break;
				case "H8": Result = new Hn(this, Parent, Start, 8); break;
				case "H9": Result = new Hn(this, Parent, Start, 9); break;
				case "HEAD":
					Head Head = new Head(this, Parent, Start);
					Result = Head;
					if (this.head == null)
						this.head = Head;
					break;

				case "HEADER":
					Header Header = new Header(this, Parent, Start);
					Result = Header;
					if (this.header == null)
						this.header = new LinkedList<Header>();
					this.header.AddLast(Header);
					break;

				case "HGROUP": Result = new HGroup(this, Parent, Start); break;
				case "HR": Result = new Hr(this, Parent, Start); break;
				case "HTML":
					Elements.Html Html = new Elements.Html(this, Parent, Start);
					Result = Html;
					if (this.html == null)
						this.html = Html;
					break;

				case "I": Result = new I(this, Parent, Start); break;
				case "IFRAME": Result = new IFrame(this, Parent, Start); break;
				case "IMAGE": Result = new Image(this, Parent, Start); break;
				case "IMG":
					Img Img = new Img(this, Parent, Start);
					Result = Img;
					if (this.img == null)
						this.img = new LinkedList<Img>();
					this.img.AddLast(Img);
					break;

				case "INPUT": Result = new Input(this, Parent, Start); break;
				case "INS": Result = new Ins(this, Parent, Start); break;
				case "ISINDEX": Result = new IsIndex(this, Parent, Start); break;
				case "KBD": Result = new Kbd(this, Parent, Start); break;
				case "KEYGEN": Result = new Keygen(this, Parent, Start); break;
				case "LABEL": Result = new Label(this, Parent, Start); break;
				case "LEGEND": Result = new Legend(this, Parent, Start); break;
				case "LI": Result = new Li(this, Parent, Start); break;
				case "LINK":
					Link Link = new Link(this, Parent, Start);
					Result = Link;
					if (this.link == null)
						this.link = new LinkedList<Link>();
					this.link.AddLast(Link);
					break;

				case "LISTING": Result = new Listing(this, Parent, Start); break;
				case "MAIN":
					Main Main = new Main(this, Parent, Start);
					Result = Main;
					if (this.main == null)
						this.main = new LinkedList<Main>();
					this.main.AddLast(Main);
					break;

				case "MAP": Result = new Map(this, Parent, Start); break;
				case "MARK": Result = new Mark(this, Parent, Start); break;
				case "MARQUEE": Result = new Marquee(this, Parent, Start); break;
				case "MENU": Result = new Menu(this, Parent, Start); break;
				case "MENUITEM": Result = new MenuItem(this, Parent, Start); break;
				case "META":
					Meta Meta = new Meta(this, Parent, Start);
					Result = Meta;
					if (this.meta == null)
						this.meta = new LinkedList<Meta>();
					this.meta.AddLast(Meta);
					break;

				case "METER": Result = new Meter(this, Parent, Start); break;
				case "MULTICOL": Result = new MultiCol(this, Parent, Start); break;
				case "NAV":
					Nav Nav = new Nav(this, Parent, Start);
					Result = Nav;
					if (this.nav == null)
						this.nav = new LinkedList<Nav>();
					this.nav.AddLast(Nav);
					break;

				case "NEXTID": Result = new NextId(this, Parent, Start); break;
				case "NOBR": Result = new NoBr(this, Parent, Start); break;
				case "NOEMBED": Result = new NoEmbed(this, Parent, Start); break;
				case "NOFRAMES": Result = new NoFrames(this, Parent, Start); break;
				case "NOSCRIPT": Result = new NoScript(this, Parent, Start); break;
				case "OBJECT": Result = new Elements.Object(this, Parent, Start); break;
				case "OL": Result = new Ol(this, Parent, Start); break;
				case "OPTGROUP": Result = new OptGroup(this, Parent, Start); break;
				case "OPTION": Result = new Option(this, Parent, Start); break;
				case "OUTPUT": Result = new Output(this, Parent, Start); break;
				case "P": Result = new P(this, Parent, Start); break;
				case "PARAM": Result = new Param(this, Parent, Start); break;
				case "PICTURE":
					Picture Picture = new Picture(this, Parent, Start);
					Result = Picture;
					if (this.picture == null)
						this.picture = new LinkedList<Picture>();
					this.picture.AddLast(Picture);
					break;

				case "PLAINTEXT": Result = new PlainText(this, Parent, Start); break;
				case "PRE": Result = new Pre(this, Parent, Start); break;
				case "PROGRESS": Result = new Progress(this, Parent, Start); break;
				case "Q": Result = new Q(this, Parent, Start); break;
				case "RP": Result = new Rp(this, Parent, Start); break;
				case "RT": Result = new Rt(this, Parent, Start); break;
				case "RTC": Result = new Rtc(this, Parent, Start); break;
				case "RUBY": Result = new Ruby(this, Parent, Start); break;
				case "S": Result = new S(this, Parent, Start); break;
				case "SAMP": Result = new Samp(this, Parent, Start); break;
				case "SCRIPT":
					Elements.Script Script = new Elements.Script(this, Parent, Start);
					Result = Script;
					if (this.script == null)
						this.script = new LinkedList<Elements.Script>();
					this.script.AddLast(Script);
					break;

				case "SECTION":
					Section Section = new Section(this, Parent, Start);
					Result = Section;
					if (this.section == null)
						this.section = new LinkedList<Section>();
					this.section.AddLast(Section);
					break;

				case "SELECT": Result = new Select(this, Parent, Start); break;
				case "SHADOW": Result = new Shadow(this, Parent, Start); break;
				case "SLOT": Result = new Slot(this, Parent, Start); break;
				case "SMALL": Result = new Small(this, Parent, Start); break;
				case "SOURCE": Result = new Source(this, Parent, Start); break;
				case "SPACER": Result = new Spacer(this, Parent, Start); break;
				case "SPAN": Result = new Span(this, Parent, Start); break;
				case "STRIKE": Result = new Strike(this, Parent, Start); break;
				case "STRONG": Result = new Strong(this, Parent, Start); break;
				case "STYLE":
					Style Style = new Style(this, Parent, Start);
					Result = Style;
					if (this.style == null)
						this.style = new LinkedList<Style>();
					this.style.AddLast(Style);
					break;

				case "SUB": Result = new Sub(this, Parent, Start); break;
				case "SUMMARY":
					Summary Summary = new Summary(this, Parent, Start);
					Result = Summary;
					if (this.summary == null)
						this.summary = new LinkedList<Summary>();
					this.summary.AddLast(Summary);
					break;

				case "SUP": Result = new Sup(this, Parent, Start); break;
				case "TABLE": Result = new Table(this, Parent, Start); break;
				case "TBODY": Result = new TBody(this, Parent, Start); break;
				case "TD": Result = new Td(this, Parent, Start); break;
				case "TEMPLATE": Result = new Template(this, Parent, Start); break;
				case "TEXTAREA": Result = new TextArea(this, Parent, Start); break;
				case "TFOOT": Result = new TFoot(this, Parent, Start); break;
				case "TH": Result = new Th(this, Parent, Start); break;
				case "THEAD": Result = new THead(this, Parent, Start); break;
				case "TIME":
					Time Time = new Time(this, Parent, Start);
					Result = Time;
					if (this.time == null)
						this.time = new LinkedList<Time>();
					this.time.AddLast(Time);
					break;

				case "TITLE":
					Title Title = new Title(this, Parent, Start);
					Result = Title;
					if (this.title == null)
						this.title = Title;
					break;

				case "TR": Result = new Tr(this, Parent, Start); break;
				case "TRACK": Result = new Track(this, Parent, Start); break;
				case "TT": Result = new Tt(this, Parent, Start); break;
				case "U": Result = new U(this, Parent, Start); break;
				case "UL": Result = new Ul(this, Parent, Start); break;
				case "VAR": Result = new Var(this, Parent, Start); break;
				case "VIDEO":
					Elements.Video Video = new Elements.Video(this, Parent, Start);
					Result = Video;
					if (this.video == null)
						this.video = new LinkedList<Elements.Video>();
					this.video.AddLast(Video);
					break;

				case "WBR": Result = new Wbr(this, Parent, Start); break;
				case "XMP": Result = new Xmp(this, Parent, Start); break;
				default: Result = new HtmlElement(this, Parent, Start, TagName); break;
			}

			if (Parent == null)
			{
				if (this.root == null)
					this.root = Result;
			}
			else
				Parent?.Add(Result);

			if (Result.IsEmptyElement)
				Result.EndPosition = Pos;

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
