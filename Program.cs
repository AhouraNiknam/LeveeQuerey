using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Program program = new Program();
            //This should return true
            bool test = await program.GetLeveeAsync(-121.67755, 38.08212, 5.00000);
            //This should return false
            //bool test = await program.GetLeveeAsync(-121.00000,37.50000, 0.05000);
            if (test)
            {
                Console.WriteLine("There is at least one Levee nearby");
            }
            else
            {
                Console.WriteLine("There are no Levees nearby");
            }
        }

        public async Task<bool> GetLeveeAsync(double minX, double minY, double offset)
        {

            #region Offset Conversion
            //Convert degrees into meters for the offset range
            var radius = 6378.137;
            var dLat = offset / radius;
            var dLon = offset / (radius * Math.Cos(Math.PI * minY / 180));

            var offsetLat = minY + dLat * 180 / Math.PI;
            var offsetLon = minX + dLon * 180 / Math.PI;

            #endregion

            #region API Call

            Rootobject json;

            string httpAddress = "https://gis.water.ca.gov/arcgis/rest/services/Boundaries/bam_viewer/MapServer/2/query?geometry=" +
            $"{minX}%2C{minY}%2C{offsetLon}%2C{offsetLat}&inSR=4326&spatialRel=esriSpatialRelCrosses&returnGeometry=false&f=pjson";

            var client = new HttpClient();
            var Uri = new Uri(httpAddress);
            HttpResponseMessage response = await client.GetAsync(Uri);

            if (response.IsSuccessStatusCode)
            {
                json = await response.Content.ReadFromJsonAsync<Rootobject>();

                int count = 0;
                foreach (var w in json.features)
                {
                    count++;
                }
                if (count >= 1)
                {
                    //At least one levee found
                    return true;
                }
                else
                {
                    //No levees found
                    return false;
                }
            }
            else
            {
                Console.WriteLine("failure");
                return false;
            }

            #endregion
        }

        #region JSON Classes

        public class Rootobject
        {
            public Feature[] features { get; set; }
        }

        public class Feature
        {
            public Attributes attributes { get; set; }
        }

        public class Attributes
        {
            public object FEATURE_NAME { get; set; }
        }

        #endregion
    }
}