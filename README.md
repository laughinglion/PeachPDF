# PeachPDF
Peach PDF is a pure .NET HTML -> PDF rendering library. This library does not depend on Puppeter, wkhtmltopdf, or any other process to render the HTML to PDF. As a result, this should work in virtually any environment where .NET 8+ works. As a side benefit of being pure .NET, performance improvements in future .NET versions immediately benefit this library. 

## PeachPDF Requirements

- .NET 8

_Note: This package depends on PeachPDF.PdfSharpCore and various SixLabors libraries. Both have their own licenses, but the end result is still open source_

## Installing PeachPDF

Install the PeachPDF package from nuget.org

## Using PeachPDF

### Simple example
Simple example to render PDF to a Stream. All images and assets must be local to the file on the file system or in data: URIs

```csharp
PdfGenerateConfig pdfConfig = new(){
  PageSize = PageSize.Letter,
  PageOrientation = PageOrientation.Portrait
};

PdfGenerator generator = new();

var stream = new MemoryStream();

var document = await generator.GeneratePdf(html, pdfConfig);
document.Save(stream);
```

### Rendering an MHTML file

You can generate PDF documents using self contained MHTML files (what Chrome calls "single page documents") by using the included MimeKitNetworkAdapter

```csharp
PdfGenerateConfig pdfConfig = new(){
  PageSize = PageSize.Letter,
  PageOrientation = PageOrientation.Portrait
  NetworkAdapter = new MimeKitNetworkAdapter(File.OpenRead("example.mhtml"))
};

PdfGenerator generator = new();

var stream = new MemoryStream();

// Passing null to GeneratePdf will load the HTML from the provided network adapter instance instead
var document = await generator.GeneratePdf(null, pdfConfig);
document.Save(stream);
```

### Rending HTML from a URI

You can also render HTML from the Internet to a PDF

_Note: a future version will be required in order to have the base URI automatically detected, so make sure the document has a <base href> tag set for images, styles, and links to resolve correctly_

```csharp
HttpClient httpClient = new();

PdfGenerateConfig pdfConfig = new(){
  PageSize = PageSize.Letter,
  PageOrientation = PageOrientation.Portrait
  NetworkAdapter = new HttpClientNetworkADapter(httpClient, "https://www.example.com")
};

PdfGenerator generator = new();

var stream = new MemoryStream();

// Passing null to GeneratePdf will load the HTML from the provided network adapter instance instead
var document = await generator.GeneratePdf(null, pdfConfig);
document.Save(stream);
```
