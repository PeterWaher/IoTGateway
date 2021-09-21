<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:script="http://waher.se/Schema/Script.xsd"
				exclude-result-prefixes="msxsl">

	<xsl:output method="html" indent="no"/>

	<xsl:template match="/script:Script">
		<html>
			<head>
				<style type="text/css">
					<xsl:text>
						<![CDATA[
						body
						{
							padding: 0;
							margin: 0;
							font-family: "Segoe UI", "Segoe UI Web Regular", "Segoe UI Symbol", "Helvetica Neue", "BBAlpha Sans", "S60 Sans", Arial, sans-serif;
							font-size: 100%;
							width:100%;
							height:100%;
							position:relative;
							margin:0
						}
					
						div.expression
						{
                            font-family: Courier New, Monospace, sans-serif;
                            color:black;
                            margin:0.5em;
						}
					
						div.error
						{
                            font-family: Courier New, Monospace, sans-serif;
                            color:red;
                            font-weight: bold;
                            margin:0.5em;
                        }
					
                        div.result
                        {
                            font-family: Courier New, Monospace, sans-serif;
                            color:red;
                            margin:0.5em;
						}
					
						div.print
						{
                            font-family: Courier New, Monospace, sans-serif;
                            color:blue;
                            margin:0.5em;
						}
						]]>
					</xsl:text>
				</style>
			</head>
			<body style="font-family: Segoe UI, Helvetica, sans-serif">
				<xsl:for-each select="*">
					<xsl:choose>
						<xsl:when test="name()='Expression'">
							<div class="expression">
								<xsl:value-of select="."/>
							</div>
						</xsl:when>
						<xsl:when test="name()='Error'">
							<div class="error">
								<xsl:value-of select="."/>
							</div>
						</xsl:when>
						<xsl:when test="name()='Result'">
							<div class="result">
								<xsl:value-of select="."/>
							</div>
						</xsl:when>
						<xsl:when test="name()='Print'">
							<div class="print">
								<xsl:value-of select="."/>
							</div>
						</xsl:when>
						<xsl:when test="name()='Image'">
							<figure>
								<img>
									<xsl:attribute name="width">
										<xsl:value-of select="@width"/>
									</xsl:attribute>
									<xsl:attribute name="height">
										<xsl:value-of select="@height"/>
									</xsl:attribute>
									<xsl:attribute name="src">
										<xsl:text>data:image/png;base64,</xsl:text>
										<xsl:value-of select="."/>
									</xsl:attribute>
								</img>
							</figure>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>
