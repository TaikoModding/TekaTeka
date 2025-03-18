using Scripts.UserData;
using System.Text;
using TekaTeka.Plugins;
using Tommy;

namespace TekaTeka.Utils
{
    internal class FumenSongMod : SongMod
    {
        public string modName;
        public List<MusicDataInterface.MusicInfo> songList = new List<MusicDataInterface.MusicInfo>();
        private Dictionary<string, SongEntry> chartFileToEntry = new Dictionary<string, SongEntry>();
        private Dictionary<string, SongEntry> songFileToEntry = new Dictionary<string, SongEntry>();

        public FumenSongMod(string modName)
        {

            this.modFolder = modName;
            this.modName = modName;
            string modPath = Path.Combine(CustomSongLoader.songsPath, this.modFolder);

            StreamReader configToml;

            if (!this.IsValidMod())
            {
                this.enabled = false;
                return;
            }

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
                    Logger.Log($"'config.toml' not found in mod {modName}. Creating...", LogType.Warning);
                }
                else if (e.GetType() != typeof(TomlParseException))
                {
                    Logger.Log($"'config.toml' from {modName} was not valid. Recreating...", LogType.Warning);
                }
            }

            if (table == null)
            {
                table = new TomlTable();
                table.Add("enabled", true);
                table.Add("name", modName);
                table.Add("version", "0.1");
                table.Add("description", "");

                using (StreamWriter writer = File.CreateText(Path.Combine(modPath, "config.toml")))
                {
                    table.WriteTo(writer);
                    writer.Flush();
                }
            }

            this.enabled = table["enabled"].IsBoolean ? table["enabled"].AsBoolean.Value : true;
            this.modName = table["name"].IsString ? table["name"].AsString.Value : modName;

            if (!this.enabled)
            {
                Logger.Log($"Mod {this.modName} explicitly disabled, skipping...", LogType.Debug);
                return;
            }

            if (this.ReadMusicDb())
            {
                this.enabled = true;
            };
        }

        public override bool IsValidMod()
        {
            string modFolder = Path.Combine(CustomSongLoader.songsPath, this.modFolder);
            string musicDataPath = Path.Combine(modFolder, CustomSongLoader.ASSETS_FOLDER, "musicinfo");
            if (!Directory.Exists(modFolder))
            {
                return false;
            }

            if (!File.Exists(musicDataPath + ".json") && !File.Exists(musicDataPath + ".bin"))
            {
                return false;
            }

            return true;
        }

        private bool ReadMusicDb()
        {
            string musicDataPath =
                Path.Combine(CustomSongLoader.songsPath, this.modFolder, CustomSongLoader.ASSETS_FOLDER, "musicinfo");
            string jsonString;
            if (File.Exists(musicDataPath + ".json"))
            {
                musicDataPath += ".json";
                jsonString = File.ReadAllText(musicDataPath);
            }
            else if (File.Exists(musicDataPath + ".bin"))
            {
                musicDataPath += ".bin";
                var bytes = Cryptgraphy.ReadAllAesAndGZipBytes(musicDataPath, Cryptgraphy.AesKeyType.Type2);
                jsonString = Encoding.UTF8.GetString(bytes);
            }
            else
            {
                Logger.Log($"File \"musicinfo\" at {musicDataPath + "{.bin/.json}"} not present", LogType.Warning);
                return false;
            }

            this.songList.AddRange(JsonSupports.ReadJson<MusicDataInterface.MusicInfo>(jsonString));

            return true;
        }

        public override SongEntry GetSongEntry(string id, bool idIsSongFile = false)
        {
            if (idIsSongFile)
            {
                SongEntry songEntry = this.songFileToEntry[id];
                return songEntry;
            }
            else
            {
                SongEntry songEntry = this.chartFileToEntry[id.Split('_')[0]];
                songEntry.songFile = id;
                return songEntry;
            }
        }
        public override void RemoveMod(string songId, ModdedSongsManager manager)
        {
            MusicDataInterface.MusicInfo? target = this.songList.Find(hit => hit.Id == songId);
            if (target != null)
            {
                Logger.Log($"Removing Fumen mod '{modName}' song id {target.UniqueId}");
                manager.RemoveMusicInfo(target);
            }
        }
        public override void AddMod(ModdedSongsManager manager)
        {
            int songsAdded = 0;
            for (int i = 0; i < this.songList.Count; i++)
            {
                MusicDataInterface.MusicInfo song = this.songList[i];
                if (!manager.HasSong(song.UniqueId))
                {
                    songsAdded++;

                    var entry = new FumenSongEntry(this.modFolder, song);
                    this.chartFileToEntry.Add(song.Id, entry);
                    this.songFileToEntry.Add(song.SongFileName, entry);

                    manager.RetainMusicInfo(song, this);
                }
                else
                {
#if DEBUG
                    Logger.Log($"{song.UniqueId} from {this.modName} Skipped", LogType.Debug);
#endif
                }
            }
            Logger.Log($"{songsAdded} out of {this.songList.Count} songs were added from mod \"{this.modName}\"");
        }

        public override void SaveUserData(UserData userData)
        {
            throw new NotImplementedException();
        }

        public override Scripts.UserData.MusicInfoEx LoadUserData()
        {
            throw new NotImplementedException();
        }
    }
}
