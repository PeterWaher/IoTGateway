<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:report="http://waher.se/Schema/Report.xsd"
				exclude-result-prefixes="msxsl">
	
    <xsl:output method="html" indent="no"/>

    <xsl:template match="/report:Report">
		<html>
			<head/>
			<body style="font-family: Segoe UI, Helvetica, sans-serif">
				<table cellspacing="0" cellpadding="2" border="0">
					<tr>
						<th>Timestamp</th>
						<th style="width:10px"/>
						<th>Content</th>
					</tr>
					<xsl:for-each select="*">
						<tr>
							<xsl:choose>
								<xsl:when test="name()='DataReceived'">
									<xsl:attribute name="style">
										<xsl:text>color:white;background-color:navy</xsl:text>
									</xsl:attribute>
								</xsl:when>
								<xsl:when test="name()='DataTransmitted'">
									<xsl:attribute name="style">
										<xsl:text>color:black;background-color:white</xsl:text>
									</xsl:attribute>
								</xsl:when>
								<xsl:when test="name()='TextReceived'">
									<xsl:attribute name="style">
										<xsl:text>color:white;background-color:navy</xsl:text>
									</xsl:attribute>
								</xsl:when>
								<xsl:when test="name()='TextTransmitted'">
									<xsl:attribute name="style">
										<xsl:text>color:black;background-color:white</xsl:text>
									</xsl:attribute>
								</xsl:when>
								<xsl:when test="name()='Information'">
									<xsl:attribute name="style">
										<xsl:text>color:yellow;background-color:darkgreen</xsl:text>
									</xsl:attribute>
								</xsl:when>
								<xsl:when test="name()='Warning'">
									<xsl:attribute name="style">
										<xsl:text>color:black;background-color:yellow</xsl:text>
									</xsl:attribute>
								</xsl:when>
								<xsl:when test="name()='Error'">
									<xsl:attribute name="style">
										<xsl:text>color:yellow;background-color:red</xsl:text>
									</xsl:attribute>
								</xsl:when>
								<xsl:when test="name()='Exception'">
									<xsl:attribute name="style">
										<xsl:text>color:yellow;background-color:darkred</xsl:text>
									</xsl:attribute>
								</xsl:when>
							</xsl:choose>
							<td>
								<xsl:value-of select="substring(@timestamp,12,8)"/>
							</td>
							<td/>
							<td>
								<xsl:value-of select="."/>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</body>
		</html>
    </xsl:template>
</xsl:stylesheet>
