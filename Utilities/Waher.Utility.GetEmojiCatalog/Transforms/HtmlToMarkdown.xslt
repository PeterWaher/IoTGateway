<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
				exclude-result-prefixes="msxsl">
	
    <xsl:output method="text" indent="no"/>

    <xsl:template match="/">
		<xsl:text disable-output-escaping="yes">Title: Emojis
Description: List of emojis supported by IoT Gateway Markdown.
Date: 2016-02-05
Author: Peter Waher
Copyright: Waher Data AB

Emojis
=============

Emojis are supported in markdown text. Emojis are provided by [EmojiOne](http://emojione.com/). They reside in the `/Graphics/Emoji1` folder. There are
four versions of the emojis, one SVG version, which is resolution independent, and three PNG versions, one with resolutions 512x512, one with 128x128 
and one with 64x64 pixels. You can include the corresponding emoji using normal image inclusion in markdown. This allows you to control the size of the emoji.

You can also include an emoji, by using a *short name*. Not all of the emojis have a recognized short name. 
The following table[^List deduced from: <http://unicodey.com/emoji-data/table.htm>] contains the short names recognized by the gateway.
If you use the short name to insert a short name, the SVG version of the emoji will be used, with a size set to 24x24 pixels.

| Characters     | Emoji                          |
|:--------------:|:------------------------------:|</xsl:text>
		<xsl:for-each select="/html/body/table/tbody/tr">
			<xsl:variable name="ShortName" select="td[position()=7]/."/>
			<xsl:text>
|`</xsl:text>
			<xsl:value-of select="$ShortName"/>
			<xsl:text>`|</xsl:text>
			<xsl:value-of select="$ShortName"/>
			<xsl:text>|</xsl:text>
		</xsl:for-each>
    </xsl:template>
</xsl:stylesheet>