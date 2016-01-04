<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:chat="http://waher.se/Chat.xsd"
				exclude-result-prefixes="msxsl">
	
    <xsl:output method="html" indent="no"/>

    <xsl:template match="/chat:Chat">
		<html>
			<head/>
			<body style="font-family: Segoe UI, Helvetica, sans-serif">
				<table cellspacing="0" cellpadding="2" border="0">
					<tr>
						<th>Received</th>
						<th style="width:10px"/>
						<th>Chat history</th>
						<th style="width:10px"/>
						<th>Sent</th>
					</tr>
					<xsl:for-each select="*">
						<tr>
							<xsl:choose>
								<xsl:when test="name()='Received'">
									<xsl:attribute name="style">
										<xsl:text>color:black;background-color:#F0F8FF</xsl:text>
									</xsl:attribute>
								</xsl:when>
								<xsl:when test="name()='Transmitted'">
									<xsl:attribute name="style">
										<xsl:text>color:black;background-color:#F0FFF0</xsl:text>
									</xsl:attribute>
								</xsl:when>
							</xsl:choose>
							<td>
								<xsl:if test="name()='Received'">
									<xsl:value-of select="substring(@timestamp,12,8)"/>
								</xsl:if>
							</td>
							<td/>
							<td>
								<xsl:value-of select="."/>
							</td>
							<td/>
							<td>
								<xsl:if test="name()='Transmitted'">
									<xsl:value-of select="substring(@timestamp,12,8)"/>
								</xsl:if>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</body>
		</html>
    </xsl:template>
</xsl:stylesheet>
