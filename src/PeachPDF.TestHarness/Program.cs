// See https://aka.ms/new-console-template for more information
using PeachPDF;
using PeachPDF.PdfSharpCore;

var html = File.ReadAllText("acid2.html");

PdfGenerateConfig pdfConfig = new()
{
    PageSize = PageSize.Letter,
    PageOrientation = PageOrientation.Portrait
};

var stream = new MemoryStream();

var document = PdfGenerator.GeneratePdf(html, pdfConfig);
document.Save(stream);

File.Delete("acid2.pdf");
File.WriteAllBytes("acid2.pdf", stream.ToArray());