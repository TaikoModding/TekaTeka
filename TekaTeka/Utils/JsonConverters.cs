using System.Text.Json.Serialization;
using System.Text.Json;

namespace TekaTeka.Utils
{
    internal class JsonConverters
    {
        internal class EnsoRecordArrayConverter : JsonConverter<CleanMusicInfoEx.CleanEnsoRecordInfo[][]>
        {
            public override CleanMusicInfoEx.CleanEnsoRecordInfo[][] Read(ref Utf8JsonReader reader, Type typeToConvert,
                                                                          JsonSerializerOptions options)
            {
                var flatArray = JsonSerializer.Deserialize<CleanMusicInfoEx.CleanEnsoRecordInfo[]>(ref reader, options);
                if (flatArray == null)
                {
                    return new CleanMusicInfoEx.CleanEnsoRecordInfo[1][] {
                        new CleanMusicInfoEx.CleanEnsoRecordInfo[5]
                    };
                }
                return new CleanMusicInfoEx.CleanEnsoRecordInfo[][] { flatArray };
            }

            public override void Write(Utf8JsonWriter writer, CleanMusicInfoEx.CleanEnsoRecordInfo[][] value,
                                       JsonSerializerOptions options)
            {
                if (value.Length != 1)
                {
                    throw new JsonException("Expected a 2D array with exactly one row.");
                }

                JsonSerializer.Serialize(writer, value[0], options);
            }
        }
    }
}
