﻿Title: Markdown
Description: Markdown syntax reference, as understood by the {{AppName:=Waher.IoTGateway.Gateway.ApplicationName}}.
Date: 2016-02-11
Author: Peter Waher
Master: /Master.md
JavaScript: /Events.js

Markdown syntax reference
=============================

*Markdown* is a very simple, yet efficient format for editing text-based content. The **{{AppName}}** converts Markdown to HTML automatically 
when web browsers download it, making it a powerful tool for publishing online content. The syntax is inspired by the 
[original markdown syntax](http://daringfireball.net/projects/markdown/syntax) as defined by John Gruber at Daring Fireball, but contains 
numerous other additions and modifications. Some of these are introduced in the **{{AppName}}**, others are inspired by selected features used 
in [MultiMarkdown](https://rawgit.com/fletcher/human-markdown-reference/master/index.html) and 
[Markdown Extra](https://michelf.ca/projects/php-markdown/extra/). Below, is a summary of the markdown syntax, as understood by the **{{AppName}}**.

**Note**: You can use the [Markdown Lab](MarkdownLab/MarkdownLab.md) to experiment with Markdown syntax.

![Table of Contents](ToC)

=========================================================================================================================================================

Inline constructs
-----------------------------

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

#### Superscript text

You can add superscript text by inserting the superscript text between a `^[` and a `]`, or a `^(` and a `)`. If you want to use parenthesis `(` or `)`
in your superscript text, you should use `^[` and `]`. You could also escape the parenthesis using `\(` or `\)`. Some special characters,
like the digits and ASCII letters can superseed the `^` sign without having to be enclosed in parenthesis or brackets. See 
[Typographical enhancements](#typographicalEnhancements) for details.

Examples: 

* `Some^[superscript]` ==> Some^[superscript]
* `a^[b+c]=a^ba^c` ==> a^[b+c]=a^ba^c
* `a^2+b^2=c^2` ==> a^2+b^2=c^2
* `a^n+b^n<>c^n, n>=3` ==> a^n+b^n<>c^n, n>=3
* `a^[(b+c)/2]=sqrt(a^ba^c)` ==> a^[(b+c)/2]=sqrt(a^ba^c)
* `a^(\(b+c\)/2)=sqrt(a^ba^c)` ==> a^(\(b+c\)/2)=sqrt(a^ba^c)

#### Subscript

Text between brackets `[` and `]`, that do not form part of a link, a link reference, reference definitions or a multi-media definition or reference, 
is treated as subscript text. Examples:

* `Some[subscript]` ==> Some[subscript]
* `a[i]=A[i,j]` ==> a[i]=A[i,j]

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

**Note**: Characters that have special meaning in markdown, such as \*, \_, \~, etc., are shown as normal characters in inline code.

**Note 2**: If you want to include a back tick in the inline code, you can surround the inline code using double back ticks and a space, one after the
first double back tick, and one before the last back tick, such as this: <code>\`\` \`Inline code\` \`\`</code>. This sequence was used to produce `` `Inline code` ``.

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

#### Shorthand links

Markdown help you include links to online resources (URLs) or mail addresses automatically by surrounding them with `<` and `>` characters, such as 
`<http://example.com/>` or `<address@example.com>`. These would turn into clickable links in the HTML representation, as follows: <http://example.com/> 
and <address@example.com>.

**Note**: It's important to include the *URI Scheme* (for example `http://`) in links or the @ sign in mail addresses, for the parser to understand it's 
an automatic link or an address, and not another type of construct.

#### Automatic web links

The Markdown parser will create automatic links if your link is a web link, i.e. if it starts with `http://` or `https://`. Example:

	http://waher.se/

Simply becomes:

http://waher.se/

**Note**: Some of the multi-media interfaces supported, can also manage automatic links. If you include a web link that such a multimedia interface
recognizes, the corresponding presentation will be used.

#### Abbreviations

You can define abbreviations in your text by writing links using the predefined `abbr` URI schema, as follows:

    [LOL](abbr:Laugh out Loud) and [OMG](abbr:Oh My God) are two abbreviations commonly used in social networks.

Result:

[LOL](abbr:Laugh out Loud) and [OMG](abbr:Oh My God) are two abbreviations commonly used in social networks.

**Note**: In ![HTML](abbr:Hyper Text Markup Language), the `<abbr>` tag is used. Instead of using the `title` attribute to present the
definition, the `data-title` attribute is used. This allows the web designer to define how to present the definition, using CSS.

#### Hashtags

You can create #hashtags in markdown by adding a hash character (`#`) followed by a sequence of letters and/or numbers. No space characters
or punctuation marks are allowed. The main function of hashtags is to highlight keywords in a text. Applications can also use them to create 
spceial types of links. How such a link would work, is application-specific however, if it exists.

Example: `#hashtag` becomes #hashtag.

### Inline HTML

Inline HTML elements can be inserted anywhere in markdown text, by just writing it. It can be freely combined with 
other inline markdown constructs. Example: `This text is <span style='color:{{Theme.NegColor}}'>colored and **bold**</span>`. This is 
transformed into: This text is <span style='color:{{Theme.NegColor}}'>colored and **bold**</span>. You can also use [HTML entities](Entities.md) 
directly in markdown. For example `&copy;` is transformed into &copy;.

**Note**: Care has to be taken so that the end result is HTML compliant. While HTML can be inserted anywhere, it's only 
useful if the markdown is used to generate HTML pages. If the markdown is used to generate other types of content, such 
as XAML, inline HTML will be omitted. Since inline HTML is used within block constructs, only span-level HTML constructs 
should be used.

### Special characters in HTML

In HTML, certain characters are used to define certain constructs. This includes `<`, `>` and `&`. In markdown, you don't have to escape these, unless they
form part of a markdown construct, an HTML tag or an HTML entity. In all other cases, the markdown parser will escape them for you. So, you can write things
such as "4<5", and "AT&T", without having to escape the < into `&lt;` and the & into `&amp;`.

### Escape character

If you want to use a character that otherwise has a special funcion in markdown, you can escape it with the backslash character `\`, to avoid it being
interpreted as a control character. If you want to include a backslash character in your text, you need to escape it also, and write two `\\`.

The following table lists supported escape sequences. Characters not listed in this table do not need to be escaped.

| Sequence | Result |<span style='width:30px'></span>| Sequence | Result |<span style='width:30px'></span>| Sequence | Result |<span style='width:30px'></span>| Sequence | Result |<span style='width:30px'></span>| Sequence | Result |
|:--------:|:------:|--------------------------------|:--------:|:------:|--------------------------------|:--------:|:------:|--------------------------------|:--------:|:------:|--------------------------------|:--------:|:------:|
| `\*`     | \*     |                                | `\{`     | \{     |                                | `\)`     | \)     |                                | `\-`     | \-     |                                | `\%`     | \%     |
| `\_`     | \_     |                                | `\}`     | \}     |                                | `\<`     | \<     |                                | `\.`     | \.     |                                | `\=`     | \=     |
| `\~`     | \~     |                                | `\[`     | \[     |                                | `\>`     | \>     |                                | `\!`     | \!     |                                | `\:`     | \:     |
| `\\`     | \\     |                                | `\]`     | \]     |                                | `\#`     | \#     |                                | `\"`     | \"     |                                | <code>\\&#124;</code> | &#124; |
| ` \` `   | \`     |                                | `\(`     | \(     |                                | `\+`     | \+     |                                | `\^`     | \^     |                                |          |        |

**Note**: Some characters only have special meaning in certain situations, such as the parenthesis, brackets, etc. The occurrence of such a character
in any other situation does not require escaping.

### Typographical enhancements

There are numerous typographical enhancements added to the markdown parser. This makes it easier to generate beautiful text. Some of these additions are
are inspired by the the [Smarty Pants](http://daringfireball.net/projects/smartypants/) addition to the original markdown, but numerous other character 
sequences have been added to the **{{AppName}}** version of markdown, as shown in the following table:

| Sequence | Becomes |<span style='width:30px'></span>| Sequence | Becomes |<span style='width:30px'></span>| Sequence | Becomes |<span style='width:30px'></span>| Sequence | Becomes |
|:--------:|:-------:|--------------------------------|:--------:|:-------:|--------------------------------|:--------:|:-------:|--------------------------------|:--------:|:-------:|
| `...`    | ...     |                                | `-+`     | -+      |                                | `^l`     | ^l      |                                | `^L`     | ^L      |
| `"text"` | "text"  |                                | `<>`     | <>      |                                | `^m`     | ^m      |                                | `^M`     | ^M      |
| `'text'` | 'text'  |                                | `<=`     | <=      |                                | `^n`     | ^n      |                                | `^N`     | ^N      |
| `--`     | --      |                                | `>=`     | >=      |                                | `^o`     | ^o      |                                | `^O`     | ^O      |
| `---`    | ---     |                                | `==`     | ==      |                                | `^p`     | ^p      |                                | `^P`     | ^P      |
| `(c)`    | (c)     |                                | `^0`     | ^0      |                                | `^q`     | ^q      |                                | `^Q`     | ^Q      |
| `(C)`    | (C)     |                                | `^1`     | ^1      |                                | `^r`     | ^r      |                                | `^R`     | ^R      |
| `(r)`    | (r)     |                                | `^2`     | ^2      |                                | `^s`     | ^s      |                                | `^S`     | ^S      |
| `(R)`    | (R)     |                                | `^3`     | ^3      |                                | `^t`     | ^t      |                                | `^T`     | ^T      |
| `(p)`    | (p)     |                                | `^4`     | ^4      |                                | `^u`     | ^u      |                                | `^U`     | ^U      |
| `(P)`    | (P)     |                                |	`^5`     | ^5      |                                | `^v`     | ^v      |                                | `^V`     | ^V      |
| `(s)`    | (s)     |                                | `^6`     | ^6      |                                | `^w`     | ^w      |                                | `^W`     | ^W      |
| `(S)`    | (S)     |                                | `^7`     | ^7      |                                | `^x`     | ^x      |                                | `^X`     | ^X      |
| `<<`     | <<      |                                | `^8`     | ^8      |                                | `^y`     | ^y      |                                | `^Y`     | ^Y      |
| `>>`     | >>      |                                | `^9`     | ^9      |                                | `^z`     | ^z      |                                | `^Z`     | ^Z      |
| `<<<`    | <<<     |                                | `^a`     | ^a      |                                | `^A`     | ^A      |                                | `^TM`    | ^TM     |
| `>>>`    | >>>     |                                | `^b`     | ^b      |                                | `^B`     | ^B      |                                | `^st`    | ^st     |
| `<--`    | <--     |                                | `^c`     | ^c      |                                | `^C`     | ^C      |                                | `^nd`    | ^nd     |
| `-->`    | -->     |                                | `^d`     | ^d      |                                | `^D`     | ^D      |                                | `^rd`    | ^rd     |
| `<-->`   | <-->    |                                | `^e`     | ^e      |                                | `^E`     | ^E      |                                | `^th`    | ^th     |
| `<==`    | <==     |                                | `^f`     | ^f      |                                | `^F`     | ^F      |                                | `%0`     | %0      |
| `==>`    | ==>     |                                | `^g`     | ^g      |                                | `^G`     | ^G      |                                | `%00`    | %00     |
| `<==>`   | <==>    |                                | `^h`     | ^h      |                                | `^H`     | ^H      |                                |          |         |
| `[[`     | [[      |                                |	`^i`     | ^i      |                                | `^I`     | ^I      |                                |          |         |
| `]]`     | ]]      |                                | `^j`     | ^j      |                                | `^J`     | ^J      |                                |          |         |
| `+-`     | +-      |                                |	`^k`     | ^k      |                                | `^K`     | ^K      |                                |          |         |

### Emojis

Emojis are supported, and included into the document using the shortname syntax `:shortname:`. You can provide more *emphasis* by using
more colons before and after: `::shortname::`, `:::shortname:::`, `::::shortname::::`, etc., each resulting in a larger emoji:

	:smiley:, ::smiley::, :::smiley:::, ::::smiley::::, :::::smiley:::::, etc...

Result in:

:smiley:, ::smiley::, :::smiley:::, ::::smiley::::, :::::smiley:::::, etc...

For a list of supported emojis, click [here](Emojis.md).

### Smileys

Smileys are supported in markdown text, and converted to the corresponding emojis. For a list of supported smileys, click [here](Smileys.md).

### HTML Entities

HTML Entities are supported in markdown text, and converted to the corresponding UNICODE characters. For a list of 
supported HTML entities, click [here](Entities.md).

=========================================================================================================================================================


Block constructs
-----------------------------

Block constructs are larger constructs representing larger blocks in the document. They are all separated from each other using empty rows 
(or rows including only white space characters). The following subsections lists the different block constructs that are available in the 
**{{AppName}}** version of markdown.

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
length, containing only equal characters (`=`), as follows:

	First level header
	========================

A second level header is written in a similar fashion, but instead of equal signs, hyphens (`-`) are used:

	Second level header
	------------------------

Headers can also be written on a single line, prefixing them with hash signs (`#`) and a space character. The number of hash signs defines the level of 
the header:

	# First level header
	
	## Second level header
	
	### Third level header
	
	#### Fourth level header
	
	...

**Note**: If you omit the space character after the hash signs, you create a [hashtag](#hashtags) instead.

If using hash signs to define headers, you can suffix any number of hash signs at the end of the row for clarity in the markown. These will not be
displayed in the generated output.

	# First level header #######
	
	## Second level header #####
	
	### Third level header #####
	
	#### Fourth level header ###
	
	...

**Note**: Each header will be assigned a local *id* that you can link to. You can link to any header in a document, by adding a *fragment*, starting
with the hash sign, and then followed by the automatically generated *id*. The *id* is formed by joining the words in the header together using lower case, 
capitalizing the first letter of each word except the first word which is kept all lower case. This is called *Camel Casing*, or *camelCasing*. 
To link to the "Block constructs" header above, for instance, you would write something like this:

	[Block constructs](#blockConstructs)

This would result in the following link: [Block constructs](#blockConstructs)

**Note also**: You can easily add a [Table of Contents](#tableOfContents) constract to the document. It will automatically generate a table of contents 
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

The `>` sign can be optionally prefixed by a `+` or a `-` sign to show the block has been
inserted (`+`) or deleted(`-`). Example:

	+> This paragraph has been added.

	This paragraph is unchanged.

	-> This paragraph has been deleted.

This is transformed to:

+> This paragraph has been added.

This paragraph is unchanged.

-> This paragraph has been deleted.

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
separated by empty rows (or rows including only white space) will be treated as items containing paragraphs. Multi-paragraph items are indented.
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

### Definition Lists

Definition lists can be used to create glossaries, or similar constructs where terms are defined. A definition list is divided into definition blocks. 
Each definition block can have one or more terms followed by a one or more descriptions. The terms are simple inline text, written one term per row. 
The descriptions are prefixed by a colon (`:`) on the first paragraph. If it has more than one paragraph, the first row (at least) each paragraph must 
be indented using 1-4 space characters or one tab character. If you want, you can indent all rows in the paragraphs, to make the text easier to read.

A simple definition list only contains a sequence of terms and simple definitions:

	Term 1
	:	Definition 1

	Term 2
	:	Definition 2

	Term 3
	:	Definition 3

This becomes:

Term 1
:	Definition 1

Term 2
:	Definition 2

Term 3
:	Definition 3

You can group multiple terms for a definition:

	Term 1
	Term 2
	:	Definition for Term 1 and 2.

	Term 3
	:	Definition 3

Which is transformed to:

Term 1
Term 2
:	Definition for Term 1 and 2.

Term 3
:	Definition 3

You can also have multiple descriptions for a single term:

	Term 1
	:	Definition 1.1
	:	Definition 1.2

	Term 2
	:	Definition 2

This is shown as:

Term 1
:	Definition 1.1
:	Definition 1.2

Term 2
:	Definition 2

As with the other forms of lists mentioned above, if you include an empty row (or a row with only whitespace) between terms and definitions, definitions
are considered paragraphs instead of inline text.

	Term 1

	:	Definition 1

	Term 2

	:	Definition 2

	Term 3

	:	Definition 3

Which is displayed as:

Term 1

:	Definition 1

Term 2

:	Definition 2

Term 3

:	Definition 3

You can also have long descriptions spanning multiple paragraphs, or join types, some of inline type, others of paragraph type.

	Term 1

	:	Long Definition for term 1.

		It continues to a second paragraph.

	Term 2
	:	Definition 2

Which becomes:

Term 1

:	Long Definition for term 1.

	It continues to a second paragraph.

Term 2
:	Definition 2

### Task lists

Task lists are lists where items are either checked or unchecked. An unchecked item is prefixed using `[ ]` (note the single space character).
Checked items are prefixed using `[x]` or `[X]`. As with bullet lists or numbered lists, items written together are treated as inline text, while items
separated by empty rows (or rows including only white space) will be treated as items containing paragraphs. Multi-paragraph items are indented.
Example of a simple task list:

	[ ] Unchecked item
	[x] Checked item
	[X] Another checked item

This is displayed as:

[ ] Unchecked item
[x] Checked item
[X] Another checked item

An example of a nested task list:

	[ ] Unchecked item
	[x] Checked item
		[ ] Unchecked subitem
		[x] Checked subitem
		[X] Another checked subitem
	[X] A second checked item

This gets shown as:

[ ] Unchecked item
[x] Checked item
	[ ] Unchecked subitem
	[x] Checked subitem
	[X] Another checked subitem
[X] A second checked item

### Horizontal Alignment of text

You can control horizontal alignment of blocks in Markdown, by using combinations of `<<` and `>>` around the contents of the
blocks, as illustrated in the following sections. You can choose to put the `<<` and `>>` operators on each row, or only on the
first or last rows of each block correspondingly.

**Note**: For the following examples, you might need to decrease the width of the browser window, to properly see how text
alignment works.

#### Left alignment of text

Add `<<` at the beginning of each block, or at the beginning of each row in each block, to left-align the contents. Example:

```
<<##### Left-aligned Example
<<
<<This text is left-aligned. Left-alignment is done by placing `<<` in the beginning of each block, or each row in each block,
<<as appropriate.
```

This is shown as:

<<##### Left-aligned Example
<<
<<This text is left-aligned. Left-alignment is done by placing `<<` in the beginning of each block, or each row in each block,
<<as appropriate.

#### Right alignment of text

Add `>>` at the end of each block, or at the end of each row in each block, to right-align the contents. Example:

```
##### Right-aligned Example>>
>>
This text is right-aligned. Right-alignment is done by placing `>>` at the end of each block, or each row in each block,>>
as appropriate.>>
```

This is shown as:

##### Right-aligned Example>>
>>
This text is right-aligned. Right-alignment is done by placing `>>` at the end of each block, or each row in each block,>>
as appropriate.>>

#### Center alignment of text

Add `>>` at the beginning of each block, or at the beginning of each row in each block, and `<<` at the end of each block,
or at the end of each row in each block, to center-align the contents. Example:

```
>>##### Center-aligned Example<<
>><<
>>This text is center-aligned. Center-alignment is done by placing `>>` in the beginning of each block, or each row in each block,<<
>>and `<<` at the end of each block, or at the end of each row in each block, as appropriate.<<
```

This is shown as:

>>##### Center-aligned Example<<
>><<
>>This text is center-aligned. Center-alignment is done by placing `>>` in the beginning of each block, or each row in each block,<<
>>and `<<` at the end of each block, or at the end of each row in each block, as appropriate.<<

#### Margin alignment of text

Add `<<` at the beginning of each block, or at the beginning of each row in each block, and `>>` at the end of each block,
or at the end of each row in each block, to margin-align the contents. Example:

```
<<##### Margin-aligned Example>>
<<>>
<<This text is margin-aligned. Margin-alignment is done by placing `<<` in the beginning of each block, or each row in each block,>>
<<and `>>` at the end of each block, or at the end of each row in each block, as appropriate.>>
```

This is shown as:

<<##### Margin-aligned Example>>
<<>>
<<This text is margin-aligned. Margin-alignment is done by placing `<<` in the beginning of each block, or each row in each block,>>
<<and `>>` at the end of each block, or at the end of each row in each block, as appropriate.>>

### Code blocks

If you want to include larger blocks of code, there are two ways to do this. In both cases you write the code, as-is, with empty rows before and after.
You can choose to either indent each line of the code with 1-4 spaces or one tab characters:

		10 PRINT "*";

		20 GOTO 10

Or, you can write the code without special indentation, but beginning and ending the the block with rows consisting of three or more back ticks 
(<code>\`\`\`</code>), as follows:

	```
	10 PRINT "*";

	20 GOTO 10
	```

Note that you can insert blank rows in code. Note also that you need to terminate the code block with the same amount of back ticks.
The indentation in the first case, or the three (or more) back ticks in the second case, tell the parser when the code
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

If the language begins with `base64`, and the contents is Base64-encoded UTF-8-encoded text, the corresponding text will be decided and
displayed, where the language is whatever comes after `base64`. This method can be used to maintain literal content and syntax, especially 
if not aware at design time, and avoid conflicts with the Markdown parser. The following example shows how to present XML from script,
in a readable manner, without interfering with the overall structure of the document:

<pre><code class="nohighlight">&#96;&#96;&#96;base64xml
&#123;&#123;Base64Encode(Utf8Encode(PrettyXml(Xml)))&#125;&#125;
&#96;&#96;&#96;</code></pre>

**Note**: By default, the `default.css` highlight style is used on the page, if syntax highlighting using [highlight.js](https://highlightjs.org/)
is available. The library is accessible through the `/Highlight` web folder. You can control the style used for highlighting, by including a
`CSS: /Highlight/style/STYLE_NAME.css` header at the top of the page, where `STYLE_NAME` refers to the actual style to use on the page.

**Note 2**: If you use back-ticks, you must use the same amount of back-ticks when closing the code block, as you did when opening
the code block.

**Note 3**: You can embed code-blocks defined using back-ticks in code-blocks defined by back-ticks, if each embedded code-block uses
fewer back-ticks compared to the parent block.

The {{AppName}} provides a pluggable architecture when it comes to rendering code blocks. Depending on the language, the code can be rendered in 
different ways. The following subsections illustrate such renderings.

#### Indentation of HTML

It is common to indent HTML code, to make it easier to track nesting of HTML elements on the page.
Since HTML can be embedded into Markdown, it is important to be able to allow such HTML code without
converting it to code blocks. For this reason, any indented paragraph that starts with a `<` character
will be treated as indented HTML. Instead of rendering it as a code block, the indentation is simply
removed, and the contents parsed as normal Markdown. However, if you want to explicitly show HTML in
a code block, do not use the indentation method. Instead prefix the HTML with <code>\`\`\`html</code>,
and a <code>\`\`\`</code> at the end.

Example of indented HTML:

```
<div style="background-color: #f0f0f0; border: 1px solid #c0c0c0; padding: 10px; color: black;">

    <p>

        This is a paragraph of text, written in *Markdown*.

    </p>

</div>
```

This renders as:

<div style="background-color: #f0f0f0; border: 1px solid #c0c0c0; padding: 10px; color: black;">

    <p>

        This is a paragraph of text, written in *Markdown*.

    </p>

</div>

#### 2D Layout diagrams

You can make the Markdown engine transform XML that conforms to the `http://waher.se/Schema/Layout2D.xsd` namespace directly to images,
by placing it in a code block with language **layout**. The layout namespace is defined in the `Waher.Layout.Layout2D` library. 

Example of a **layout** diagram (some parts have been removed for splicity; full example here: [GitHub](https://github.com/PeterWaher/IoTGateway/blob/master/Layout/Waher.Layout.Layout2D.Test/Xml/Test_39_Stack.xml)):

	```layout: Neuron architecture
    <Layout2D xmlns="http://waher.se/Schema/Layout2D.xsd"
              background="ThemeBackground" font="Text" textColor="Black">
      <SolidBackground id="ThemeBackground" color="{Theme.BackgroundColor}"/>
      <SolidBackground id="Core" color="{Alpha('Red',128)}"/>
      <SolidBackground id="IoTGateway" color="{Alpha('Orange',128)}"/>
      <SolidBackground id="NeuroLedger" color="{Alpha('Blue',128)}"/>
      <SolidBackground id="Neuron" color="{Alpha('Green',128)}"/>
      <SolidBackground id="App" color="{Alpha('Gray',128)}"/>
      <SolidBackground id="ThirdParty" color="{Alpha('DeepSkyBlue',128)}"/>
      <Font id="Text" name="Arial" size="20pt" color="White"/>
      <Overlays>
        <Grid columns="13">
          <Cell colSpan="2"/>
          <Cell colSpan="2">
            <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
              <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="ThirdParty">
                <Margins left="0.5em" right="0.5em">
				    <Label text="votodirecto.online" x="50%" y="50%" halign="Center"
						    valign="Center" />
                </Margins>
              </RoundedRectangle>
            </Margins>
          </Cell>
          <Cell colSpan="2">
            <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
              <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="ThirdParty">
                <Margins left="0.5em" right="0.5em">
                  <Label text="abc4.io" x="50%" y="50%" halign="Center" valign="Center"/>
                </Margins>
              </RoundedRectangle>
            </Margins>
          </Cell>
          ...
        </Grid>
        <Scale scaleX="0.65" scaleY="0.65">
          <Vertical>
            <Cell>
              <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
                <RoundedRectangle radiusX="5mm" radiusY="5mm" fill="ThirdParty">
                  <Margins left="0.5em" right="0.5em" top="0.25em" bottom="0.25em">
                    <Label text="Third Party applications" x="50%" y="50%" halign="Center" valign="Center"/>
                  </Margins>
                </RoundedRectangle>
              </Margins>
            </Cell>
            ...
          </Vertical>
        </Scale>
      </Overlays>
    </Layout2D>
    ```

```layout: Neuron architecture
<Layout2D xmlns="http://waher.se/Schema/Layout2D.xsd"
          background="ThemeBackground" font="Text" textColor="Black">
    <SolidBackground id="ThemeBackground" color="{Theme.BackgroundColor}"/>
    <SolidBackground id="Core" color="{Alpha('Red',128)}"/>
    <SolidBackground id="IoTGateway" color="{Alpha('Orange',128)}"/>
    <SolidBackground id="NeuroLedger" color="{Alpha('Blue',128)}"/>
    <SolidBackground id="Neuron" color="{Alpha('Green',128)}"/>
    <SolidBackground id="App" color="{Alpha('Gray',128)}"/>
    <SolidBackground id="ThirdParty" color="{Alpha('DeepSkyBlue',128)}"/>
    <Font id="Text" name="Arial" size="20pt" color="White"/>
    <Overlays>
    <Grid columns="13">
        <Cell colSpan="2"/>
        <Cell colSpan="2">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="ThirdParty">
            <Margins left="0.5em" right="0.5em">
				<Label text="votodirecto.online" x="50%" y="50%" halign="Center"
						valign="Center" />
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="2">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="ThirdParty">
            <Margins left="0.5em" right="0.5em">
                <Label text="abc4.io" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="2">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="ThirdParty">
            <Margins left="0.5em" right="0.5em">
                <Label text="lils.is" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="2">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="App">
                <Margins left="0.5em" right="0.5em">
				    <Label text="SPARQL" x="50%" y="50%" halign="Center" valign="Center" />
                </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="3">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="App">
            <Margins left="0.5em" right="0.5em">
                <Label text="Paiwise" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>

        <Cell colSpan="2"/>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Digital ID" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Smart Contracts" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="IoT" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Decision Support" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Provisioning" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Discovery" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Ownership" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Updates" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Monetization" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Tokens" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Synchronization" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>

        <Cell colSpan="2"/>
        <Cell colSpan="11">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Neuro-Foundation Edge Services" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>

        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="App">
            <Margins left="0.5em" right="0.5em">
                <Label text="Tag ID" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="App">
            <Margins left="0.5em" right="0.5em">
                <Label text="Bridges" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell rowSpan="2">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text=".NET Services" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="2">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Web Apps" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="3">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Web Services" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="3">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="NeuroLedger">
            <Margins left="0.5em" right="0.5em">
                <Label text="Distributed DB" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Neuron" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="Server Protocols" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>

        <Cell colSpan="2">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" fill="App">
            <Margins left="0.5em" right="0.5em">
                <Label text="SDK" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="IoTGateway">
            <Margins left="0.5em" right="0.5em">
                <Label text="Markdown" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="2">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="IoTGateway">
            <Margins left="0.5em" right="0.5em">
                <Label text="Web Server" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="IoTGateway">
            <Margins left="0.5em" right="0.5em">
                <Label text="Script" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="2">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="IoTGateway">
            <Margins left="0.5em" right="0.5em">
                <Label text="Object DB" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="IoTGateway">
            <Margins left="0.5em" right="0.5em">
                <Label text="Log" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="2">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="NeuroLedger">
            <Margins left="0.5em" right="0.5em">
                <Label text="Neuro-Ledger" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="IoTGateway">
            <Margins left="0.5em" right="0.5em">
                <Label text="Client Protocols" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>

        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Neuron">
            <Margins left="0.5em" right="0.5em">
                <Label text="NuGets" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell>
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="IoTGateway">
            <Margins left="0.5em" right="0.5em">
                <Label text="NuGets" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
        <Cell colSpan="11">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="IoTGateway">
            <Margins left="0.5em" right="0.5em">
                <Label text="IoT Gateway" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>

        <Cell colSpan="13">
        <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" height="2cm" fill="Core">
            <Margins left="0.5em" right="0.5em">
                <Label text=".NET Core/Standard" x="50%" y="50%" halign="Center" valign="Center"/>
            </Margins>
            </RoundedRectangle>
        </Margins>
        </Cell>
    </Grid>
    <Scale scaleX="0.65" scaleY="0.65">
        <Vertical>
        <Cell>
            <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" fill="ThirdParty">
                <Margins left="0.5em" right="0.5em" top="0.25em" bottom="0.25em">
                <Label text="Third Party applications" x="50%" y="50%" halign="Center" valign="Center"/>
                </Margins>
            </RoundedRectangle>
            </Margins>
        </Cell>
        <Cell>
            <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" fill="App">
                <Margins left="0.5em" right="0.5em" top="0.25em" bottom="0.25em">
                <Label text="TAG applications" x="50%" y="50%" halign="Center" valign="Center"/>
                </Margins>
            </RoundedRectangle>
            </Margins>
        </Cell>
        <Cell>
            <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" fill="Neuron">
                <Margins left="0.5em" right="0.5em" top="0.25em" bottom="0.25em">
                <Label text="TAG Neuron" x="50%" y="50%" halign="Center" valign="Center"/>
                </Margins>
            </RoundedRectangle>
            </Margins>
        </Cell>
        <Cell>
            <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" fill="NeuroLedger">
                <Margins left="0.5em" right="0.5em" top="0.25em" bottom="0.25em">
                <Label text="TAG Neuro-Ledger" x="50%" y="50%" halign="Center" valign="Center"/>
                </Margins>
            </RoundedRectangle>
            </Margins>
        </Cell>
        <Cell>
            <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" fill="IoTGateway">
                <Margins left="0.5em" right="0.5em" top="0.25em" bottom="0.25em">
                <Label text="IoT Gateway" x="50%" y="50%" halign="Center" valign="Center"/>
                </Margins>
            </RoundedRectangle>
            </Margins>
        </Cell>
        <Cell>
            <Margins left="1mm" top="1mm" bottom="1mm" right="1mm">
            <RoundedRectangle radiusX="5mm" radiusY="5mm" fill="Core">
                <Margins left="0.5em" right="0.5em" top="0.25em" bottom="0.25em">
                <Label text="Operating System" x="50%" y="50%" halign="Center" valign="Center"/>
                </Margins>
            </RoundedRectangle>
            </Margins>
        </Cell>
        </Vertical>
    </Scale>
    </Overlays>
</Layout2D>
```

You can also place a layout graph definition in an `.xml` file and link to it from an IMG tag in your web pages. It will be automatically
converted to an image, as requests from `IMG` tags request image content only. Example:

    <img src="/Images/Layout2DExample.xml"/>

Results in:

<img src="/Images/Layout2DExample.xml"/>

If you embed the image in a Markdown page, you will need to add an additional image extension to the resource, to let the Markdown parser know you are embedding image content, and not some other form of content. That will explicitly convert the graph to an image of the requested type. The both cases, the graph will be converted to an image of the requested type when requested by the browser. Example:

    ![Image Link to 2D Layout diagram](/Images/Layout2DExample.xml.webp)

Results in:

![Image Link to 2D Layout diagram](/Images/Layout2DExample.xml.webp)

**Note**: Recognized image file extensions are: `.{{
Extensions:=Create(Waher.Content.Images.ImageCodec).FileExtensions;
LastExtension:=PopLast(Extensions);
Concat(Extensions,"`, `.")+"` and `."+LastExtension}}`

#### GraphViz diagrams

If [GraphViz](http://www.graphviz.org/) is installed on the same machine as the {{AppName}}, it can be used to render diagrams from code blocks. 
There are six diagram types: **dot**, **neato**, **fdp**, **sfdp**, **twopi** and **circo**. Use the corresponding diagram type as language for 
the code block. The diagram type can be suffixed by a colon `:` and a title. The diagram will be rendered as an *SVG* image.

Example of a **dot** GraphViz diagram:

	```dot: Fancy graph
	digraph G { 
		size="4,4"; 
		main [shape=box]; /* this is a comment */ 
		main -> parse [weight=8]; 
		parse -> execute; 
		main -> init [style=dotted]; 
		main -> cleanup; 
		execute -> { make_string; printf} 
		init -> make_string; 
		edge [color=red]; // so is this 
		main -> printf [style=bold,label="100 times"]; 
		make_string [label="make a\nstring"]; 
		node [shape=box,style=filled,color=".7 .3 1.0"]; 
		execute -> compare; 
	}
	```

This is rendered as:

```dot: Fancy graph
digraph G { 
	size="4,4"; 
	main [shape=box]; /* this is a comment */ 
	main -> parse [weight=8]; 
	parse -> execute; 
	main -> init [style=dotted]; 
	main -> cleanup; 
	execute -> { make_string; printf} 
	init -> make_string; 
	edge [color=red]; // so is this 
	main -> printf [style=bold,label="100 times"]; 
	make_string [label="make a\nstring"]; 
	node [shape=box,style=filled,color=".7 .3 1.0"]; 
	execute -> compare; 
}
```

**Note**: If after having installed [GraphViz](http://www.graphviz.org/), the above is not displayed as a graph, make sure to restart the 
{{AppName}} service (or the machine). GraphViz is detected during initialization of the service. Make sure that GraphViz is installed in the 
program data folder, preferrably in its default folder.

**Note 2**: You can make the graph clickable by embedding [URL attributes](http://www.graphviz.org/doc/info/attrs.html#d:URL) on either nodes, edges
or the entire graph.

**Note 3**: You can use the [GraphViz Lab](GraphVizLab/GraphVizLab.md) to experiment with GraphViz syntax.

You can also place a GraphViz graph definition in a `.dv` or `.dot` file and link to it from an IMG tag in your web pages. It will be
automatically converted to an image, as requests from `IMG` tags request image content only. Example:

    <img src="/Images/GraphVizExample.gv"/>

Results in:

<img src="/Images/GraphVizExample.gv"/>

If you embed the image in a Markdown page, you will need to add the extension `.png` or `.svg` to the resource, to let the Markdown parser know
you are embedding image content, and not some other form of content. That will explicitly convert the graph to an image of the requested type
(i.e to PNG or SVG formats). The both cases, the graph will be converted to an image of the requested type when requested by the browser. Example:

    ![Image Link to GraphViz diagram](/Images/GraphVizExample.gv.png)

Results in:

![Image Link to GraphViz diagram](/Images/GraphVizExample.gv.png)

For more information about the GraphViz syntax, see the [GraphViz documentation](http://www.graphviz.org/documentation/).

#### UML with PlantUML

If you have the [PlantUML.jar](http://plantuml.com/download) file stored in the program files folder, and have [Java](https://www.java.com) installed
on the same machine as the {{AppName}}, they can be used to render UML diagrams from code blocks. The diagram will be rendered as an *SVG* image.

Example of a sequence **uml** PlantUML diagram:

	```uml: Simple Sequence diagram
	@startuml
	Alice -> Bob: Authentication Request
	Bob --> Alice: Authentication Response

	Alice -> Bob: Another authentication Request
	Alice <-- Bob: another authentication Response
	@enduml
	```

	```uml: Simple Timing diagram
	@startuml
	robust "Web Browser" as WB
	concise "Web User" as WU

	@0
	WU is Idle
	WB is Idle

	@100
	WU is Waiting
	WB is Processing

	@300
	WB is Waiting
	@enduml
	```

This is rendered as:

```uml: Simple Sequence diagram
@startuml
Alice -> Bob: Authentication Request
Bob --> Alice: Authentication Response

Alice -> Bob: Another authentication Request
Alice <-- Bob: another authentication Response
@enduml
```

```uml: Simple Timing diagram
@startuml
robust "Web Browser" as WB
concise "Web User" as WU

@0
WU is Idle
WB is Idle

@100
WU is Waiting
WB is Processing

@300
WB is Waiting
@enduml
```

**Note**: If after having installed [PlantUML](http://plantuml.com/download) and [Java](https://www.java.com), the above is not displayed as a graph, 
make sure to restart the {{AppName}} service (or the machine). PlantUML and Java are detected during initialization of the service. Make sure that 
PlantUML is installed in the program data folder.

**Note 2**: You can use the [PlantUML Lab](PlantUmlLab/PlantUmlLab.md) to experiment with PlantUML syntax.

You can also place a PlantUML graph definition in a `.uml` file and link to it from an IMG tag in your web pages. It will be
automatically converted to an image, as requests from `IMG` tags request image content only. Example:

    <img src="/Images/PlantUmlExample.uml"/>

Results in:

<img src="/Images/PlantUmlExample.uml"/>

If you embed the image in a Markdown page, you will need to add the extension `.png` or `.svg` to the resource, to let the Markdown parser know
you are embedding image content, and not some other form of content. That will explicitly convert the graph to an image of the requested type
(i.e to PNG or SVG formats). The both cases, the graph will be converted to an image of the requested type when requested by the browser. Example:

    ![Image Link to PlantUML diagram](/Images/PlantUmlExample.uml.png)

Results in:

![Image Link to PlantUML diagram](/Images/PlantUmlExample.uml.png)

For more information about PlantUML syntax, see the [PlantUML Language Reference Guide](http://plantuml.com/PlantUML_Language_Reference_Guide.pdf).

#### Embed inline images

You can embed an image directly into the Markdown by placing the BASE64-encoded version of the image, together with its
image Content-Type, in side a code block.

Example:

    ```image/png:PNG as a code block
    iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAMAAAD04JH5AAAAwFBMVEUAAADtTFztTFztTFz5+fkq
    X57tTFztTFztTFztTFz5+fkqX57tTFz5+fkqX57tTFztTFztTFztTFz5+fn5+fkqX54qX575+fn5
    +fkqX54qX575+fkqX54qX57tTFz5+fn5+fkqX575+fkqX575+fntTFztTFwqX575+fn5+fn5+fkq
    X54qX54qX57tTFz5+fkqX56Fosa4yd2ettE3aaTF0+Ls7/NEcqnS3Ojf5u5RfK94mcBehrWSrMxr
    j7qrv9fPtihIAAAALnRSTlMA74AQ7+9Aj89gEBCfgIAgv1DfQGBAz4/PYI+fIJ+vIN+/v99QcDBQ
    cK8wMK9wikih8QAAA3lJREFUeAHslwVi61AMBD+8MjNjyrQyh3P/U5UhsIZaKncuoMmTtVL+/FKK
    X5Yup/f2huQO3OH296dOFt6n9uba1rJ0gS5WNxZ33rb68XpF+kAfuysHb1X9fH1IBsEgbuXMvvrE
    2rJQQFldHLctPz0kKSAFNzVuX54BvLkCL08EiILFyFckE2Syqw2HiQvJATmcjqvmfki0AnDlc2Fi
    XfJBPislH2FzWWwEsFoqoJeGxEoArsS3uCbFQDEWXz38YiuAV0bCllgLYONt6gvMDXj9RkMtgA1N
    /31fL4Cp8t9/CIT5AjazsCSEBuAZCKBAHmwOCSECqhYCLjcTJ2j+JgCQGAhgNW8v8P3jAUBgIYCV
    nP0rlCoA1EwEkLmdJ4aEkeCepomAy2rChVAC3NMxEcBpsQn0Wi/UcE/cesEjAvpZrHQ/e4QMokQh
    sFswgj2k4mlaAEwV/ALrVVCqddEJuPGiO6gFQku6gdkT0BFs1tBHrW8YYfYE08IIA/QQhGIggCn6
    AJwXAxrHsHqCNUnBRxe+hQC/DJaFk6CHxEhgtb/+uRBIGnhGAjjjazhlEQJx/JgBegG+loeyO+CH
    oc97gJI4fgfwDsRtuaUdsx6gLAfFOlAD4IePmeCTswQ2PagIp/748+94fIQ6F9DtxE1JIUAQ9gVj
    YCSAnSIpFDWlj2ZkJbCY/2eUYyWwQWJQIaALQ/kIAdBjVCGgOE4vP0bghN0iCgHFVbL3MQL7n0dg
    6GMEnMUUChT8Cnwagc2PEnj+c3D1Qfx54lfgV+Cm3brAkSSGoQD6HSskRUUHKGGX+N//eCvqcQ/0
    QEG89A4QMkXoQnA30cX0+xwg0EXA3Y0ubrhLdJHwgi5gCh0UmEYHDabSQYUZ6WAEzMLuFjzK7C7j
    0czuZrwiPqPQZJ8ImI2dbXij+LRBU326kFFhR6J4J/j8RYyKzwOY4PMARsXnAUxweQCHmbjgmcQu
    Ep5a2cGK51R8MtDM3f8B6D2WMz6nhZcqii+MwgvJiC8lpwo0tecvoG9LDvimxks0fFtz2t80p/1N
    cIq/qS75/ygJTyMJO4yFJykjdtHMU2TFXrPwMJlxgK48aFUckxYesCQcF4Q7ScApNMi+7RVn0SA+
    2xuthT9QquJ0WxZ+i+QNF5nzwi8secalxtoKnyitjugi3cI0CV/INIVbwh7//fcL7aUSs7ldhxUA
    AAAASUVORK5CYII=
    ```

Result:

```image/png:PNG as a code block
iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAMAAAD04JH5AAAAwFBMVEUAAADtTFztTFztTFz5+fkq
X57tTFztTFztTFztTFz5+fkqX57tTFz5+fkqX57tTFztTFztTFztTFz5+fn5+fkqX54qX575+fn5
+fkqX54qX575+fkqX54qX57tTFz5+fn5+fkqX575+fkqX575+fntTFztTFwqX575+fn5+fn5+fkq
X54qX54qX57tTFz5+fkqX56Fosa4yd2ettE3aaTF0+Ls7/NEcqnS3Ojf5u5RfK94mcBehrWSrMxr
j7qrv9fPtihIAAAALnRSTlMA74AQ7+9Aj89gEBCfgIAgv1DfQGBAz4/PYI+fIJ+vIN+/v99QcDBQ
cK8wMK9wikih8QAAA3lJREFUeAHslwVi61AMBD+8MjNjyrQyh3P/U5UhsIZaKncuoMmTtVL+/FKK
X5Yup/f2huQO3OH296dOFt6n9uba1rJ0gS5WNxZ33rb68XpF+kAfuysHb1X9fH1IBsEgbuXMvvrE
2rJQQFldHLctPz0kKSAFNzVuX54BvLkCL08EiILFyFckE2Syqw2HiQvJATmcjqvmfki0AnDlc2Fi
XfJBPislH2FzWWwEsFoqoJeGxEoArsS3uCbFQDEWXz38YiuAV0bCllgLYONt6gvMDXj9RkMtgA1N
/31fL4Cp8t9/CIT5AjazsCSEBuAZCKBAHmwOCSECqhYCLjcTJ2j+JgCQGAhgNW8v8P3jAUBgIYCV
nP0rlCoA1EwEkLmdJ4aEkeCepomAy2rChVAC3NMxEcBpsQn0Wi/UcE/cesEjAvpZrHQ/e4QMokQh
sFswgj2k4mlaAEwV/ALrVVCqddEJuPGiO6gFQku6gdkT0BFs1tBHrW8YYfYE08IIA/QQhGIggCn6
AJwXAxrHsHqCNUnBRxe+hQC/DJaFk6CHxEhgtb/+uRBIGnhGAjjjazhlEQJx/JgBegG+loeyO+CH
oc97gJI4fgfwDsRtuaUdsx6gLAfFOlAD4IePmeCTswQ2PagIp/748+94fIQ6F9DtxE1JIUAQ9gVj
YCSAnSIpFDWlj2ZkJbCY/2eUYyWwQWJQIaALQ/kIAdBjVCGgOE4vP0bghN0iCgHFVbL3MQL7n0dg
6GMEnMUUChT8Cnwagc2PEnj+c3D1Qfx54lfgV+Cm3brAkSSGoQD6HSskRUUHKGGX+N//eCvqcQ/0
QEG89A4QMkXoQnA30cX0+xwg0EXA3Y0ubrhLdJHwgi5gCh0UmEYHDabSQYUZ6WAEzMLuFjzK7C7j
0czuZrwiPqPQZJ8ImI2dbXij+LRBU326kFFhR6J4J/j8RYyKzwOY4PMARsXnAUxweQCHmbjgmcQu
Ep5a2cGK51R8MtDM3f8B6D2WMz6nhZcqii+MwgvJiC8lpwo0tecvoG9LDvimxks0fFtz2t80p/1N
cIq/qS75/ygJTyMJO4yFJykjdtHMU2TFXrPwMJlxgK48aFUckxYesCQcF4Q7ScApNMi+7RVn0SA+
2xuthT9QquJ0WxZ+i+QNF5nzwi8secalxtoKnyitjugi3cI0CV/INIVbwh7//fcL7aUSs7ldhxUA
AAAASUVORK5CYII=
```

#### Embed PDF documents

You can embed a PDF document directly on the Markdown page using a code block, in a way similar
to embedding images. You include the BASE64-encoded contents of the PDF document into the code
block, and ensure you use the Internet Content-Type `application/pdf` for PDF as the language 
of the code block.

Example:

    ```application/pdf:PDF Document
    JVBERi0xLjcNCiW1tbW1DQoxIDAgb2JqDQo8PC9UeXBlL....
    ....
    ```

This will result in an embedded object being rendered in HTML, displaying the contents of the
PDF document, if the browser supports embedding documents in HTML 5.

### Comments

You can add comments to Markdown documents. Comments are not exported when rendering output. Comments are put in separate blocks, each row
prefixed by double slash `//`. Example:

	This is a paragraph

	// This is a comment on one row

	This is another paragraph

	// Comments can be written
	// on multiple rows, but
	// each row needs to be
	// prefixed by //.

### Horizontal rules

Horizontal rules can be used to separate sections of the text. There are various ways of including a horizontal rule. On a separate line, write a
line only consisting of asterisks (`*`) or hyphens (`-`). You can optionally insert spaces between the asterisks or hyphens if you want. Examples:

	*********************

	---------------------

	* * * * * * * * * * *

	- - - - - - - - - - -

The all produce the same result:

*********************

---------------------

* * * * * * * * * * *

- - - - - - - - - - -

### Sections

Sections can be used to divide a longer text into sections, and provide customized layout for each section. Sections are separated using a block
consisting of a single row of only equal signs (`=`). Example:

	===============================

You can also provide guidance on how many columns you think the new section should have. You can do this by dividing the section separator into blocks,
delimited by space characters. The following example creates a section for content that should be displayed in two columns, if column support is provided.

	=============    ==============

For a section with three columns, write:

	========   =========   ========

This document is an example of a document that has been divided into sections using section separators.

**Note**: Column support is only available in some web clients (HTML). Column support is not available in XAML rendering.

### Invisible breaks

You can add invisible breaks to your markdown. Such breaks can be used by calling applications to divide the document depending on context. An example
can be to cut the document at a logical place to display a brief version of the document. Invisible breaks are inserted using a block
consisting of a single row of only tilde signs (`~`). Example:

	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


### Footnotes

You can include footnotes into a document in two ways. Either you insert one into the flowing text directly, or by reference. To include a footnote
directly into the text you annotate, you include it as follows: `[^footnote text]`. Note that the foot note text can be formatted using inline constructs.
Example:

	In this text we[^With **we**, *we* mean second-person plural] reference a footnote.

This becomes:

In this text we[^With **we**, *we* mean second-person plural] reference a footnote.

Note that footnotes in HTML are clickable, and shown at the bottom of the page.

It's also possible to include a footnote through reference. This makes it possible to create a text that is more similar to the final output. However,
footnotes are always displayed at the bottom of the page, not the place where you write them in the text. Example:

	In this text we[^we] reference a footnote.

This is transformed to:

In this text we[^we] reference a footnote.

We also need to write the actual footnote text somewhere in the document. This is done as follows:

	[^we]: With **we**, *we* mean second-person plural

[^we]: With **we**, *we* mean second-person plural

**Note**: The numbers used in footnotes are automatically generated. If you create footnotes such as `[^1]: ...`, etc., there's no guarantee that the
final footnote will actually get the number you used in the text.

### Tables

Tables are formed by a collection of rows, each row having a given number of cells. A table can also have an optional caption and id. Each column is separated
by a pipe character (`|`). Each row can also optionally start and end with a pipe character. Each row in the table is written using one row in the markdown
text. If you want to include a lot of information into the table, consider including content using the [Markdown inclusion](#markdownInclusion) operator,
described below.

One row in the table is special: It separates the header rows from the content rows of the table. The contents of the columns in this separation row
must only be hyphens (`-`), with optional colons either prefixed or suffixed (or both) to it, to illustrate column alignment. Any white space before and
after is ignored. The followig table shows how column alignment is controlled using header separators:

| Example | Meaning |
|---------|---------|
|`-------`|Default alignment|
|`:------`|Left alignment|
|`------:`|Right alignment|
|`:-----:`|Center alignment|

Cells can also be joined together horizontally. This is done by leaving the deleted column completely blank, not even including white space. 
The preceding column will increase its width in the table, to include the removed cell.

At the end of a table, you can include an optional caption, and an optional id. This is done between brackets. To include only an id, simply
add at the end:

	[table_id]

This id will also be used as a caption for the table. If you want to include a caption that is different from the id, you write:

	[table caption][table_id]

The following examples, borrowed from [MultiMarkdown](https://rawgit.com/fletcher/human-markdown-reference/master/index.html) are illustrative, and show
how tables can be built using Markdown:

	| First Header | Second Header |         Third Header |  
	| :----------- | :-----------: | -------------------: |  
	| First row    |      Data     | Very long data entry |  
	| Second row   |    **Cell**   |               *Cell* |  
	[simple_table]

This becomes:

| First Header | Second Header |         Third Header |  
| :----------- | :-----------: | -------------------: |  
| First row    |      Data     | Very long data entry |  
| Second row   |    **Cell**   |               *Cell* |  
[simple_table]

A more complex example:

	|              | Grouping                    ||  
	| First Header | Second Header | Third Header |  
	| ------------ | :-----------: | -----------: |  
	| Content      | *Long Cell*                 ||  
	| Content      | **Cell**      | Cell         |  
	| New section  | More          | Data         |  
	[Prototype table][reference_table]

This is transformed to:

|              | Grouping                    ||  
| First Header | Second Header | Third Header |  
| ------------ | :-----------: | -----------: |  
| Content      | *Long Cell*                 ||  
| Content      | **Cell**      | Cell         |  
| New section  | More          | Data         |  
[Prototype table][reference_table]

**Note**: It is not important to keep columns aligned in the markdown text. The Markdown parser makes sure the table is exported correctly. The only
reason for maintaining columns in the markdown text aligned, is to make it more readable.

You can provide cell-specific alignment overrides by using the left, right and center alignment constructs within a cell. This can be done both
in headers and in data cells of the table. Example:

    | Cell alignment table                        |||
    | Left          | Center        | Right         |
    | <<Left        | <<Left        | <<Left        |
    | Right>>       | Right>>       | Right>>       |
    | >>Center<<    | >>Center<<    | >>Center<<    |
    |:--------------|:-------------:|--------------:|
    | Normal        | Normal        | Normal        |
    | <<Left        | <<Left        | <<Left        |
    | Right>>       | Right>>       | Right>>       |
    | >>Center<<    | >>Center<<    | >>Center<<    |

This generates a table that looks like:

| Cell alignment table                        |||
| Left          | Center        | Right         |
| <<Left        | <<Left        | <<Left        |
| Right>>       | Right>>       | Right>>       |
| >>Center<<    | >>Center<<    | >>Center<<    |
|:--------------|:-------------:|--------------:|
| Normal        | Normal        | Normal        |
| <<Left        | <<Left        | <<Left        |
| Right>>       | Right>>       | Right>>       |
| >>Center<<    | >>Center<<    | >>Center<<    |

You can combine footnotes and tables as a way to create tables with cells that contain more
complex content, that does not fit into simple short lines. If a cell has a single footnote
reference, it will be assumed that the contents of that footnote should be rendered in the
cell, instead of a reference. Example:

    | Table with complex cells ||
    |:-------------|:-----------|
    | [^e11]       | [^e12]     |
    | [^e21]       | [^e22]     |

    [^e11]:	| Info about cell ||
	    |:--------|-------:|
	    | Row     | 1      |
	    | Column  | 1      |

    [^e12]:	| Info about cell ||
	    |:--------|-------:|
	    | Row     | 1      |
	    | Column  | 2      |

    [^e21]:	| Info about cell ||
	    |:--------|-------:|
	    | Row     | 2      |
	    | Column  | 1      |

    [^e22]:	| Info about cell ||
	    |:--------|-------:|
	    | Row     | 2      |
	    | Column  | 2      |

This is rendered as:

| Table with complex cells ||
|:-------------|:-----------|
| [^e11]       | [^e12]     |
| [^e21]       | [^e22]     |

[^e11]:	| Info about cell ||
	|:--------|-------:|
	| Row     | 1      |
	| Column  | 1      |

[^e12]:	| Info about cell ||
	|:--------|-------:|
	| Row     | 1      |
	| Column  | 2      |

[^e21]:	| Info about cell ||
	|:--------|-------:|
	| Row     | 2      |
	| Column  | 1      |

[^e22]:	| Info about cell ||
	|:--------|-------:|
	| Row     | 2      |
	| Column  | 2      |

### Block-level HTML

You can add HTML blocks to your markdown text. As with all block constructs, HTML blocks must be separated from other text by empty rows (or rows only
including whitespace). The difference between block-level HTML and inline HTML is that block-level HTML reside outside of paragraphs and other similar
constructs (i.e. div-type, or block-type HTML constructs are possible), while inline HTML is limited to span-type or inline-type HTML constructs.
Block-level HTML can be combined with markdown.

Example:

	<div style="border:1px solid black;background-color:#e0e0e0;color:navy;padding:30px;text-align:center">
	This text is _formatted_ using **Markdown**.
	</div>

This is shown as:

<div style="border:1px solid black;background-color:#e0e0e0;color:navy;padding:30px;text-align:center">
This text is _formatted_ using **Markdown**.
</div>


=========================================================================================================================================================



Multimedia
-----------------------------

Multimedia items are defined in a similar way as links in a markdown document. They can both be defined inline, or by reference, as links are too. 
Four things differ, between multimedia links and normal links:

1. The link to a multimedia item must be prefixed by an exclamation mark (`!`).
2. The definition can have an optional `WIDTH` and `HEIGHT` value after the optional title. Both are positive integers, and both can be provided in both
the inline version and the referenced version.
3. The URL the link is pointing to, selects the best multimedia interface.
4. It is possible to define multi-resolution or multi-format multimedia content items, by listing a sequence of URLs pointing to resources of
different sizes and formats. If the multimedia interface supports multi-format or multi-resolution content, all these resources will be used. If the
interface only supports a single source, the first source in the definition will be used. Examples of multi-resolution and multi-format content items
will be given below.

Developers on the platform can add their own multimedia interfaces. All they need to do is implement a class with a default constructor, that
implements the `Waher.Content.Markdown.Model.IMultimediaContent` interface. The parser will find the class and instantiate it, and then use it for
content that it matches. The multimedia interfaces described below only cover the interfaces that are included by default.

**Note**: If no particular multimedia handler is found for a URL, it is considered to be an image by default.

### Images

An image can both be included inline, in flowing text, or standalone in a separate block. In the latter case, it's rendered as a figure, with a
figure caption. To include an image inline, you can do as follows:

	This is an inline image: ![Smiley](/Graphics/Emoji1/png/64x64/2714.png "Check" 24 24)

This will be displayed as follows:

This is an inline image: ![Checkmark](/Graphics/Emoji1/png/64x64/2714.png "Check" 24 24)

If you put an image on a row by itself, it will be rendered as a figure, with a figure caption. Example:

	![Flag of Chile](/Graphics/Emoji1/png/128x128/1f1e8-1f1f1.png "Check" 128 128)

This becomes:

![Flag of Chile](/Graphics/Emoji1/png/128x128/1f1e8-1f1f1.png "Check" 128 128)

You can also define multi-resolution images as follows. In HTML, they are rendered using the `<picture>` element.

	![Banner](CactusRoseBanner2000x600.png 2000 600)
		(CactusRoseBanner1900x500.png 1900 500)
		(CactusRoseBanner1400x425.png 1400 425)

Now, the browser will select the most appropriate image, based on available space, if the browser supports responsive images based on the 
`<picture>` element. This is how it will look in your browser:

![Banner](/Themes/CactusRose/CactusRoseBanner2000x600.png 2000 600)
	(/Themes/CactusRose/CactusRoseBanner1900x500.png 1900 500)
	(/Themes/CactusRose/CactusRoseBanner1400x425.png 1400 425)

In the same way, you can also define multi-format images using reference notation. You simply list the media items and their different resolutions, if availble
one after the other, optionally on separate rows.

	![Cactus Rose][]
	
	[Cactus Rose]: CactusRose1600x1600.png 1600 1600
		CactusRose800x800.png 800 800

In your browser, this is displayed as:

![Cactus Rose][]
	
[Cactus Rose]: /Themes/CactusRose/CactusRose1600x1600.png 1600 1600
	/Themes/CactusRose/CactusRose800x800.png 800 800

A short summary:

* `<img>` elements are used in HTML to display an image.
* Multi-resolution images are encapsulated in `<picture>` elements, where each image is made available in a separate `<source>` element.
* If the image is alone on a paragraph, it is furthermore encapsulated in a `<figure>` element, and its caption in a `<figcapton>` element.

### Video

You can insert video content into your markdown documents, as you would insert images. The file extension is used to identify the content item as video.
When publishing video on web pages, it's important to remember that different clients have support for different video container formats and codecs.
For this reason, it's recommended to publish multi-format video so that the client can choose the stream that best suits its capabilities. 
Example[^This example uses video from <http://techslides.com/sample-webm-ogg-and-mp4-video-files-for-html5>]:

	![Sample video](/Video/small.webm 560 320)
		(/Video/small.ogv 560 320)
		(/Video/small.mp4 560 320)
		(/Video/small.3gp 352 288)
		(/Video/small.flv 320 240)

This becomes:

![Sample video](/Video/small.webm 560 320)
	(/Video/small.ogv 560 320)
	(/Video/small.mp4 560 320)
	(/Video/small.3gp 352 288)
	(/Video/small.flv 320 240)

### Audio

You can also insert audio content into your markdown documents. The file extension is used to identify the content item as audio.
When publishing audio on web pages, it's important to remember that different clients have support for different audio container formats and codecs.
For this reason, it's recommended to publish multi-format audio so that the client can choose the stream that best suits its capabilities.
Example[^This example uses sound from <http://soundbible.com/2084-Glass-Ping.html>]:

	![Sample audio](/Audio/glass_ping-Go445-1207030150.mp3)
		(/Audio/glass_ping-Go445-1207030150.wav)

**Note**: This will not be visible in the browser, but will cause it to play the sound when the page loads, if sound is supported. Audio clips will not loop.

![Sample audio](/Audio/glass_ping-Go445-1207030150.mp3)
	(/Audio/glass_ping-Go445-1207030150.wav)

### YouTube

To include YouTube clips into your document is easy. A YouTube multimedia content plugin recognizes the YouTube video URL and inserts it accordingly
into the generated page inside an `<iframe>` element. Example:

	![Complex perturbation](https://www.youtube.com/watch?v=whBPLc8m4SU 800 600)

or:

	![Complex perturbation](https://www.youtu.be/whBPLc8m4SU 800 600)

This is transformed to:

![Complex perturbation](https://www.youtube.com/watch?v=whBPLc8m4SU 800 600)

### External web page

You can embed external web content in an `<iframe>` by using the multimedia inclusion syntax. If the content points to a text page (HTML included),
or the resource ends with `/`, and no other multimedia interface provides a better match, the content is embedded as a web page. Example:

	![Wikipedia](http://wikipedia.com/ 1200 300)

This becomes:

![Wikipedia](http://wikipedia.com/ 1200 300)

**Note**: You can't embed local markdown this way, since it will be included directly into the document, as described [below](#markdownInclusion).

### Table of Contents

Inserting a Table of Contents into your document is easy. It's compiled automatically from all headers in the document. To insert it, you simply write
the following where you want it inserted. This segment is taken from the Table Of Contents shown at the [top of the page](#markdownSyntaxReference).

	![Table of Contents](ToC)

**Note**: If a page only contains one level 1 header, it's considered a page title, and not included in the table of contents.

### Markdown inclusion

It is possible to include other local markdown documents directly into the flowing text of the current document. This is done by loading the 
document, parsing it and generating the corresponding output in the same place where the inclusion was made. This makes it possible to create 
reusable markdown templates that you can reuse from your whole site. It also allows you to create output that would not be possible using normal 
markdown syntax. You can also pass parameters to the referenced markdown documents using query parameters in the local URL.

Example:

	| Table 3                           | Table     4                       | Table 5                           |
	|:---------------------------------:|:---------------------------------:|:---------------------------------:|
	|![Table 3](Templates/Repeat.md?x=3)|![Table 4](Templates/Repeat.md?x=4)|![Table 5](Templates/Repeat.md?x=5)|

Where the contents of the `Repeat.md` file is:

	| n  | \*{x}  |
	|:--:|:------:|
	| 1  | {1*x}  |
	| 2  | {2*x}  |
	| 3  | {3*x}  |
	| 4  | {4*x}  |
	| 5  | {5*x}  |
	| 6  | {6*x}  |
	| 7  | {7*x}  |
	| 8  | {8*x}  |
	| 9  | {9*x}  |
	| 10 | {10*x} |

This is then transformed into:

| Table 3                           | Table     4                       | Table 5                           |
|:---------------------------------:|:---------------------------------:|:---------------------------------:|
|![Table 3](Templates/Repeat.md?x=3)|![Table 4](Templates/Repeat.md?x=4)|![Table 5](Templates/Repeat.md?x=5)|

**Note**: Remember that the inclusion paths of the markdown content you want to include, are relative to the location of the main markdown file.
The system will detect circular references and return an error if you try to create a document that creates such a circular reference. Also, 
included markdown files must not contain any metadata.

**Note 2**: Script parameters can be either **double**, **boolean** or **string** values. If the value cannot be parsed as a double or a
boolean value, it is taken to be a string. Any further parsing must be done by script in the template.


=========================================================================================================================================================


Script
-----------------------------

[Script](Script.md) can be used to make your markdown pages dynamic. The following sections describe different options. For more information
about script, see the [Script reference](Script.md). You can also use the [Prompt](Prompt.md) to experiment with script syntax.

Script in Markdown can be processed in three different ways:

1. **Inline processing**: The script is evaluated as part of rendering, and the result presented in the place of the script.
2. **Pre-processing**: Script is evaluated prior to rendering the page for display. This allows script to modify the structure of the
Markdown document.
3. **Asynchronous processing**: Long-running script can be run asynchronously. This means the page is rendered and returned to the client.
When the script is evaluated, it is returned to the client, which inserts it in the place of the script.

### Inline script

[Script](Script.md) can be embedded inline in a block, between curly braces `{` and `}`. The result is then presented in the final output.
Example:

	*a* is {a:=5} and *b* is {b:=6}. *a*\**b* is therefore {a*b}.

This becomes:

*a* is {a:=5} and *b* is {b:=6}. *a*\**b* is therefore {a*b}.

**Note**: Inline script must all reside in a block. While new-line can be used in such inline script, empty rows separating blocks cannot.

#### Graphs

If you use inline script, the result may control how the data is output. Normally, strings are inserted. But graphs and images can also be
generated in script. In these cases, they are displayed directly. Example:

	{
	GraphWidth:=800;
	GraphHeight:=400;
	x:=-10..10|0.1;
	y:=sin(5*x)*exp(-(x^2/10));
	plot2dcurve(x,y)
	}

This script is evaluated as:

{
GraphWidth:=800;
GraphHeight:=400;
x:=-10..10|0.1;
y:=sin(5*x)*exp(-(x^2/10));
plot2dcurve(x,y)
}

### Pre-processed script

Pre-processed script is inserted into the markdown page between double curly braces <code>\{\{</code> and <code>\}\}</code>. Such script is 
evaluated before parsing the markdown, and the script is replaced by the result, *as strings*. Normal inline script between single braces are 
evaluated after markdown processing, and can contain any type of result. Such script does not change the structure of the markdown document. 
Pre-processed script however, can change the actual structure and formatting of the document.

Example:

<pre><code class="nohighlight">
Result of execution: &#123;&#123;s:=&quot;some text&quot;;&quot;\*\*&quot;+s+&quot;\*\*&quot;&#125;&#125;.
</code></pre>

This is transformed to:

Result of execution: {{s:="some text";"**"+s+"**"}}.

#### Loops and dynamic content

You can use the *implicit print* operation in script to dynamically fill the document with contents available though script. This makes it
possible to create dynamic documents with markdown, and not only static ones. The basics consists of defining the script as a pre-processed
script block, and implicitly printing the contents. The following example illustrates this point, by creating a multiplication table:

<pre><code class="nohighlight">
| \\\* |\{\{
for each x in 1..15 do
	\]\] \(\(x\)\) |\[\[;
&nbsp;  
\]\]
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|\[\[;
&nbsp;  
for each y in 1..15 do
(
	\]\]
| \*\*((y))\*\* |\[\[;
&nbsp;  
	for each x in 1..15 do
		\]\] ((x*y)) |\[\[;
)
\}\}
</code></pre>

| \* |{{
for each x in 1..15 do
	]] ((x)) |[[;

]]
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|[[;

for each y in 1..15 do
(
	]]
| **((y))** |[[;

	for each x in 1..15 do
		]] ((x*y)) |[[;
)		
}}

### Asynchronous processing of script

It is possible to execute script on the page asynchronously as well. This is done by adding a *Code Block* of type `async`.
The `async` can be succeeded by a colon (`:`) and some text that will be displayed while the script is being executed and the
result loaded to the client. You can use the `preview` function to display a partial result during the calculation. Example:

    ```async:Previewing intermediate results
    PlotCPU(TP[],CPU[]):=
    (
        one:=Ones(count(TP));
        TP2:=TP.^2;
        TP3:=TP.^3;
        M1:=[one,TP];
        M2:=[one,TP,TP2];
        M3:=[one,TP,TP2,TP3];
        [[a1,b1]]:=((CPU[,])*(M1 T))*inv(M1*(M1 T));
        [[a2,b2,c2]]:=((CPU[,])*(M2 T))*inv(M2*(M2 T));
        [[a3,b3,c3,d3]]:=((CPU[,])*(M3 T))*inv(M3*(M3 T));
        G:=plot2dcurve(TP,avg(CPU)*one,"Blue")+
            plot2dcurve(TP,a1+b1*TP,"Green")+
            plot2dcurve(TP,a2+b2*TP+c2*TP2,"Yellow")+
            plot2dcurve(TP,a3+b3*TP+c3*TP2+d3*TP3,"Orange")+
            plot2dcurve(TP,CPU,"Red",5);
        G.Title:="CPU";
        G.LabelX:="Time (s)";
        G.LabelY:="CPU (%)";
        G
    );
    
    TP:=[];
    CPU:=[];
    Start:=Now;
    foreach x in 1..60 do
    (
        Sleep(1000);
        CPU:=join(CPU,(PerformanceCounterValue("Processor","_Total","% Processor Time") ??? Uniform(0,100)));
        TP:=join(TP,Now.Subtract(Start).TotalSeconds);
        preview(PlotCPU(TP,CPU))
    );
    ```

This generates the following result (reload to restart):

```async:Previewing intermediate results
PlotCPU(TP[],CPU[]):=
(
    one:=Ones(count(TP));
    TP2:=TP.^2;
    TP3:=TP.^3;
    M1:=[one,TP];
    M2:=[one,TP,TP2];
    M3:=[one,TP,TP2,TP3];
    [[a1,b1]]:=((CPU[,])*(M1 T))*inv(M1*(M1 T));
    [[a2,b2,c2]]:=((CPU[,])*(M2 T))*inv(M2*(M2 T));
    [[a3,b3,c3,d3]]:=((CPU[,])*(M3 T))*inv(M3*(M3 T));
    G:=plot2dcurve(TP,avg(CPU)*one,"Blue")+
        plot2dcurve(TP,a1+b1*TP,"Green")+
        plot2dcurve(TP,a2+b2*TP+c2*TP2,"Yellow")+
        plot2dcurve(TP,a3+b3*TP+c3*TP2+d3*TP3,"Orange")+
        plot2dcurve(TP,CPU,"Red",5);
    G.Title:="CPU";
    G.LabelX:="Time (s)";
    G.LabelY:="CPU (%)";
    G
);

TP:=[];
CPU:=[];
Start:=Now;
foreach x in 1..60 do
(
    Sleep(1000);
    CPU:=join(CPU,(PerformanceCounterValue("Processor","_Total","% Processor Time") ??? Uniform(0,100)));
    TP:=join(TP,Now.Subtract(Start).TotalSeconds);
    preview(PlotCPU(TP,CPU))
);
```

**Note**: If the script returns XML, and there is an XML Visualizer available in the code (i.e.a class that has implemented
`Waher.Content.Markdown.Model.XmlVisualizer`), the XML is first transformed before being visualized. For example, script may compute
a 2D Layout XML document, that is then used to generate a visual image, as shown in the following example:

    ```async:Some statistics
    <Layout2D xmlns="http://waher.se/Schema/Layout2D.xsd"
              background="ThemeBackground" pen="BlackPen"
              font="Text" textColor="Black">
	    <SolidPen id="BlackPen" color="Black" width="1px"/>
	    <SolidPen id="LightGrayPen" color="LightGray" width="1px"/>
	    <SolidPen id="GreenPen" color="Green" width="2mm"/>
	    <SolidPen id="RedPen" color="Red" width="2mm"/>
	    <SolidBackground id="ThemeBackground" color="{Theme.BackgroundColor}"/>
	    <SolidBackground id="GreenBackground" color="{Alpha('Green',128)}"/>
	    <SolidBackground id="RedBackground" color="{Alpha('Red',128)}"/>
	    <Font id="Text" name="Arial" size="36pt" color="White"/>
	    <Rectangle x="0%" y="0%" x2="100%" y2="100%" pen="BlackPen" fill="ThemeBackground"/>
	    <ForEach variable="k" expression="(10..90|10)+'%'">
		    <Line x="{k}" y="0%" x2="{k}" y2="100%" pen="LightGrayPen"/>
		    <Line x="0%" y="{k}" x2="100%" y2="{k}" pen="LightGrayPen"/>
	    </ForEach>
	    <CircleArc x="45%" y="50%" radius="30%" startDegrees="{Ok:=20;Total:=24;DegGreen:=360*Ok/Total;DegRed:=360-DegGreen;DegRed/2}" endDegrees="{360-DegRed/2}" clockwise="true" center="true" pen="GreenPen" fill="GreenBackground"/>
	    <CircleArc x="55%" y="50%" radius="30%" startDegrees="{360-DegRed/2}" endDegrees="{DegRed/2}" clockwise="true" center="true" pen="RedPen" fill="RedBackground"/>
	    <Label x="30%" y="50%" font="Text" halign="Center" valign="Center" text="{Ok}"/>
	    <Label x="70%" y="50%" font="Text" halign="Center" valign="Center" text="{Total-Ok}"/>
    </Layout2D>
    ```

Is shown as

```async:Some statistics
<Layout2D xmlns="http://waher.se/Schema/Layout2D.xsd"
          background="ThemeBackground" pen="BlackPen"
          font="Text" textColor="Black">
	<SolidPen id="BlackPen" color="Black" width="1px"/>
	<SolidPen id="LightGrayPen" color="LightGray" width="1px"/>
	<SolidPen id="GreenPen" color="Green" width="2mm"/>
	<SolidPen id="RedPen" color="Red" width="2mm"/>
	<SolidBackground id="ThemeBackground" color="{Theme.BackgroundColor}"/>
	<SolidBackground id="GreenBackground" color="{Alpha('Green',128)}"/>
	<SolidBackground id="RedBackground" color="{Alpha('Red',128)}"/>
	<Font id="Text" name="Arial" size="36pt" color="White"/>
	<Rectangle x="0%" y="0%" x2="100%" y2="100%" pen="BlackPen" fill="ThemeBackground"/>
	<ForEach variable="k" expression="(10..90|10)+'%'">
		<Line x="{k}" y="0%" x2="{k}" y2="100%" pen="LightGrayPen"/>
		<Line x="0%" y="{k}" x2="100%" y2="{k}" pen="LightGrayPen"/>
	</ForEach>
	<CircleArc x="45%" y="50%" radius="30%" startDegrees="{Ok:=20;Total:=24;DegGreen:=360*Ok/Total;DegRed:=360-DegGreen;DegRed/2}" endDegrees="{360-DegRed/2}" clockwise="true" center="true" pen="GreenPen" fill="GreenBackground"/>
	<CircleArc x="55%" y="50%" radius="30%" startDegrees="{360-DegRed/2}" endDegrees="{DegRed/2}" clockwise="true" center="true" pen="RedPen" fill="RedBackground"/>
	<Label x="30%" y="50%" font="Text" halign="Center" valign="Center" text="{Ok}"/>
	<Label x="70%" y="50%" font="Text" halign="Center" valign="Center" text="{Total-Ok}"/>
</Layout2D>
```

### Sessions and variables

When a user connects to the server, it will receive a session. This session will maintain all variables that is created in script.
These variabes will be available from any page the user views. Each user will have its own set of variables stored in its own session.
If the user does not access the server for 20 minutes by default, the session is lost, and any variables created will be lost.

### Query parameters

When loading a markdown page, any query parameters listed using the [PARAMETER metadata tag](#parameter) will be available as variables in script. 
If the query variable is not available, the parameter will be set to the empty string. By default, the variable type is a string, unless
it can be parsed as a double number or a boolean value.

### Global variables

When a user session is created, it will contain a variable named `Global` that points to a global variables collection. The global variables
collection and the session variables collection can be used by resources to keep application states. States will be available 
for all script on the server, if accessed through the `Global` variables collection.

Example:

	This page has been viewed {Global.NrTimesMarkdownLoaded:=exists(Global.NrTimesMarkdownLoaded) ? Global.NrTimesMarkdownLoaded+1 : 1} times since the server was last restarted.

This becomes:

This page has been viewed {Global.NrTimesMarkdownLoaded:=exists(Global.NrTimesMarkdownLoaded) ? Global.NrTimesMarkdownLoaded+1 : 1} times since the server was last restarted.

**Note**: If the count does not increment when the page is loaded or refreshed, it means you're receiving a cached result. You can control
page cache rules using [Metadata tags](#metadata).

### Page-local variables

When navigating on Markdown pages, a page-local collection of variables is available, by referencing the session variable named `Page`. 
Every time a new page is viewed by the same session, the Page-collection is cleared. The `Page` collection can be a good place to store
temporary information related to the current page.

Example:

	This page has been viewed {Page.NrTimesMarkdownLoaded:=exists(Page.NrTimesMarkdownLoaded) ? Page.NrTimesMarkdownLoaded+1 : 1} times since you last navigated to this page.

This becomes:

This page has been viewed {Page.NrTimesMarkdownLoaded:=exists(Page.NrTimesMarkdownLoaded) ? Page.NrTimesMarkdownLoaded+1 : 1} times since you last navigated to this page.

### Current request

The session state will contain a variable named `Request` that contains detailed information about the current request. The variable will
contain an object of type `Waher.Networking.HTTP.HttpRequest`.

### Current response

The session state will contain a variable named `Response` where the response is being built. The variable will contain an object of type 
`Waher.Networking.HTTP.HttpResponse`. It can be used to set custom header information, etc.

### Posted data

Data posted to a page can be accessed, in decoded form, by accessing the `Posted` variable, defined in the `Waher.Networking.HTTP` module.
This makes it possible to implement a form into a Markdown page, and process posted information from script embedded in the document itself,
or linked to it in the meta-data headers.

### Web Services

There are multiple ways to define web services using Markdown and/or script, as outlined in the following sub-sections.

#### Web Script

Web Script (Web Service, or *Waher Script*) is script that is stored in files with extension `.ws`. A client can POST data to such a
Web Script file. The data will be decoded and made available in the script through the `Posted` variable. The result of the script is
then encoded to the client, based on the `Accept` header in the HTTP Request. For JSON responses, the `Accept` header should have the
value `application/json`.

#### Markdown-based Services

You can create separate Markdown files with the [`BodyOnly`](#bodyOnly) meta-data tag set to `true`, to create web services that return
HTML directly. This is suitable if the response does not need to be parsed and conditionally processed, but only displayed to the user.
A client POSTs data to such a Markdown file. The data will be decoded and made available in the script through the `Posted` variable. 
The result of the script is then encoded to the client, based on the `Accept` header in the HTTP Request, which should be `text/html`.

### Generation of JavaScript

The Markdown engine can generate JavaScript for you automatically, facilitating the dynamic creation of items on web pages. This conversion
can be done either by specifying an `Accept: application/javascript` header when requesting for a Markdown file, or by referring to a
Markdown file with an additional `.js` extension after the traditional `.md` extension. Referring this way to a file `Test.md.js` for instance,
will generate a JavaScript rendering of the `Test.md` file. This method is useful if referring to JavaScript files from the header of an
HTML file, for instance, where you cannot control the browser's selection of HTTP Accept header field in the request.

When converting a Markdown file (for example `Test.md`) to JavaScript (for example `Test.md.js`), two functions are created and included in the
file:

```javascript
function CreateHTMLTest(Args);
function CreateInnerHTMLTest(ElementId, Args);
```

The final `Test` in the funcion names are taken from the name of the Markdown file being converted. This makes it possible to include multiple
JavaScript files generated from multiple Markdown files on the same page. The `Args` argument can be used to send information to the function,
which is later used by inline script when generating HTML.

The first function returns a string containing the HTML generated by the JavaScript. The second function calls the first function to generate HTML,
the looks in the DOM of the page to find an element with a given `id` attribute, and then sets the `innerHTML` property of that element to the
generated HTML.

Things to keep in mind when converting Markdown to JavaScript:

* Script placed between double braces <code>\{\{</code> and <code>\}\}</code> is preprocessed on the server and affect the structure of the Markdown,
which in turn affects the generated JavaScript. Such script do not have access to the `Args` argument. Instead they have access to any session
variables that may exist.

* Script placed between single braces `{` and `}` is not processed on the server at all. Instead, it is assumed to be JavaScript itself, and inserted
as-is into the JavaScript. This allows you to populate the dynamic HTML using values from your browser, without having to request the server to
do it. This also means, that the script syntax normally used for single-braces evaluation on the server, is not used in the JavaScript case. The
inline script has access to `Args`, but as it runs in the browser, does not have access to server-side session-state variables.

* If the Markdown only contains a header-less table (i.e. a table with zero header rows), the JavaScript rendered will only generate the table rows,
not the surrounding `table`, `thead` and `tbody` elements, to facilitate dynamic addition of rows to a table.

#### Generated JavaScript Example

Consider the following Markdown page, saved as `Test.md`:

```
JavaScript: TestTable.md.js
Title: JavaScript generation test
Description: This page displays a table dynamically generated by JavaScript, rendered from a Markdown template.
AllowScriptTag: 1

JavaScript generation Test
-----------------------------

The following table was generated by JavaScript:

<div id="TestTableLocation"></div>

<script>
CreateInnerHTMLTestTable("TestTableLocation", {"A": 5, "B": 7});
</script>
```

It refers to a Markdown template called `TestTable.md`, in a JavaScript header. To make sure the server converts this Markdown to JavaScript, the
extension `.js` is added to the filename, resulting in a reference to `TestTable.md.js` file. When the browser requests this file, the server
recognizes that the file does not exist. Instead, it recognizes the `.js` extension, understands that it refers to the `Accept: application/javascript`
header, and modifies the request to refer to `TestTable.md` with the corresponding `Accept` header set. The server then loads the Markdown,
converts it to JavaScript, and returns JavaScript that generates HTML from the following Markdown template:

```
| Table populated by JavaScript   ||
|:---------------|-----------------|
| A:             | {Args.A}        |
| B:             | {Args.B}        |
| A+B:           | {Args.A+Args.B} |
| A-B:           | {Args.A-Args.B} |
| A\*B:          | {Args.A*Args.B} |
| A/B:           | {Args.A/Args.B} |
```

This will generate a page similar to:

<h2 id="javascriptGenerationTest">JavaScript generation Test</h2>
<p>The following table was generated by JavaScript:</p>
<div id="TestTableLocation"><table>
<colgroup>
<col style="text-align:left">
<col style="text-align:left">
</colgroup>
<thead>
<tr>
<th style="text-align:left" colspan="2">Table populated by JavaScript</th>
</tr>
</thead>
<tbody>
<tr>
<td style="text-align:left">A:</td>
<td style="text-align:left">5</td>
</tr>
<tr>
<td style="text-align:left">B:</td>
<td style="text-align:left">7</td>
</tr>
<tr>
<td style="text-align:left">A+B:</td>
<td style="text-align:left">12</td>
</tr>
<tr>
<td style="text-align:left">A-B:</td>
<td style="text-align:left">-2</td>
</tr>
<tr>
<td style="text-align:left">A\*B:</td>
<td style="text-align:left">35</td>
</tr>
<tr>
<td style="text-align:left">A/B:</td>
<td style="text-align:left">0.7142857142857143</td>
</tr>
</tbody>
</table>
</div>


=========================================================================================================================================================


Metadata
-----------------------------

The first block in a markdown document has the option to be a metadata block. Such a block is not directly visible on the page, but is used to
provide metadata information to the parser, search engines and other entities loading the page. Metadata is provided in the following form:

	Key1: Value 1
	Key2: Value 2
	...

Apart from providing metadata information about the page, you can access the metadata information from your page by using the `[%Key]` operator. That operator
will be replaced by the value of the korresponding `key`. Example:

	The title of this document is "[%Title]". It describes [%Description]
	It was written [%Date] by [%Author].

This is then transformed to:

The title of this document is "[%Title]". It describes [%Description]
It was written [%Date] by [%Author].

Note: If a metadata record does not exist for a given key, but a variable exists with the given name, the corresponding variable value will be
inserted instead.

The following subsections list the different metadata keys that have special meaning to the **{{AppName}}** Markdown parser. You're not limited to 
these metadata keys, and can freely add your own.

### AllowScriptTag

If the `<SCRIPT>` tag should be allowed or not. Value is a boolean value. Strings representing `true`, include `1`, `true`, `yes` and `on`.
Strings representing `false`, include `0`, `false`, `no` and `off`.

Default value, if not provided, is `false`.

### Alternate

Link to alternate page.

### AudioAutoplay

If audio should be played automatically or not. Value is a boolean value. Strings representing `true`, include `1`, `true`, `yes` and `on`.
Strings representing `false`, include `0`, `false`, `no` and `off`.

Default value, if not provided, is `true`.

### AudioControls

If audio should be played automatically or not. Value is a boolean value. Strings representing `true`, include `1`, `true`, `yes` and `on`.
Strings representing `false`, include `0`, `false`, `no` and `off`.

Default value, if not provided, is `false`.

### Author

Write the name of the author or authors using this tag.

### BodyOnly

If only the contents of the body should be returned or not. This is useful if you create dynamic pages using the XmlHttpRequest object (AJAX),
as it removes the DOCTYPE and header (and body tag) from the response, and only returns the corresponding HTML content. Value is a boolean 
value. Strings representing `true`, include `1`, `true`, `yes` and `on`. Strings representing `false`, include `0`, `false`, `no` and `off`.

Default value, if not provided, is `false`.

### Copyright

Allows you to provide a link to a copyright statement.

### CSS

Links to Cascading Style Sheets that should be used for visual formatting of the generated HTML page.

### Date

Provide a date for when the document was created. This date is presented in the metadata header of the document. The web server uses the last write
date of the file to tell clients when the file was last updated.

### Description

Provides a description for the page. This description is shown to search engines and other clients, and should contain a short description of the page
motivating people to view your page.

### Details

Points to the place in a master document, where the details section is to be inserted. The `[%Details]` operator differs from the other meta reference
tags, in that it can stand alone in a separate block.

### Help

Link to help page.

### Icon

Link to an icon for the page.

### Image

Link to an image for the page.

### Init

Links to server-side script files that should be executed before processing the page. The script is only executed once, regardless of how
many times the markdown page is processed. It can be used to initialize the backend appropriately. To execute the script again, a newer
version must be available. The file time stamps are used to determine if a file is newer than a previous version or not. For script that 
is to be executed every time the page is processed, see the [Script](#script) tag.

### JavaScript

Links to JavaScript files that should be included in the generated HTML page.

### Keywords

Here you can provide a set of keywords describing the contents of the document.

### Login

Link to a login page. This page will be shown if the user variable does not contain a valid user.

### Master

Points to a master content file that embeds the current file in a `[%Details]` section (if written in Markdown).

### Next

Link to next document, in a paginated set of documents.

### Parameter

Name of a query parameter recognized by the page. Any query parameter values for parameters listed in the document will be available in script.
If the query parameters are missing, the corresponding parameters will be set to empty strings. By default, all parameters are strings, unless
they can be parsed as a double number or boolean value.

### Previous or Prev

Link to previous document, in a paginated set of documents.

### Privilege

Requered user privileges to display page. Meta-data tag can be used multiple times, one for each privilege required.
The `IUser.HasPrivilege` method (defined in `Waher.Security`) will be called to check that the valid user has the corresponding privilege, 
before the page is displayed.

### Refresh

Tells the browser to refresh the page after a given number of seconds.

### Script

Links to server-side script files that should be included before processing the page. Script linked to here will be executed every time
the markdown document is processed. For script that is to be executed only once, see the [Init](#init) tag.

### Subtitle

Provides a means to create a subtitle for the document. If provided, will be shown, together with the title, in the browser header or tab.

### Title

Use this key to provide a title for the document. The title of the page will be shown in the browser header or tab.

### UserVariable

Name of the variable that will hold a reference to the user object for the currently logged in user. If privileges are defined using the 
[Privilege](#privilege) meta-data tag, this user object must implement the `IUser` interface (defined in `Waher.Security`).
If multiple UserVariable meta-data tags are defined, the first one that is found will be used.

### VideoAutoplay

If video should be played automatically or not. Value is a boolean value. Strings representing `true`, include `1`, `true`, `yes` and `on`.
Strings representing `false`, include `0`, `false`, `no` and `off`.

Default value, if not provided, is `false`.

### VideoControls

If video should be played automatically or not. Value is a boolean value. Strings representing `true`, include `1`, `true`, `yes` and `on`.
Strings representing `false`, include `0`, `false`, `no` and `off`.

Default value, if not provided, is `true`.

### Viewport

Defines Viewport meta-data for the page, for better presentation on mobile-phones and devices. See
[Using the viewport meta tag to control layout on mobile browsers](https://developer.mozilla.org/en-US/docs/Web/HTML/Viewport_meta_tag) 
for more information.

### Web

Link to a web page.


HTTP Header Fields
----------------------------

The following meta-data tags are transparently mapped to HTTP response headers when the markdown document is converted to HTML over a web interface.

### Access-Control-Allow-Origin

Allows you to define a [Cross-origin resource sharing (CORS)](http://www.w3.org/TR/cors/) header.

### Cache-Control

The value of this tag will be used when returning the document over an HTTP interface. The value will be literally used as a `Cache-control`
HTTP header value of the generated HTML contents. Together with the [Vary](#vary) meta-tag they provide a means to control how the generated
page will be cached.

**Note**: If the markdown page is dynamic, and no `Cache-Control` metadata header is present, the following HTTP header will be added
automatically:

	Cache-Control: max-age=0, no-cache, no-store

If the markdown page is static, and no `Cache-Control` tag is present, the following will be used:

	Cache-Control: no-transform,public,max-age=60,s-maxage=60,stale-while-revalidate=604800

### Content-Security-Policy

Allows web clients to know what the expected behaviour of the page is, by setting [Content Security Policies](http://www.w3.org/TR/CSP/).

### Public-Key-Pins

Tells web clients to [pin a specific public key](https://tools.ietf.org/html/rfc7469) with the site, to decrease the risk of MITM attacks.

### Strict-Transport-Security

[HTTP Strict Transport Security (HSTS)](https://tools.ietf.org/html/rfc6797) forces clients to connect to the site using a secure connection.

### Sunset

[HTTP Sunset Header](https://tools.ietf.org/html/draft-wilde-sunset-header) allows you to flag content for removal at a future point in time.
It allows clients to prepare.

### Vary

The value of this tag will be used when returning the document over an HTTP interface. The value will be literally used as a `Vary`
HTTP header value of the generated HTML contents. Together with the [Cache-Control](#cacheControl) meta-tag they provide a means to 
control how the generated page will be cached.
