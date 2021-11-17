using Newtonsoft.Json;

/*
 * @Source: Dr. Schuab's APIError.cs in git class examples
 */

namespace TicketingAppProject.Services
{
    public class APIError
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}