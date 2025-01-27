// See https://aka.ms/new-console-template for more information

using PeachPDF;
using PeachPDF.Network;
using PeachPDF.PdfSharpCore;

var httpClient = new HttpClient();

PdfGenerateConfig pdfConfig = new()
{
    PageSize = PageSize.Letter,
    PageOrientation = PageOrientation.Portrait,
    MarginLeft = 0,
    MarginRight = 0,
    MarginTop = 0,
    MarginBottom = 0,
    DotsPerInch = 96,
    NetworkLoader = new HttpClientNetworkLoader(httpClient, new Uri("https://www.w3.org/Style/CSS/Test/CSS1/current/test11.htm"))
};

PdfGenerator generator = new();

var stream = new MemoryStream();

var document = await generator.GeneratePdf(null, pdfConfig);
document.Save(stream);

File.Delete("test11.pdf");
File.WriteAllBytes("test11.pdf", stream.ToArray());