using PdfFileWriter;
using ZXing;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using PassKitSharp;
namespace PdfMaker
{
    /*
    public class PassJson
    {
        public string FormatVersion { get; set; }
        public string PassTypeIdentifier { get; set; }
        public Barcode Barcode { get; set; }
        public string ForegroundColor { get; set; }
        public string BackgroundColor { get; set; }
        public string LabelColor { get; set; }
        public EventTicket EventTicket { get; set; }
    }

    public class Barcode
    {
        public string Message { get; set; }
        public string AltText { get; set; }
        public string Format { get; set; }
        public string MessageEncoding { get; set; }

    }
    public class EventTicket
    {
        public List<Item> HeaderFields { get; set; }
        public List<Item> PrimaryFields { get; set; }
        public List<Item> SecondaryFields { get; set; }
        public List<Item> AuxiliaryFields { get; set; }
        public List<Item> BackFields { get; set; }
    }

    public class Item
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }

    */
    public class TicketPdfMaker
    {
        private PdfFont ArialNormal;
        private PdfFont ArialBold;
        private PdfFont ArialItalic;
        private PdfFont ArialBoldItalic;
        private PdfFont TimesNormal;
        private PdfFont Comic;
        private PdfTilingPattern WaterMark;
        private PdfDocument Document;
        private PdfPage Page;
        private PdfContents Contents;

        //private PassJson PdfPassJson;
        private PassKit pk;
        ////////////////////////////////////////////////////////////////////
        // Create article's example test PDF document
        ////////////////////////////////////////////////////////////////////

