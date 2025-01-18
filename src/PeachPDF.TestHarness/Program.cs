// See https://aka.ms/new-console-template for more information
using PdfSharpCore;
using PeachPDF;

var html = File.ReadAllText("rfc2324.html");

PdfGenerateConfig pdfConfig = new()
{
    PageSize = PageSize.Letter,
    PageOrientation = PageOrientation.Portrait
};

var stream = new MemoryStream();

var document = PdfGenerator.GeneratePdf(html, pdfConfig);
document.Save(stream);

File.Delete("rfc2324.pdf");
File.WriteAllBytes("rfc2324.pdf", stream.ToArray());