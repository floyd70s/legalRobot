using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using Microsoft.Data.Sqlite;
namespace suseso
{
    public class Suseso
    {
        #region properties

        [JsonProperty("cid")]
        public int cid { get; set; }

        [JsonProperty("pid")]
        public string pid { get; set; }

        [JsonProperty("property-value_620_pvid")]
        public string property_value_620_pvid { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("property-value_532")]
        public string comment { get; set; }

        [JsonProperty("abstract")]
        public string abstrac { get; set; }

        [JsonProperty("property-value_620_name")]
        public string theme { get; set; }

        [JsonProperty("extended-property-value_pvid")]
        public string linkedCirculars { get; set; }

        [JsonProperty("property-value_620_pid")]
        public string property_value_620_pid { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("property-value_546_iso8601")]
        public DateTime sentenceDate { get; set; }

        [JsonProperty("binary_id")]
        public string binary_id { get; set; }

        [JsonProperty("aid")]
        public string aid { get; set; }

        [JsonProperty("title")]
        public string title { get; set; }

        [JsonProperty("iid")]
        public string iid { get; set; }

        [JsonProperty("score")]
        public string score { get; set; }

        [JsonProperty("property-value_525")]
        public string property_value_525 { get; set; }

        [JsonProperty("hl1")]
        public string hl1 { get; set; }

        [JsonProperty("using_cids")]
        public string using_cids { get; set; }

        public int _rol;
        public int rol {
            get {
                return _rol;
            }
            set
            {
                _rol = Convert.ToInt32(this.hl1.Substring(9));
            }
        }

        private string conStringSQLite = ConfigurationManager.ConnectionStrings["conStringSQLite"].ConnectionString;
        private DataManager _myDataManager;
        private DataManager myDataManager
        {
            get => _myDataManager;
            set => _myDataManager = new DataManager(conStringSQLite);
        }

        public Suseso()
        {
        }

        #endregion
        /// <summary>
        /// insert instance of suseso to SQLite Database.
        /// </summary>
        /// <returns> OK/Error </returns>
        public string Add()
        {
            try
            {
                this.myDataManager = new DataManager(this.conStringSQLite);
                string SQL = "INSERT INTO  'SUSESO' ('aid', 'title', 'abstract', 'name', 'insertDate', 'status', 'sentenceDate','rol') VALUES (" +
                            this.aid + "," +
                            "'" + this.title + "'," +
                            "'" + this.abstrac + "'," +
                            "'" + this.name + "'," +
                            "'" + System.DateTime.Now + "'," +
                            "0," +
                            "'" + this.sentenceDate.ToString("yyyy/MM/dd") + "'," +
                            "" + this.rol + ");";

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
            this.myDataManager = new DataManager(this.conStringSQLite);

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

        /// <summary>
        /// Obtain all records from suseso table with status "0" -->pending
        /// </summary>
        /// <returns></returns>
        public DataTable getAll()
        {
            this.myDataManager = new DataManager(this.conStringSQLite);
            string SQL = "select aid,title,abstract,name,insertDate,status,rol,sentenceDate from SUSESO where status=0";
            DataTable miDataTable = myDataManager.getData(SQL);
            return miDataTable;
        }
    }
}
