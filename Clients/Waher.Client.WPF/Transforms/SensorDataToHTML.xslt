<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:sd1="http://waher.se/Schema/SensorData.xsd"
				xmlns:sd2="urn:ieee:iot:sd:1.0"
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
          <xsl:for-each select="sd2:resp">
            <xsl:for-each select="sd2:nd">
              <xsl:variable name="NodeId" select="@id"/>
              <xsl:variable name="SourceId" select="@src"/>
              <xsl:variable name="CacheType" select="@pt"/>
              <xsl:for-each select="sd2:ts">
                <xsl:variable name="Timestamp" select="@v"/>
                <xsl:for-each select="*">
                  <tr>
                    <xsl:attribute name="style">
                      <xsl:choose>
                        <xsl:when test="@iv or @ic">
                          <xsl:text>color:black;background-color:#FFD700</xsl:text>
                        </xsl:when>
                        <xsl:when test="@eos">
                          <xsl:text>color:black;background-color:#ADD8E6</xsl:text>
                        </xsl:when>
                        <xsl:when test="@so">
                          <xsl:text>color:black;background-color:#90EE90</xsl:text>
                        </xsl:when>
                        <xsl:when test="@er">
                          <xsl:text>color:black;background-color:#FFB6C1</xsl:text>
                        </xsl:when>
                        <xsl:when test="@pf or @of or @w">
                          <xsl:text>color:black;background-color:#FFFFE0</xsl:text>
                        </xsl:when>
                        <xsl:when test="@ms or @pr">
                          <xsl:text>color:black;background-color:#D3D3D3</xsl:text>
                        </xsl:when>
                        <xsl:when test="@ae or @me">
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
                      <xsl:value-of select="@n"/>
                    </td>
                    <td/>
                    <td>
                      <xsl:attribute name="style">
                        <xsl:choose>
                          <xsl:when test="name()='b'">
                            <xsl:text>text-align:center</xsl:text>
                          </xsl:when>
                          <xsl:when test="name()='i' or name()='l' or name()='q'">
                            <xsl:text>text-align:right</xsl:text>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:text>text-align:left</xsl:text>
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:attribute>
                      <xsl:value-of select="@v"/>
                    </td>
                    <td/>
                    <td>
                      <xsl:if test="name()='q'">
                        <xsl:value-of select="@u"/>
                      </xsl:if>
                    </td>
                    <td/>
                    <td>
                      <xsl:if test="@ms">
                        <xsl:text>Missing</xsl:text>
                      </xsl:if>
                      <xsl:if test="@pr">
                        <xsl:if test="@ms">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>In Progress</xsl:text>
                      </xsl:if>
                      <xsl:if test="@ae">
                        <xsl:if test="@ms or @pr">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Automatic Estimate</xsl:text>
                      </xsl:if>
                      <xsl:if test="@me">
                        <xsl:if test="@ms or @pr or @ae">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Manual Estimate</xsl:text>
                      </xsl:if>
                      <xsl:if test="@mr">
                        <xsl:if test="@ms or @pr or @ae or @me">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Manual Readout</xsl:text>
                      </xsl:if>
                      <xsl:if test="@ar">
                        <xsl:if test="@ms or @pr or @ae or @me or @mr">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Automatic Readout</xsl:text>
                      </xsl:if>
                      <xsl:if test="@of">
                        <xsl:if test="@ms or @pr or @ae or @me or @mr or @ar">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Time Offset</xsl:text>
                      </xsl:if>
                      <xsl:if test="@w">
                        <xsl:if test="@ms or @pr or @ae or @me or @mr or @ar or @of">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Warning</xsl:text>
                      </xsl:if>
                      <xsl:if test="@er">
                        <xsl:if test="@ms or @pr or @ae or @me or @mr or @ar or @of or @w">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Error</xsl:text>
                      </xsl:if>
                      <xsl:if test="@so">
                        <xsl:if test="@ms or @pr or @ae or @me or @mr or @ar or @of or @w or @er">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Signed</xsl:text>
                      </xsl:if>
                      <xsl:if test="@iv">
                        <xsl:if test="@ms or @pr or @ae or @me or @mr or @ar or @of or @w or @er or @so">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Invoiced</xsl:text>
                      </xsl:if>
                      <xsl:if test="@eos">
                        <xsl:if test="@ms or @pr or @ae or @me or @mr or @ar or @of or @w or @er or @so or @iv">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>End of Series</xsl:text>
                      </xsl:if>
                      <xsl:if test="@pf">
                        <xsl:if test="@ms or @pr or @ae or @me or @mr or @ar or @of or @w or @er or @so or @iv or @eos">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Power Failure</xsl:text>
                      </xsl:if>
                      <xsl:if test="@ic">
                        <xsl:if test="@ms or @pr or @ae or @me or @mr or @ar or @of or @w or @er or @so or @iv or @eos or @pf">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Invoice Confirmed</xsl:text>
                      </xsl:if>
                    </td>
                    <td/>
                    <td>
                      <xsl:if test="@m">
                        <xsl:text>Momentary</xsl:text>
                      </xsl:if>
                      <xsl:if test="@p">
                        <xsl:if test="@m">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Peak</xsl:text>
                      </xsl:if>
                      <xsl:if test="@s">
                        <xsl:if test="@m or @p">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Status</xsl:text>
                      </xsl:if>
                      <xsl:if test="@c">
                        <xsl:if test="@m or @p or @s">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Computed</xsl:text>
                      </xsl:if>
                      <xsl:if test="@i">
                        <xsl:if test="@m or @p or @s or @c">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Identity</xsl:text>
                      </xsl:if>
                      <xsl:if test="@h">
                        <xsl:if test="@m or @p or @s or @c or @i">
                          <xsl:text>, </xsl:text>
                        </xsl:if>
                        <xsl:text>Historical</xsl:text>
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
