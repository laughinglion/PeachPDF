// See https://aka.ms/new-console-template for more information
using PeachPDF;
using PeachPDF.PdfSharpCore;

var html = File.ReadAllText("rfc2324.html");

PdfGenerateConfig pdfConfig = new()
{
    PageSize = PageSize.Letter,
    PageOrientation = PageOrientation.Portrait
};

var fontStream = File.OpenRead("LiberationMono-Regular.ttf");

var stream = new MemoryStream();

PdfGenerator generator = new();
generator.AddFontFromStream(fontStream);
generator.AddFontFamilyMapping("monospace","Liberation Mono");

var document = await generator.GeneratePdf(html, pdfConfig);
document.Save(stream);

File.Delete("rfc2324.pdf");
File.WriteAllBytes("rfc2324.pdf", stream.ToArray());