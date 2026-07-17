using System.Collections.Generic;
using System.Text.Json;

namespace Application.Services.Setting.NoticeSrv.Dto
{
    public static class NoticeMetadataSerializer
    {
        public static Dictionary<string, string> Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, string>();
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
