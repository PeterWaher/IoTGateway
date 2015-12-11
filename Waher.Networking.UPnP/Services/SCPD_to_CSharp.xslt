<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
				xmlns:scpd="urn:schemas-upnp-org:service-1-0"
				exclude-result-prefixes="msxsl">
	
    <xsl:output method="text" indent="no"/>

    <xsl:template match="/">
		<xsl:text>using System;
using System.Collections.Generic;

namespace RetroSharp.Networking.UPnP.Services
{
#pragma warning disable
	public class UPnPServiceInterface
	{
		private ServiceDescriptionDocument service;</xsl:text>
		<xsl:for-each select="/scpd:scpd/scpd:actionList/scpd:action">
			<xsl:text>
		private UPnPAction action</xsl:text>
			<xsl:value-of select="scpd:name"/>
			<xsl:text> = null;</xsl:text>
		</xsl:for-each>
		<xsl:text>

		public UPnPServiceInterface(ServiceDescriptionDocument Service)
		{
			this.service = Service;
		}</xsl:text>
		<xsl:for-each select="/scpd:scpd/scpd:actionList/scpd:action">
			<xsl:text>

		public </xsl:text>
			<xsl:choose>
				<xsl:when test="scpd:argumentList/scpd:argument/scpd:retval">
					<xsl:call-template name="VariableType">
						<xsl:with-param name="VariableName" select="scpd:argumentList/scpd:argument/scpd:retval/../scpd:relatedStateVariable"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>void</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:text> </xsl:text>
			<xsl:value-of select="scpd:name"/>
			<xsl:text>(</xsl:text>
			<xsl:for-each select="scpd:argumentList/scpd:argument[not(scpd:retval)]">
				<xsl:if test="position()>1">
					<xsl:text>, </xsl:text>
				</xsl:if>
				<xsl:if test="scpd:direction='out'">
					<xsl:text>out </xsl:text>
				</xsl:if>
				<xsl:call-template name="VariableType">
					<xsl:with-param name="VariableName" select="scpd:relatedStateVariable"/>
				</xsl:call-template>
				<xsl:text> </xsl:text>
				<xsl:value-of select="scpd:name"/>
			</xsl:for-each>
			<xsl:text>)
		{
			Dictionary&lt;string, object&gt; OutputValues = new Dictionary&lt;string, object&gt;();
			
			if (action</xsl:text>
			<xsl:value-of select="scpd:name"/>
			<xsl:text> == null)
				action</xsl:text>
			<xsl:value-of select="scpd:name"/>
			<xsl:text> = this.service.GetAction("</xsl:text>
			<xsl:value-of select="scpd:name"/>
			<xsl:text>");

			</xsl:text>
			<xsl:if test="scpd:argumentList/scpd:argument/scpd:retval">
				<xsl:call-template name="VariableType">
					<xsl:with-param name="VariableName" select="scpd:argumentList/scpd:argument/scpd:retval/../scpd:relatedStateVariable"/>
				</xsl:call-template>
				<xsl:text> Result = (</xsl:text>
				<xsl:call-template name="VariableType">
					<xsl:with-param name="VariableName" select="scpd:argumentList/scpd:argument/scpd:retval/../scpd:relatedStateVariable"/>
				</xsl:call-template>
				<xsl:text>)</xsl:text>
			</xsl:if>
			<xsl:text>this.action</xsl:text>
			<xsl:value-of select="scpd:name"/>
			<xsl:text>.Invoke(out OutputValues</xsl:text>
			<xsl:for-each select="scpd:argumentList/scpd:argument[scpd:direction='in']">
				<xsl:text>,
				new KeyValuePair&lt;string, object&gt;("</xsl:text>
				<xsl:value-of select="scpd:name"/>
				<xsl:text>", </xsl:text>
				<xsl:value-of select="scpd:name"/>
				<xsl:text>)</xsl:text>
			</xsl:for-each>
			<xsl:text>);</xsl:text>
			<xsl:for-each select="scpd:argumentList/scpd:argument[scpd:direction='out' and not(scpd:retval)]">
				<xsl:if test="position()=1">
					<xsl:text>
</xsl:text>
				</xsl:if>
				<xsl:text>
			</xsl:text>
				<xsl:value-of select="scpd:name"/>
				<xsl:text> = (</xsl:text>
				<xsl:call-template name="VariableType">
					<xsl:with-param name="VariableName" select="scpd:relatedStateVariable"/>
				</xsl:call-template>
				<xsl:text>)OutputValues["</xsl:text>
				<xsl:value-of select="scpd:name"/>
				<xsl:text>"];</xsl:text>
			</xsl:for-each>
			<xsl:if test="scpd:argumentList/scpd:argument/scpd:retval">
				<xsl:text>

			return Result;</xsl:text>
			</xsl:if>
			<xsl:text>
		}</xsl:text>
		</xsl:for-each>
		<xsl:text>
	}
#pragma warning enable
}</xsl:text>
	</xsl:template>

	<xsl:template name="VariableType">
		<xsl:param name="VariableName"/>
		<xsl:variable name="DataType" select="/scpd:scpd/scpd:serviceStateTable/scpd:stateVariable[scpd:name=$VariableName]/scpd:dataType"/>
		<xsl:choose>
			<xsl:when test="$DataType='ui1'">
				<xsl:text>byte</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='ui2'">
				<xsl:text>ushort</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='ui4'">
				<xsl:text>uint</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='i1'">
				<xsl:text>sbyte</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='i2'">
				<xsl:text>short</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='i4'">
				<xsl:text>int</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='int'">
				<xsl:text>long</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='char'">
				<xsl:text>char</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='string'">
				<xsl:text>string</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='uri'">
				<xsl:text>Uri</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='uuid'">
				<xsl:text>Guid</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='r4'">
				<xsl:text>float</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='r8'">
				<xsl:text>double</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='number'">
				<xsl:text>double</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='float'">
				<xsl:text>double</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='fixed.14.4'">
				<xsl:text>double</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='date'">
				<xsl:text>DateTime</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='dateTime'">
				<xsl:text>DateTime</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='dateTime.tz'">
				<xsl:text>DateTimeOffset</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='time'">
				<xsl:text>TimeSpan</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='time.tz'">
				<xsl:text>TimeSpan</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='boolean'">
				<xsl:text>bool</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='bin.base64'">
				<xsl:text>byte[]</xsl:text>
			</xsl:when>
			<xsl:when test="$DataType='bin.hex'">
				<xsl:text>byte[]</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>string</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
