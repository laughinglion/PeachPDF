// See https://aka.ms/new-console-template for more information

using PeachPDF;
using PeachPDF.Network;
using PeachPDF.PdfSharpCore;

var httpClient = new HttpClient();

PdfGenerateConfig pdfConfig = new()
{
    PageSize = PageSize.Letter,
    PageOrientation = PageOrientation.Portrait,
    NetworkLoader = new HttpClientNetworkLoader(httpClient, new Uri("http://www.example.com"))
};

PdfGenerator generator = new();

var stream = new MemoryStream();

var html = """
           <link href="https://fonts.googleapis.com/css2?family=Playwrite+AR+Guides&family=Roboto:ital,wght@0,100..900;1,100..900&display=swap" rel="stylesheet">
           <style type="text/css">
           p { font-family: "Playwrite AR Guides"; font-size: 48pt}
           </style>
           <p>Hello World!</p>
           """;

var document = await generator.GeneratePdf(html, pdfConfig);
document.Save(stream);

File.Delete("test_google_font.pdf");
File.WriteAllBytes("test_google_font.pdf", stream.ToArray());