<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:cstat="http://waher.se/Schema/Networking/ClockStatistics.xsd"
								exclude-result-prefixes="msxsl cstat xsl">

	<xsl:output method="html" indent="yes"/>

	<xsl:template match="/cstat:ClockStatistics">
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
				
							.Right
							{
								text-align:right;
							}
				
							.Center
							{
								text-align:center;
							}
				
							.Debug
							{
								background-color:darkblue;
								color:white;
							}
				
							.Ok
							{
								background-color:white;
								color:black;
							}
				
							.Warning
							{
								background-color:yellow;
								color:black;
							}
				
							.Notice
							{
								background-color:lightyellow;
								color:black;
							}
				
							.Error
							{
								background-color:red;
								color:yellow;
							}
						]]>
					</xsl:text>
				</style>
			</head>
			<body>
				<xsl:for-each select="cstat:Parameters">
					<h1>Parameters</h1>
					<table class="ParametersTable" cellspacing="0">
						<thead>
							<tr class="ParametersTableHeader">
								<th>Parameter</th>
								<th>Value</th>
							</tr>
						</thead>
						<tbody>
							<tr>
								<td>Client JID</td>
								<td>
									<xsl:value-of select="@clientJid"/>
								</td>
							</tr>
							<tr>
								<td>Clock Source JID</td>
								<td>
									<xsl:value-of select="@sourceJid"/>
								</td>
							</tr>
							<tr>
								<td>Records</td>
								<td>
									<xsl:value-of select="@records"/>
								</td>
							</tr>
							<tr>
								<td>Interval</td>
								<td>
									<xsl:value-of select="@interval"/>
								</td>
							</tr>
							<tr>
								<td>Averaging Window</td>
								<td>
									<xsl:value-of select="@history"/>
								</td>
							</tr>
							<tr>
								<td>Filter Window</td>
								<td>
									<xsl:value-of select="@window"/>
								</td>
							</tr>
							<tr>
								<td>Spike Position</td>
								<td>
									<xsl:value-of select="@spikePos"/>
								</td>
							</tr>
							<tr>
								<td>Spike Width</td>
								<td>
									<xsl:value-of select="@spikeWidth"/>
								</td>
							</tr>
							<tr>
								<td>HF Increments/s</td>
								<td>
									<xsl:value-of select="@hfFreq"/>
								</td>
							</tr>
						</tbody>
					</table>
				</xsl:for-each>
				<xsl:for-each select="cstat:Samples">
					<h1>Measurements</h1>
					<table class="SampleTable" cellspacing="0">
            <thead>
              <tr class="SampleTableHeader">
								<th>Date</th>
								<th>Time</th>
								<th class="Right">Raw Latency (ms)</th>
								<th class="Center">Spike</th>
								<th class="Right">Raw Difference (ms)</th>
								<th class="Center">Spike</th>
								<th class="Right">Filtered Latency (ms)</th>
								<th class="Right">Filtered Difference (ms)</th>
								<th class="Right">Avg Latency (ms)</th>
								<th class="Right">Avg Difference (ms)</th>
								<th class="Right">StdDev Latency (ms)</th>
								<th class="Right">StdDev Difference (ms)</th>
								<th class="Right">Raw Latency (HF)</th>
								<th class="Center">Spike</th>
								<th class="Right">Raw Difference (HF)</th>
								<th class="Center">Spike</th>
								<th class="Right">Filtered Latency (HF)</th>
								<th class="Right">Filtered Difference (HF)</th>
								<th class="Right">Avg Latency (HF)</th>
								<th class="Right">Avg Difference (HF)</th>
								<th class="Right">StdDev Latency (HF)</th>
								<th class="Right">StdDev Difference (HF)</th>
							</tr>
            </thead>
            <tbody>
							<xsl:for-each select="cstat:Sample">
								<tr>
									<td>
										<xsl:value-of select="substring(@timestamp,1,10)"/>
									</td>
									<td>
										<xsl:value-of select="substring(@timestamp,12,8)"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@rawLatencyMs"/>
									</td>
									<td class="Center">
										<xsl:value-of select="@spikeLatencyRemoved"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@rawDifferenceMs"/>
									</td>
									<td class="Center">
										<xsl:value-of select="@spikeDifferenceRemoved"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@filteredLatencyMs"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@filteredDifferenceMs"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@avgLatencyMs"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@avgDifferenceMs"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@stdDevLatencyMs"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@stdDevDifferenceMs"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@rawLatencyHf"/>
									</td>
									<td class="Center">
										<xsl:value-of select="@spikeLatencyHfRemoved"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@rawDifferenceHf"/>
									</td>
									<td class="Center">
										<xsl:value-of select="@spikeDifferenceHfRemoved"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@filteredLatencyHf"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@filteredDifferenceHf"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@avgLatencyHf"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@avgDifferenceHf"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@stdDevLatencyHf"/>
									</td>
									<td class="Right">
										<xsl:value-of select="@stdDevDifferenceHf"/>
									</td>
								</tr>
							</xsl:for-each>
            </tbody>
          </table>
        </xsl:for-each>
      </body>
		</html>
	</xsl:template>
  
</xsl:stylesheet>
