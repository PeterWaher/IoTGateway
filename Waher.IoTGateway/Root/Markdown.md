Title: Markdown
Description: Markdown syntax reference, as understood by the IoT Gateway.
Date: 2016-02-11
Author: Peter Waher
Copyright: Copyright.md
Master: Master.md

Markdown syntax reference
=============================

*Markdown* is a very simple, yet efficient format for editing text-based content. The **IoT Gateway** converts Markdown to HTML automatically when
web browsers download it, making it a powerful tool for publishing online content. The syntax is inspired by the 
[original markdown syntax](http://daringfireball.net/projects/markdown/syntax) as defined by John Gruber at Daring Fireball, but contains numerous other 
additions and modifications. Some of these are introduced in the **IoT Gateway**, others are inspired by selected features used in
[MultiMarkdown](https://rawgit.com/fletcher/human-markdown-reference/master/index.html) and
[Markdown Extra](https://michelf.ca/projects/php-markdown/extra/). Below, is a summary of the markdown syntax, as understood by the **IoT Gateway**.

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

### Automatic links

Markdown help you include links to online resources (URLs) or mail addresses automatically by surrounding them with `<` and `>` characters, such as 
`<http://example.com/>` or `<address@example.com>`. These would turn into clickable links in the HTML representation, as follows: <http://example.com/> 
and <address@example.com>.

**Note**: It's important to include the *URI Scheme* (for example `http://`) in links or the @ sign in mail addresses, for the parser to understand it's 
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

**Note**: Care has to be taken so that the end result is HTML compliant. While HTML can be inserted anywhere, it's only useful if the markdown is 
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

| Sequence | Result |<span style='width:30px'></span>| Sequence | Result |<span style='width:30px'></span>| Sequence | Result |<span style='width:30px'></span>| Sequence | Result |<span style='width:30px'></span>| Sequence | Result |
|:--------:|:------:|--------------------------------|:--------:|:------:|--------------------------------|:--------:|:------:|--------------------------------|:--------:|:------:|--------------------------------|:--------:|:------:|
| `\*`     | \*     |                                | `\{`     | \{     |                                | `\)`     | \)     |                                | `\-`     | \-     |                                | `\^`     | \^     |
| `\_`     | \_     |                                | `\}`     | \}     |                                | `\<`     | \<     |                                | `\.`     | \.     |                                | `\%`     | \%     |
| `\~`     | \~     |                                | `\[`     | \[     |                                | `\>`     | \>     |                                | `\!`     | \!     |                                | `\=`     | \=     |
| `\\`     | \\     |                                | `\]`     | \]     |                                | `\#`     | \#     |                                | `\\`     | \\     |                                | `\:`     | \:     |
| ` \` `   | \`     |                                | `\(`     | \(     |                                | `\+`     | \+     |                                | `\"`     | \"     |                                | <code>\\&#124;</code>     | &#124;     |

**Note**: Some characters only have special meaning in certain situations, such as the parenthesis, brackets, etc. The occurrence of such a character
in any other situation does not require escaping.

### Typographical enhancements

There are numerous typographical enhancements added to the markdown parser. This makes it easier to generate beautiful text. Some of these additions are
are inspired by the the [Smarty Pants](http://daringfireball.net/projects/smartypants/) addition to the original markdown, but numerous other character 
sequences have been added to the **IoT Gateway** version of markdown, as shown in the following table:

| Sequence | Becomes |<span style='width:30px'></span>| Sequence | Becomes |<span style='width:30px'></span>| Sequence | Becomes |<span style='width:30px'></span>| Sequence | Becomes |<span style='width:30px'></span>| Sequence | Becomes |
|:--------:|:-------:|--------------------------------|:--------:|:-------:|--------------------------------|:--------:|:-------:|--------------------------------|:--------:|:-------:|--------------------------------|:--------:|:-------:|
| `...`    | ...     |                                | `(R)`    | (R)     |                                | `>>>`    | >>>     |                                | `]]`     | ]]      |                                | `^o`     | ^o      |
| `"text"` | "text"  |                                | `(p)`    | (p)     |                                | `<--`    | <--     |                                | `+-`     | +-      |                                | `^0`     | ^0      |
| `'text'` | 'text'  |                                | `(P)`    | (P)     |                                | `-->`    | -->     |                                | `-+`     | -+      |                                | `^1`     | ^1      |
| `--`     | --      |                                | `(s)`    | (s)     |                                | `<-->`   | <-->    |                                | `<>`     | <>      |                                | `^2`     | ^2      |
| `---`    | ---     |                                | `(S)`    | (S)     |                                | `<==`    | <==     |                                | `<=`     | <=      |                                | `^3`     | ^3      |
| `(c)`    | (c)     |                                | `<<`     | <<      |                                | `==>`    | ==>     |                                | `>=`     | >=      |                                | `^TM`    | ^TM     |
| `(C)`    | (C)     |                                | `>>`     | >>      |                                | `<==>`   | <==>    |                                | `==`     | ==      |                                | `%0`     | %0      |
| `(r)`    | (r)     |                                | `<<<`    | <<<     |                                | `[[`     | [[      |                                | `^a`     | ^a      |                                | `%00`    | %00     |

### Emojis

Emojis are supported, and included into the document using the shortname syntax `:shortname:`.
For a list of supported emojis, click [here](Emojis.md).

### Smileys

Smileys are supported in markdown text, and converted to the corresponding emojis. For a list of supported smileys, click [here](Smileys.md).


=========================================================================================================================================================


Block constructs
-----------------------------

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

### Block-level HTML

You can add HTML blocks to your markdown text. As with all block constructs, HTML blocks must be separated from other text by empty rows (or rows only
including whitespace). The difference between block-level HTML and inline HTML is that block-level HTML reside outside of paragraphs and other similar
constructs (i.e. div-type, or block-type HTML constructs are possible), while inline HTML is limited to span-type or inline-type HTML constructs.
Block-level HTML can be combined with markdown.

Example:

	<div style="border:1px solid black;background-color:#e0e0e0;color:navy;padding:30px;width:500px;text-align:center">
	This text is _formatted_ using **Markdown**.
	</div>

This is shown as:

<div style="border:1px solid black;background-color:#e0e0e0;color:navy;padding:30px;width:500px;text-align:center">
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

	![Banner](/Images/Banner1%20-%202000x600.png 2000 600)
		(/Images/Banner1%20-%201900x500.png 1900 500)
		(/Images/Banner1%20-%201400x425.png 1400 425)

Now, the browser will select the most appropriate image, based on available space, if the browser supports responsive images based on the 
`<picture>` element. This is how it will look in your browser:

![Banner](/Images/Banner1%20-%202000x600.png 2000 600)
	(/Images/Banner1%20-%201900x500.png 1900 500)
	(/Images/Banner1%20-%201400x425.png 1400 425)

In the same way, you can also define multi-format images using reference notation. You simply list the media items and their different resolutions, if availble
one after the other, optionally on separate rows.

	![Cactus Rose][]
	
	[Cactus Rose]: /Images/Cactus%20Rose%201600x1600.png 1600 1600
		/Images/Cactus%20Rose%20800x800.png 800 800

In your browser, this is displayed as:

![Cactus Rose][]
	
[Cactus Rose]: /Images/Cactus%20Rose%201600x1600.png 1600 1600
	/Images/Cactus%20Rose%20800x800.png 800 800

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

TO include YouTube clips into your document is easy. A YouTube multimedia content plugin recognizes the YouTube video URL and inserts it accordingly
into the generated page inside an `<iframe>` element. Example:

	![Complex perturbation](https://www.youtube.com/watch?v=whBPLc8m4SU 800 600)

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
about script, see the [Script reference](Script.md).

### Inline script

[Script](Script.md) can be embedded inline in a block, between curly braces `{` and `}`. The result is then presented in the final output.
Example:

	*a* is {a:=5} and *b* is {b:=6}. *a*\**b* is therefore {a*b}.

This becomes:

*a* is {a:=5} and *b* is {b:=6}. *a*\**b* is therefore {a*b}.

**Note**: Inline script must all reside in a block. While new-line can be used in such inline script, empty rows separating blocks cannot.

### Sessions and variables

When a user connects to the server, it will receive a session. This session will maintain all variables that is created in script.
These variabes will be available from any page the user views. Each user will have its own set of variables stored in its own session.
If the user does not access the server for 20 minutes by default, the session is lost, and any variables created will be lost.

### Global variables

When a user session is created, it will contain a variable named `Global` that points to a global variables collection. The global variables
collection and the session variables collection can be used by resources to keep application states. States will be available 
for all script on the server, if accessed through the `Global` variables collection.

Example:

	This page has been generated {Global.NrTimesMarkdownLoaded:=try Global.NrTimesMarkdownLoaded+1 catch 1} times since the start of the server.

This becomes:

This page has been generated {Global.NrTimesMarkdownLoaded:=try Global.NrTimesMarkdownLoaded+1 catch 1} times since the start of the server.

**Note**: If the count does not increment when the page is loaded or refreshed, it means you're receiving a cached result. You can control
page cache rules using [Metadata tags](#metadata).

### Current request

The session state will contain a variable named `Request` that contains detailed information about the current request. The variable will
contain an object of type `Waher.Networking.HTTP.HttpRequest`.


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

The following subsections list the different metadata keys that have special meaning to the **IoT Gateway** Markdown parser. You're not limited to these
metadata keys, and can freely add your own.

### Alternate

Link to alternate page.

### Author

Write the name of the author or authors using this tag.

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

### Keywords

Here you can provide a set of keywords describing the contents of the document.

### Master

Points to a master content file that embeds the current file in a `[%Details]` section (if written in Markdown).

### Next

Link to next document, in a paginated set of documents.

### Previous or Prev

Link to previous document, in a paginated set of documents.

### Subtitle

Provides a means to create a subtitle for the document. If provided, will be shown, together with the title, in the browser header or tab.

### Title

Use this key to provide a title for the document. The title of the page will be shown in the browser header or tab.

### Web

Link to a web page.