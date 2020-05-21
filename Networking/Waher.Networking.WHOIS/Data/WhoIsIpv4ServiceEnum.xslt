<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl"
                xmlns:iana="http://www.iana.org/assignments">

  <xsl:output method="text" indent="yes"/>

  <xsl:key name="whois" match="/iana:registry/iana:record" use="iana:whois" />

  <xsl:template match="/iana:registry">
    <xsl:text>using System;

namespace Waher.Networking.WHOIS
{
	/// &lt;summary&gt;
	/// Enumeration of WHOIS Services available for IPv4 addresses.
	/// &lt;/summary&gt;
	public enum WhoIsIpv4ServiceEnum
	{
</xsl:text>
    <xsl:for-each select="iana:record[generate-id()=generate-id(key('whois',iana:whois))]">
      <xsl:text>		/// &lt;summary&gt;
		/// </xsl:text>
      <xsl:value-of select="iana:designation"/>
      <xsl:text>
		/// RDAP: </xsl:text>
      <xsl:value-of select="iana:rdap/iana:server"/>
      <xsl:text>
		/// &lt;/summary&gt;
</xsl:text>
      <xsl:text>		</xsl:text>
      <xsl:value-of select="translate(iana:whois,'.','_')"/>
      <xsl:text> = </xsl:text>
      <xsl:value-of select="position() - 1"/>
      <xsl:text>,

</xsl:text>
    </xsl:for-each>
    <xsl:text>		/// &lt;summary&gt;
		/// WHOIS Service undefined.
		/// &lt;/summary&gt;
		undefined
	}
}
</xsl:text>
  </xsl:template>
</xsl:stylesheet>
