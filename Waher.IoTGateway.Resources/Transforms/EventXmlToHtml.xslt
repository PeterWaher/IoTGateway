<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:eo="http://waher.se/Schema/EventOutput.xsd"
								exclude-result-prefixes="msxsl eo xsl">

	<xsl:output method="html" indent="yes"/>

	<xsl:template match="/eo:EventOutput">
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
							}
					
							tr
							{
								vertical-align:top;
							}

							tr:nth-child(odd) td
							{
								background-color: rgba(0,0,0,0.1);
								border-top: solid 1px rgba(0,0,0,0.3);
							}

							tr:nth-child(even) td
							{
								border-bottom: solid 1px rgba(0,0,0,0.3);
							}

							th
							{
								text-align:left;
							}

							td
							{
								text-align:left;
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
				
							.Debug
							{
								background-color:darkblue;
								color:white;
							}
				
							.Informational
							{
								background-color:white;
								color:black;
							}
				
							.Notice
							{
								background-color:lightyellow;
								color:black;
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
				
							.Critical
							{
								background-color:darkred;
								color:white;
							}
				
							.Alert
							{
								background-color:purple;
								color:white;
							}
				
							.Emergency
							{
								background-color:black;
								color:white;
							}
						]]>
					</xsl:text>
				</style>
			</head>
			<body>
				<table class="EventTable" cellspacing="0">
					<thead>
						<tr class="EventTableHeader">
							<th>Time</th>
							<th>Type</th>
							<th>Level</th>
							<th>ID</th>
							<th>Object</th>
							<th>Actor</th>
							<th>Module</th>
							<th>Facility</th>
						</tr>
					</thead>
					<tbody>
						<xsl:apply-templates/>
					</tbody>
				</table>
			</body>
		</html>
	</xsl:template>
	
	<xsl:template match="eo:Debug">
		<xsl:call-template name="EventContent">
			<xsl:with-param name="ClassName" select="'Debug'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="eo:Informational">
		<xsl:call-template name="EventContent">
			<xsl:with-param name="ClassName" select="'Informational'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="eo:Notice">
		<xsl:call-template name="EventContent">
			<xsl:with-param name="ClassName" select="'Notice'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="eo:Warning">
		<xsl:call-template name="EventContent">
			<xsl:with-param name="ClassName" select="'Warning'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="eo:Error">
		<xsl:call-template name="EventContent">
			<xsl:with-param name="ClassName" select="'Error'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="eo:Critical">
		<xsl:call-template name="EventContent">
			<xsl:with-param name="ClassName" select="'Critical'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="eo:Alert">
		<xsl:call-template name="EventContent">
			<xsl:with-param name="ClassName" select="'Alert'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="eo:Emergency">
		<xsl:call-template name="EventContent">
			<xsl:with-param name="ClassName" select="'Emergency'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="EventContent">
		<xsl:param name="ClassName"/>
		<tr xmlns="http://www.w3.org/1999/xhtml">
			<xsl:attribute name="class">
				<xsl:value-of select="$ClassName"/>
			</xsl:attribute>
			<td>
				<xsl:value-of select="substring(@timestamp,12)"/>
			</td>
			<td>
				<xsl:value-of select="$ClassName"/>
			</td>
			<td>
				<xsl:value-of select="@level"/>
			</td>
			<td>
				<xsl:value-of select="@id"/>
			</td>
			<td>
				<xsl:value-of select="@object"/>
			</td>
			<td>
				<xsl:value-of select="@actor"/>
			</td>
			<td>
				<xsl:value-of select="@module"/>
			</td>
			<td>
				<xsl:value-of select="@facility"/>
			</td>
		</tr>
		<tr xmlns="http://www.w3.org/1999/xhtml">
			<xsl:attribute name="class">
				<xsl:value-of select="$ClassName"/>
			</xsl:attribute>
			<td colspan="8">
				<xsl:for-each select="eo:Message/eo:Row">
					<xsl:value-of select="."/>
					<br/>
				</xsl:for-each>
				<xsl:if test="eo:StackTrace">
					<pre>
						<code>
							<xsl:for-each select="eo:StackTrace/eo:Row">
								<xsl:value-of select="."/>
								<br/>
							</xsl:for-each>
						</code>
					</pre>
				</xsl:if>
				<xsl:if test="eo:Tag">
					<table class="TagTable" cellspacing="0">
						<thead>
							<tr class="TagTableHeader">
								<th>Key</th>
								<th>Value</th>
							</tr>
						</thead>
						<tbody>
							<xsl:for-each select="eo:Tag">
								<tr>
									<td>
										<xsl:value-of select="@key"/>
									</td>
									<td>
										<xsl:value-of select="@value"/>
									</td>
								</tr>
							</xsl:for-each>
						</tbody>
					</table>
				</xsl:if>
			</td>
		</tr>
	</xsl:template>
</xsl:stylesheet>
