<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:cstat="http://waher.se/Schema/Networking/ClockStatistics.xsd"
								exclude-result-prefixes="msxsl cstat xsl">

	<xsl:output method="text" indent="yes"/>

	<xsl:template match="/cstat:ClockStatistics">
		<xsl:for-each select="cstat:Samples">
			<xsl:text>Date</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Time</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Raw Latency (ms)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Spike</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Raw Difference (ms)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Spike</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Filtered Latency (ms)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Filtered Difference (ms)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Avg Latency (ms)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Avg Difference (ms)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Raw Latency (HF)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Spike</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Raw Difference (HF)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Spike</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Filtered Latency (HF)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Filtered Difference (HF)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Avg Latency (HF)</xsl:text>
			<xsl:text>	</xsl:text>
			<xsl:text>Avg Difference (HF)</xsl:text>
			<xsl:text>
</xsl:text>
			<xsl:for-each select="cstat:Sample">
				<xsl:value-of select="substring(@timestamp,1,10)"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="substring(@timestamp,12,8)"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@rawLatencyMs"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@spikeLatencyRemoved"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@rawDifferenceMs"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@spikeDifferenceRemoved"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@filteredLatencyMs"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@filteredDifferenceMs"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@avgLatencyMs"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@avgDifferenceMs"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@rawLatencyHf"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@spikeLatencyHfRemoved"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@rawDifferenceHf"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@spikeDifferenceHfRemoved"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@filteredLatencyHf"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@filteredDifferenceHf"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@avgLatencyHf"/>
				<xsl:text>	</xsl:text>
				<xsl:value-of select="@avgDifferenceHf"/>
				<xsl:text>
</xsl:text>
			</xsl:for-each>
		</xsl:for-each>
	</xsl:template>
</xsl:stylesheet>
