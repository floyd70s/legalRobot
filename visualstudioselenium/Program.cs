using System;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Pdfocr.Tesseract4;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Data;

namespace suseso
{
    class MainClass
    {
        private static readonly Tesseract4OcrEngineProperties tesseract4OcrEngineProperties = new Tesseract4OcrEngineProperties();

        public static void Main(string[] args)
        {


            string iniDate = "2019-09-29";//DateTime.Now.ToString("yyyy/MM/dd"); 
            string endDate = "2020-09-29";// DateTime.Now.AddDays(-365).ToString("yyyy/MM/dd");

            string URL = "https://suseso-engine.newtenberg.com/mod/find/cgi/find.cgi?action=jsonquery&" +
                         "engine=SwisheFind&rpp=16&" +
                         "cid=512&" +
                         "iid=612&" +
                         "pnid_search=&searchon=aid&properties=546,523,525,532,620&json=1&keywords=&" +
                         "pnid546_desde=" + iniDate +
                         "&pnid546_hasta=" + endDate +
                         "&start=0&group=1&show_ancestors=1&searchmode=and&pvid_and=500%3A510&aditional_query=%27%20cid%3D(512)%27%20-L%" +
                         "20property-value.546.iso8601%20" + iniDate +
                         "%20" + endDate +
                         "T23%3A59%3A59%20-s%20property-value.546.iso8601%20desc%20title%20desc&" +
                         "callback=jQuery20009428819093757522_1601343927812&_=1601343927813";

            string siteBase = extractWebSuseso(URL);

            extractinfoSuseso(siteBase);
            //string text = ExtractTextFromPDF("/Users/claudioperez/Documents/programacion/legalRobot/visualstudioselenium/PDFLoaded/R22-2019.pdf");
            Console.WriteLine("-----------------------------------------------------------------");

        }

        public static void extractinfoSuseso(string siteBase)
        {
            int iniJson = siteBase.IndexOf("[");
            int endJson = siteBase.IndexOf("]");

            string jResult = siteBase.Substring(iniJson, endJson - iniJson + 1);
            dynamic json = JsonConvert.DeserializeObject(jResult);

            DataTable dt = (DataTable)JsonConvert.DeserializeObject(jResult, typeof(DataTable));

            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                string sAid = dt.Rows[i]["aid"].ToString();
                string sFilePDF = "https://www.suseso.cl/612/articles-" + sAid + "_archivo_01.pdf";

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(sFilePDF, "/Users/claudioperez/Downloads/PDF/"+ sAid + "_archivo_01.pdf");
                    Console.WriteLine("Successfully Downloaded File \"{0}\" from \"{1}\"", sAid, sFilePDF);
                }
            }

            Console.WriteLine("----end");
        }


        public static string extractWebSuseso(string URL)
        {
            try
            {
                string docImportSrc = string.Empty;
                string infoBase = "";
                //se descarga el archivo el JSON con la info de suseso.
                using (WebClient webClient = new WebClient())
                {
                    docImportSrc = URL;
                    infoBase = webClient.DownloadString(URL);
                    //webClient.DownloadFile(docImportSrc,"");
                }
                Console.WriteLine("load website Ok");
                return infoBase;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
                //si no logra descargar no registra nada en la db
                return "error";
            }


        }

        public static string ExtractTextFromPDF(string filePath)
        {
            PdfReader pdfReader = new PdfReader(filePath);
            PdfDocument pdfDoc = new PdfDocument(pdfReader);
            string pageContent = "";
            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                Console.WriteLine(pageContent);
            }
            pdfDoc.Close();
            pdfReader.Close();
            return pageContent;
        }
    }
}

