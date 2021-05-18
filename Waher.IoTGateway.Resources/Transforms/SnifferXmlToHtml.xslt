<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:so="http://waher.se/Schema/SnifferOutput.xsd"
								exclude-result-prefixes="msxsl so xsl">

	<xsl:output method="html" indent="yes"/>

	<xsl:template match="/so:SnifferOutput">
		<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">
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
								margin-bottom: 1.5em;
								box-shadow: 0.3em 0.3em 0.3em #999;
								border-collapse: separate;
								border-width: 0;
								border-spacing:0;
								background-color:white;
							}
					
							tr
							{
								vertical-align:top;
							}

							tr th:nth-of-type(1)
							{
								text-align:center;
							}

							tr th:nth-of-type(2)
							{
								text-align:left;
							}

							tr td:nth-of-type(1)
							{
								text-align:center;
							}

							tr td:nth-of-type(2)
							{
								text-align:left;
								word-break: break-all;
							}

							table, th, td
							{
								border: 0;
							}

							th
							{
								background-color: #4CAF50;
								color: white;
							}
 
							th, td
							{
								padding: 0.2em 0.6em 0.2em 0.6em;
							}
				
							.Rx
							{
								background-color:navy;
								color:white;
							}
				
							.Tx
							{
								background-color:white;
								color:black;
							}
				
							.Info
							{
								background-color:green;
								color:yellow;
							}
				
							.Warning
							{
								background-color:yellow;
								color:black;
							}
				
							.Error
							{
								background-color:red;
								color:yellow;
							}
				
							.Exception
							{
								background-color:darkred;
								color:white;
							}
						]]>
					</xsl:text>
				</style>
			</head>
			<body>
				<table class="SnifferTable" cellspacing="0">
					<thead>
						<tr class="SnifferTableHeader">
							<th>Time</th>
							<th>Event</th>
						</tr>
					</thead>
					<tbody>
						<xsl:apply-templates/>
					</tbody>
				</table>
			</body>
		</html>
	</xsl:template>
	
	<xsl:template name="RowContents">
		<td>
			<xsl:value-of select="substring(@timestamp,12)"/>
		</td>
		<td>
      <xsl:for-each select="so:Row">
        <xsl:if test="position()>1">
          <br/>
        </xsl:if>
        <xsl:value-of select="."/>
      </xsl:for-each>
		</td>
	</xsl:template>

	<xsl:template match="so:Rx">
		<tr class="Rx">
			<xsl:call-template name="RowContents"/>
		</tr>
	</xsl:template>

	<xsl:template match="so:Tx">
		<tr class="Tx">
			<xsl:call-template name="RowContents"/>
		</tr>
	</xsl:template>

	<xsl:template match="so:Info">
		<tr class="Info">
			<xsl:call-template name="RowContents"/>
		</tr>
</xsl:template>

	<xsl:template match="so:Warning">
		<tr class="Warning">
			<xsl:call-template name="RowContents"/>
		</tr>
	</xsl:template>

	<xsl:template match="so:Error">
		<tr class="Error">
			<xsl:call-template name="RowContents"/>
		</tr>
	</xsl:template>

	<xsl:template match="so:Exception">
		<tr class="Exception">
			<xsl:call-template name="RowContents"/>
		</tr>
	</xsl:template>
</xsl:stylesheet>
