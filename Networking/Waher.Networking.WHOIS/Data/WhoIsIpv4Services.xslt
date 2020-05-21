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
		private static readonly string[] ipv4WhoIsServices = new string[]
		{
</xsl:text>
    <xsl:for-each select="iana:record[generate-id()=generate-id(key('whois',iana:whois))]">
      <xsl:text>			"</xsl:text>
      <xsl:value-of select="iana:whois"/>
      <xsl:text>",
</xsl:text>
    </xsl:for-each>
    <xsl:text>			string.Empty
		};
	}
}
</xsl:text>
  </xsl:template>
</xsl:stylesheet>
