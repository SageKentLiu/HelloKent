using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Net;
using System.Web.Mvc;
using TestHelloKent.Models;
using System.Web.Script.Serialization;
using System.Text;


namespace TestHelloKent.Controllers
{
    public class HomeController : Controller
    {
        List<string> URLs;
        List<string> ProperTypes;
        List<string> DeprMethods;
        List<string> DeprPcts;
        List<string> Conventions;

        string localURL1 = "http://127.0.0.1:81/api/depreciation/CalcProjection";
        string localURL2 = "http://127.0.0.1:81/api/depreciation/CalcDepreciation";
        string localURL3 = "http://127.0.0.1:81/api/depreciation/Calc168KAmount";
        string localURL4 = "http://127.0.0.1:81/api/depreciation/CalcFullCostBasis";
        string cloudURL1 = "http://hellokent.cloudapp.net/api/depreciation/CalcProjection";
        string cloudURL2 = "http://hellokent.cloudapp.net/api/depreciation/CalcDepreciation";
        string cloudURL3 = "http://hellokent.cloudapp.net/api/depreciation/Calc168KAmount";
        string cloudURL4 = "http://hellokent.cloudapp.net/api/depreciation/CalcFullCostBasis";

        public HomeController()
        {
            URLs = new List<string>()
            {
                localURL1, localURL2, localURL3, localURL4, 
                cloudURL1, cloudURL2, cloudURL3, cloudURL4
            };
            ProperTypes = new List<string>()
            {
                "P", "R"
            };
            DeprMethods = new List<string>()
            {
                "SL", "MF", "MI", "MT", "AD", "AT", "SA", "ST", "SL", "SF", "SH", "SD", "DC", "DI", "DE", "DB", "DH", "DD", "YS", "YH", "YD", "RV", "OC", "NO", "MA", "MB", "MR", "AA", "SB",  "DBs", "DBn"
            };
            DeprPcts = new List<string>()
            {
                "0", "100", "150", "200"
            };
            Conventions = new List<string>()
            {
                "MM", "MMM", "FM","NM", "MQ", "HY", "FY", "MHY"
            };

            DeprScheduleItemForView model = new DeprScheduleItemForView
            {
                PropertyType = "P",
                PlaceInServiceDate = new DateTime(2000, 1, 1),
                AcquisitionValue = 2000.00m,
                DepreciationMethod = "SL",
                DepreciationPercent = "0",
                EstimatedLife = 10,
                Convention = "MMM",
            };

            ViewBag.URLList = new SelectList(URLs, model.URL);
            ViewBag.ProperTypeList = new SelectList(ProperTypes, model.PropertyType);
            ViewBag.DeprMethodList = new SelectList(DeprMethods, model.DepreciationMethod);
            ViewBag.DeprPctList = new SelectList(DeprPcts, model.DepreciationPercent);
            ViewBag.ConventionList = new SelectList(Conventions, model.Convention);
        }

        [HttpPost]
        public ActionResult Depreciate(DeprScheduleItemForView deprBook)
        {
            var deprbook = new
            {
                PropertyType = deprBook.PropertyType,
                PlaceInServiceDate = deprBook.PlaceInServiceDate.Date,
                AcquiredValue = deprBook.AcquisitionValue,
                DepreciateMethod = deprBook.DepreciationMethod,
                DepreciatePercent = deprBook.DepreciationPercent,
                EstimatedLife = deprBook.EstimatedLife,
                Section179 = deprBook.Section179,
                Bonus911Percent = deprBook.Bonus911Percent,
                SalvageDeduction = deprBook.SalvageDeduction,
                ITCAmount = deprBook.ITCAmount,
                ITCReduce = deprBook.ITCReduce,
                Convention = deprBook.Convention,
                RunDate= deprBook.RunDate
            };

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string jsonstring = serializer.Serialize(deprbook);
            string content = jsonstring;
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] data = encoding.GetBytes(content);

            string url = deprBook.URL;
            //string url = localURL;
            // string url = cloudURL;
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);

            // set method as post
            webrequest.Method = "POST";
            // set content type
            webrequest.ContentType = "application/json";
            //webrequest.Accept = "text/xml";
            // set content length
            webrequest.ContentLength = data.Length;
            // get stream data out of webrequest object
            Stream newStream = webrequest.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            // declare & read response from service
            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();

            // set utf8 encoding
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            // read response stream from response object
            // read string from stream data
            // close the stream object

            StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc);
            string strResult = loResponseStream.ReadToEnd();
            loResponseStream.Close();


            //Stream strm = webresponse.GetResponseStream();
            //DataContractJsonSerializer deser = new DataContractJsonSerializer(typeof(IEnumerable<PeriodDeprItemModel>));
            //IEnumerable<PeriodDeprItemModel> items = (deser.ReadObject(strm)) as IEnumerable<PeriodDeprItemModel>;

            try
            {
                IEnumerable<PeriodDeprItem> items = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<PeriodDeprItem>>(strResult);
                ViewBag.ResultList = items;
                ViewBag.Title = "Projection Report";
            }
            catch
            {
                if (url.Contains("CalcDepreciation"))
                {
                    PeriodDeprItem items = Newtonsoft.Json.JsonConvert.DeserializeObject<PeriodDeprItem>(strResult);
                    ViewBag.ResultList = items;
                    ViewBag.Title = "Depreciation";
                }
                else if (url.Contains("Calc168KAmount"))
                {
                    ViewBag.Result = Convert.ToDecimal(strResult);
                    ViewBag.Title = "168K Amount";
                }
                else
                {
                    ViewBag.Result = Convert.ToDecimal(strResult);
                    ViewBag.Title = "Reduced Cost Basis";
                }
            }

            webresponse.Close();

            return View();
        }

        

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
