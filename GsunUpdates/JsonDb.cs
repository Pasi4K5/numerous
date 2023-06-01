using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GsunUpdates;

public sealed class JsonDb
{
    private const string Path = "./data.json";

    private readonly object _lock = new();

    private readonly JObject _default = new()
    {
        ["channels"] = new JArray(),
        ["pageSection"] = null
    };

    public JObject Data
    {
        get
        {
            lock (_lock)
            {
                var text = File.ReadAllText(Path);
                var data = JsonConvert.DeserializeObject<JObject>(text);

                if (data is null)
                {
                    throw new JsonException(
                        "Failed to deserialize data. Please ensure that ./data.json is valid JSON.\n"
                        + "Delete the file to reset."
                    );
                }

                return data;
            }
        }
    }

    public JsonDb()
    {
        lock (_lock)
        {
            if (File.Exists(Path))
            {
                return;
            }

            File.WriteAllText(Path, _default.ToString());
        }
    }

    public void Save(JObject data)
    {
        lock (_lock)
        {
            var text = data.ToString();

            File.WriteAllText(Path, text);
        }
    }
}
