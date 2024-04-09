function CreateInnerHTML(ElementId)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHtml();
}

function CreateHTML()
{
	var Segments = [
		"<p>",
		"Image: ",
		"<img src=\"/path/to/img.jpg\" alt=\"Alt text 1\"/>",
		"</p>\r\n",
		"<figure><img src=\"/path/to/img.jpg\" alt=\"Alt text 1\" class=\"aloneUnsized\"/><figcaption>Alt text 1</figcaption></figure>\r\n",
		"<figure><img src=\"/path/to/img.jpg\" alt=\"Alt text 2\" title=\"Optional title\" class=\"aloneUnsized\"/><figcaption>Alt text 2</figcaption></figure>\r\n",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 3\" title=\"Optional title attribute\" class=\"aloneUnsized\"/><figcaption>Alt text 3</figcaption></figure>\r\n",
		"Alt text 3",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 4\" title=\"Optional title attribute\" class=\"aloneUnsized\"/><figcaption>Alt text 4</figcaption></figure>\r\n",
		"Alt text 4",
		"<figure><img src=\"/path/to/img.jpg\" alt=\"Alt text 5\" width=\"320\" height=\"200\"/><figcaption>Alt text 5</figcaption></figure>\r\n",
		"<figure><img src=\"/path/to/img.jpg\" alt=\"Alt text 6\" title=\"Optional title 2\" width=\"320\" height=\"200\"/><figcaption>Alt text 6</figcaption></figure>\r\n",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 7\" title=\"Optional title attribute 2\" width=\"320\" height=\"200\"/><figcaption>Alt text 7</figcaption></figure>\r\n",
		"Alt text 7",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 8\" width=\"320\" height=\"200\"/><figcaption>Alt text 8</figcaption></figure>\r\n",
		"Alt text 8",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 9\" title=\"Optional title attribute 2\" width=\"320\" height=\"200\"/><figcaption>Alt text 9</figcaption></figure>\r\n",
		"Alt text 9",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 10\" width=\"320\" height=\"200\"/><figcaption>Alt text 10</figcaption></figure>\r\n",
		"Alt text 10",
		"<audio autoplay=\"autoplay\">\r\n<source src=\"/local/music.mp3\" type=\"audio/mpeg\"/>\r\nYour browser does not support the audio tag</audio>\r\n",
		"<video controls=\"controls\" width=\"320\" height=\"200\">\r\n<source src=\"/local/video.mp4\" type=\"video/mp4\"/>\r\nYour browser does not support the video tag</video>\r\n",
		"<iframe src=\"https://www.youtube.com/embed/whBPLc8m4SU\" width=\"800\" height=\"600\">Your browser does not support the iframe tag</iframe>\r\n",
		"<figure><picture>\r\n<source srcset=\"img1.svg\" type=\"image/svg+xml\"/>\r\n<source srcset=\"img2.png\" type=\"image/png\"/>\r\n<img src=\"img1.svg\" alt=\"MultiImage1\" srcset=\"img1.svg, img2.png\" sizes=\"100vw\"/></picture>\r\n<figcaption>MultiImage1</figcaption></figure>\r\n",
		"<figure><picture>\r\n<source srcset=\"img1.svg\" type=\"image/svg+xml\" media=\"(min-width:320px)\"/>\r\n<source srcset=\"img2.png\" type=\"image/png\" media=\"(min-width:320px)\"/>\r\n<img src=\"img1.svg\" alt=\"MultiImage2\" width=\"320\" height=\"200\" srcset=\"img1.svg 320w, img2.png 320w\" sizes=\"100vw\"/></picture>\r\n<figcaption>MultiImage2</figcaption></figure>\r\n",
		"<figure><picture>\r\n<source srcset=\"img1.svg\" type=\"image/svg+xml\" media=\"(min-width:320px)\"/>\r\n<source srcset=\"img2.png\" type=\"image/png\" media=\"(min-width:320px)\"/>\r\n<img src=\"img1.svg\" alt=\"MultiImage3\" title=\"SVG\" width=\"320\" height=\"200\" srcset=\"img1.svg 320w, img2.png 320w\" sizes=\"100vw\"/></picture>\r\n<figcaption>MultiImage3</figcaption></figure>\r\n",
		"<audio autoplay=\"autoplay\">\r\n<source src=\"audio1.mp3\" type=\"audio/mpeg\"/>\r\n<source src=\"audio2.wav\" type=\"audio/x-wav\"/>\r\n<source src=\"audio3.ogg\" type=\"audio/ogg\"/>\r\nMultiAudio1</audio>\r\n",
		"<video controls=\"controls\">\r\n<source src=\"video1.mp4\" type=\"video/mp4\"/>\r\n<source src=\"video2.webm\" type=\"video/webm\"/>\r\n<source src=\"video3.ogv\" type=\"video/ogg\"/>\r\n<source src=\"video4.3gp\" type=\"video/3gpp\"/>\r\n<source src=\"video5.flv\" type=\"video/x-flv\"/>\r\nMultiVideo1</video>\r\n",
		"<video controls=\"controls\" width=\"320\" height=\"200\">\r\n<source src=\"video1.mp4\" type=\"video/mp4\"/>\r\n<source src=\"video2.webm\" type=\"video/webm\"/>\r\n<source src=\"video3.ogv\" type=\"video/ogg\"/>\r\n<source src=\"video4.3gp\" type=\"video/3gpp\"/>\r\n<source src=\"video5.flv\" type=\"video/x-flv\"/>\r\nMultiVideo2</video>\r\n",
		"<figure><picture>\r\n<source srcset=\"img1.svg\" type=\"image/svg+xml\"/>\r\n<source srcset=\"img2.png\" type=\"image/png\"/>\r\n<img src=\"img1.svg\" alt=\"MultiImage1\" srcset=\"img1.svg, img2.png\" sizes=\"100vw\"/></picture>\r\n<figcaption>MultiImage1</figcaption></figure>\r\n",
		"MultiImage1",
		"<figure><picture>\r\n<source srcset=\"img1.svg\" type=\"image/svg+xml\" media=\"(min-width:320px)\"/>\r\n<source srcset=\"img2.png\" type=\"image/png\" media=\"(min-width:320px)\"/>\r\n<img src=\"img1.svg\" alt=\"MultiImage2\" width=\"320\" height=\"200\" srcset=\"img1.svg 320w, img2.png 320w\" sizes=\"100vw\"/></picture>\r\n<figcaption>MultiImage2</figcaption></figure>\r\n",
		"MultiImage2",
		"<figure><picture>\r\n<source srcset=\"img1.svg\" type=\"image/svg+xml\" media=\"(min-width:320px)\"/>\r\n<source srcset=\"img2.png\" type=\"image/png\" media=\"(min-width:320px)\"/>\r\n<img src=\"img1.svg\" alt=\"MultiImage3\" title=\"SVG\" width=\"320\" height=\"200\" srcset=\"img1.svg 320w, img2.png 320w\" sizes=\"100vw\"/></picture>\r\n<figcaption>MultiImage3</figcaption></figure>\r\n",
		"MultiImage3",
		"<audio autoplay=\"autoplay\">\r\n<source src=\"audio1.mp3\" type=\"audio/mpeg\"/>\r\n<source src=\"audio2.wav\" type=\"audio/x-wav\"/>\r\n<source src=\"audio3.ogg\" type=\"audio/ogg\"/>\r\nMultiAudio1</audio>\r\n",
		"MultiAudio1",
		"<video controls=\"controls\">\r\n<source src=\"video1.mp4\" type=\"video/mp4\"/>\r\n<source src=\"video2.webm\" type=\"video/webm\"/>\r\n<source src=\"video3.ogv\" type=\"video/ogg\"/>\r\n<source src=\"video4.3gp\" type=\"video/3gpp\"/>\r\n<source src=\"video5.flv\" type=\"video/x-flv\"/>\r\nMultiVideo1</video>\r\n",
		"MultiVideo1",
		"<video controls=\"controls\" width=\"320\" height=\"200\">\r\n<source src=\"video1.mp4\" type=\"video/mp4\"/>\r\n<source src=\"video2.webm\" type=\"video/webm\"/>\r\n<source src=\"video3.ogv\" type=\"video/ogg\"/>\r\n<source src=\"video4.3gp\" type=\"video/3gpp\"/>\r\n<source src=\"video5.flv\" type=\"video/x-flv\"/>\r\nMultiVideo2</video>\r\n",
		"MultiVideo2",
		"<iframe src=\"http://google.com/\" width=\"1200\" height=\"300\">Google</iframe>\r\n"];
	return Segments.join("");
}
