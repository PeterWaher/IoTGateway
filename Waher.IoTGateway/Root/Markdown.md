Title: Markdown
Description: Markdown syntax, as understood by the IoT Gateway.
Date: 2016-02-02
Author: Peter Waher
Copyright: Waher Data AB

Markdown syntax
=============================

*Markdown* is a very simple, yet efficient format for editing text-based content. The **IoT Gateway** converts Markdown to HTML automatically when
web browsers download it, making it a powerful tool for publishing online content. The syntax is inspired by the 
[original markdown syntax](http://daringfireball.net/projects/markdown/syntax) as defined by John Gruber at Daring Fireball, but contains numerous other 
additions and modifications. Some of these are introduced in the **IoT Gateway**, others are inspired by selected features used in
[MultiMarkdown](https://rawgit.com/fletcher/human-markdown-reference/master/index.html) and
[Markdown Extra](https://michelf.ca/projects/php-markdown/extra/). Below, is a summary of the markdown syntax, as understood by the **IoT Gateway**.

![Table of Contents](ToC)


Inline constructs
----------------------------

Markdown includes a series of simple syntax constructs, categorized into different types. Inline constructs are constructs that can be used in
normal text flow. The follow subsections show available inline constructs that can be used to enhance the text.

### Text formatting

In Markdown it's easy to format text. Special characters are used around the text you want to format, as is shown in the following subsections.

#### Emphasized text

To emphasize text, enclose the text using asterisks `*`, such as this: `*Emphasized text*`. In HTML, this becomes: *Emphasized text*. Emphasized text 
can be included in the *middle* of a sentance, or in the mi*dd*le of a word. In HTML, emphasized text gets surrounded by `<em>` and `</em>` tags.

#### Strong text

Strong text is included by surrounding it with double asterisks `**`. Example: `**Strong text**`. Result: **Strong text**. As with emphasized text, 
it can be included in the **middle** of a sentance, or in the mi**dd**le of a word. In HTML, strong text gets surrounded by `<strong>` and `</strong>` tags.

#### Underlined text

Underlined text is created by surrounding the underlined text with underscores `_`. Example: `_Underlined text_`. This is transformed to: 
_Underlined text_. As with other text formatting operators, underlined text can be included in the _middle_ of a sentance, or in the
mi_dd_le of a word. In HTML, underlined text gets surrounded by `<u>` and `</u>` tags.

#### Inserted text

Inserted text, which by default is also shown as underlined (but which can be changed to a different style using style sheets), is created by surrounding 
the inserted text with double underscores `__`. Example: `__Inserted text__`. This becomes: __Inserted text__. As with the operators above, inserted text 
can be included in the __middle__ of a sentance  or in the mi__dd__le of a word. In HTML, inserted text gets surrounded by `<ins>` and `</ins>` tags.

#### Strikethrough text

Strikethrough text is created by surrounding its text using tildes `~`. Example: `~Strikethrough text~`. This is transformed to: ~Strikethrough text~. 
As with the other text formatting operators, it can be included in the ~middle~ of a sentance or in the mi~dd~le of a word. In HTML, strikethrough text 
gets surrounded by `<s>` and `</s>` tags.

#### Deleted text

Deleted text, which by default is also shown as strikethrough text (but which can be changed to a different style using style sheets), is created by 
surrounding the inserted text with double tildes `~~`. Example: `~~Deleted text~~`- Result: ~~Deleted text~~. As with the operators above, deleted text 
can be included in the ~~middle~~ of a sentance or in the mi~~dd~~le of a word. In HTML, inserted text gets surrounded by `<del>` and `</del>` tags.

#### Inline code

Inline code can be used to include code into flowing text. To include inline code, surround it using single or double back ticks `` ` ``, as shown in the 
following example: `` `Inline code` ``. This is transformed to: `Inline code`. As with other text formatting operators, inline code can be included in the 
`middle` of a sentance or in the mi`dd`le of a word. In HTML, inserted text gets surrounded by `<code>` and `</code>` tags.

**Note:** Characters that have special meaning in markdown, such as \*, \_, \~, etc., are shown as normal characters in inline code.

**Note 2:** If you want to include a back tick in the inline code, you can surround the inline code using double back ticks and a space, one after the
first double back tick, and one before the last back tick, such as this: <code>\`\` \`Inline code\` \`\`</code>. This sequence was used to produce `` `Inline code` ``.

### Automatic links

Markdown help you include links to online resources (URLs) or mail addresses automatically by surrounding them with `<` and `>` characters, such as 
`<http://example.com/>` or `<address@example.com>`. These would turn into clickable links in the HTML representation, as follows: <http://example.com/> 
and <address@example.com>.

**Note:** It's important to include the *URI Scheme* (for example `http://`) in links or the @ sign in mail addresses, for the parser to understand it's 
an automatic link or an address, and not another type of construct.

### Links

To include a link into a markdown text, you can, apart from automatic links, also include custom links. These are written in the form
`[Text](URL)` or `[Text](URL "Title")`. The text can include inline formatting, if desired. URLs can be absolute (include URI scheme) or local relative
links (without URI scheme).

Some examples:

| Markdown                                                          | Result                                                          |
|-------------------------------------------------------------------|-----------------------------------------------------------------|
| `[An example](http://example.com/)`                               | [An example](http://example.com/)                               |
| `[An example](http://example.com/ "Example link")`                | [An example](http://example.com/ "Example link")                |
| `[A *local* link](/Index.md "Local link back to the main page.")` | [A *local* link](/Index.md "Local link back to the main page.") |

To facilitate writing text, and reusing links, it's possible to use a reference instead of a direct URL in the link definition. This is done using 
brackets instead of parenthesis, with an optional space between the two sections, and a reference ID instead of the URL, as this: `[Text][Reference]` or
`[Text] [Reference]`. References are case insensitive. It's also possible to use an implicit reference identity. In this case, the second set of brackets 
is empty. The reference identity is taken to be the same as the text for the link.

| Markdown              | Result              |
|-----------------------|---------------------|
| `[An example][EX]`    | [An example][EX]    |
| `[An example] [ex]`   | [An example] [ex]   |
| `[Example 1][]`       | [Example 1][]       |
| `[Example 2][]`       | [Example 2][]       |
| `[Example 3][3]`      | [Example 3][3]      |

The references can then be written anywhere in the document (apart from other text). There are various ways to writing link references. They begin on
separate rows, and start with a the reference ID between brackets followed by a colon and then the link `[ID]: URL`. Optionally, the URL can be surrounded
by angle brackets, as follows: `[ID]: <URL>`. The reference can also have an optional title. This title can follow the URL directly, or be written on the 
following row: `[ID]: URL "Title"`. The title can be surounded between double quotes `"Title"`, single quotes `'Title'` or parenthesis `(Title)`, the 
choice is up to the writer. While the references are visible in the markdown document, they will be removed, and not displayed in the generated HTML page.

The following list shows some examples. These examples are used above to create the links in the table.

	[EX]: http://example.com/
	[Example 1]: http://example.com/ "Example 1"
	[ExAMPLE 2]: <http://example.com/> 'Example 2'
	[3]: http://example.com/
		(Example 3)

[EX]: http://example.com/
[Example 1]: http://example.com/ "Example 1"
[ExAMPLE 2]: <http://example.com/> 'Example 2'
[3]: http://example.com/
	(Example 3)

### Inline HTML

Inline HTML elements can be inserted anywhere in markdown text, by just writing it. It can be freely combined with other inline markdown constructs.
Example: `This text is <span style='color:red'>red and **bold**</span>`. This is transformed into: This text is <span style='color:red'>red and **bold**</span>.
You can also use *HTML entities* directly in markdown. For example `&copy;` is transformed into &copy;.

**Note:** Care has to be taken so that the end result is HTML compliant. While HTML can be inserted anywhere, it's only useful if the markdown is 
used to generate HTML pages. If the markdown is used to generate other types of content, such as XAML, inline HTML will be omitted. Since inline HTML
is used within block constructs, only span-level HTML constructs should be used.

### Special characters in HTML

In HTML, certain characters are used to define certain constructs. This includes `<`, `>` and `&`. In markdown, you don't have to escape these, unless they
form part of a markdown construct, an HTML tag or an HTML entity. In all other cases, the markdown parser will escape them for you. So, you can write things
such as "4<5", and "AT&T", without having to escape the < into `&lt;` and the & into `&amp;`.

### Escape character

If you want to use a character that otherwise has a special funcion in markdown, you can escape it with the backslash character `\`, to avoid it being
interpreted as a control character. If you want to include a backslash character in your text, you need to escape it also, and write two `\\`.

The following table lists supported escape sequences. Characters not listed in this table do not need to be escaped.

| Sequence | Result |<div style='width:30px'/>| Sequence | Result |<div style='width:30px'/>| Sequence | Result |<div style='width:30px'/>| Sequence | Result |<div style='width:30px'/>| Sequence | Result |
|:--------:|:------:|-------------------------|:--------:|:------:|-------------------------|:--------:|:------:|-------------------------|:--------:|:------:|-------------------------|:--------:|:------:|
| `\*`     | \*     |                         | `\{`     | \{     |                         | `\)`     | \)     |                         | `\-`     | \-     |                         | `\^`     | \^     |
| `\_`     | \_     |                         | `\}`     | \}     |                         | `\<`     | \<     |                         | `\.`     | \.     |                         | `\%`     | \%     |
| `\~`     | \~     |                         | `\[`     | \[     |                         | `\>`     | \>     |                         | `\!`     | \!     |                         | `\=`     | \=     |
| `\\`     | \\     |                         | `\]`     | \]     |                         | `\#`     | \#     |                         | `\\`     | \\     |                         | `\:`     | \:     |
| ` \` `   | \`     |                         | `\(`     | \(     |                         | `\+`     | \+     |                         | `\"`     | \"     |                         | <code>\\&#124;</code>     | &#124;     |

**Note:** Some characters only have special meaning in certain situations, such as the parenthesis, brackets, etc. The occurrence of such a character
in any other situation does not require escaping.

### Typographical enhancements

There are numerous typographical enhancements added to the markdown parser. This makes it easier to generate beautiful text. Some of these additions are
are inspired by the the [Smarty Pants](http://daringfireball.net/projects/smartypants/) addition to the original markdown, but numerous other character 
sequences have been added to the **IoT Gateway** version of markdown, as shown in the following table:

| Sequence | Becomes |<div style='width:30px'/>| Sequence | Becomes |<div style='width:30px'/>| Sequence | Becomes |<div style='width:30px'/>| Sequence | Becomes |<div style='width:30px'/>| Sequence | Becomes |
|:--------:|:-------:|-------------------------|:--------:|:-------:|-------------------------|:--------:|:-------:|-------------------------|:--------:|:-------:|-------------------------|:--------:|:-------:|
| `...`    | ...     |                         | `(R)`    | (R)     |                         | `>>>`    | >>>     |                         | `]]`     | ]]      |                         | `^o`     | ^o      |
| `"text"` | "text"  |                         | `(p)`    | (p)     |                         | `<--`    | <--     |                         | `+-`     | +-      |                         | `^0`     | ^0      |
| `'text'` | 'text'  |                         | `(P)`    | (P)     |                         | `-->`    | -->     |                         | `-+`     | -+      |                         | `^1`     | ^1      |
| `--`     | --      |                         | `(s)`    | (s)     |                         | `<-->`   | <-->    |                         | `<>`     | <>      |                         | `^2`     | ^2      |
| `---`    | ---     |                         | `(S)`    | (S)     |                         | `<==`    | <==     |                         | `<=`     | <=      |                         | `^3`     | ^3      |
| `(c)`    | (c)     |                         | `<<`     | <<      |                         | `==>`    | ==>     |                         | `>=`     | >=      |                         | `^TM`    | ^TM     |
| `(C)`    | (C)     |                         | `>>`     | >>      |                         | `<==>`   | <==>    |                         | `==`     | ==      |                         | `%0`     | %0      |
| `(r)`    | (r)     |                         | `<<<`    | <<<     |                         | `[[`     | [[      |                         | `^a`     | ^a      |                         | `%00`    | %00     |


Block constructs
----------------------------

Block constructs are larger constructs representing larger blocks in the document. They are all separated from each other using empty rows (or rows including
only white space characters). The following subsections lists the different block constructs that are available in the **IoT Gateway** version of markdown.

### Paragraphs

Paragraphs are created by writing blocks of text and separating them with empty rows (or rows with only white space characters). They are placed within
`<p>` and `</p>` in the generated HTML. Line breaks in your markdown text files are ignored by the markdown parser and interpreted as normal white space.
The generated output will display all text in the paragraph as a continuous block of text, that will adapt itself to the width of the available display
area.

### Line breaks

If you want to include hard line breaks  
in a paragraph, you must terminate the  
rows you want to break with two space  
characters.

### Headers

Headers can be written in different ways, depending on what you prefer. A first level header can be written on one row, followed by a row of variable
length, containing only equal characters (=), as follows:

	First level header
	========================

A second level header is written in a similar fashion, but instead of equal signs, hyphens (-) are used:

	Second level header
	------------------------

Headers can also be written on a single line, prefixing them with hash signs (#) and a space character. The number of hash signs defines the level of 
the header:

	# First level header
	
	## Second level header
	
	### Third level header
	
	#### Fourth level header
	
	...

If using hash signs to define headers, you can suffix any number of hash signs at the end of the row for clarity in the markown. These will not be
displayed in the generated output.

	# First level header #######
	
	## Second level header #####
	
	### Third level header #####
	
	#### Fourth level header ###
	
	...

**Note:** Each header will be assigned a local *id* that you can link to. You can link to any header in a document, by adding a *fragment*, starting
with the hash sign, and then followed by the automatically generated *id*. The *id* is formed by joining the words in the header together using lower case, 
capitalizing the first letter of each word except the first word which is kept all lower case. This is called *Camel Casing*, or *camelCasing*. 
To link to the "Block constructs" header above, for instance, you would write something like this:

	[Block constructs](#blockConstructs)

This would result in the following link: [Block constructs](#blockConstructs)

**Note also:** You can easily add a [Table of Contents](#tableOfContents) constract to the document. It will automatically generate a table of contents 
in the output that will link to all headers available in the document using the automatically generated ids.

### Block quotes

Block quotes are blocks of text, where each paragraph is prefixed by a `>` character and 1-3 space characters (or a tab character). Alternativly,
each row in each paragraph of the block quote can be prefixed by the `>` character and white space, making the text look tidier. Block quotes allow
nested constructs.

Example:

	> A block quote can include other block quotes:
	> 
	> > Like this one
	>
	> It can include tables:
	>
	> | a | b |
	> |---|---|
	> | 1 | 2 |
	> | 3 | 4 |
	> | 5 | 6 |
	>
	> Or code:
	>
	>		10 PRINT "*";
	>		20 GOTO 10
	>
	> It can include lists:
	>
	>	* Item
	>		1. Sub item
	>		2. Sub item 2
	>	* Item 2
	>
	> etc.

This is transformed into:

> A block quote can include other block quotes:
> 
> > Like this one
>
> It can include tables:
>
> | a | b |
> |---|---|
> | 1 | 2 |
> | 3 | 4 |
> | 5 | 6 |
>
> Or code:
>
>		10 PRINT "*";
>		20 GOTO 10
>
> It can include lists:
>
>	* Item
>		1. Sub item
>		2. Sub item 2
>	* Item 2
>
> etc.

### Bullet Lists

Bullet lists are created by simply writing the items prefixed by either asterisks `*`, plus signs `+` or minus signs (hyphens) `-`, followed by one to three
space characters or a tab. If the items are written together, as in the following example, Each item will contain just inline text (including inline 
constructs):

	* Normal text
	* *Emphasized text*
	* **Strong text**

This is displayed as:

* Normal text
* *Emphasized text*
* **Strong text**

If the items are written with empty rows (or rows including only white space) separating them, the items are formatted as paragraphs:

	+ Normal text

	+ *Emphasized text*

	+ **Strong text**

When displayed, this becomes:

+ Normal text

+ *Emphasized text*

+ **Strong text**

Items can span multiple paragraphs as well. In that case, separate the paragraphs, but make sure to indent at least the first row of each paragraph
with 4 space characters, or a tab character. (Each row in the paragraph can be indented, to make the text look tidier, but this is not required.)

	-	This is the first item.

		The first item is written using normal text.

	-	*This is the second item.*

		The first item is written using emphasized text.

	-	*This is the third item.*

		The third item is written using strong text.

This results in:

-	This is the first item.

	The first item is written using normal text.

-	*This is the second item.*

	The first item is written using emphasized text.

-	**This is the third item.**

	The third item is written using strong text.

### Numbered Lists

Numbered lists are created by simply writing the items prefixed by their corresponding number followed by a period `.` a space character. The number used
will be the number that the item receives in the generated list. As with bullet lists, items written together are treated as inline text, while items
separated by empty rows (or rows including only white space) will be treated as items containing paragraphs. Multi-paragraph items are create indenting.
Example of a simple list:

	1. Normal text
	10. *Emphasized text*
	100. **Strong text**

This becomes:

1. Normal text
10. *Emphasized text*
100. **Strong text**

An alternative exists to the fixed numbering scheme. Instead of writing the item number, the hash sign (`#`) can be used to create a lazy numbered list,
as follows:

	#. Normal text
	#. *Emphasized text*
	#. **Strong text**

This is shown as:

#. Normal text
#. *Emphasized text*
#. **Strong text**

All types of lists can be nested. The nesting level is kept track of using 4 space characters or 1 tab character per level. Example:

	* Item 1
		#. Item 1.1
			- Item 1.1.1
			- Item 1.1.2
		#. Item 1.2
	* Item 2
		#. Item 2.1
			- Item 2.1.1
			- Item 2.1.2
		#. Item 2.2

This is tranformed to:

* Item 1
	#. Item 1.1
		- Item 1.1.1
		- Item 1.1.2
	#. Item 1.2
* Item 2
	#. Item 2.1
		- Item 2.1.1
		- Item 2.1.2
	#. Item 2.2

### Code blocks

If you want to include larger blocks of code, there are two ways to do this. In both cases you write the code, as-is, with empty rows before and after.
You can choose to either indent each line of the code with 1-4 spaces or one tab characters:

		10 PRINT "*";

		20 GOTO 10

Or, you can write the code without special indentation, but beginning and ending the the block with rows consisting of three back ticks 
(<code>\`\`\`</code>), as follows:

	```
	10 PRINT "*";

	20 GOTO 10
	```

Note that you can insert blank rows in code. The indentation in the first case, or the three back ticks in the second case, tell the parser when the code
block ends. In both cases, you get the following result:

```
10 PRINT "*";

20 GOTO 10
```

If you want, you can specify the language the code was written in. By doing this, you activate the syntax highlighting feature provided by
[highlight.js](https://highlightjs.org/). Example:

	```basic
	10 PRINT "*";
	20 GOTO 10
	```

This is transformed into:

```basic
10 PRINT "*";
20 GOTO 10
```

### Horizontal rules

### Tables

### Definition Lists

### Footnotes


Multimedia
----------------------------

### Images

### Video

### Audio

### YouTube

### Table of Contents

### Emojis

### Smileys

### Pluggable multi-media interface


Metadata
----------------------------

-   Markdown syntax within block-level HTML constructs is allowed.
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
