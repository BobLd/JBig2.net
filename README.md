# JBig2.net

This is a C# library for decoding JBig2 images. This type of image is mostly encountered in PDF files.

## Usage

```
var dec = new JBIG2Decoder();

dec.decodeJBIG2(input_data);

if (dec.NumberOfPages > 0)
{
   JBIG2Bitmap page = dec.GetPage(0);
   byte[] data = page.getData(true);
   int w = page.getWidth();
   int h = page.getHeight();

   ...
}

```

## Requirements

While the project targets framework 4.8, it will work with older frameworks, possibly as far back as 1.0.

## About the code

This was originally written for [JPedal](https://github.com/Lonzak/OpenViewerFX/tree/master/src/main/java/org/jpedal/jbig2).
