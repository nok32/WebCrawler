using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebCrawler
{
    static class FindAndLoadLinksByKeyWord
    {
        const string APattern = @"<a.+?href.+?(\''|\"")(?<link>.+?)(\1|\?)";
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter site URL:");
            string url = "http://www." + Console.ReadLine();
            Console.WriteLine("Please enter how deeply recource you want to do:");
            int dbootomOfRecurce = int.Parse(Console.ReadLine());

            Crawler crab = new Crawler(url);

            Console.WriteLine("Loading");
            Recurce(url, crab, 0, APattern, dbootomOfRecurce);
            Console.WriteLine("We found {0} links in this Web site", crab.Links.Count);

            Console.WriteLine("Please enter the word, for which you want to see link:");
            string wordPattern = Console.ReadLine();
            bool succsess = false;
            Parallel.ForEach(crab.Links, link =>
            {
                if (link.ToUpper().Contains(wordPattern.ToUpper()))
                {
                    var startName = link.Substring(0, 7);
                    if (startName != "http://")
                    {
                        link = url + "/" + link;
                    }
                    System.Diagnostics.Process.Start(link);
                    succsess = true;
                }
            });
            if (succsess == false)
            {
                Console.WriteLine("Sorry, we dont found links, which contain the word: {0}", wordPattern);
            }
        }

        public static void Recurce(string url, Crawler crab, int level, string pattern, int bootomOfRecurse)
        {
            if (level > bootomOfRecurse)
            {
                return;
            }
            List<string> links = crab.FindLinks(pattern, url);
            for (int i = 0; i < links.Count; i++)
            {
                Recurce(links[i], crab, level + 1, pattern, bootomOfRecurse);
            }

        }
    }
}
