<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:report="http://waher.se/Schema/Report.xsd"
				exclude-result-prefixes="msxsl">
	
    <xsl:output method="html" indent="no"/>

    <xsl:template match="/report:Report">
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
					
						table
						{
                            border:1px solid black;
						}
					
						th
						{
                            background-color:#e0e0e0;
							padding:0.1em 0.5em;
                        }
					
						td
						{
							padding:0.1em 0.5em;

                        }

						tr:nth-child(odd)
						{
                            background-color:#ffffff;
						}

						tr:nth-child(even)
						{
                            background-color:#f0f0f0;
						}
						]]>
					</xsl:text>
				</style>
			</head>
			<body style="font-family: Segoe UI, Helvetica, sans-serif">
				<h1>
					<xsl:value-of select="@title"/>
				</h1>
				<xsl:for-each select="*">
					<xsl:choose>
						<xsl:when test="name()='SectionStart'">
							<xsl:text disable-output-escaping="yes"><![CDATA[<section>]]></xsl:text>
							<h2>
								<xsl:value-of select="."/>
							</h2>
						</xsl:when>
						<xsl:when test="name()='SectionEnd'">
							<xsl:text disable-output-escaping="yes"><![CDATA[</section>]]></xsl:text>
						</xsl:when>
						<xsl:when test="name()='TableStart'">
							<xsl:variable name="TableId" select="@tableId"/>
							<h3>
								<xsl:value-of select="@name"/>
							</h3>
							<table cellpadding="0" cellspacing="0">
								<thead>
									<tr>
										<xsl:for-each select="report:Column">
											<th>
												<xsl:attribute name="style">
													<xsl:choose>
														<xsl:when test="@alignment='Left'">
															<xsl:text>text-align:left</xsl:text>
														</xsl:when>
														<xsl:when test="@alignment='Center'">
															<xsl:text>text-align:center</xsl:text>
														</xsl:when>
														<xsl:when test="@alignment='Right'">
															<xsl:text>text-align:right</xsl:text>
														</xsl:when>
													</xsl:choose>
												</xsl:attribute>
												<xsl:value-of select="@header"/>
											</th>	
										</xsl:for-each>
									</tr>
								</thead>
								<tbody>
									<xsl:for-each select="/report:Report/report:Records[@tableId=$TableId]/report:Record">
										<tr>
											<xsl:for-each select="*">
												<xsl:variable name="Pos" select="position()"/>
												<td>
													<xsl:attribute name="style">
														<xsl:variable name="ColAlignment" select="/report:Report/report:TableStart[@tableId=$TableId]/report:Column[position()=$Pos]/@alignment"/>
														<xsl:choose>
															<xsl:when test="$ColAlignment='Left'">
																<xsl:text>text-align:left</xsl:text>
															</xsl:when>
															<xsl:when test="$ColAlignment='Center'">
																<xsl:text>text-align:center</xsl:text>
															</xsl:when>
															<xsl:when test="$ColAlignment='Right'">
																<xsl:text>text-align:right</xsl:text>
															</xsl:when>
														</xsl:choose>
													</xsl:attribute>
													<xsl:value-of select="."/>
												</td>
											</xsl:for-each>
										</tr>
									</xsl:for-each>
								</tbody>
							</table>
						</xsl:when>
						<xsl:when test="name()='Event'">
							<div>
								<xsl:attribute name="style">
									<xsl:choose>
										<xsl:when test="@type='Information'">
											<xsl:text>color:black;background-color:white</xsl:text>
										</xsl:when>
										<xsl:when test="@type='Warning'">
											<xsl:text>color:black;background-color:yellow</xsl:text>
										</xsl:when>
										<xsl:when test="@type='Error'">
											<xsl:text>color:yellow;background-color:red</xsl:text>
										</xsl:when>
										<xsl:when test="@type='Exception'">
											<xsl:text>color:yellow;background-color:darkred</xsl:text>
										</xsl:when>
									</xsl:choose>
									<xsl:text>;font-family:Courier New</xsl:text>
								</xsl:attribute>
							</div>
						</xsl:when>
						<xsl:when test="name()='Object'">
							<img>
								<xsl:attribute name="src">
									<xsl:text>data:</xsl:text>
									<xsl:value-of select="@contentType"/>
									<xsl:text>;base64,</xsl:text>
									<xsl:value-of select="."/>
								</xsl:attribute>
							</img>
						</xsl:when>
					</xsl:choose>
				</xsl:for-each>
			</body>
		</html>
    </xsl:template>
	
</xsl:stylesheet>
