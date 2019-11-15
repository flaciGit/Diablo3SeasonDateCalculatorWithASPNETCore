using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {

        private IFetchHtml fetchHtml;
        private string url = "https://diablo.fandom.com/wiki/Season";


        private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
        public HomeController(IFetchHtml _fetchHtml)
        {
            fetchHtml = _fetchHtml;
        }

        public IActionResult Index()
        {
            // get all html content
            string htmlString = fetchHtml.GetHtmlByUrl(url);

            //  preprocess string
            htmlString = htmlString.Substring(htmlString.IndexOf("</tr>") + 5,
                htmlString.LastIndexOf("</td>") - htmlString.IndexOf("</tr>") + 5
                );
            htmlString = "<table>" + htmlString + "</table>";

            // convert to xml

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(htmlString);

            XmlNodeList items = xmlDoc.GetElementsByTagName("td");

            XElement table = XElement.Parse(htmlString);

            // feed values to string array
            string[] values = table.Descendants("td").Select(td => td.Value).ToArray();

            List<DateTime> dateArray = new List<DateTime>();
            List<string> dateArrayString = new List<string>();

            // parse string to datetime
            int i = 1;
            while (i < values.Length)
            {
                values[i].Trim();
                if (!values[i].Equals("") && !values[i].Equals("TBD"))
                {
                    string strippedData = values[i].Substring(0, values[i].IndexOf('['));
                    string format = "dd MMM yyyy";

                    try
                    {
                        dateArrayString.Add(strippedData);
                        dateArray.Add(DateTime.ParseExact(strippedData.Trim(), format, CultureInfo.InvariantCulture));
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Incorrect date format!");
                    }

                }
                // skip unwanted columns
                if (i % 2 == 0)
                {
                    i += 3;
                }
                else
                    i++;
            }

            // calculate season lengths (days)

            List<int> seasonLengthList = new List<int>(); // duration of the season
            List<int> seasonTimeBetweenLengthList = new List<int>(); // duration of the time between seasons

            for (int j = 0; j < dateArray.Count; j++)
            {
                if (j % 2 == 0 && j + 1 < dateArray.Count)
                    seasonLengthList.Add((dateArray[j + 1] - dateArray[j]).Days);
                else
                    if(j + 1 < dateArray.Count)
                        seasonTimeBetweenLengthList.Add((dateArray[j + 1] - dateArray[j]).Days);
            }

            // separate start and end dates to different arrays
            List<string> startDateArray = new List<string>();
            List<string> endDateArray = new List<string>();

            for (int j = 0; j < dateArray.Count; j++)
            {
                if (j % 2 == 0)
                    startDateArray.Add(dateArray[j].ToString("yyyy.MM.dd"));
                else
                    endDateArray.Add(dateArray[j].ToString("yyyy.MM.dd"));
            }


            // more statistics

            ViewBag.AverageSeasonLength = (int)seasonLengthList.Average();
            ViewBag.AverageBetweenLength = (int)seasonTimeBetweenLengthList.Average();

            if(endDateArray.Count == startDateArray.Count) // if the season did not start yet
            {
                ViewBag.CalculatedSeasonStart1 = dateArray[dateArray.Count-1].AddDays(5).ToString("yyyy.MM.dd"); // addition of 5 days (expected)
                ViewBag.CalculatedSeasonStart2 = dateArray[dateArray.Count-1].AddDays((int)seasonTimeBetweenLengthList.Average()).ToString("yyyy.MM.dd"); // addition of the average length

            }
            else
            {
                ViewBag.CalculatedSeasonEnd1 = dateArray[dateArray.Count-1].AddDays(90).ToString("yyyy.MM.dd"); // addition of 90 days (expected)
                ViewBag.CalculatedSeasonEnd2 = dateArray[dateArray.Count-1].AddDays((int)seasonLengthList.Average()).ToString("yyyy.MM.dd"); // addition of the average length

            }




            // pass data to view

            //ViewBag.HtmlContentResult = htmlString;
            //ViewBag.ContentArray = dateArray;
            //ViewBag.ContentArray = dateArrayString;
            ViewBag.StartDateArray = startDateArray;
            ViewBag.EndDateArray = endDateArray;

            ViewBag.DayArray = seasonLengthList;
            ViewBag.BetweenArray = seasonTimeBetweenLengthList;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
