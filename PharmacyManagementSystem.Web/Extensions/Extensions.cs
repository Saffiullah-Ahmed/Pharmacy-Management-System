using System.Text.Json;

namespace PharmacyManagementSystem.Web.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObject<T>(
            this ISession session,
            string key,
            T value)
        {
            string json =
                JsonSerializer.Serialize(value);

            session.SetString(key, json);
        }


        public static T? GetObject<T>(
            this ISession session,
            string key)
        {
            string? json =
                session.GetString(key);

            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(
                json);
        }
    }
}
