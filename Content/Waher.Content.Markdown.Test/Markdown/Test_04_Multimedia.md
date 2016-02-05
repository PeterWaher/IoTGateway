Title: Test_04_Multimedia.md

Image: ![Alt text 1](/path/to/img.jpg)

![Alt text 1](/path/to/img.jpg)

![Alt text 2](/path/to/img.jpg "Optional title")

![Alt text 3][id]

![Alt text 4][id2]

[id]: url/to/image  "Optional title attribute"
[id2]: url/to/image  
	"Optional title attribute"

![Alt text 5](/path/to/img.jpg 320 200)

![Alt text 6](/path/to/img.jpg "Optional title 2" 320 200)

![Alt text 7][id3]

![Alt text 8][id4]

![Alt text 9][id5]

![Alt text 10][id6]

[id3]: url/to/image  "Optional title attribute 2" 320 200
[id4]: url/to/image  320 200
[id5]: url/to/image  
	"Optional title attribute 2" 320 200
[id6]: url/to/image  
	320 200

![Your browser does not support the audio tag](/local/music.mp3)

![Your browser does not support the video tag](/local/video.mp4 320 200)

![Your browser does not support the iframe tag](https://www.youtube.com/watch?v=whBPLc8m4SU 800 600)

![MultiImage1](img1.svg)(img2.png)

![MultiImage2](img1.svg 320 200)(img2.png 320 200)

![MultiImage3](img1.svg "SVG" 320 200)(img2.png "PNG" 320 200)

![MultiAudio1](audio1.mp3)(audio2.wav)(audio3.ogg)

![MultiVideo1](video1.mp4)(video2.webm)(video3.ogv)(video4.3gp)(video5.flv)

![MultiVideo2](video1.mp4 320 200)
	(video2.webm 320 200)
	(video3.ogv 320 200)
	(video4.3gp 320 200)
	(video5.flv 320 200)

![MultiImage1][mid1]

![MultiImage2][mid2]

![MultiImage3][mid3]

![MultiAudio1][mid4]

![MultiVideo1][mid5]

![MultiVideo2][mid6]

[mid1]: img1.svg img2.png
[mid2]: img1.svg 320 200 
		img2.png 320 200
[mid3]: img1.svg "SVG" 320 200
		img2.png "PNG" 320 200

[mid4]:	audio1.mp3 audio2.wav audio3.ogg
[mid5]: video1.mp4 video2.webm video3.ogv video4.3gp video5.flv

[mid6]: video1.mp4 320 200
	video2.webm 320 200
	video3.ogv 320 200
	video4.3gp 320 200
	video5.flv 320 200

![Google](http://google.com/ 1200 300)