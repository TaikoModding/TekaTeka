using Scripts.UserData;
using Scripts.UserData.Flag;
using System.Xml;
using TekaTeka.Plugins;

namespace TekaTeka.Utils
{
    internal class ModdedSongsManager
    {
        // Current songs is all music identifiers: core content, DLC, and mods.
        private HashSet<int> currentSongs = new HashSet<int>();
        // Other data structures only hold information about mods. musicInfos should
        // not contain MusicInfo for non-mod content.
        private Dictionary<int, SongMod> uniqueIdToMod = new Dictionary<int, SongMod>();
        private Dictionary<string, SongMod> idToMod = new Dictionary<string, SongMod>();
        private Dictionary<string, SongMod> songFileToMod = new Dictionary<string, SongMod>();
        private List<MusicDataInterface.MusicInfo> musicInfos = new List<MusicDataInterface.MusicInfo>();

        public MusicDataInterface musicData => TaikoSingletonMonoBehaviour<DataManager>.Instance.MusicData;
        public InitialPossessionDataInterface initialPossessionData =>
            TaikoSingletonMonoBehaviour<DataManager>.Instance.InitialPossessionData;

        public List<SongMod> modsEnabled = new List<SongMod>();

        public int tjaSongs = 0;

        public ModdedSongsManager()
        {
            foreach (MusicDataInterface.MusicInfoAccesser accesser in musicData.MusicInfoAccesserList)
            {
                this.currentSongs.Add(accesser.UniqueId);
            }
            this.SetupMods();
            this.PublishSongs();
        }

        public List<SongMod> GetMods()
        {
            List<SongMod> mods = new List<SongMod>();
            AddFumenMods(mods);
            AddTjaMods(mods);
            return mods;
        }

        public void AddFumenMods(List<SongMod> mods)
        {
            foreach (string path in Directory.GetDirectories(CustomSongLoader.songsPath))
            {
                string folder = Path.GetFileName(path) ?? "";
                if (folder != "" && folder != "TJAsongs")
                {
                    try
                    {
                        FumenSongMod mod = new FumenSongMod(folder);
                        if (mod.enabled)
                        {
                            Logger.Log($"Mod {mod.modName} Loaded", LogType.Info);
                            mods.Add(mod);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error when loading Fumen mod \"{path}\":\n{ex.ToString()}", LogType.Error);
                    }
                }
            }
        }

        public void AddTjaMods(List<SongMod> mods)
        {
            //
            foreach (TjaSongMod.Genre genre in Enum.GetValues(typeof(TjaSongMod.Genre)))
            {
                string genrePath =
                    Path.Combine(CustomSongLoader.songsPath, "TJAsongs", TjaSongMod.GenreFolders[(int)genre]);
                if (!Directory.Exists(genrePath))
                {
                    Directory.CreateDirectory(genrePath);
                }
                foreach (string path in Directory.GetDirectories(genrePath))
                {
                    string folder = Path.GetFileName(path) ?? "";
                    if (folder != "")
                    {
                        try
                        {
                            TjaSongMod mod = new TjaSongMod(folder, 3000 + tjaSongs, genre);
                            if (mod.enabled)
                            {
                                Logger.Log($"Mod {mod.name} Loaded", LogType.Info);
                                mods.Add(mod);
                                tjaSongs++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Error when loading TJA mod \"{path}\":\n{ex.ToString()}", LogType.Error);
                        }
                    }
                }
            }
        }

        public void RemoveMusicInfo(MusicDataInterface.MusicInfo musicInfo)
        {
            this.currentSongs.Remove(musicInfo.UniqueId);
            this.songFileToMod.Remove(musicInfo.Id);
            this.uniqueIdToMod.Remove(musicInfo.UniqueId);
            this.idToMod.Remove(musicInfo.Id);
            this.musicInfos.Remove(musicInfo);
            musicData.RemoveMusicInfo(musicInfo.UniqueId);
            musicData.SortByOrder();
        }

        public void RetainMusicInfo(MusicDataInterface.MusicInfo musicInfo, SongMod mod)
        {
            this.currentSongs.Add(musicInfo.UniqueId);
            this.songFileToMod.Add(musicInfo.SongFileName, mod);
            this.uniqueIdToMod.Add(musicInfo.UniqueId, mod);
            this.idToMod.Add(musicInfo.Id, mod);
            this.musicInfos.Add(musicInfo);
            this.initialPossessionData.InitialPossessionInfoAccessers.Add(
                new InitialPossessionDataInterface.InitialPossessionInfoAccessor(
                    (int)InitialPossessionDataInterface.RewardTypes.Song, musicInfo.UniqueId));
        }

        public void PublishSongs()
        {
            for (int i = 0; i < this.musicInfos.Count; i++)
            {
                var tmp = this.musicInfos[i];
                this.musicData.AddMusicInfo(ref tmp);
            }
            this.musicData.SortByOrder();
        }

        public void SetupMods()
        {
            List<SongMod> mods = this.GetMods();
            foreach (SongMod mod in mods)
            {
                if (mod.enabled)
                {
                    try
                    {
                        mod.AddMod(this);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error when setting up mod \"{mod.name}\":\n{ex.ToString()}", LogType.Error);
                        continue;
                    }
                    modsEnabled.Add(mod);
                }
            }
        }

        public bool HasSong(int uniqueId)
        {
            return currentSongs.Contains(uniqueId);
        }

        public SongMod? GetModPath(string songFileName)
        {
            SongMod? songMod = null;
            songMod = this.GetModFromSongFile(songFileName);
            if (songMod != null)
            {
                return songMod;
            }

            songMod = this.GetModFromId(songFileName);
            if (songMod != null)
            {
                return songMod;
            }

            return null;
        }

        public SongMod? GetModFromId(string songId)
        {
            if (!this.idToMod.ContainsKey(songId))
            {
                return null;
            }
            return this.idToMod[songId];
        }

        public SongMod? GetModFromSongFile(string songFile)
        {
            if (!this.songFileToMod.ContainsKey(songFile))
            {
                return null;
            }
            return this.songFileToMod[songFile];
        }

        public XmlElement FilterModdedData(XmlElement userData)
        {

            foreach (SongMod mod in this.modsEnabled)
            {
                if (mod is TjaSongMod)
                {
                    mod.SaveUserData(userData);
                }
            }

            int nodeCount = userData["MusicsData"]?["Datas"]?.ChildNodes.Count ?? 0;
            for (int i = nodeCount; i >= 3000; i--)
            {
                var data = userData["MusicsData"]?["Datas"]?.ChildNodes[i];
                if (data != null)
                {
                    userData["MusicsData"]?["Datas"]?.RemoveChild(data);
                }

                data = userData["UserFlagData"]?["userFlagData"]
                           ?.ChildNodes[(int)UserFlagData.FlagType.Song]
                           ?.ChildNodes[i];
                if (data != null)
                {
                    userData["UserFlagData"]?["userFlagData"]?.ChildNodes[(int)UserFlagData.FlagType.Song]?.RemoveChild(
                        data);
                }

                data = userData["UserFlagData"]?["userFlagData"]
                           ?.ChildNodes[(int)UserFlagData.FlagType.TitleSongId]
                           ?.ChildNodes[i];
                if (data != null)
                {
                    userData["UserFlagData"]?["userFlagData"]
                        ?.ChildNodes[(int)UserFlagData.FlagType.TitleSongId]
                        ?.RemoveChild(data);
                }
            }

            return userData;
        }
    }
}
