namespace Library.API.Models
{
    public class LinkDto
    {
        public LinkDto(string href, string type, string method)
        {
            Href = href;
            Type = type;
            Method = method;
        }

        public string Href { get; set; }

        public string Type { get; set; }

        public string Method { get; set; }
    }
}
