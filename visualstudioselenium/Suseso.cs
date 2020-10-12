using System;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Net;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using Microsoft.Data.Sqlite;

namespace suseso
{
    public class Suseso
    {
        private int aid;
        private string title;
        private string abstrac;
        private string name;
        private string theme;
        private string comment;
        private string linkedCirculars;
        private DateTime insertDate;
        private int status;
        private DateTime sentenceDate;
        private DataManager myDataManager;
        private string conStringSQLite = ConfigurationManager.ConnectionStrings["conStringSQLite"].ConnectionString;


        public Suseso(int iAID, string sTitle, string sAbstract, string sName,string sTheme,string sComment,string sLinkedCirculars, DateTime dtInsertDate, int iStatus, DateTime dtSentenceDate)
        {
            this.aid = iAID;
            this.title = sTitle;
            this.abstrac = sAbstract;
            this.name = sName;
            this.theme = sTheme;
            this.comment = sComment;
            this.linkedCirculars = sLinkedCirculars;
            this.insertDate = dtInsertDate;
            this.status = iStatus;
            this.sentenceDate = dtSentenceDate;
            this.myDataManager = new DataManager(conStringSQLite);

        }

        /// <summary>
        /// Add instance of suseso to SQLite Database.
        /// </summary>
        /// <returns> OK/Error </returns>
        public string Add()
        {
            try
            {
                string SQL = "INSERT INTO  'SUSESO' ('aid', 'title', 'abstract', 'name', 'insertDate', 'status', 'sentenceDate','id') VALUES (" +
                            this.aid + "," +
                            "'" + this.title + "'," +
                            "'" + this.abstrac + "'," +
                            "'" + this.name + "'," +
                            "'" + System.DateTime.Now +"',"+
                            "0," +
                            "'" + this.sentenceDate + "',"+
                            ""+ this.aid + ");";


                string sMsg = myDataManager.setData(SQL);
                if (sMsg == "ok")
                {
                    Console.WriteLine("El registro  \"{0}\" ingresado correctamente.", this.aid);
                    return "ok";
                }
                else
                {
                    return "error en el ingreso";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
                return "error";
            }
        }

        /// <summary>
        ///  validate AID
        ///  if the record exists, then it returns true
        /// </summary>
        /// <returns></returns>
        public bool validateAID()
        {
            DataTable dtTemp;

            string sSQL = "select AID from SUSESO where AID=" + this.aid + " LIMIT 1;";
            dtTemp = this.myDataManager.getData(sSQL);
            if (dtTemp.Rows.Count > 0)
            {
                if (dtTemp.Rows[0][0].ToString() != "")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

    }
}
