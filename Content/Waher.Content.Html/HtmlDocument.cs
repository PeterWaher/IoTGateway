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
							sb.Append(ch);
							Empty = false;
							State++;
						}
						break;

					case 6: // Attribute name
						if (ch <= ' ')
						{
							CurrentElement.AddAttribute(new HtmlAttribute(CurrentElement, sb.ToString(), string.Empty));

							sb.Clear();
							Empty = true;

							State--;
						}
						else if (ch == '>')
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
							sb.Append(ch);
							Empty = false;
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

		private HtmlElement CreateElement(HtmlElement Parent, string TagName)
		{
			HtmlElement Result;

			if (Parent != null && Parent.IsEmptyElement)
				Parent = Parent.Parent as HtmlElement;

			TagName = TagName.ToUpper();

			switch (TagName)
			{
				// TODO: Non-empty elements
				case "AREA": Result = new Area(Parent); break;
				case "BASE": Result = new Base(Parent); break;
				case "BR": Result = new Br(Parent); break;
				case "COL": Result = new Col(Parent); break;
				case "EMBED": Result = new Embed(Parent); break;
				case "HR": Result = new Hr(Parent); break;
				case "IMG": Result = new Img(Parent); break;
				case "INPUT": Result = new Input(Parent); break;
				case "KEYGEN": Result = new Keygen(Parent); break;
				case "LINK": Result = new Link(Parent); break;
				case "META": Result = new Meta(Parent); break;
				case "PARAM": Result = new Param(Parent); break;
				case "SOURCE": Result = new Source(Parent); break;
				case "TRACK": Result = new Track(Parent); break;
				case "WBR": Result = new Wbr(Parent); break;
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
