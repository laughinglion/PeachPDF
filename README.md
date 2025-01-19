# PeachPDF
Peach PDF is a pure .NET HTML -> PDF rendering library. This library does not depend on Puppeter, wkhtmltopdf, or any other process to render the HTML to PDF. As a result, this should work in virtually any environment where .NET 8+ works. As a side benefit of being pure .NET, performance improvements in future .NET versions immediately benefit this library. 

## PeachPDF Requirements

- .NET 8

_Note: This package depends on PeachPDF.PdfSharpCore and various SixLabors libraries. Both have their own licenses, but the end result is still open source_

## Installing PeachPDF

Install the PeachPDF package from nuget.org

## Using PeachPDF

Simple example to render PDF 

```csharp
PdfGenerateConfig pdfConfig = new(){
  PageSize = PageSize.Letter,
  PageOrientation = PageOrientation.Portrait
};

var stream = new MemoryStream();

var document = PdfGenerator.GeneratePdf(html, pdfConfig);
document.Save(stream);
```
