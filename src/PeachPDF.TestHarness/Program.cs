// See https://aka.ms/new-console-template for more information

using PeachPDF;
using PeachPDF.Network;
using PeachPDF.PdfSharpCore;
using PeachPDF.PdfSharpCore.Pdf;

HttpClient httpClient = new();

PdfGenerateConfig pdfConfig = new()
{
    PageSize = PageSize.Letter,
    PageOrientation = PageOrientation.Portrait,
    MarginTop = 0,
    MarginBottom = 0,
    MarginLeft = 0,
    MarginRight = 0,
    DotsPerInch = 96,
    NetworkLoader = new HttpClientNetworkLoader(httpClient, new Uri("https://www.w3.org/Style/CSS/Test/CSS1/current/test5526c.htm")),
};

PdfGenerator generator = new();

var stream = new MemoryStream();

PdfDocument document;
document = await generator.GeneratePdf(null, pdfConfig);
document.Save(stream);

File.Delete("example.pdf");
File.WriteAllBytes("example.pdf", stream.ToArray());

/*
pdfConfig = new()
{
    PageSize = PageSize.Letter,
    PageOrientation = PageOrientation.Portrait,
};

stream = new MemoryStream();

var html = await File.ReadAllTextAsync("acid2.html");
document = await generator.GeneratePdf(html, pdfConfig);
document.Save(stream);

File.Delete("4uiwmf4z.pdf");
await File.WriteAllBytesAsync("4uiwmf4z.pdf", stream.ToArray());
*/
