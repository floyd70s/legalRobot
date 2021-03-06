﻿using System;
using System.Configuration;
using System.Data;
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
                Console.Clear();
                Console.WriteLine("****************************************");
                Console.WriteLine(" Version 1.0.01  09-11-2020");
                Console.WriteLine("****************************************");
                int range = Convert.ToInt32(ConfigurationManager.AppSettings["range"]);     //365 days
                string PDFPath = ConfigurationManager.AppSettings["PDFPath"];               //Path to save PDF
                string iniDate = DateTime.Now.AddDays(-range).ToString("yyyy/MM/dd");      // initial search date
                string endDate = DateTime.Now.ToString("yyyy/MM/dd");                       // search end date
                int start = 0;                                                              // start for pagination
                int group = 0;                                                              // group for pagination 
                string jResult = "-";                                                       // string for save JSON 
                string sResult = "";                                                        // string for print messages  
                int iCountCycle = 0;                                                        // records saved in the current cycle
                int iGeneralCount = 0;                                                      // number of records saved
                int iMainCount = 0;                                                         // total number of records analyzed
                int iNotSaved = 0;                                                          // total number of records not saved
                int iPage = 0;                                                              // page
                int iCountJur = 0;                                                          // count register for JUR_ADMIN
                int iCountNoNewsJur = 0;                                                    // unsaved record count for JUR_ADMIN
                int iLimit = 100;
                JurAdmin miJurAdmin = new JurAdmin();                                       // new instance of JurAdmin
                Suseso miSuseso = new Suseso();                                             // new instance of SUSESO

                while (jResult != "" || start < iLimit)
                {
                    Console.WriteLine("****************************************");
                    Console.WriteLine(" Página {0} Lote:{1} ", iPage, start);
                    Console.WriteLine("****************************************");
                    iCountCycle = 0;

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

                    //-----------------------------------------------------------------------------------------------------------------------
                    // we get all the data
                    // we create a class that maps the structure of the json obtained from the suseso website --> suseso.cs
                    //-----------------------------------------------------------------------------------------------------------------------
                    string siteBase = miSuseso.extractWebSuseso(URL);
                    jResult = miSuseso.extractJson(siteBase);

                    //-----------------------------------------------------------------------------------------------------------------------
                    // iLimit--> total number of elements of the JSON service response
                    //-----------------------------------------------------------------------------------------------------------------------
                    iLimit = miSuseso.extractNumberOfElements(siteBase);

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
                            sResult = ElementSuseso.addElement();
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
                    //-----------------------------------------------------------------------------------------------------------------------
                    // This variable is used to keep track of the paging.
                    //-----------------------------------------------------------------------------------------------------------------------
                    start = start + 16;
                    iPage++;
                }



                DataTable miDataTableSuseso = miSuseso.getAll();                //get pending records from SUSESO - Status=0

                foreach (DataRow dtRow in miDataTableSuseso.Rows)
                {
                    string sAID = dtRow[0].ToString();
                    miSuseso.aid = sAID;
                    miJurAdmin.rol = dtRow[6].ToString();
                    bool bExistAIDJur = miJurAdmin.validateRol();

                    if (bExistAIDJur)
                    {
                        Console.WriteLine("EL REGISTRO {0} YA EXISTE EN JUR_ADMINISTRATIVA", dtRow[0].ToString());
                        miSuseso.update();
                        iCountNoNewsJur++;
                    }
                    else
                    {
                        Console.WriteLine("NO  EXISTE EL REGISTRO {0} EN JUR_ADMINISTRATIVA", dtRow[0].ToString());
                        miJurAdmin.sumario = dtRow[2].ToString();
                        miJurAdmin.fechaSentencia = Convert.ToDateTime(dtRow[7]);
                        miJurAdmin.titulo = dtRow[1].ToString();
                        miJurAdmin.rol = dtRow[6].ToString();
                        miJurAdmin.fechaRegistro = Convert.ToDateTime(dtRow[4]);
                        miJurAdmin.linkOrigen = dtRow[0].ToString() + "_archivo_01.pdf";
                        miJurAdmin.tipoDocumento = Convert.ToInt32(ConfigurationManager.AppSettings["DocumentType"]);
                        miJurAdmin.linkOrigen = miSuseso.savePdf(sAID);

                        string sLocalPath = PDFPath + "\\" + sAID + "_archivo_01.pdf";
                        miJurAdmin.textoSentencia = miSuseso.extractTextFromPDF(sLocalPath);
                        miJurAdmin.addElement();
                        miSuseso.update();

                        iCountJur++;
                    }
                }

                #region COMMENTS
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine("-- PASO 1                                                        ");
                Console.WriteLine("-- FIN DE LA OBTENCION DE DATOS                                  ");
                Console.WriteLine("-- A LAS " + System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
                Console.WriteLine("-- TOTAL DE REGISTROS REVISADOS :" + iMainCount);
                Console.WriteLine("-- TOTAL DE REGISTROS INGRESADOS PARA VALIDAR:" + iGeneralCount);
                Console.WriteLine("-- TOTAL DE REGISTROS NO INGRESADOS:" + iNotSaved);
                Console.WriteLine("-- PAGINAS RECORRIDAS :" + iCountCycle);
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine("-- PASO 2                                                        ");
                Console.WriteLine("-- SE CRUZAN LOS DATOS ENTRE LA BD SQLITE Y SQLSERVER            ");
                Console.WriteLine("-- SE GUARDAN LOS ARCHIVOS PDF                                   ");
                Console.WriteLine("-- SE GUARDA EN TXT EL CONTENIDO                                 ");
                Console.WriteLine("-- TOTAL DE REGISTROS REVISADOS :" + miDataTableSuseso.Rows.Count);
                Console.WriteLine("-- TOTAL DE REGISTROS INGRESADOS:" + iCountJur);
                Console.WriteLine("-- TOTAL DE REGISTROS NO INGRESADOS:" + iCountNoNewsJur);
                Console.WriteLine("-----------------------------------------------------------------");
                #endregion

                Email miEmail = new Email();
                miEmail.sendEmail(iMainCount, 0, iCountJur);

                Console.WriteLine("-- FIN DE LA EJECUCION " + DateTime.Now + "----");

                if (ConfigurationManager.AppSettings["Debug"] == "on")
                {
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........ERROR GENERAL");
            }
        }
    }
}

