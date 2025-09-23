namespace Apha.VIR.Web.Utilities
{
    public static class SessionBreadcrumbExtensions
    {
        private const string SessionKey = "BreadcrumbTrail";
        
        public static void AddOrUpdateBreadcrumb(this ISession session, string url, Dictionary<string, string> parameters)
        {
            var list = GetBreadcrumbs(session);

            var existing = list.FirstOrDefault(x => x.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {                
                existing.Parameters = parameters;
            }
            else
            {
                list.Add(new BreadcrumbEntry { Url = url, Parameters = parameters });
            }

            session.SetString(SessionKey, System.Text.Json.JsonSerializer.Serialize(list));
        }
        
        public static List<BreadcrumbEntry> GetBreadcrumbs(this ISession session)
        {
            var json = session.GetString(SessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<BreadcrumbEntry>()
                : System.Text.Json.JsonSerializer.Deserialize<List<BreadcrumbEntry>>(json)!;
        }

        public static string? GetFullUrlFor(this ISession session, string? url)
        {
            var list = GetBreadcrumbs(session);
            var entry = list.FirstOrDefault(x => x.Url.Equals(url, StringComparison.OrdinalIgnoreCase));

            if (entry == null) return null;

            if (entry.Parameters == null || entry.Parameters.Count == 0)
                return entry.Url;

            var query = string.Join("&", entry.Parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            return $"{entry.Url}?{query}";
        }
    }

    public class BreadcrumbEntry
    {
        public string Url { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new();
    }
}
