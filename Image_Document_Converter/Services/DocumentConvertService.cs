using ImageMagick;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PDFtoImage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace IDC.Services;

public static class DocumentConvertService
{
    public static void ConvertImageToPdf(IEnumerable<FileInfo> imageList, string pdfPath)
    {
        using PdfDocument doc = new();

        doc.Info.Title = "Test";

        foreach (var imageFile in imageList)
        {
            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            using var gfx = XGraphics.FromPdfPage(page);
            using var img = XImage.FromFile(imageFile.FullName);

            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;

            double imgWidth = img.PointWidth;
            double imgHeight = img.PointHeight;

            double scale = Math.Min(pageWidth / imgWidth, pageHeight / imgHeight);
            double drawWidth = imgWidth * scale;
            double drawHeight = imgHeight * scale;

            double x = (pageWidth - drawWidth) / 2;
            double y = (pageHeight - drawHeight) / 2;

            gfx.DrawImage(img, x, y, drawWidth, drawHeight);
        }

        doc.Save(pdfPath);
    }

    public static void ConvertPdfToImage(FileInfo pdfFile)
    {
        byte[] pdfBytes = File.ReadAllBytes(pdfFile.FullName);
        int pageIndex = 0;

        foreach(var image in Conversion.ToImages(pdfBytes))
        {
            pageIndex++;
            string outputPath = $@"{pdfFile.DirectoryName}\{pdfFile.Name.Replace(pdfFile.Extension, "")}_{pageIndex}.jpg";

            using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 90);
            using var stream = File.OpenWrite(outputPath);

            data.SaveTo(stream);

            image.Dispose();
        }
    }

    public static void ConvertHeicToJpg(IEnumerable<FileInfo> imageList)
    {
        foreach(var imageFile in imageList)
        {
            using var image = new MagickImage(imageFile.FullName);

            image.Format = MagickFormat.Jpeg;
            image.Quality = 90;
            image.Write($@"{imageFile.DirectoryName}\{imageFile.Name.Replace(imageFile.Extension, "")}.jpg");
        }
    }
}
