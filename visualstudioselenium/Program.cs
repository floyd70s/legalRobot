using System;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Net;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using Microsoft.Data.Sqlite;
using System.Text;
using suseso;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;


namespace suseso
{
    class MainClass
    {

        public static void Main(string[] args)
        {

            int range = Convert.ToInt32(ConfigurationManager.AppSettings["range"]);   //365 days

            string iniDate = "2019-09-29";                  // DateTime.Now.ToString("yyyy/MM/dd"); 
            string endDate = "2020-09-29";                  // DateTime.Now.AddDays(-365).ToString("yyyy/MM/dd");
            int start = 0;                                  // start for pagination
            int group = 0;                                  // group for pagination 
            string jResult = "-";                            // string for save JSON 
            string sResult = "";                            // string for print messages  
            int iCountCycle = 0;                            // records saved in the current cycle
            int iGeneralCount = 0;                          // number of records saved
            int iMainCount = 0;                             // total number of records analyzed
            int iNotSaved = 0;                              // total number of records not saved
            while (jResult != "")
            {
                iCountCycle = 0;

                Console.WriteLine("****************************************");
                Console.WriteLine(" Ciclo {0}", group);
                Console.WriteLine("****************************************");

                //-----------------------------------------------------------------------------------------------------------------------
                // get info from website SUSESO
                //-----------------------------------------------------------------------------------------------------------------------
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
                DataTable dtResult = extractInfoSuseso(jResult);  //--> datatable with the JSON data
                //-----------------------------------------------------------------------------------------------------------------------

                //-----------------------------------------------------------------------------------------------------------------------
                // we get all the data
                //-----------------------------------------------------------------------------------------------------------------------
                var listProduct = JsonConvert.DeserializeObject<List<ResponseSuseso>>(jResult);

                //-----------------------------------------------------------------------------------------------------------------------
                // we get all the data
                //-----------------------------------------------------------------------------------------------------------------------
                foreach (dynamic prod in listProduct)
                {
                    int aid = Convert.ToInt32(prod.aid.ToString());
                    string title = prod.title;
                    string abstrac = prod.abstrac;
                    string name = prod.name;
                    string theme = prod.theme;
                    string comment = prod.comment;
                    string linkedCirculars = prod.linkedCirculars;
                    DateTime insertDate = System.DateTime.Now;
                    int status = Convert.ToInt32(ConfigurationManager.AppSettings["statusByDefault"]); ;
                    DateTime sentenceDate = Convert.ToDateTime(prod.sentenceDate);

                    Suseso mySuseso = new Suseso(aid, title, abstrac, name, theme, comment, linkedCirculars, insertDate, status, sentenceDate);

                    //-----------------------------------------------------------------------------------------------------------------------
                    // get records from SQLite database suseso 
                    //-----------------------------------------------------------------------------------------------------------------------
                    bool bExistAID = mySuseso.validateAID();

                    if (bExistAID)
                    {
                        sResult = "El registro  \"{0}\" ya fue ingresado anteriormente." + aid;
                    }
                    else
                    {
                        sResult = mySuseso.Add();
                        if (sResult == "ok")
                        {
                            iGeneralCount++;
                            iCountCycle++;
                        }
                        else{
                            iNotSaved++;
                        }
                    }
                    iMainCount++;
                }

                //-----------------------------------------------------------------------------------------------------------------------
                // if no new records were entered in the loop, the initial loop is terminated.
                //-----------------------------------------------------------------------------------------------------------------------
                if (iCountCycle == 0)
                {
                    jResult = "";
                }
                else
                {
                    jResult = "----";
                }

                group++;
            }

            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("-- PASO 1                                                        ");
            Console.WriteLine("-- FIN DE LA OBTENCION DE DATOS                                  ");
            Console.WriteLine("-- A LAS " + System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
            Console.WriteLine("-- TOTAL DE REGISTROS REVISADOS :" + iMainCount);
            Console.WriteLine("-- TOTAL DE REGISTROS INGRESADOS:" + iGeneralCount);
            Console.WriteLine("-- TOTAL DE REGISTROS NO INGRESADOS:" + iNotSaved);
            Console.WriteLine("-- PAGINAS RECORRIDAS :" + iCountCycle);
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
                //get JSON from suseso
                using (WebClient webClient = new WebClient())
                {
                    docImportSrc = URL;
                    infoBase = webClient.DownloadString(URL);
                }
                Console.WriteLine("load website Ok");
                return infoBase;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
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

