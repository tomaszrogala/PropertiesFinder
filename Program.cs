using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;


namespace ConsoleApplication3
{
    public class Flat
    {
        public string address { get; set; }
        public string metre { get; set; }
        public string rooms { get; set; }
        public string floor { get; set; }
        public string prize { get; set; }

        public Flat(string address, string metre, string rooms, string floor, string prize)
        {
            this.address = address;
            this.metre = metre;
            this.rooms = rooms;
            this.floor = floor;
            this.prize = prize;
        }

        

            //public int Compare(Flat x, Flat y)
            //{
            //    if (x == null)
            //    {
            //        if (y == null)
            //        {
            //            // If x is null and y is null, they're equal. 
            //            return 0;
            //        }
            //        else
            //        {
            //            // If x is null and y is not null, y is greater. 
            //            return -1;
            //        }
            //    }
            //    else
            //    {
            //        // If x is not null...
            //        if (y == null)
            //        {
            //            // ...and y is null, x is greater.
            //            return 1;
            //        }
            //        else
            //        {
            //            // ...and y is not null, compare the lengths of the two strings.
            //            int retval = x.Name.Length.CompareTo(y.Name.Length);

            //            if (retval != 0)
            //            {
            //                // If the strings are not of equal length, the longer string is greater.
            //                return retval;
            //            }
            //            else
            //            {
            //                // If the strings are of equal length, sort them with ordinary string comparison.
            //                return x.Name.CompareTo(y.Name);
            //            }
            //        }
            //    }
            //}
        }

    class Program
    {
        public static List<Flat> compare(List<Flat> newest_list, List<Flat> oldest_list)
        {
            /*
             * Compares two annoucements.
             * Returns offer with new prize
             */
            var difference = new List<Flat>();
            var zip = newest_list.Zip(oldest_list, (n, o) => new { n, o });
            foreach (var z in zip)
            {
                if (z.n.address == z.o.address && z.n.floor == z.o.floor)
                {
                    if (z.n.prize != z.o.prize)
                    {
                        difference.Add(z.n);
                    }
                }
            }
            return difference;
        }

        static List<Flat> import_flat_to_dict(HtmlNodeCollection addr_node, HtmlNodeCollection spec_node, HtmlNodeCollection priz_node)//, HtmlNodeCollection flo_node, HtmlNodeCollection pri_node)
        {
            //You can use zip to deacrease the lines of code
            //example: 
            // var zip = names.Zip(places, (n, p) => new { n, p }).Zip(colors, (t, c) => new { Name = t.n, Place = t.p, Color = c });
            // foreach (var z in zip)

            var json_list = new List<Flat>();

            List<string> addr = new List<string>();
            List<string> metr = new List<string>();
            List<string> room = new List<string>();
            List<string> floo = new List<string>();
            List<string> priz = new List<string>();

            foreach (HtmlNode an in addr_node.Elements()) {
                addr.Add(an.InnerText);
            }

            foreach (HtmlNode sn in spec_node.Elements()) {
                string[] separator = {", "};
                string[] temp = sn.InnerText.Split(separator, 3, StringSplitOptions.RemoveEmptyEntries);
                metr.Add(temp[0]);
                room.Add(temp[1]);
                floo.Add(temp[2]);
            }

            foreach (HtmlNode pn in priz_node.Elements()) {
                priz.Add(pn.InnerText);
            }


            for (int i = 0; i < addr.Count; i++) {
                var temp = new Flat(addr[i],metr[i],room[i], floo[i], priz[i]);
                json_list.Add(temp);
            }

            return json_list;
        }

        public static void print_prize_change(List<Flat> diff)
        {
            foreach (Flat d in diff)
            {
                Console.WriteLine("Flat with address: " + d.address + " changed prize:");
                Console.WriteLine("Prize changed to: " + d.prize);
            }
        }

        static void Main(string[] args)
        {
            bool option = false; //1 - save to json; 0 - compare
            var html = @"http://www.bezposrednio.com/mieszkania,sprzedaz";
            string jsonPath = @"C:\Users\rogal\Desktop\test.json";

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(html);

            var add_nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='tytul']");
            var met_nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='linia']");
            var pri_nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='cena']");

            //var node = htmlDoc.DocumentNode.SelectSingleNode("//td[@class='tresc']")

            if (option)
            {
                //...create a json file:
                List<Flat> jsonElements = import_flat_to_dict(add_nodes, met_nodes, pri_nodes);
                string json = JsonConvert.SerializeObject(jsonElements);
                File.WriteAllText(jsonPath, json);
            }
            else {
                string j = File.ReadAllText(jsonPath);
                List<Flat> oldest = JsonConvert.DeserializeObject<List<Flat>>(j);

                List<Flat> newest = import_flat_to_dict(add_nodes, met_nodes, pri_nodes);

                List<Flat> diff = compare(newest, oldest);
                if (diff.Count < 1)
                    Console.WriteLine("There is no difference");
                else {
                    print_prize_change(diff);
                }
            }
        }
    }
}
