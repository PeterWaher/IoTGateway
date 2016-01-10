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

-   Any multimedia, not just images, can be inserted using the `!` syntax. This includes audio and video. The architecture is pluggable and allows for 
    customization of inclusion of content, including web content such as YouTube videos, etc. Linking to a local markdown file will include the file into 
	the context of the document. This allows for markdown templates to be used, and for more complex constructs, such as richer tables, to be built. Multimedia 
	can have additional width and height information. Multimedia handler is selected based on URL or file extension. If no particular multimedia handler 
	is found, the source is considered to be an image.
    
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
-   Tables are supported.