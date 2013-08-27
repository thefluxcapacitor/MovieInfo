namespace MovieInfoCmd
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Web;
    using System.Xml.Linq;

    using HtmlAgilityPack;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var movieFile = args[0];

                Console.WriteLine("==== Getting metadata for... " + movieFile + " ====");

                var video = new Video(Path.GetFileNameWithoutExtension(movieFile));
                if (string.IsNullOrEmpty(video.Title))
                {
                    Console.WriteLine("Could not get movie title.");
                    Exit(args);
                    return;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en");

                var imdbId = FindOnImdb(client, video);

                SaveMetadata(client, imdbId, movieFile);
                Exit(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Exit(args);
            }
        }

        private static void Exit(string[] args)
        {
            if (args.Length <= 1 || args[1] != "-nowait")
            {
                Console.WriteLine("\r\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        private static void SaveMetadata(HttpClient client, string imdbId, string movieFile)
        {
            var task2 = client.GetStringAsync(string.Format("http://www.imdb.com/title/{0}/", imdbId));
            task2.Wait();
            var html = task2.Result;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var movie = new MovieMetadata();

            movie.ImdbId = imdbId;
            movie.Title = GetTitle(doc);
            movie.Duration = int.Parse(GetDuration(doc));
            movie.Directors = GetDirectors(doc);
            movie.Rating = double.Parse(GetRating(doc));
            movie.Genres = GetGenres(doc);
            movie.Year = int.Parse(GetYear(doc));
            movie.Cast = GetCast(doc);
            movie.Plot = GetPlot(doc);

            Console.WriteLine(movie);

            var aux = Path.ChangeExtension(movieFile, ".xml");
            WriteMetadata(movie, aux);
            Console.WriteLine("\r\nMetadata saved to " + aux);

            var imgSrc = GetImgSrc(doc);

            var task3 = client.GetByteArrayAsync(imgSrc);
            task3.Wait();
            var bytes = task3.Result;
            var aux2 = Path.ChangeExtension(movieFile, ".metathumb");
            File.WriteAllBytes(aux2, bytes);
            Console.WriteLine("\r\nThumbnail saved to " + aux2);
        }

        private static string FindOnImdb(HttpClient client, Video video)
        {
            var task = client.GetStringAsync(string.Format("http://www.imdb.com/xml/find?xml=1&nr=1&tt=on&q={0}", HttpUtility.UrlEncode(video.Title)));
            task.Wait();

            string imdbId;
            string titleFound;

            var xmlDoc = XDocument.Parse(task.Result);
            var popular = xmlDoc.Descendants("ResultSet").FirstOrDefault(el => el.Attribute("type").Value == "title_popular");
            if (popular != null)
            {
                var elem = popular.Descendants("ImdbEntity").First();
                imdbId = elem.Attribute("id").Value;
                titleFound = elem.Value;
            }
            else
            {
                var elem = xmlDoc.Descendants("ImdbEntity").First();
                imdbId = elem.Attribute("id").Value;
                titleFound = elem.Value;
            }

            Console.WriteLine("Title found on imdb. \r\n\tTitle: {0}\r\n\tID: {1}", titleFound, imdbId);
            return imdbId;
        }

        private static string GetImgSrc(HtmlDocument doc)
        {
            var result = doc.DocumentNode.Descendants("img")
                .First(el => 
                    el.GetAttributeValue("title", string.Empty).ToLower().Contains("poster") &&
                    el.GetAttributeValue("itemprop", string.Empty).Equals("image"))
                .GetAttributeValue("src", string.Empty);
            return result;
        }

        private static string GetRating(HtmlDocument doc)
        {
            var result = doc.DocumentNode.Descendants("span")
                .Single(el => el.GetAttributeValue("itemprop", string.Empty).Equals("ratingValue"))
                .InnerText;
            return result;
        }

        private static string GetDuration(HtmlDocument doc)
        {
            var techSpecs = doc.DocumentNode.Descendants("h4")
                .SingleOrDefault(el => el.InnerText.Equals("Runtime:", StringComparison.OrdinalIgnoreCase));
            if (techSpecs == null)
            {
                return "0";
            }

            var result = techSpecs.ParentNode.Descendants("time")
                .First(el => el.GetAttributeValue("itemprop", string.Empty).Equals("duration"))
                .InnerText;
            return result.Substring(0, result.IndexOf(" "));
        }

        private static string[] GetGenres(HtmlDocument doc)
        {
            var genres = doc.DocumentNode.Descendants("span")
                .Where(el => el.GetAttributeValue("class", string.Empty).Equals("itemprop") &&
                el.GetAttributeValue("itemprop", string.Empty).Equals("genre"));

            return genres.Select(g => g.InnerText).ToArray();
        }

        private static string[] GetCast(HtmlDocument doc)
        {
            var actorsDiv = doc.DocumentNode.Descendants("div")
                .Single(el => el.GetAttributeValue("itemprop", string.Empty).Equals("actors"));
            var result = actorsDiv.Descendants("span")
                .Where(el => el.GetAttributeValue("itemprop", string.Empty).Equals("name") &&
                    el.GetAttributeValue("class", string.Empty).Equals("itemprop"));

            return result.Select(g => g.InnerText).ToArray();
        }

        private static string GetTitle(HtmlDocument doc)
        {
            var titleHeader = doc.DocumentNode.Descendants("h1")
                .Single(el => el.GetAttributeValue("class", string.Empty).Equals("header"));
            var result = titleHeader.Descendants("span")
                .Single(el => el.GetAttributeValue("itemprop", string.Empty).Equals("name") &&
                    el.GetAttributeValue("class", string.Empty).Equals("itemprop"))
                .InnerText;
            return result;
        }

        private static string GetPlot(HtmlDocument doc)
        {
            var description = doc.DocumentNode.Descendants("p")
                .SingleOrDefault(el => el.GetAttributeValue("itemprop", string.Empty).Equals("description"));
            return description == null ? string.Empty : description.InnerText;
        }

        private static string GetYear(HtmlDocument doc)
        {
            var titleHeader = doc.DocumentNode.Descendants("h1")
                .Single(el => el.GetAttributeValue("class", string.Empty).Equals("header"));
            var result = titleHeader.Descendants("a")
                .Single(el => el.GetAttributeValue("href", string.Empty).StartsWith("/year/"))
                .InnerText;
            return result;
        }

        private static string[] GetDirectors(HtmlDocument doc)
        {
            var directorDiv = doc.DocumentNode.Descendants("div")
                .Single(el => el.GetAttributeValue("itemprop", string.Empty).Equals("director"));
            var result = directorDiv.Descendants("span")
                .Where(el => el.GetAttributeValue("itemprop", string.Empty).Equals("name") &&
                    el.GetAttributeValue("class", string.Empty).Equals("itemprop"));
            return result.Select(dir => dir.InnerText).ToArray();
        }

        private static void WriteMetadata(MovieMetadata movie, string filePath)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<details>");
            xml.WriteElement("imdb_id", movie.ImdbId);
            xml.WriteElement("genre", movie.GenresStr);
            xml.WriteElement("overview", movie.Plot);

            //NOTE: using date instead of year so WDTV reads info
            //TODO: get full release date from imdb, not just year
            xml.WriteElement("year", movie.Year.ToString(CultureInfo.InvariantCulture) + "-01-01");

            xml.WriteElement("runtime", movie.Duration.ToString(CultureInfo.InvariantCulture));
            xml.WriteElement("title", string.Format("{0} ({1})", movie.Title, movie.Rating));
            xml.WriteElement("cast", movie.CastStr);
            xml.WriteElement("director", movie.DirectorsStr);
            xml.AppendLine("</details>");

            File.WriteAllText(filePath, xml.ToString());
        }
    }
}
