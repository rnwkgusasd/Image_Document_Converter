using IDC.Models;
using ImageMagick;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PDFtoImage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

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

    public static void ConvertImageToPdf(IEnumerable<ConvertItemRow> itemList, string pdfPath)
    {
        using PdfDocument doc = new();

        doc.Info.Title = "Test";

        foreach (var item in itemList)
        {
            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            using var gfx = XGraphics.FromPdfPage(page);
            using var img = XImage.FromFile(item.FileInfo.FullName);

            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;

            double imgWidth = img.PointWidth;
            double imgHeight = img.PointHeight;

            // 1. 각도를 라디안으로 변환하여 회전 후 차지하는 가로/세로 폭 계산
            double rad = item.Angle * Math.PI / 180.0;
            double cos = Math.Abs(Math.Cos(rad));
            double sin = Math.Abs(Math.Sin(rad));

            // 회전된 상태에서의 가상 바운딩 박스 크기 비율을 고려한 스케일 계산
            double scaleX = pageWidth / (imgWidth * cos + imgHeight * sin);
            double scaleY = pageHeight / (imgWidth * sin + imgHeight * cos);

            // 전체 이미지가 다 보이게 하려면 Math.Min, 여백 없이 꽉 채우려면 Math.Max 사용
            double scale = Math.Min(scaleX, scaleY);

            // 스케일이 적용된 실제 이미지의 드로잉 크기
            double drawWidth = imgWidth * scale;
            double drawHeight = imgHeight * scale;

            // 2. 페이지 정중앙 좌표 계산
            double centerX = pageWidth / 2;
            double centerY = pageHeight / 2;

            // 3. 그래픽스 상태 저장
            XGraphicsState state = gfx.Save();

            // 4. 페이지 중심을 원점(0,0)으로 이동 후 회전 적용
            gfx.TranslateTransform(centerX, centerY);
            if (item.Angle != 0)
            {
                gfx.RotateTransform(item.Angle);
            }

            // 5. 중심을 기준으로 이미지 그리기
            gfx.DrawImage(img, -drawWidth / 2, -drawHeight / 2, drawWidth, drawHeight);

            // 6. 그래픽스 상태 복원
            gfx.Restore(state);
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
