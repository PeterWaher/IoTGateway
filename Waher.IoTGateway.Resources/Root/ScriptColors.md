Title: Script Scolors
Description: Shows available colors and their corresponding names
Master: /Master.md
Author: Peter Waher
Date: 2023-05-05

Script Colors
=================

The following named colors can be used in script functions where color arguments are expected:

| Name | Color | Name | Color | Name | Color | Name | Color |
|:-----|:------|:-----|:------|:-----|:------|:-----|:------|
{{
ColorNames:=Fields(SkiaSharp.SKColors);
c:=count(ColorNames);
c1:=floor(c/4);
c2:=c1;
c3:=c1;
c4:=c-c1-c2-c3;

PrintColor([ColorName]):=
(
	]]| `((ColorName))` | <img alt="((ColorName))" src="((
C:=Canvas(128,24,"White",ColorName);
V:=Create(Waher.Script.Variables,[]);
Png:=C.CreatePixels(V);
"data:image/png;base64,"+Base64Encode(Png.Binary)
))"/> [[
);

for i:=1 to c1 do
(
	PrintColor(ColorNames[i-1]);
	PrintColor(ColorNames[c1+i-1]);
	PrintColor(ColorNames[c1+c2+i-1]);
	if (i<=c4) then
		PrintColor(ColorNames[c1+c2+c3+i-1])
	else
		]]| | [[;

	]]|
[[
)}}