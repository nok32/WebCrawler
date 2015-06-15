using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class Crawler
    {
        private string ParrentLink { get; set; }

        private string CurrentLink { get; set; }

        private ConcurrentBag<string> NorepeatLinks { get; set; }

        private WebClient Wc { get; set; }

        private string PhotoPattren { get; set; }

        public List<string> Links { get; private set; }

        public Crawler(string url)
        {
            this.Wc = new WebClient();
            this.ParrentLink = url;
            this.Links = new List<string>();
            this.NorepeatLinks = new ConcurrentBag<string>();
            this.PhotoPattren = @"<img.+?src.+?(\'|\"")(?<pic>.+?)(\1|\?).*>";
        }

        private ConcurrentBag<Match> FindElement(string pattern, string html)
        {
            ConcurrentBag<Match> match = new ConcurrentBag<Match>();
            Regex rg = new Regex(pattern);
            MatchCollection m = rg.Matches(html);

            Parallel.ForEach(m.Cast<Match>(), e =>
            {
                match.Add(e);
            });
            return match;
        }

        public void DonwloadPhotos(string url)
        {
            string html = this.Wc.DownloadString(url);
            ConcurrentBag<Match> matches = FindElement(this.PhotoPattren, html);
            foreach (var match in matches)
            {
                string currentPicAllName = match.Groups[3].Value;
                var startName = currentPicAllName.Substring(0, 7);
                if (startName != "http://")
                {
                    currentPicAllName = this.ParrentLink + "/" + currentPicAllName;
                }
                int index = currentPicAllName.LastIndexOf('/');
                string uri = currentPicAllName.Substring(0, index) + "/";
                string picName = currentPicAllName.Substring(index + 1);
                try
                {
                    this.Wc.DownloadFile(uri, picName);
                }
                catch (WebException)
                {

                }
            }
        }
        public List<string> FindLinks(string pattern, string url)
        {
            try
            {
                var urlStandart = url.Substring(0, 7);
                if (urlStandart != "http://")
                {
                    url = this.ParrentLink + "/" + url;
                }
                string html = this.Wc.DownloadString(url);

                ConcurrentBag<Match> matches = FindElement(pattern, html);

                Parallel.ForEach(matches, match =>
                {
                    if (!this.NorepeatLinks.Contains(match.Groups[3].Value) && (char.IsLetter(match.Groups[3].Value[0]) || match.Groups[3].Value[0] == '.' || match.Groups[3].Value[0] == '/'))
                    {
                        this.NorepeatLinks.Add(match.Groups[3].Value);
                    }
                });

                this.Links = this.NorepeatLinks.ToList();
            }
            catch (WebException)
            {
                Console.WriteLine("Resource not found 404!");
                return this.Links;
            }
            return this.Links;
        }
    }
}