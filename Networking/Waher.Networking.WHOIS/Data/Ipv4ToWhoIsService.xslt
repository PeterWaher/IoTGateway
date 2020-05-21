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
	public partial class WhoIsClient
	{
		private static readonly WhoIsIpv4ServiceEnum[] ipv4ToWhoIsService = new WhoIsIpv4ServiceEnum[]
		{
</xsl:text>
    <xsl:for-each select="iana:record">
      <xsl:choose>
        <xsl:when test="iana:whois">
          <xsl:text>			WhoIsIpv4ServiceEnum.</xsl:text>
          <xsl:value-of select="translate(iana:whois,'.','_')"/>
          <xsl:text>,
</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>			WhoIsIpv4ServiceEnum.undefined,
</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
    <xsl:text>			WhoIsIpv4ServiceEnum.undefined
		};
	}
}
</xsl:text>
  </xsl:template>
</xsl:stylesheet>
