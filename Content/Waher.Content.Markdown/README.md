Introduction
=============

The **Waher.Content.Markdown** library is a Markdown parser written in C#. The library can, apart from parsing Markdown, also export to HTML and XAML, 
which makes it suitable for server-side CMS applictions, as well as client-side applications working with markdown. 
It's part of the [IoTGateway solution](https://github.com/PeterWaher/IoTGateway).

Markdown
==============

The markdown parser defined in **Waher.Content.Markdown** is inspired by the [original markdown syntax](http://daringfireball.net/projects/markdown/syntax)
as defined by John Gruber at Daring Fireball, but contains numerous other additions:

-   Markdown syntax within block-level HTML constructs is allowed.
-   Numbered lists retain the number used in the text.
-   Lazy numbering supported by prefixing items using `#.` instead of using actual numbers.
-   `_underline_` underlines text.
-   `__inserted__` displays inserted text.
-   `~strike through~` strikes through text.
-   `~~deleted~~` displays deleted text.
-   \`\` is solely used to display code. Curly quotes are inserted using normal quotes.
-   Headers receive automatic id's (camel casing).
-   Emojis are supported using the shortname syntax `:shortname:`.
-   Smileys are supported, and converted to emojis. Inspired from: http://git.emojione.com/demos/ascii-smileys.html

-   Any multimedia, not just images, can be inserted using the `!` syntax. This includes audio and video. The architecture is pluggable and allows for 
    customization of inclusion of content, including web content such as YouTube videos, etc. Linking to a local markdown file will include the file into 
	the context of the document. This allows for markdown templates to be used, and for more complex constructs, such as richer tables, to be built. 
	Multimedia can have additional width and height information. Multimedia handler is selected based on URL or file extension. If no particular 
	multimedia handler is found, the source is considered to be an image.
    
    Examples:
    
    * `![some text](/some/url "some title" WIDTH HEIGHT)` where `WIDTH` and `HEIGHT` are positive integers.
    * `![Your browser does not support the audio tag](/local/music.mp3)` (is rendered using the `<audio>` tag)
    * `![Your browser does not support the video tag](/local/video.mp4 320 200)` (is rendered using the `<video>` tag)
    * `![Your browser does not support the iframe tag](https://www.youtube.com/watch?v=whBPLc8m4SU 800 600)` inserts an `<iframe>` embedding the YouTube video.
	* `![Table of Contents](ToC)` inserts a table of contents (`ToC` is case insensitive).
    
    Width and Height can also be defined in referenced content. Example: `![some text][someref]`  
    `[someref]: some/url "some title" WIDTH HEIGHT`

There are numerous typographical enhancements added to the parser. These are inspired by the the [Smarty Pants](http://daringfireball.net/projects/smartypants/) 
addition to the original markdown, but numerous other character sequences are also supported:

| Sequence | Changed to |
|:-----------:|:-------------------:|
| `(c)` | &copy; |
| `(C)` | &COPY; |
| `(r)` | &reg; |
| `(R)` | &REG; |
| `(p)` | &copysr; |
| `(P)` | &copysr; |
| `(s)` | &oS; |
| `(S)` | &circledS; |
| `<<` | &laquo; |
| `>>` | &raquo; |
| `<<<` | &Ll; |
| `>>>` | &Gg; |
| `<--` | &larr; |
| `-->` | &rarr; |
| `<-->` | &harr; |
| `<==` | &lArr; |
| `==>` | &rArr; |
| `<==>` | &hArr; |
| `[[` | &LeftDoubleBracket; |
| `]]` | &RightDoubleBracket; |
| `+-` | &PlusMinus; |
| `-+` | &MinusPlus; |
| `<>` | &ne; |
| `<=` | &leq; |
| `>=` | &geq; |
| `==` | &equiv; |
| `^a` | &ordf; |
| `^o` | &ordm; |
| `^0` | &deg; |
| `^1` | &sup1; |
| `^2` | &sup2; |
| `^3` | &sup3; |
| `^TM` | &trade; |
| `%0` | &permil; |
| `%00` | &pertenk; |

Selected features from [MultiMarkdown](https://rawgit.com/fletcher/human-markdown-reference/master/index.html) and
[Markdown Extra](https://michelf.ca/projects/php-markdown/extra/) have also been included:

-   Images placed in a paragraph by itself is wrapped in a `<figure>` tag.
-   Tables.
-   Definition lists.
-   Metadata.
-   Footnotes.

Meta-data tags that are recognized by the parser are, as follows. Other meta-data tags are simply copied into the meta-data section of the 
generated HTML document. Keys are case insensitive.

| Key           | Description                                                                                                |
|:--------------|:-----------------------------------------------------------------------------------------------------------|
| Title			| Title of document.																						 |
| Subtitle		| Subtitle of document.																						 |
| Description	| Description of document.																					 |
| Author		| Author(s) of document.																					 |
| Date			| (Publication) date of document.																			 |
| Copyright		| Link to copyright statement.																				 |
| Previous		| Link to previous document, in a paginated set of documents.												 |
| Prev			| Synonymous with Previous.																					 |
| Next			| Link to next document, in a paginated set of documents.													 |
| Alternate		| Link to alternate page.																					 |
| Help			| Link to help page.																						 |
| Icon			| Link to icon for page.																					 |
| CSS			| Link(s) to Cascading Style Sheet(s) that should be used for visial formatting of the generated HTML page.	 |
| Keywords		| Keywords.																									 |
| Image			| Link to image for page.																					 |
| Web			| Link to web page																							 |

## License

The source code provided in this project is provided open for the following uses:

* For **Personal evaluation**. Personal evaluation means evaluating the code, its libraries and underlying technologies, including learning 
	about underlying technologies.

* For **Academic use**. If you want to use the following code for academic use, all you need to do is to inform the author of who you are, what academic
	institution you work for (or study for), and in what projects you intend to use the code.

* For **Security analysis**. If you perform any security analysis on the code, to see what security aspects the code might have,
	all I ask is that you inform me of any findings so that any vulnerabilities might be addressed. I am thankful for any such contributions,
	and will acknowledge them.

All rights to the source code are reserved. If you're interested in using the source code, as a whole, or partially, you need a license agreement
with the author. You can contact him through [LinkedIn](http://waher.se/).

This software is provided by the copyright holders and contributors "as is" and any express or implied warranties, including, but not limited to, 
the implied warranties of merchantability and fitness for a particular purpose are disclaimed. In no event shall the copyright owner or contributors 
be liable for any direct, indirect, incidental, special, exemplary, or consequential damages (including, but not limited to, procurement of substitute 
goods or services; loss of use, data, or profits; or business interruption) however caused and on any theory of liability, whether in contract, strict 
liability, or tort (including negligence or otherwise) arising in any way out of the use of this software, even if advised of the possibility of such 
damage.

## Example

Following is a simple example in C# on how to parse markdown text, export it to XAML and then convert it to a UIElement for display:

```csharp
Emoji1LocalFiles Emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Png64, 24, 24, "pack://siteoforigin:,,,/Graphics/Emoji1/png/64x64/%FILENAME%");
MarkdownDocument Markdown = new MarkdownDocument(Text, new MarkdownSettings(Emoji1_24x24, false));

XamlSettings Settings = new XamlSettings();
Settings.TableCellRowBackgroundColor1 = "#20404040";
Settings.TableCellRowBackgroundColor2 = "#10808080";

string XAML = Markdown.GenerateXAML(Settings);
object UIElement = XamlReader.Parse(XAML);
```

Emojis are defined in [Waher.Content.Emoji](../../Content/Waher.Content.Emoji), and the set of free emojis from **Emoji1** is made available in the
[Waher.Content.Emoji.Emoji1](../../Content/Waher.Content.Emoji.Emoji1) project. The above code is taken from **Waher.Client.WPF** and is used to create 
chat sessions in a WPF client like the following:

![Waher.Client.WPF](../../Images/Waher.Client.WPF.1.png)

The above chat session is made with a mock temperature devices defined in [Waher.Mock.Temperature](../../Mocks/Waher.Mock.Temperature), which
uses the [Waher.Networking.XMPP.Chat](../../Networking/Waher.Networking.XMPP.Chat) project to create a sensor chat bot.