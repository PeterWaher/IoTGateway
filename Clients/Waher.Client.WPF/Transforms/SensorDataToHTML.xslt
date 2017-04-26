<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:sd1="http://waher.se/Schema/SensorData.xsd"
				xmlns:sd2="urn:xmpp:iot:sensordata"
				exclude-result-prefixes="msxsl">

	<xsl:output method="html" indent="no"/>

	<xsl:template match="/sd1:SensorData">
		<html>
			<head/>
			<body style="font-family: Segoe UI, Helvetica, sans-serif">
				<table cellspacing="0" cellpadding="2" border="0">
					<tr>
						<th>Timestamp</th>
						<th style="width:10px"/>
						<th>Field</th>
						<th style="width:10px"/>
						<th>Value</th>
						<th style="width:10px"/>
						<th>Unit</th>
						<th style="width:10px"/>
						<th>Status</th>
						<th style="width:10px"/>
						<th>Type</th>
					</tr>
					<xsl:for-each select="sd2:fields">
						<xsl:for-each select="sd2:node">
							<xsl:variable name="NodeId" select="@nodeId"/>
							<xsl:variable name="SourceId" select="@sourceId"/>
							<xsl:variable name="CacheType" select="@cacheType"/>
							<xsl:for-each select="sd2:timestamp">
								<xsl:variable name="Timestamp" select="@value"/>
								<xsl:for-each select="*">
									<tr>
										<xsl:attribute name="style">
											<xsl:choose>
												<xsl:when test="@invoiced or @invoiceConfirmed">
													<xsl:text>color:black;background-color:#FFD700</xsl:text>
												</xsl:when>
												<xsl:when test="@endOfSeries">
													<xsl:text>color:black;background-color:#ADD8E6</xsl:text>
												</xsl:when>
												<xsl:when test="@signed">
													<xsl:text>color:black;background-color:#90EE90</xsl:text>
												</xsl:when>
												<xsl:when test="@error">
													<xsl:text>color:black;background-color:#FFB6C1</xsl:text>
												</xsl:when>
												<xsl:when test="@powerFailure or @timeOffset or @warning">
													<xsl:text>color:black;background-color:#FFFFE0</xsl:text>
												</xsl:when>
												<xsl:when test="@missing or @inProgress">
													<xsl:text>color:black;background-color:#D3D3D3</xsl:text>
												</xsl:when>
												<xsl:when test="@automaticEstimate or @manualEstimate">
													<xsl:text>color:black;background-color:#F5F5F5</xsl:text>
												</xsl:when>
												<xsl:otherwise>
													<xsl:text>color:black;background-color:white</xsl:text>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:attribute>
										<td>
											<xsl:value-of select="substring($Timestamp,1,10)"/>
											<xsl:text>, </xsl:text>
											<xsl:value-of select="substring($Timestamp,12,8)"/>
										</td>
										<td/>
										<td>
											<xsl:value-of select="@name"/>
										</td>
										<td/>
										<td>
											<xsl:attribute name="style">
												<xsl:choose>
													<xsl:when test="name()='boolean'">
														<xsl:text>text-align:center</xsl:text>
													</xsl:when>
													<xsl:when test="name()='int' or name()='long' or name()='numeric'">
														<xsl:text>text-align:right</xsl:text>
													</xsl:when>
													<xsl:otherwise>
														<xsl:text>text-align:left</xsl:text>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:attribute>
											<xsl:value-of select="@value"/>
										</td>
										<td/>
										<td>
											<xsl:if test="name()='numeric'">
												<xsl:value-of select="@unit"/>
											</xsl:if>
										</td>
										<td/>
										<td>
											<xsl:if test="@missing">
												<xsl:text>Missing</xsl:text>
											</xsl:if>
											<xsl:if test="@inProgress">
												<xsl:if test="@missing">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>In Progress</xsl:text>
											</xsl:if>
											<xsl:if test="@automaticEstimate">
												<xsl:if test="@missing or @inProgress">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Automatic Estimate</xsl:text>
											</xsl:if>
											<xsl:if test="@manualEstimate">
												<xsl:if test="@missing or @inProgress or @automaticEstimate">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Manual Estimate</xsl:text>
											</xsl:if>
											<xsl:if test="@manualReadout">
												<xsl:if test="@missing or @inProgress or @automaticEstimate or @manualEstimate">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Manual Readout</xsl:text>
											</xsl:if>
											<xsl:if test="@automaticReadout">
												<xsl:if test="@missing or @inProgress or @automaticEstimate or @manualEstimate or @manualReadout">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Automatic Readout</xsl:text>
											</xsl:if>
											<xsl:if test="@timeOffset">
												<xsl:if test="@missing or @inProgress or @automaticEstimate or @manualEstimate or @manualReadout or @automaticReadout">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Time Offset</xsl:text>
											</xsl:if>
											<xsl:if test="@warning">
												<xsl:if test="@missing or @inProgress or @automaticEstimate or @manualEstimate or @manualReadout or @automaticReadout or @timeOffset">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Warning</xsl:text>
											</xsl:if>
											<xsl:if test="@error">
												<xsl:if test="@missing or @inProgress or @automaticEstimate or @manualEstimate or @manualReadout or @automaticReadout or @timeOffset or @warning">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Error</xsl:text>
											</xsl:if>
											<xsl:if test="@signed">
												<xsl:if test="@missing or @inProgress or @automaticEstimate or @manualEstimate or @manualReadout or @automaticReadout or @timeOffset or @warning or @error">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Signed</xsl:text>
											</xsl:if>
											<xsl:if test="@invoiced">
												<xsl:if test="@missing or @inProgress or @automaticEstimate or @manualEstimate or @manualReadout or @automaticReadout or @timeOffset or @warning or @error or @signed">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Invoiced</xsl:text>
											</xsl:if>
											<xsl:if test="@endOfSeries">
												<xsl:if test="@missing or @inProgress or @automaticEstimate or @manualEstimate or @manualReadout or @automaticReadout or @timeOffset or @warning or @error or @signed or @invoiced">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>End of Series</xsl:text>
											</xsl:if>
											<xsl:if test="@powerFailure">
												<xsl:if test="@missing or @inProgress or @automaticEstimate or @manualEstimate or @manualReadout or @automaticReadout or @timeOffset or @warning or @error or @signed or @invoiced or @endOfSeries">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Power Failure</xsl:text>
											</xsl:if>
											<xsl:if test="@invoiceConfirmed">
												<xsl:if test="@missing or @inProgress or @automaticEstimate or @manualEstimate or @manualReadout or @automaticReadout or @timeOffset or @warning or @error or @signed or @invoiced or @endOfSeries or @powerFailure">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Invoice Confirmed</xsl:text>
											</xsl:if>
										</td>
										<td/>
										<td>
											<xsl:if test="@momentary">
												<xsl:text>Momentary</xsl:text>
											</xsl:if>
											<xsl:if test="@peak">
												<xsl:if test="@momentary">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Peak</xsl:text>
											</xsl:if>
											<xsl:if test="@status">
												<xsl:if test="@momentary or @peak">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Status</xsl:text>
											</xsl:if>
											<xsl:if test="@computed">
												<xsl:if test="@momentary or @peak or @status">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Computed</xsl:text>
											</xsl:if>
											<xsl:if test="@identity">
												<xsl:if test="@momentary or @peak or @status or @computed">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Identity</xsl:text>
											</xsl:if>
											<xsl:if test="@historicalSecond">
												<xsl:if test="@momentary or @peak or @status or @computed or @identity">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Historical (Second)</xsl:text>
											</xsl:if>
											<xsl:if test="@historicalMinute">
												<xsl:if test="@momentary or @peak or @status or @computed or @identity or @historicalSecond">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Historical (Minute)</xsl:text>
											</xsl:if>
											<xsl:if test="@historicalHour">
												<xsl:if test="@momentary or @peak or @status or @computed or @identity or @historicalSecond or @historicalMinute">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Historical (Hour)</xsl:text>
											</xsl:if>
											<xsl:if test="@historicalDay">
												<xsl:if test="@momentary or @peak or @status or @computed or @identity or @historicalSecond or @historicalMinute or @historicalHour">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Historical (Day)</xsl:text>
											</xsl:if>
											<xsl:if test="@historicalWeek">
												<xsl:if test="@momentary or @peak or @status or @computed or @identity or @historicalSecond or @historicalMinute or @historicalHour or @historicalDay">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Historical (Week)</xsl:text>
											</xsl:if>
											<xsl:if test="@historicalMonth">
												<xsl:if test="@momentary or @peak or @status or @computed or @identity or @historicalSecond or @historicalMinute or @historicalHour or @historicalDay or @historicalWeek">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Historical (Month)</xsl:text>
											</xsl:if>
											<xsl:if test="@historicalQuarter">
												<xsl:if test="@momentary or @peak or @status or @computed or @identity or @historicalSecond or @historicalMinute or @historicalHour or @historicalDay or @historicalWeek or @historicalMonth">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Historical (Quarter)</xsl:text>
											</xsl:if>
											<xsl:if test="@historicalYear">
												<xsl:if test="@momentary or @peak or @status or @computed or @identity or @historicalSecond or @historicalMinute or @historicalHour or @historicalDay or @historicalWeek or @historicalMonth or @historicalQuarter">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Historical (Year)</xsl:text>
											</xsl:if>
											<xsl:if test="@historicalOther">
												<xsl:if test="@momentary or @peak or @status or @computed or @identity or @historicalSecond or @historicalMinute or @historicalHour or @historicalDay or @historicalWeek or @historicalMonth or @historicalQuarter or @historicalYear">
													<xsl:text>, </xsl:text>
												</xsl:if>
												<xsl:text>Historical (Other)</xsl:text>
											</xsl:if>
										</td>
									</tr>
								</xsl:for-each>
							</xsl:for-each>
						</xsl:for-each>
					</xsl:for-each>
				</table>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>
