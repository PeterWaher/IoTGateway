<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:sr="http://waher.se/Schema/SearchResult.xsd"
				exclude-result-prefixes="msxsl">

  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/sr:SearchResult">
    <html>
      <head/>
      <body style="font-family: Segoe UI, Helvetica, sans-serif">
        <table cellspacing="0" cellpadding="2" border="0">
          <tr>
            <xsl:for-each select="sr:Headers/sr:Header">
              <xsl:if test="position()>1">
                <th style="width:10px"/>
              </xsl:if>
              <th>
                <xsl:value-of select="@label"/>
              </th>
            </xsl:for-each>
          </tr>
          <xsl:for-each select="sr:Records/sr:Record">
            <tr>
              <xsl:variable name="Record" select="."/>
              <xsl:for-each select="/sr:SearchResult/sr:Headers/sr:Header">
                <xsl:if test="position()>1">
                  <th style="width:10px"/>
                </xsl:if>
                <xsl:variable name="Var" select="@var"/>
                <td>
                  <xsl:for-each select="$Record/sr:Field[@var=$Var]">
                    <xsl:value-of select="@value"/>
                  </xsl:for-each>
                </td>
              </xsl:for-each>
            </tr>
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
