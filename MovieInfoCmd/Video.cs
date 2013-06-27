namespace MovieInfoCmd
{
    using System;

    public class Video
    {
        public string Title { get; set; }

        public int Year { get; set; }

        public Video()
        {
        }

        public Video(string fileName)
        {
            this.ParseYear(fileName);
        }

        private void ParseYear(string fileName)
        {
            var consecutiveDigits = 0;
            var lastCharIsDigit = false;

            for (var i = 0; i < fileName.Length; i++)
            {
                if (char.IsDigit(fileName[i]))
                {
                    if (lastCharIsDigit)
                    {
                        consecutiveDigits++;
                    }
                    else
                    {
                        consecutiveDigits = 1;
                        lastCharIsDigit = true;
                    }
                }
                else
                {
                    lastCharIsDigit = false;

                    if (consecutiveDigits == 4)
                    {
                        this.Year = int.Parse(fileName.Substring(i - 4, 4));

                        if (string.IsNullOrEmpty(this.Title))
                        {
                            this.Title = fileName.Substring(0, i - 5).Replace('.', ' ');
                        }

                        consecutiveDigits = 0;
                    }
                }
            }

            if (this.Year == 0 && consecutiveDigits == 4)
            {
                this.Year = int.Parse(fileName.Substring(fileName.Length - 4, 4));

                if (string.IsNullOrEmpty(this.Title))
                {
                    this.Title = fileName.Substring(0, fileName.Length - 5).Replace('.', ' ');
                }
            }

            Console.WriteLine("Filename parsed.\r\n\tTitle: {0}\r\n\tYear: {1}", this.Title, this.Year);
        }
    }
}