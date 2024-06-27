using Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using DocumentFormat.OpenXml.Packaging;
using System.IO.Compression;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Reflection.PortableExecutable;
using IronOcr;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using Google.Cloud.Translation.V2;
namespace Application.Services.Implementations
{
    public class FileProcessingService : IFileProcessingService
    {
        public async Task<IActionResult> ExtractTextImages(IFormFile file)
        {
            if (file == null)
            {
                return ("No file uploaded.").BadRequest();
            }
            if (file.ContentType.ToLower() == "application/pdf")
            {
                extractFromPdf(file);
                return null;
            }
            else if (file.ContentType.ToLower() == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                extractFromDocx(file);
                return null;
            }
            else
            {
                return "The file is not a supported format.Only PDF and DOCX files are supported.".BadRequest();
            }



        }
        public async void extractFromDocx(IFormFile file)
        {
            var saveDirectory = Directory.GetCurrentDirectory();
            var tempFilePath = Path.GetTempFileName();

            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            using (WordprocessingDocument doc = WordprocessingDocument.Open(tempFilePath, false))
            {
                // Extract text
                var paragraphs = doc.MainDocumentPart.Document.Body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();
                string extractedText = string.Join("\n", paragraphs.Select(p => p.InnerText));
                string textPath = Path.Combine(saveDirectory, "download/extractedText.txt");
                File.WriteAllText(textPath, extractedText);

                // Extract images
                var imageParts = doc.MainDocumentPart.ImageParts;
                int imageCounter = 0;
                foreach (var imagePart in imageParts)
                {
                    Stream stream = imagePart.GetStream();
                    string imageFileName = Path.Combine(saveDirectory, $"download/images/image{imageCounter++}.png");
                    using (FileStream fileStream = new FileStream(imageFileName, FileMode.Create))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }

            System.IO.File.Delete(tempFilePath);
        }
        public async void extractFromPdf(IFormFile file)
        {
            var saveDirectory = Directory.GetCurrentDirectory();
            // Save temp file
            var tempFilePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var pdfDocument = PdfDocument.FromFile(tempFilePath);
            // extract text from pdf
            var extractedText = pdfDocument.ExtractAllText();
            // Extract images from the PDF document
            var images = pdfDocument.ExtractAllImages();
            for (int i = 0; i < images.Count; i++)
            {
                //save image
                string imgPath = Path.Combine(saveDirectory, $"download/images/image{i}.png");

                string dirPath = Path.GetDirectoryName(imgPath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                images[i].SaveAs(imgPath);
            }

            string textPath = Path.Combine(saveDirectory, "download/extractedText.txt");
            string directoryPath = Path.GetDirectoryName(textPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(textPath, extractedText);


            System.IO.File.Delete(tempFilePath);

        }
        public async Task<FileStreamResult> DownloadFile()
        {
            var saveDirectory = Directory.GetCurrentDirectory();

            var filePath = Path.Combine(saveDirectory, "download");

            if (!Directory.Exists(filePath))
            {
                return null;
            }

            var zipPath = Path.Combine(saveDirectory, "folder.zip");

            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath); // delete the zip file if it already exists
            }

            ZipFile.CreateFromDirectory(filePath, zipPath); // create a zip file from the folder

            var memory = new MemoryStream();
            using (var stream = new FileStream(zipPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return new FileStreamResult(memory, "application/zip") { FileDownloadName = "download.zip" };
        }
        // Uppercae 
        public async Task<IActionResult> UpperCaseText(IFormFile file)
        {
            if (file == null)
            {
                return ("No file uploaded.").BadRequest();
            }
            if (file.ContentType.ToLower() == "application/pdf")
            {
                upperFromPdf(file);
                return "Done".Ok();
            }
            else if (file.ContentType.ToLower() == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                UpperFromDocx(file);
                return "Done".Ok();
            }
            else
            {
                return "The file is not a supported format.Only PDF and DOCX files are supported.".BadRequest();
            }
        }
        private async Task UpperFromDocx(IFormFile file)
        {
            var saveDirectory = Directory.GetCurrentDirectory();
            var tempFilePath = Path.GetTempFileName();

            // Save the uploaded file
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var outputPath = Path.Combine(saveDirectory, "output");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var outputFilePath = Path.Combine(outputPath, $"UPPERCASE_{file.FileName}");
            // Copy the original document to start modifications on the copy
            File.Copy(tempFilePath, outputFilePath, true);

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(outputFilePath, true))
            {
                var paragraphs = wordDoc.MainDocumentPart.Document.Body.Elements<Paragraph>();

                foreach (var para in paragraphs)
                {
                    foreach (var run in para.Elements<Run>())
                    {
                        // Check if the run has RunProperties
                        if (run.RunProperties != null)
                        {
                            // Here you could potentially read and store the existing formatting
                            // For simplicity, this example just focuses on converting text to uppercase
                        }

                        foreach (var text in run.Elements<DocumentFormat.OpenXml.Wordprocessing.Text>())
                        {
                            text.Text = text.Text.ToUpper(); // Convert the text to uppercase
                        }
                    }
                }

                wordDoc.MainDocumentPart.Document.Save(); // Save the changes to the document
            }

            // Cleanup: Optionally, delete the temporary file after processing
            File.Delete(tempFilePath);
        }
        private async Task upperFromPdf(IFormFile file)
        {
            var saveDirectory = Directory.GetCurrentDirectory();
            var uploadsFolderPath = Path.Combine(saveDirectory, "uploads");
            var outputPath = Path.Combine(saveDirectory, "output");

            if (!Directory.Exists(uploadsFolderPath))
                Directory.CreateDirectory(uploadsFolderPath);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var filePath = Path.Combine(uploadsFolderPath, file.FileName);

            // Save the uploaded file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Initialize a new PdfDocument
            var pdfDocument = PdfDocument.FromFile(filePath);
            var textContent = pdfDocument.ExtractAllText(); // Extracting all text

            // Convert the extracted text to uppercase
            var upperCaseText = textContent.ToUpper();

            // Create a new PDF with the uppercase text
            var pdfGenerator = new HtmlToPdf();
            var newPdf = pdfGenerator.RenderHtmlAsPdf($"<p>{upperCaseText}</p>");

            var newFilePath = Path.Combine(outputPath, $"UPPERCASE_{file.FileName}");
            newPdf.SaveAs(newFilePath);

            // Cleanup: Optionally, delete the uploaded file after processing
            File.Delete(filePath);
        }
        public async Task<IActionResult> ExtractFromPP(IFormFile file)
        {
            if (file == null)
            {
                return ("No file uploaded.").BadRequest();
            }
            if (file.ContentType.ToLower() == "application/vnd.openxmlformats-officedocument.presentationml.presentation")
            {
                ExtractFromPPTX(file);
                return "Done".Ok();
            }
            else
            {
                return "The file is not a supported format.Only PPTX files are supported.".BadRequest();
            }
        }
        public async Task<IActionResult> ExtractFromPPTX(IFormFile file)
        {
            try
            {
                var saveDirectory = Directory.GetCurrentDirectory();
                var tempFilePath = Path.GetTempFileName();

                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                using (PresentationDocument presentationDocument = PresentationDocument.Open(tempFilePath, true))
                {
                    PresentationPart presentationPart = presentationDocument.PresentationPart;

                    foreach (var slidePart in presentationPart.SlideParts)
                    {
                        // Extracting text from slides
                        var slideTexts = slidePart.Slide.Descendants<A.Text>().Select(t => t.Text).ToList();

                        // Extracting images from slides (demonstration purpose - you might need additional handling based on your requirements)
                        var imageParts = slidePart.ImageParts;
                        foreach (var imagePart in imageParts)
                        {
                            using (Stream imageStream = imagePart.GetStream())
                            {
                                // Handle image stream (e.g., save to file)
                            }
                        }

                        // Translate and reinsert text
                        foreach (var text in slideTexts)
                        {
                            string translatedText = await TranslateTextAsync(text, "vi"); // Placeholder method
                                                                                          // Assuming a method to append translated text to slides exists
                            AppendTranslatedTextToSlide(slidePart, text, translatedText);
                        }
                    }
                }

                File.Delete(tempFilePath);
            }
            catch (Exception ex) {

                Console.WriteLine($" error: {ex.Message}");
            }
            return new OkObjectResult("Processed PPTX file successfully.");
        }

        // Placeholder for a translation method
        private async Task<string> TranslateTextAsync(string inputText, string targetLanguage)
        {
            try
            {
                TranslationClient client = TranslationClient.Create();
                var response = await client.TranslateTextAsync(inputText, targetLanguage);
                return response.TranslatedText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Translation error: {ex.Message}");

                return inputText;
            }
        }


        // Method to append translated text to a slide
        private void AppendTranslatedTextToSlide(SlidePart slidePart, string originalText, string translatedText)
        {
            // This is a simplified version. You might need to adjust based on your slide layout and requirements.
            var slide = slidePart.Slide;
            var body = slide.CommonSlideData.ShapeTree.AppendChild(new Shape());

            var textBox = body.AppendChild(new TextBody());
            var paragraph = textBox.AppendChild(new A.Paragraph());

            // Adding original text
            var run = paragraph.AppendChild(new A.Run());
            run.AppendChild(new A.Text(originalText));

            // Adding a line break
            paragraph.AppendChild(new A.Break());

            // Adding translated text
            var runTranslated = paragraph.AppendChild(new A.Run());
            runTranslated.AppendChild(new A.Text(translatedText));

            // Set properties such as font size here if necessary
        }
    }
}
