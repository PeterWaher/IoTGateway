<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:sc="urn:ieee:iot:leg:sc:1.0"
                exclude-result-prefixes="msxsl">

  <xsl:output method="text" indent="no"/>

  <xsl:template match="sc:contract">
    <xsl:for-each select="sc:humanReadableText">
      <xsl:call-template name="BlockElements">
        <xsl:with-param name="HeaderLevel">
          <xsl:text>##</xsl:text>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="BlockElements">
    <xsl:param name="HeaderLevel"/>
    <xsl:for-each select="./*">
      <xsl:variable name="Name" select="local-name()"/>
      <xsl:choose>
        <xsl:when test="$Name='paragraph'">
          <xsl:call-template name="InlineElements"/>
        </xsl:when>
        <xsl:when test="$Name='section'">
          <xsl:for-each select="sc:header">
            <xsl:value-of select="$HeaderLevel"/>
            <xsl:text> </xsl:text>
            <xsl:call-template name="InlineElements"/>
            <xsl:text>

</xsl:text>
          </xsl:for-each>
          <xsl:for-each select="sc:body">
            <xsl:call-template name="BlockElements">
              <xsl:with-param name="HeaderLevel">
                <xsl:value-of select="$HeaderLevel"/>
                <xsl:text>#</xsl:text>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:for-each>
        </xsl:when>
        <xsl:when test="$Name='bulletItems'">
          <xsl:for-each select="./sc:item">
            <xsl:text>*	</xsl:text>
            <xsl:call-template name="InlineElements"/>
            <xsl:text>
</xsl:text>
          </xsl:for-each>
        </xsl:when>
        <xsl:when test="$Name='numberedItems'">
          <xsl:for-each select="./sc:item">
            <xsl:text>#.	</xsl:text>
            <xsl:call-template name="InlineElements"/>
            <xsl:text>
</xsl:text>
          </xsl:for-each>
        </xsl:when>
      </xsl:choose>
      <xsl:text>

</xsl:text>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="InlineElements">
    <xsl:for-each select="./*">
      <xsl:variable name="Name" select="local-name()"/>
      <xsl:choose>
        <xsl:when test="$Name='text'">
          <xsl:call-template name="EscapeMarkdown">
            <xsl:with-param name="Text">
              <xsl:value-of select="."/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="$Name='parameter'">
          <xsl:variable name="ParameterName" select="@name"/>
          <xsl:for-each select="/sc:contract/sc:parameters/*[@name=$ParameterName]">
            <xsl:value-of select="@value"/>
          </xsl:for-each>
        </xsl:when>
        <xsl:when test="$Name='bold'">
          <xsl:text>**</xsl:text>
          <xsl:call-template name="InlineElements"/>
          <xsl:text>**</xsl:text>
        </xsl:when>
        <xsl:when test="$Name='italic'">
          <xsl:text>*</xsl:text>
          <xsl:call-template name="InlineElements"/>
          <xsl:text>*</xsl:text>
        </xsl:when>
        <xsl:when test="$Name='underline'">
          <xsl:text>_</xsl:text>
          <xsl:call-template name="InlineElements"/>
          <xsl:text>_</xsl:text>
        </xsl:when>
        <xsl:when test="$Name='strikeThrough'">
          <xsl:text>~</xsl:text>
          <xsl:call-template name="InlineElements"/>
          <xsl:text>~</xsl:text>
        </xsl:when>
        <xsl:when test="$Name='super'">
          <xsl:text>^[</xsl:text>
          <xsl:call-template name="InlineElements"/>
          <xsl:text>]</xsl:text>
        </xsl:when>
        <xsl:when test="$Name='sub'">
          <xsl:text>[</xsl:text>
          <xsl:call-template name="InlineElements"/>
          <xsl:text>]</xsl:text>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="EscapeMarkdown">
    <xsl:param name="Text"/>
    <xsl:if test="$Text != ''">
      <xsl:variable name="C" select="substring($Text,1,1)"/>
      <xsl:if test="$C='*' or $C='_' or $C='~' or $C='\' or $C='`' or $C='{' or $C='}' or $C='[' or $C=']' or $C='(' or $C=')' or $C='&lt;' or $C='&gt;' or $C='&amp;' or $C='#' or $C='+' or $C='-' or $C='.' or $C='!' or $C='^' or $C='%' or $C='=' or $C=':' or $C='|'">
        <xsltext>\</xsltext>
      </xsl:if>
      <xsl:value-of select="$C"/>
      <xsl:call-template name="EscapeMarkdown">
        <xsl:with-param name="Text" select="substring($Text,2,(string-length($Text)-1) div 2 + 1)"/>
      </xsl:call-template>
      <xsl:call-template name="EscapeMarkdown">
        <xsl:with-param name="Text" select="substring($Text,(string-length($Text)-1) div 2 + 3)"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>