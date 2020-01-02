Title: IoT Gateway
Description: Main page of the IoT Gateway
Date: 2016-02-02
Author: Peter Waher
Copyright: /Copyright.md
Master: /Master.md

==================================       ======================================

Welcome to the IoT Gateway
=============================

The web server hosts any type of content under the `Root` folder. These are accessible directly from the root `/` in web applications. 
The `Graphics` folder contains graphics for different libraries that are included, such as Emojis from [Emoji One](http://emojione.com/), etc. 
The `Highlight` folder contains code from the [Syntax Highlighter](http://alexgorbatchev.com/SyntaxHighlighter/) project by Alex Gorbatchev.

You can publish content written directly in [Markdown](Markdown.md) under the `Root` folder, without preprocessing them or compiling them. 
Files with extensions `.md` or `.markdown` will be automatically converted to HTML if viewed by a browser. To retrieve the markdown file as-is, 
make sure the `HTTP GET` method includes `Accept: text/markdown` in its header.

You can make your markdown pages dynamic by the use of [Script](Script.md). Script allows you to perform calculations, draw graphs and access 
logic in your back-end applications, such as accessing databases, etc. The script engine is extensible, and is very easy to extend by modules
loaded by the web server. Check out the [Script reference](Script.md) page for more details. You can also use the [Calculator](Calculator.md)
to play around with script.

You can also host dynamic CSS (`CSSX`) files, containing script embedded between sun characters `¤`. Such script will execute on the server, 
and the results inserted in the CSS before being returned seamlessly to the browser. This allows you to adapt the layout of the page based on 
user context, and allows for use the consistent use of themes across applications.

When the **IoT Gateway** runs, it loads all dynamic link modules (with file extension `.dll`) automatically. All extensions and executable
modules defined in these modules will be found and started automatically.

To use this server software, you need to accept the [License agreement](Copyright.md). Check out the [License page](Copyright.md) for details.

The **IoT Gateway** is (c) [Waher Data AB](http://waher.se/) 2016-2020. All rights reserved.
 
[![](/Images/logo-WaherDataAB-400x77.png)](http://waher.se/)