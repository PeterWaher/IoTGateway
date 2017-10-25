<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								xmlns:msxsl="urn:schemas-microsoft-com:xslt"
								xmlns:dbstat="http://waher.se/Schema/Persistence/Statistics.xsd"
								exclude-result-prefixes="msxsl dbstat xsl">

	<xsl:output method="html" indent="yes"/>

	<xsl:template match="/dbstat:DatabaseStatistics">
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
        <xsl:for-each select="dbstat:File">
          <h1>
            <xsl:value-of select="@collectionName"/>
          </h1>
          <table class="CollectionTable" cellspacing="0">
            <thead>
              <tr class="CollectionTableHeader">
                <th>File Name</th>
                <th class="Right">Block Size</th>
                <th class="Right">Objects</th>
                <th class="Center">Encoding</th>
                <th class="Center">Encrypted</th>
                <th class="Center">Balanced</th>
                <th class="Center">Corrupt</th>
                <th class="Right">Blocks</th>
                <th class="Right">Bytes</th>
                <th class="Right">Used</th>
                <th class="Right">Unused</th>
                <th class="Right">Usage (%)</th>
                <th class="Right">Avg(Bytes/Block)</th>
                <th class="Right">Avg(Obj size)</th>
                <th class="Right">Avg(Obj/Block)</th>
                <th class="Right">Max(Bytes/Block)</th>
                <th class="Right">Max(Obj size)</th>
                <th class="Right">Max(Obj/Block)</th>
                <th class="Right">Max(Depth)</th>
                <th class="Right">Min(Bytes/Block)</th>
                <th class="Right">Min(Obj size)</th>
                <th class="Right">Min(Obj/Block)</th>
                <th class="Right">Min(Depth)</th>
              </tr>
            </thead>
            <tbody>
              <xsl:call-template name="FileStat"/>
            </tbody>
          </table>
        </xsl:for-each>
      </body>
		</html>
	</xsl:template>
  
  <xsl:template name="FileStat">
    <xsl:variable name="FileColor">
      <xsl:choose>
        <xsl:when test="dbstat:Stat/@isCorrupt='true' or dbstat:Stat/@isBalanced='false'">
          <xsl:text>Error</xsl:text>
        </xsl:when>
        <xsl:when test="dbstat:Stat/@hasComments='true'">
          <xsl:text>Warning</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>Ok</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <tr>
      <xsl:attribute name="class">
        <xsl:value-of select="$FileColor"/>
      </xsl:attribute>
      <td>
        <xsl:value-of select="@fileName"/>
        <xsl:if test="dbstat:Field">
          <xsl:text> (</xsl:text>
          <xsl:for-each select="dbstat:Field">
            <xsl:if test="position()>1">
              <xsl:text>, </xsl:text>
            </xsl:if>
            <xsl:value-of select="."/>
          </xsl:for-each>
          <xsl:text>)</xsl:text>
        </xsl:if>
      </td>
      <td class="Right">
        <xsl:value-of select="@blockSize"/>
      </td>
      <td class="Right">
        <xsl:value-of select="@count"/>
      </td>
      <td class="Center">
        <xsl:value-of select="@encoding"/>
      </td>
      <td class="Center">
        <xsl:value-of select="@encrypted"/>
      </td>
      <td class="Center">
        <xsl:value-of select="dbstat:Stat/@isBalanced"/>
      </td>
      <td class="Center">
        <xsl:value-of select="dbstat:Stat/@isCorrupt"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@nrBlocks"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@nrBytes"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@nrBytesUsed"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@nrBytesUnused"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@usage"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@avgBytesPerBlock"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@avgObjSize"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@avgObjPerBlock"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@maxBytesPerBlock"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@maxObjSize"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@maxObjPerBlock"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@maxDepth"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@minBytesPerBlock"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@minObjSize"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@minObjPerBlock"/>
      </td>
      <td class="Right">
        <xsl:value-of select="dbstat:Stat/@minDepth"/>
      </td>
    </tr>
    <xsl:if test="dbstat:Stat/@hasComments">
      <xsl:for-each select="dbstat:Stat/dbstat:Comment">
        <tr>
          <xsl:attribute name="class">
            <xsl:value-of select="$FileColor"/>
          </xsl:attribute>
          <td/>
          <td colspan="22">
            <xsl:value-of select="."/>
          </td>
        </tr>
      </xsl:for-each>
    </xsl:if>
    <xsl:if test="string-length(@blobFileName)>0">
      <tr>
        <xsl:attribute name="class">
          <xsl:value-of select="$FileColor"/>
        </xsl:attribute>
        <td>
          <xsl:value-of select="@blobFileName"/>
        </td>
        <td class="Right">
          <xsl:value-of select="dbstat:Stat/@blobBlockSize"/>
        </td>
        <td/>
        <td class="Center">
          <xsl:value-of select="dbstat:Stat/@encoding"/>
        </td>
        <td class="Center">
          <xsl:value-of select="dbstat:Stat/@encrypted"/>
        </td>
        <td/>
        <td/>
        <td class="Right">
          <xsl:value-of select="dbstat:Stat/@nrBlobBlocks"/>
        </td>
        <td class="Right">
          <xsl:value-of select="dbstat:Stat/@nrBlobBytes"/>
        </td>
        <td class="Right">
          <xsl:value-of select="dbstat:Stat/@nrBlobBytesUsed"/>
        </td>
        <td class="Right">
          <xsl:value-of select="dbstat:Stat/@nrBlobBytesUnused"/>
        </td>
        <td class="Right">
          <xsl:value-of select="dbstat:Stat/@blobUsage"/>
        </td>
        <td/>
        <td/>
        <td/>
        <td/>
        <td/>
        <td/>
        <td/>
        <td/>
        <td/>
        <td/>
        <td/>
      </tr>
    </xsl:if>
    <xsl:for-each select="dbstat:Index">
      <xsl:call-template name="FileStat"/>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>
