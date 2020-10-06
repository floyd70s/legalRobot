using System;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Pdfocr.Tesseract4;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Microsoft.Data.Sqlite;

using System.Text;

namespace suseso
{
    class MainClass
    {

        public static void Main(string[] args)
        {


            string conStringSQLite = ConfigurationManager.ConnectionStrings["conStringSQLite"].ConnectionString;
            string PJRobotSQlite = ConfigurationManager.AppSettings["PJRobotSQlite"];
            string iniDate = "2019-09-29";//DateTime.Now.ToString("yyyy/MM/dd"); 
            string endDate = "2020-09-29";// DateTime.Now.AddDays(-365).ToString("yyyy/MM/dd");
            int start = 0;
            int group = 0;
            string jResult = "-";

            while (jResult != "")
            {
                string URL = "https://suseso-engine.newtenberg.com/mod/find/cgi/find.cgi?action=jsonquery&" +
                             "engine=SwisheFind&rpp=16&" +
                             "cid=512&" +
                             "iid=612&" +
                             "pnid_search=&searchon=aid&properties=546,523,525,532,620&json=1&keywords=&" +
                             "pnid546_desde=" + iniDate +
                             "&pnid546_hasta=" + endDate +
                             "&start=0" + start +
                             "&group=" + group +
                             "&show_ancestors=1&searchmode=and&pvid_and=500%3A510&aditional_query=%27%20cid%3D(512)%27%20-L%" +
                             "20property-value.546.iso8601%20" + iniDate +
                             "%20" + endDate +
                             "T23%3A59%3A59%20-s%20property-value.546.iso8601%20desc%20title%20desc&" +
                             "callback=jQuery20009428819093757522_1601343927812&_=1601343927813";

                string siteBase = extractWebSuseso(URL);
                int iniJson = siteBase.IndexOf("[");
                int endJson = siteBase.IndexOf("]");

                jResult = siteBase.Substring(iniJson, endJson - iniJson + 1);
                DataTable dtResult = extractInfoSuseso(jResult);
                DataTable dtJurAdministrativa = GetData(PJRobotSQlite, "select * from planilla");
                group++;
            }

            Console.WriteLine("-----------------------------------------------------------------");

        }

        /// <summary>
        /// this function transform a JSON and return datatable
        /// </summary>
        /// <param name="jResult">JSON with results from website</param>
        /// <returns>return datatable with results from website</returns>
        public static DataTable extractInfoSuseso(string jResult)
        {
            DataTable dt = (DataTable)JsonConvert.DeserializeObject(jResult, typeof(DataTable));
            return dt;
        }


        /// <summary>
        /// generic function for execute SQL command with SQLite BDDriver.
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <param name="connectionString"></param>
        /// <returns>return DataTable with result</returns>
        public static DataTable GetData(string connectionString, string query)
        {
            DataTable dt = new DataTable();
            Microsoft.Data.Sqlite.SqliteConnection connection;
            Microsoft.Data.Sqlite.SqliteCommand command;

            connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source= /Users/claudioperez/Documents/programacion/legalRobot/visualstudioselenium/BD/PJRobots.sqlite");
            try
            {
                connection.Open();
                command = new Microsoft.Data.Sqlite.SqliteCommand(query, connection);
                dt.Load(command.ExecuteReader());
                connection.Close();
            }
            catch
            {
            }

            return dt;
        }






















        /// <summary>
        /// save PDF from website SUSESO with AID
        /// </summary>
        /// <param name="PDFPath">path for save PDF file</param>
        /// <param name="sAid">unique ID for PDF file</param>
        public static void savePdf(string PDFPath, string sAid)
        {

            string sFilePDF = "https://www.suseso.cl/612/articles-" + sAid + "_archivo_01.pdf";

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile(sFilePDF, PDFPath + sAid + "_archivo_01.pdf");
                Console.WriteLine("Successfully Downloaded File \"{0}\" from \"{1}\"", sAid, sFilePDF);
            }
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

