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

            try
            {
                int range = Convert.ToInt32(ConfigurationManager.AppSettings["range"]);     //365 days
                string PDFPath = ConfigurationManager.AppSettings["PDFPath"];     //365 days

                string iniDate = DateTime.Now.AddDays(-365).ToString("yyyy/MM/dd"); ;       // DateTime.Now.AddDays(-365).ToString("yyyy/MM/dd"); 
                string endDate = DateTime.Now.ToString("yyyy/MM/dd");                       // DateTime.Now.ToString("yyyy/MM/dd"); 
                int start = 0;                                                              // start for pagination
                int group = 0;                                                              // group for pagination 
                string jResult = "-";                                                       // string for save JSON 
                string sResult = "";                                                        // string for print messages  
                int iCountCycle = 0;                                                        // records saved in the current cycle
                int iGeneralCount = 0;                                                      // number of records saved
                int iMainCount = 0;                                                         // total number of records analyzed
                int iNotSaved = 0;                                                          // total number of records not saved
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
                    //-----------------------------------------------------------------------------------------------------------------------

                    //-----------------------------------------------------------------------------------------------------------------------
                    // we get all the data
                    // we create a class that maps the structure of the json obtained from the suseso website --> suseso.cs
                    //-----------------------------------------------------------------------------------------------------------------------
                    var listElement = JsonConvert.DeserializeObject<List<Suseso>>(jResult);

                    //-----------------------------------------------------------------------------------------------------------------------
                    // we get all the data
                    //-----------------------------------------------------------------------------------------------------------------------
                    foreach (dynamic ElementSuseso in listElement)
                    {
                        //-----------------------------------------------------------------------------------------------------------------------
                        // get records from SQLite database suseso 
                        //-----------------------------------------------------------------------------------------------------------------------
                        bool bExistAID = ElementSuseso.validateAID();

                        if (bExistAID)
                        {
                            sResult = "El registro  \"{0}\" ya fue ingresado anteriormente." + ElementSuseso.aid;
                        }
                        else
                        {
                            sResult = ElementSuseso.Add();
                            if (sResult == "ok")
                            {
                                iGeneralCount++;
                                iCountCycle++;
                            }
                            else
                            {
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

                #region COMMENTS
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine("-- PASO 1                                                        ");
                Console.WriteLine("-- FIN DE LA OBTENCION DE DATOS                                  ");
                Console.WriteLine("-- A LAS " + System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
                Console.WriteLine("-- TOTAL DE REGISTROS REVISADOS :" + iMainCount);
                Console.WriteLine("-- TOTAL DE REGISTROS INGRESADOS:" + iGeneralCount);
                Console.WriteLine("-- TOTAL DE REGISTROS NO INGRESADOS:" + iNotSaved);
                Console.WriteLine("-- PAGINAS RECORRIDAS :" + iCountCycle);
                Console.WriteLine("-----------------------------------------------------------------");

                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine("-- PASO 2                                                        ");
                Console.WriteLine("-- SE CRUZAN LOS DATOS ENTRE LA BD SQLITE Y SQLSERVER            ");
                Console.WriteLine("-- SE GUARDAN LOS ARCHIVOS PDF                                   ");
                Console.WriteLine("-- SE GUARDA EN TXT EL CONTENIDO                                 ");
                Console.WriteLine("-----------------------------------------------------------------");
                #endregion
                JurAdmin miJurAdmin = new JurAdmin();
                Suseso miSuseso = new Suseso();

                DataTable miDataTableSuseso = miSuseso.getAll();

                foreach (DataRow dtRow in miDataTableSuseso.Rows)
                {
                    miJurAdmin.rol = dtRow[0].ToString();
                    bool bExistAIDJur = miJurAdmin.validateRol();

                    if (bExistAIDJur)
                    {
                        Console.WriteLine("EXISTE");
                    }
                    else
                    {
                        Console.WriteLine("NO  EXISTE EL REGISTRO {0} EN JUR_ADMINISTRATIVA", dtRow[0].ToString());
                        string sAID = dtRow[0].ToString();
                        miJurAdmin.sumario = dtRow[2].ToString();
                        miJurAdmin.fechaSentencia = Convert.ToDateTime(dtRow[7]);
                        miJurAdmin.titulo = dtRow[1].ToString();
                        miJurAdmin.rol = dtRow[0].ToString(); 
                        miJurAdmin.fechaRegistro = Convert.ToDateTime(dtRow[4]);
                        miJurAdmin.linkOrigen = dtRow[0].ToString() + "_archivo_01.pdf";
                        miJurAdmin.tipoDocumento = Convert.ToInt32(ConfigurationManager.AppSettings["DocumentType"]);

                        miJurAdmin.linkOrigen = savePdf(PDFPath, sAID);

                        string sLocalPath = PDFPath + sAID + "_archivo_01.pdf";
                        miJurAdmin.textoSentencia = "NO DISPONIBLE";// extractTextFromPDF(sLocalPath);
                        miJurAdmin.Add();

                    }
                }
                Console.WriteLine("-----fin de ejecucion "+DateTime.Now +"----");
            } 
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........ERROR GENERAL");
            }
        }

        /// <summary>
        /// save PDF from website SUSESO with AID
        /// </summary>
        /// <param name="PDFPath">path for save PDF file</param>
        /// <param name="sAid">unique ID for PDF file</param>
        /// <returns> string with link </returns>
        public static string  savePdf(string PDFPath, string sAid)
        {
            string sUrlPDF = "https://www.suseso.cl/612/articles-" + sAid + "_archivo_01.pdf";
            string sLocalPDF = PDFPath + sAid + "_archivo_01.pdf";

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile(sUrlPDF, sLocalPDF);
                return sUrlPDF;
            }
        }
        public static string extractWebSuseso(string URL)
        {
            try
            {
                string docImportSrc = string.Empty;
                string infoBase = "";
                
                using (WebClient webClient = new WebClient())       //get JSON from suseso
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

        /// <summary>
        /// Extract text from PDFFile
        /// </summary>
        /// <param name="filePath">local file path </param>
        /// <returns>string with the content of PDF file</returns>
        public static string extractTextFromPDF(string filePath)
        {
            PdfReader pdfReader = new PdfReader(filePath);
            PdfDocument pdfDoc = new PdfDocument(pdfReader);
            string pageContent = "";
            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
            }
            pdfDoc.Close();
            pdfReader.Close();
            return pageContent;
        }
    }
}