        public TicketPdfMaker(string pkpassfile)
        {
            pk = PassKit.Parse(pkpassfile);
            Console.WriteLine(pk.Barcode.Message);

        }
        /*
        public void ReadJson(String jsonfile)
        {
            using (StreamReader r = new StreamReader(jsonfile))
            {
                string json = r.ReadToEnd();

                PdfPassJson = JsonConvert.DeserializeObject<PassJson>(json);
                Console.WriteLine(PdfPassJson.EventTicket.HeaderFields[0].Key);
 
            }
            return;
        }
        */
        public void Test
                (
                Boolean Debug,
                String FileName
                )
        {
            Console.WriteLine("Start");
            // Step 1: Create empty document
            // Arguments: page width: 8.5”, page height: 11”, Unit of measure: inches
            // Return value: PdfDocument main class
            Document = new PdfDocument(PaperType.Letter, false, UnitOfMeasure.Inch, FileName);

            // for encryption test
            //		Document.SetEncryption(null, null, Permission.All & ~Permission.Print, EncryptionType.Aes128);

            // Debug property
            // By default it is set to false. Use it for debugging only.
            // If this flag is set, PDF objects will not be compressed, font and images will be replaced
            // by text place holder. You can view the file with a text editor but you cannot open it with PDF reader.
            Document.Debug = Debug;

            PdfInfo Info = PdfInfo.CreatePdfInfo(Document);
            Info.Title("TicketPassCard");
            Info.Author("blueskaie");
            Info.Keywords("PDF, .NET, C#, Library, Document Creator");
            Info.Subject("PDF File Writer C# Class Library (Version 1.14.1)");

            // Step 2: create resources
            // define font resources
            DefineFontResources();

            // define tiling pattern resources
            DefineTilingPatternResource();

            // Step 3: Add new page
            Page = new PdfPage(Document);

            // Step 4:Add contents to page
            Contents = new PdfContents(Page);

            // Step 5: add graphices and text contents to the contents object
            DrawFrameAndBackgroundWaterMark();
            
            DrawLogImage();
            //DrawTime();
            DrawStrip();
            DrawAuxiliaryFields();
            DrawBarcode();
            
            // Step 6: create pdf file
            Document.CreateFile();

            // start default PDF reader and display the file
            Process Proc = new Process();
            Proc.StartInfo = new ProcessStartInfo(FileName);
            Proc.Start();
            Console.WriteLine("end");
            Console.ReadLine();
            // exit
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Define Font Resources
        ////////////////////////////////////////////////////////////////////

        private void DefineFontResources()
        {
            // Define font resources
            // Arguments: PdfDocument class, font family name, font style, embed flag
            // Font style (must be: Regular, Bold, Italic or Bold | Italic) All other styles are invalid.
            // Embed font. If true, the font file will be embedded in the PDF file.
            // If false, the font will not be embedded
            String FontName1 = "Arial";
            String FontName2 = "Times New Roman";

            ArialNormal = PdfFont.CreatePdfFont(Document, FontName1, FontStyle.Regular, true);
            ArialBold = PdfFont.CreatePdfFont(Document, FontName1, FontStyle.Bold, true);
            ArialItalic = PdfFont.CreatePdfFont(Document, FontName1, FontStyle.Italic, true);
            ArialBoldItalic = PdfFont.CreatePdfFont(Document, FontName1, FontStyle.Bold | FontStyle.Italic, true);
            TimesNormal = PdfFont.CreatePdfFont(Document, FontName2, FontStyle.Regular, true);
            Comic = PdfFont.CreatePdfFont(Document, "Comic Sans MS", FontStyle.Bold, true);
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Define Tiling Pattern Resource
        ////////////////////////////////////////////////////////////////////

        private void DefineTilingPatternResource()
        {
            // create empty tiling pattern
            WaterMark = new PdfTilingPattern(Document);

            // the pattern will be PdfFileWriter laied out in brick pattern
            String Mark = "PdfFileWriter";

            // text width and height for Arial bold size 18 points
            Double FontSize = 18.0;
            Double TextWidth = ArialBold.TextWidth(FontSize, Mark);
            Double TextHeight = ArialBold.LineSpacing(FontSize);

            // text base line
            Double BaseLine = ArialBold.DescentPlusLeading(FontSize);

            // the overall pattern box (we add text height value as left and right text margin)
            Double BoxWidth = TextWidth + 2 * TextHeight;
            Double BoxHeight = 4 * TextHeight;
            WaterMark.SetTileBox(BoxWidth, BoxHeight);

            // save graphics state
            WaterMark.SaveGraphicsState();

            // fill the pattern box with background light blue color
            WaterMark.SetColorNonStroking(Color.FromArgb(230, 244, 255));
            WaterMark.DrawRectangle(0, 0, BoxWidth, BoxHeight, PaintOp.Fill);

            // set fill color for water mark text to white
            WaterMark.SetColorNonStroking(Color.White);

            // draw PdfFileWriter at the bottom center of the box
            WaterMark.DrawText(ArialBold, FontSize, BoxWidth / 2, BaseLine, TextJustify.Center, Mark);

            // adjust base line upward by half height
            BaseLine += BoxHeight / 2;

            // draw the right half of PdfFileWriter shifted left by half width
            WaterMark.DrawText(ArialBold, FontSize, 0.0, BaseLine, TextJustify.Center, Mark);

            // draw the left half of PdfFileWriter shifted right by half width
            WaterMark.DrawText(ArialBold, FontSize, BoxWidth, BaseLine, TextJustify.Center, Mark);

            // restore graphics state
            WaterMark.RestoreGraphicsState();
            return;
        }

        /*
        private Color GetColorfromRGBString(string colorString)
        {
            var r = 0;
            var g = 0;
            var b = 0;
            try
            {
                var cleanedString = colorString.Replace("RGB(", "");
                cleanedString = cleanedString.Replace(")", "");
                var splitStringArray = cleanedString.Split(',');
                r = int.Parse(splitStringArray[0]);
                g = int.Parse(splitStringArray[1]);
                b = int.Parse(splitStringArray[2]);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.Message);
            }

            return Color.FromArgb(r, g, b);
        }

        */


        private void DrawFrameAndBackgroundWaterMark()
        {
            // save graphics state
            Contents.SaveGraphicsState();
            Contents.SetColorNonStroking(Color.FromArgb(pk.BackgroundColor.Red, pk.BackgroundColor.Green, pk.BackgroundColor.Blue));
            Contents.DrawRectangle(0.0, 0.0, 8.5, 11, PaintOp.Fill);
            // restore graphics sate
            Contents.RestoreGraphicsState();
            return;
        }

        private void CreateLogoFile()
        {
            using (var imageFile = new FileStream("temps/logo.png", FileMode.Create))
            {
                imageFile.Write(pk.Logo.Data, 0, pk.Logo.Data.Length);
                imageFile.Flush();
            }
                return;
        }

        private void DrawLogImage()
        {
            // define local image resources
            // resolution 96 pixels per inch, image quality 100%
            PdfImageControl ImageControl = new PdfImageControl();
            ImageControl.Resolution = 96.0;
            ImageControl.ImageQuality = 100;

            CreateLogoFile();

            PdfImage Image1 = new PdfImage(Document, "temps/" + pk.Logo.Filename, ImageControl);
            
            // save graphics state
            Contents.SaveGraphicsState();

            // translate coordinate origin to the center of the picture
            Contents.Translate(0, 10);

            // adjust image size an preserve aspect ratio
            PdfRectangle NewSize = Image1.ImageSizePosition(3, 1.1, ContentAlignment.MiddleCenter);

            // clipping path
            Contents.DrawOval(NewSize.Left, NewSize.Bottom, NewSize.Width, NewSize.Height, PaintOp.Fill);

            // draw image
            Contents.DrawImage(Image1, NewSize.Left, NewSize.Bottom, NewSize.Width, NewSize.Height);

            // restore graphics state
            Contents.RestoreGraphicsState();
            return;
        }
        /*
        private Item FindItem(List<Item> source, string key) {
            
            foreach (Item item in source)
            {
                if (item.Key == key)
                {
                    return item;
                }
            }
            return null;
        }
        */
        /*
        private void DrawTime()
        {
            Item timeitem = FindItem(PdfPassJson.EventTicket.HeaderFields, "datetime");
            string time = timeitem?.Label;
            string date = timeitem?.Value;
            DrawProperty(time, date, TextJustify.Right, 7.9, 10.6);
            return;
        }
        */
        private void DrawProperty(string label, string value, TextJustify textalign, double posX, double posY)
        {
            // translate coordinate origin to the center of the picture
            Contents.SetColorNonStroking(Color.Black);
            
            Contents.DrawText(ArialNormal, 16.0, posX, posY, textalign, label);

            // save graphics state
            Contents.SaveGraphicsState();

            // change nonstroking (fill) color to purple
            Contents.SetColorNonStroking(Color.White);

            // Draw second line of heading text
            // arguments: Handwriting font, Font size 30 point, Position X=4.25", Y=9.0"
            // Text Justify: Center (text center will be at X position)
            Contents.DrawText(ArialNormal, 24.0, posX, posY-0.4, textalign, value);

            // restore graphics sate (non stroking color will be restored to default)
            Contents.RestoreGraphicsState();
            return;
        }

        private void CreateStripFile()
        {
            using (var imageFile = new FileStream("temps/strip.png", FileMode.Create))
            {
                imageFile.Write(pk.Strip.Data, 0, pk.Strip.Data.Length);
                imageFile.Flush();
            }
            return;
        }
        private void DrawStrip()
        {
            // define local image resources
            // resolution 96 pixels per inch, image quality 50%
            PdfImageControl ImageControl = new PdfImageControl();
            ImageControl.Resolution = 96.0;
            ImageControl.ImageQuality = 100;
            //		ImageControl.SaveAs = SaveImageAs.GrayImage;
            //		ImageControl.ReverseBW = true;
            CreateStripFile();
            PdfImage Image1 = new PdfImage(Document, "temps/strip.png", ImageControl);
            
            // save graphics state
            Contents.SaveGraphicsState();
            Contents.DrawImage(Image1, 0, 7.8, 8.5, 2.3);

            // restore graphics state
            Contents.RestoreGraphicsState();
            return;
        }
        private PKPassField FindPKPassField(PKPassFieldSet source, string key)
        {
            foreach (PKPassField item in source)
            {
                if (item.Key == key)
                {
                    return item;
                }
            }
            return null;
        }
        
        private void DrawAuxiliaryFields()
        {
            PKPassStringField eventtitle = (PKPassStringField)FindPKPassField(pk.SecondaryFields, "eventtitle");
            DrawProperty(eventtitle?.Label, eventtitle?.Value, TextJustify.Left, 0.5, 7.6);

            PKPassField section = (PKPassField)FindPKPassField(pk.AuxiliaryFields, "section");
            DrawProperty(section?.Label, "100", TextJustify.Left, 0.5, 6.6);
            
            PKPassNumberField row = (PKPassNumberField)FindPKPassField(pk.AuxiliaryFields, "row");
            DrawProperty(row?.Label, row?.Value.ToString(), TextJustify.Left, 1.8, 6.6);

            PKPassNumberField seat = (PKPassNumberField)FindPKPassField(pk.AuxiliaryFields, "seat");
            DrawProperty(seat?.Label, seat?.Value.ToString(), TextJustify.Left, 2.5, 6.6);

            PKPassStringField entryinfo = (PKPassStringField)FindPKPassField(pk.AuxiliaryFields, "entryinfo");
            DrawProperty(entryinfo?.Label, entryinfo?.Value, TextJustify.Right, 7.9, 6.6);
            /*
            string lavel;
            string value;

            Item eventtitle = FindItem(PdfPassJson.EventTicket.SecondaryFields, "eventtitle");
            lavel = eventtitle?.Label;
            value = eventtitle?.Value;
            DrawProperty(lavel, value, TextJustify.Left, 0.5, 7.6);

            Item section = FindItem(PdfPassJson.EventTicket.AuxiliaryFields, "section");
            lavel = section?.Label;
            value = section?.Value;
            DrawProperty(lavel, value, TextJustify.Left, 0.5, 6.6);

            Item row = FindItem(PdfPassJson.EventTicket.AuxiliaryFields, "row");
            lavel = row?.Label;
            value = row?.Value;
            DrawProperty(lavel, value, TextJustify.Left, 1.8, 6.6);

            Item seat = FindItem(PdfPassJson.EventTicket.AuxiliaryFields, "seat");
            lavel = seat?.Label;
            value = seat?.Value;
            DrawProperty(lavel, value, TextJustify.Left, 2.5, 6.6);

            Item entryinfo = FindItem(PdfPassJson.EventTicket.AuxiliaryFields, "entryinfo");
            lavel = entryinfo?.Label;
            value = entryinfo?.Value;
            DrawProperty(lavel, value, TextJustify.Right, 7.9, 6.6);
            */
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Draw Barcode
        ////////////////////////////////////////////////////////////////////

        private void DrawBarcode()
        {
            // save graphics state
            Contents.SaveGraphicsState();
            //string barcode = PdfPassJson.Barcode.Message;
            string barcode = pk.Barcode.Message;
            PdfQRCode QRCode = new PdfQRCode(Document, barcode, ErrorCorrection.M);
            Contents.DrawQRCode(QRCode, 1.75, 0.5, 5);
            Contents.DrawText(ArialNormal, 24, 4.25, 0.7, TextJustify.Center, barcode);

            // restore graphics sate
            Contents.RestoreGraphicsState();
            return;
        }
    }
}
