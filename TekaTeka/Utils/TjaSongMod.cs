using Scripts.UserData;
using System.Text.Json;
using System.Text.Json.Nodes;
using TekaTeka.Plugins;
using Tommy;

namespace TekaTeka.Utils
{
    internal class TjaSongMod : SongMod
    {
        public enum Genre : int
        {
            POP,
            ANIME,
            VOCALOID,
            VARIETY,
            CLASSICAL = 5,
            GAME,
            NAMCO
        }

        public static readonly string[] GenreFolders =
            new string[] { "01 Pop", "02 Anime",     "03 Vocaloid music", "04 Variety",
                           "",       "05 Classical", "06 Game music",     "07 NAMCO original" };

        TjaSongEntry song;
        string modPath;
        public int uniqueId;
        Genre genre;

        string modFolderPath =>
            Path.Combine(CustomSongLoader.songsPath, "TJAsongs", GenreFolders[(int)this.genre], this.name);

        string tjaPath => Path.Combine(this.modFolderPath, this.name + ".tja");
        string savesPath => Path.Combine(this.modFolderPath, "saves.json");

        string wavePath => Path.Combine(this.modFolderPath, this.song.GetWaveName());

        public TjaSongMod(string folder, int id, Genre genre)
        {
            this.modFolder = folder;
            this.genre = genre;
            this.enabled = true;
            this.name = folder;
            this.uniqueId = id;
            this.modPath = modFolderPath;

            StreamReader configToml;
            TomlTable? table = null;

            try
            {
                configToml = File.OpenText(Path.Combine(modPath, "config.toml"));

                table = TOML.Parse(configToml);
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(IOException))
                {
                    Logger.Log($"'config.toml' not found in mod {this.name}. Creating...", LogType.Warning);
                }
                else if (e.GetType() != typeof(TomlParseException))
                {
                    Logger.Log($"'config.toml' from {this.name} was not valid. Recreating...", LogType.Warning);
                }
            }

            if (table == null)
            {
                table = new TomlTable();
                table.Add("enabled", true);
                table.Add("name", this.name);
                table.Add("version", "1.0");
                table.Add("description", "");

                using (StreamWriter writer = File.CreateText(Path.Combine(modPath, "config.toml")))
                {
                    table.WriteTo(writer);
                    writer.Flush();
                }
            }

            this.song = new TjaSongEntry(this.modFolder, id, genre);
            this.enabled = table["enabled"].IsBoolean ? table["enabled"].AsBoolean.Value : true;

            if (!this.enabled)
            {
                Logger.Log($"Mod {this.name} explicitly disabled, skipping...", LogType.Debug);
                return;
            }
        }

        public override bool IsValidMod()
        {
            return File.Exists(this.tjaPath) && File.Exists(this.wavePath);
        }

        public override void AddMod(ModdedSongsManager manager)
        {
            manager.RetainMusicInfo(this.song.musicInfo, this);
        }

        public override SongEntry GetSongEntry(string id, bool idIsSongFile = false)
        {
            song.SongFile = id;
            return this.song;
        }

        public override void SaveUserData(UserData userData)
        {
            if (this.uniqueId > userData.MusicsData.Datas.Length)
            {
                return;
            }

            uint songHash = _3rdParty.MurmurHash2.Hash(File.ReadAllBytes(this.tjaPath));

            var cleanedScore = new CleanMusicInfoEx();

            cleanedScore.FromMusicInfoEx(userData.MusicsData, this.uniqueId);
            JsonSerializerOptions jsonConfig = new JsonSerializerOptions();
            jsonConfig.IncludeFields = true;
            jsonConfig.WriteIndented = true;

            string jsonString = JsonSerializer.Serialize(
                cleanedScore, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true });

            JsonObject save = new JsonObject();
            save["SongHash"] = songHash.ToString("X");
            save["musicData"] = JsonObject.Parse(jsonString);

            File.WriteAllText(this.savesPath, save.ToJsonString());
        }

        public override Scripts.UserData.MusicInfoEx LoadUserData()
        {
            // string jsonPath = Path.Combine(CustomSongLoader.songsPath, "TJAsongs", this.modFolder, "saves.json");
            if (!File.Exists(this.savesPath))
            {
                var newMusicInfo = new Scripts.UserData.MusicInfoEx();
                newMusicInfo.SetDefault();

                return newMusicInfo;
            }

            string jsonString = File.ReadAllText(this.savesPath);

            var json = JsonObject.Parse(jsonString);
            if (json == null)
            {
                var newMusicInfo = new Scripts.UserData.MusicInfoEx();
                newMusicInfo.SetDefault();
                return newMusicInfo;
            }

            var cleanMusicInfo = JsonSerializer.Deserialize<CleanMusicInfoEx>(
                json["musicData"], new JsonSerializerOptions { IncludeFields = true });

            return cleanMusicInfo.ToMusicInfoEx();
        }
    }
}
