﻿function CreateInnerHTML(ElementId, Args)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHTML(Args);
}

function CreateHTML(Args)
{
	var Segments = [
		"<p>",
		"Image: ",
		"<img src=\"/path/to/img.jpg\" alt=\"Alt text 1\"/>",
		"</p>\r\n",
		"<figure><img src=\"/path/to/img.jpg\" alt=\"Alt text 1\" class=\"aloneUnsized\"/><figcaption>Alt text 1</figcaption></figure>\r\n",
		"<figure><img src=\"/path/to/img.jpg\" alt=\"Alt text 2\" title=\"Optional title\" class=\"aloneUnsized\"/><figcaption>Alt text 2</figcaption></figure>\r\n",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 3\" title=\"Optional title attribute\" class=\"aloneUnsized\"/><figcaption>Alt text 3</figcaption></figure>\r\n",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 4\" title=\"Optional title attribute\" class=\"aloneUnsized\"/><figcaption>Alt text 4</figcaption></figure>\r\n",
		"<figure><img src=\"/path/to/img.jpg\" alt=\"Alt text 5\" width=\"320\" height=\"200\"/><figcaption>Alt text 5</figcaption></figure>\r\n",
		"<figure><img src=\"/path/to/img.jpg\" alt=\"Alt text 6\" title=\"Optional title 2\" width=\"320\" height=\"200\"/><figcaption>Alt text 6</figcaption></figure>\r\n",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 7\" title=\"Optional title attribute 2\" width=\"320\" height=\"200\"/><figcaption>Alt text 7</figcaption></figure>\r\n",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 8\" width=\"320\" height=\"200\"/><figcaption>Alt text 8</figcaption></figure>\r\n",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 9\" title=\"Optional title attribute 2\" width=\"320\" height=\"200\"/><figcaption>Alt text 9</figcaption></figure>\r\n",
		"<figure><img src=\"url/to/image\" alt=\"Alt text 10\" width=\"320\" height=\"200\"/><figcaption>Alt text 10</figcaption></figure>\r\n",
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
		"<figure><picture>\r\n<source srcset=\"img1.svg\" type=\"image/svg+xml\" media=\"(min-width:320px)\"/>\r\n<source srcset=\"img2.png\" type=\"image/png\" media=\"(min-width:320px)\"/>\r\n<img src=\"img1.svg\" alt=\"MultiImage2\" width=\"320\" height=\"200\" srcset=\"img1.svg 320w, img2.png 320w\" sizes=\"100vw\"/></picture>\r\n<figcaption>MultiImage2</figcaption></figure>\r\n",
		"<figure><picture>\r\n<source srcset=\"img1.svg\" type=\"image/svg+xml\" media=\"(min-width:320px)\"/>\r\n<source srcset=\"img2.png\" type=\"image/png\" media=\"(min-width:320px)\"/>\r\n<img src=\"img1.svg\" alt=\"MultiImage3\" title=\"SVG\" width=\"320\" height=\"200\" srcset=\"img1.svg 320w, img2.png 320w\" sizes=\"100vw\"/></picture>\r\n<figcaption>MultiImage3</figcaption></figure>\r\n",
		"<audio autoplay=\"autoplay\">\r\n<source src=\"audio1.mp3\" type=\"audio/mpeg\"/>\r\n<source src=\"audio2.wav\" type=\"audio/x-wav\"/>\r\n<source src=\"audio3.ogg\" type=\"audio/ogg\"/>\r\nMultiAudio1</audio>\r\n",
		"<video controls=\"controls\">\r\n<source src=\"video1.mp4\" type=\"video/mp4\"/>\r\n<source src=\"video2.webm\" type=\"video/webm\"/>\r\n<source src=\"video3.ogv\" type=\"video/ogg\"/>\r\n<source src=\"video4.3gp\" type=\"video/3gpp\"/>\r\n<source src=\"video5.flv\" type=\"video/x-flv\"/>\r\nMultiVideo1</video>\r\n",
		"<video controls=\"controls\" width=\"320\" height=\"200\">\r\n<source src=\"video1.mp4\" type=\"video/mp4\"/>\r\n<source src=\"video2.webm\" type=\"video/webm\"/>\r\n<source src=\"video3.ogv\" type=\"video/ogg\"/>\r\n<source src=\"video4.3gp\" type=\"video/3gpp\"/>\r\n<source src=\"video5.flv\" type=\"video/x-flv\"/>\r\nMultiVideo2</video>\r\n",
		"<iframe src=\"http://google.com/\" width=\"1200\" height=\"300\">Google</iframe>\r\n",
		"<figure><img class=\"aloneUnsized\" src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAMAAAD04JH5AAAAwFBMVEUAAADtTFztTFztTFz5+fkqX57tTFztTFztTFztTFz5+fkqX57tTFz5+fkqX57tTFztTFztTFztTFz5+fn5+fkqX54qX575+fn5+fkqX54qX575+fkqX54qX57tTFz5+fn5+fkqX575+fkqX575+fntTFztTFwqX575+fn5+fn5+fkqX54qX54qX57tTFz5+fkqX56Fosa4yd2ettE3aaTF0+Ls7/NEcqnS3Ojf5u5RfK94mcBehrWSrMxrj7qrv9fPtihIAAAALnRSTlMA74AQ7+9Aj89gEBCfgIAgv1DfQGBAz4/PYI+fIJ+vIN+/v99QcDBQcK8wMK9wikih8QAAA3lJREFUeAHslwVi61AMBD+8MjNjyrQyh3P/U5UhsIZaKncuoMmTtVL+/FKKX5Yup/f2huQO3OH296dOFt6n9uba1rJ0gS5WNxZ33rb68XpF+kAfuysHb1X9fH1IBsEgbuXMvvrE2rJQQFldHLctPz0kKSAFNzVuX54BvLkCL08EiILFyFckE2Syqw2HiQvJATmcjqvmfki0AnDlc2FiXfJBPislH2FzWWwEsFoqoJeGxEoArsS3uCbFQDEWXz38YiuAV0bCllgLYONt6gvMDXj9RkMtgA1N/31fL4Cp8t9/CIT5AjazsCSEBuAZCKBAHmwOCSECqhYCLjcTJ2j+JgCQGAhgNW8v8P3jAUBgIYCVnP0rlCoA1EwEkLmdJ4aEkeCepomAy2rChVAC3NMxEcBpsQn0Wi/UcE/cesEjAvpZrHQ/e4QMokQhsFswgj2k4mlaAEwV/ALrVVCqddEJuPGiO6gFQku6gdkT0BFs1tBHrW8YYfYE08IIA/QQhGIggCn6AJwXAxrHsHqCNUnBRxe+hQC/DJaFk6CHxEhgtb/+uRBIGnhGAjjjazhlEQJx/JgBegG+loeyO+CHoc97gJI4fgfwDsRtuaUdsx6gLAfFOlAD4IePmeCTswQ2PagIp/748+94fIQ6F9DtxE1JIUAQ9gVjYCSAnSIpFDWlj2ZkJbCY/2eUYyWwQWJQIaALQ/kIAdBjVCGgOE4vP0bghN0iCgHFVbL3MQL7n0dg6GMEnMUUChT8Cnwagc2PEnj+c3D1Qfx54lfgV+Cm3brAkSSGoQD6HSskRUUHKGGX+N//eCvqcQ/0QEG89A4QMkXoQnA30cX0+xwg0EXA3Y0ubrhLdJHwgi5gCh0UmEYHDabSQYUZ6WAEzMLuFjzK7C7j0czuZrwiPqPQZJ8ImI2dbXij+LRBU326kFFhR6J4J/j8RYyKzwOY4PMARsXnAUxweQCHmbjgmcQuEp5a2cGK51R8MtDM3f8B6D2WMz6nhZcqii+MwgvJiC8lpwo0tecvoG9LDvimxks0fFtz2t80p/1NcIq/qS75/ygJTyMJO4yFJykjdtHMU2TFXrPwMJlxgK48aFUckxYesCQcF4Q7ScApNMi+7RVn0SA+2xuthT9QquJ0WxZ+i+QNF5nzwi8secalxtoKnyitjugi3cI0CV/INIVbwh7//fcL7aUSs7ldhxUAAAAASUVORK5CYII=\" alt=\"PNG as a code block title=\"PNG as a code block\"/><figcaption>PNG as a code block</figcaption></figure>\n",
		"<pre><code class=\"xml\"><identityReview xmlns=\"urn:nf:iot:leg:id:1.0\" id=\"2f924a41-2664-df86-740c-6ae6f348b6a9@legal.neuron.kikkin.io\">\r\n\t<validatedClaim claim=\"ID\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"Account\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"Provider\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"State\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"Created\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"Updated\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"From\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"To\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"JID\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"EMAIL\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"PHONE\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"AGENT\" service=\"Waher.Service.IoTBroker.Legal.LegalComponent\" />\r\n\t<validatedClaim claim=\"FULLNAME\" service=\"TAG.Identity.Serpro.ServiceModule\" />\r\n\t<validatedClaim claim=\"PNR\" service=\"TAG.Identity.Serpro.ServiceModule\" />\r\n\t<validatedClaim claim=\"COUNTRY\" service=\"TAG.Identity.Serpro.ServiceModule\" />\r\n\t<validatedClaim claim=\"ADDR\" service=\"TAG.Identity.Serpro.ServiceModule\" />\r\n\t<validatedClaim claim=\"CITY\" service=\"TAG.Identity.Serpro.ServiceModule\" />\r\n\t<validatedClaim claim=\"REGION\" service=\"TAG.Identity.Serpro.ServiceModule\" />\r\n\t<validatedPhoto fileName=\"ProfilePhoto.jpeg\" service=\"TAG.Identity.Serpro.ServiceModule\" />\r\n</identityReview></code></pre>\r\n"];
	return Segments.join("");
}
