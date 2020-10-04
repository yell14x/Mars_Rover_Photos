using Echovoice.JSON;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Mars_Rover_Photos.Models;

namespace Mars_Rover_Photos.Controllers
{
    public class HomeController : Controller
    {
        string Baseurl = "https://api.nasa.gov/";
        public async Task<ActionResult> Index()
        {
            string textfile = "";
            var path = @"C:\RoverDates.txt";
            if (System.IO.File.Exists(path))
            {
                using (TextReader tr = new StreamReader(path))
                {
                    textfile = tr.ReadLine();
                }
            }
            List<string> datelists = new List<string>();
            string dtform = "";
            string[] arraydates = textfile.Split(';');
            foreach (string dtval in arraydates)
            {
                try
                {
                    dtform = DateTime.Now.ToString(dtval);
                    DateTime.Parse(dtform);
                    DateTime newdate = Convert.ToDateTime(dtform);
                    string newdate_ = Convert.ToString(newdate.ToString("yyyy-MM-dd"));

                    datelists.Add(newdate_);
                }
                catch (Exception)
                {
                    datelists.Add(dtval + " is invalid date");
                }

            }

            List<ImageList> imageLists = new List<ImageList>();
            List<string> listBuffer = new List<string>();


            for (int ctr = 0; ctr <= 3; ctr++)
            {

                if (!datelists[ctr].Contains("is invalid date")) //checking which has a valid date on the text file
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(Baseurl);

                        client.DefaultRequestHeaders.Clear();
                        string dateval = "";
                        dateval = datelists[ctr];
                        string api_rq = "";
                        api_rq = "mars-photos/api/v1/rovers/curiosity/photos?earth_date=" + datelists[1] + "&api_key=DEMO_KEY";
                        //"mars-photos/api/v1/rovers/curiosity/photos?earth_date=2018-6-2&api_key=DEMO_KEY");
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage Res = await client.GetAsync(api_rq);

                        var EmpResponse = "";
                        DataSet dt;
                        if (Res.IsSuccessStatusCode)
                        {

                            EmpResponse = Res.Content.ReadAsStringAsync().Result;

                            JObject parsed = JObject.Parse(EmpResponse);
                            int i = 0;
                            string concatval = "";
                            string concatval1 = "";
                            string concatval2 = "";

                            string concatval3 = "";
                            foreach (var pair in parsed)
                            {
                                string output = string.Format("{0}{1}", pair.Key, pair.Value);
                                string[] delimiter = new string[] { "\r\n" };
                                string[] output_split = output.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var s in output_split)
                                {
                                    if (s.Contains("img_src"))
                                    {
                                        string del = "img_src";
                                        concatval = s.Replace(del, "");

                                        concatval1 = concatval.Replace(",", "");
                                        concatval2 = concatval1.Remove(0, 9);
                                        concatval3 = concatval2.Remove(concatval2.Length - 1, 1);


                                        listBuffer.Add(concatval3);
                                        imageLists.Add(new ImageList { img_src = concatval3 });

                                    }

                                }

                            }

                        }

                        else
                        {
                            //sample image incase of failed api 
                            // imageLists.Add(new ImageList { img_src = "https://www.nme.com/wp-content/uploads/2020/01/black-widow.jpg" });
                        }
                    }




                }
                else { imageLists.Add(new ImageList { img_src = datelists[ctr] }); }
            }

            return View(imageLists);
            //}

        }


        public FileResult Download(string ImageName)
        {

            string aURL = ImageName;//"http://1.bp.blogspot.com/-reA6gMCM5CI/UN7YFauH12I/AAAAAAAAAd0/J6LLN3HV72E/s828/stuff%2Bcopycc.jpg";
            Stream rtn = null;
            HttpWebRequest aRequest = (HttpWebRequest)WebRequest.Create(aURL);
            HttpWebResponse aResponse = (HttpWebResponse)aRequest.GetResponse();
            rtn = aResponse.GetResponseStream();
            return File(rtn, "image/jpeg/jpg/", ImageName);
        }


    }
}