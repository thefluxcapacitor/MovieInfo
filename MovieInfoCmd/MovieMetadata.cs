namespace MovieInfoCmd
{
    public class MovieMetadata
    {
        public string ImdbId { get; set; }

        public string Title { get; set; }

        public int Year { get; set; }

        public int Duration { get; set; }

        public double Rating { get; set; }

        public string Plot { get; set; }

        public string[] Directors { get; set; }

        public string DirectorsStr
        {
            get
            {
                return this.Directors.CommaSeparated();
            }
        }

        public string[] Genres { get; set; }
        
        public string GenresStr
        {
            get
            {
                return this.Genres.CommaSeparated();
            }
        }

        public string[] Cast { get; set; }

        public string CastStr
        {
            get
            {
                return this.Cast.CommaSeparated();
            }
        }

        public override string ToString()
        {
            return string.Format(
                "Movie metadata:\r\n\tTitle: {0}\r\n\tYear: {1}\r\n\tDuration: {2}\r\n\tRating: {3}\r\n\tDirector(s): {4}\r\n\tCast: {5}\r\n\tGenre(s): {6}\r\n\tPlot: {7}",
                this.Title,
                this.Year,
                this.Duration,
                this.Rating,
                this.DirectorsStr,
                this.CastStr,
                this.GenresStr,
                this.Plot);
        }
    }
}
