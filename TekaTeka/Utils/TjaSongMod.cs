using Scripts.UserData;
using System.Text.Json;
using System.Text.Json.Nodes;
using TekaTeka.Plugins;

namespace TekaTeka.Utils
{
    internal class TjaSongMod : SongMod
    {
        TjaSongEntry song;
        string modPath;
        public int uniqueId;

        public TjaSongMod(string folder, int id)
        {
            this.modFolder = folder;
            this.enabled = true;
            this.name = folder;
            this.uniqueId = id;
            this.song = new TjaSongEntry(this.modFolder, id);
            this.modPath = Path.Combine(CustomSongLoader.songsPath, "TJAsongs", this.modFolder, this.modFolder);
        }

        public override bool IsValidMod()
        {
            return File.Exists(this.modPath + ".tja") &&
                   (File.Exists(this.modPath + ".wav") || File.Exists(this.modPath + ".ogg"));
        }

        public override void AddMod(ModdedSongsManager manager)
        {
            var musicInfo = this.song.musicInfo;

            var tjaSong = tja2fumen.Parsers.ParseTja(this.modPath + ".tja");
            uint songHash = _3rdParty.MurmurHash2.Hash(File.ReadAllBytes(this.modPath + ".tja")) & 0xFFFF_FFF;
            manager.currentSongs.Add(this.uniqueId);
            manager.songFileToMod.Add($"SONG_{songHash}", this);
            manager.uniqueIdToMod.Add(this.uniqueId, this);

            manager.idToMod.Add(songHash.ToString(), this);
            manager.musicData.AddMusicInfo(ref musicInfo);

            manager.initialPossessionData.InitialPossessionInfoAccessers.Add(
                new InitialPossessionDataInterface.InitialPossessionInfoAccessor(
                    (int)InitialPossessionDataInterface.RewardTypes.Song, musicInfo.UniqueId));
        }

        public override SongEntry GetSongEntry(string id, bool idIsSongFile = false)
        {
            song.songFile = id;
            return this.song;
        }

        public override void SaveUserData(UserData userData)
        {
            if (this.uniqueId > userData.MusicsData.Datas.Length)
            {
                return;
            }

            uint songHash = _3rdParty.MurmurHash2.Hash(File.ReadAllBytes(this.modPath + ".tja"));

            var cleanedScore = new CleanMusicInfoEx();

            cleanedScore.FromMusicInfoEx(userData.MusicsData, this.uniqueId);
            JsonSerializerOptions asd = new JsonSerializerOptions();
            asd.IncludeFields = true;
            asd.WriteIndented = true;

            string jsonString = JsonSerializer.Serialize(
                cleanedScore, new JsonSerializerOptions { IncludeFields = true, WriteIndented = true });

            JsonObject save = new JsonObject();
            save["SongHash"] = songHash.ToString("X");
            save["musicData"] = JsonObject.Parse(jsonString);

            string jsonPath = Path.Combine(CustomSongLoader.songsPath, "TJAsongs", this.modFolder, "saves.json");
            File.WriteAllText(jsonPath, save.ToJsonString());
        }

        public override Scripts.UserData.MusicInfoEx LoadUserData()
        {
            string jsonPath = Path.Combine(CustomSongLoader.songsPath, "TJAsongs", this.modFolder, "saves.json");
            if (!File.Exists(jsonPath))
            {
                var newMusicInfo = new Scripts.UserData.MusicInfoEx();
                newMusicInfo.SetDefault();

                return newMusicInfo;
            }

            string jsonString = File.ReadAllText(jsonPath);

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
