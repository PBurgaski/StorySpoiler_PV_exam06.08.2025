using System.Text.Json.Serialization;

namespace StorySpoiler.Models
{
    public class ApiResponseDTO
    {
        public string Msg { get; set; }
        public string StoryId { get; set; }
    }
}