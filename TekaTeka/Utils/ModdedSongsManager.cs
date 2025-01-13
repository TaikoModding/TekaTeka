using Scripts.UserData;
using Scripts.UserData.Flag;
using TekaTeka.Plugins;

namespace TekaTeka.Utils
{
    internal class ModdedSongsManager
    {
        public HashSet<int> currentSongs = new HashSet<int>();
        public Dictionary<int, SongMod> uniqueIdToMod = new Dictionary<int, SongMod>();
        public Dictionary<string, SongMod> idToMod = new Dictionary<string, SongMod>();
        public Dictionary<string, SongMod> songFileToMod = new Dictionary<string, SongMod>();
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
                    FumenSongMod mod = new FumenSongMod(folder);
                    if (mod.enabled)
                    {
                        Logger.Log($"Mod {mod.modName} Loaded", LogType.Info);
                        mods.Add(mod);
                    }
                }
            }
        }

        public void AddTjaMods(List<SongMod> mods)
        {
            foreach (string path in Directory.GetDirectories(Path.Combine(CustomSongLoader.songsPath, "TJAsongs")))
            {
                string folder = Path.GetFileName(path) ?? "";
                if (folder != "")
                {
                    TjaSongMod mod = new TjaSongMod(folder, 3000 + tjaSongs);
                    if (mod.enabled)
                    {
                        Logger.Log($"Mod {mod.name} Loaded", LogType.Info);
                        mods.Add(mod);
                        tjaSongs++;
                    }
                }
            }
        }

        public void SetupMods()
        {
            List<SongMod> mods = this.GetMods();
            foreach (SongMod mod in mods)
            {
                if (mod.enabled)
                {
                    mod.AddMod(this);
                    modsEnabled.Add(mod);
                }
            }
        }

        public SongMod? GetModPath(int uniqueId)
        {
            if (this.uniqueIdToMod.ContainsKey(uniqueId))
            {
                return this.uniqueIdToMod[uniqueId];
            }
            else
            {
                return null;
            }
        }

        public SongMod? GetModPath(string songFileName)
        {
            if (this.songFileToMod.ContainsKey(songFileName))
            {
                return this.songFileToMod[songFileName];
            }
            else if (this.idToMod.ContainsKey(songFileName))
            {
                return this.idToMod[songFileName];
            }
            else
            {
                return null;
            }
        }

        public UserData FilterModdedData(UserData userData)
        {

            foreach (SongMod mod in this.modsEnabled)
            {
                if (mod is TjaSongMod)
                {
                    mod.SaveUserData(userData);
                }
            }

            Scripts.UserData.MusicInfoEx[] datas = userData.MusicsData.Datas;
            UserFlagDataDefine.FlagData[] flags1 =
                userData.UserFlagData.userFlagData[(int)Scripts.UserData.Flag.UserFlagData.FlagType.Song];
            UserFlagDataDefine.FlagData[] flags2 =
                userData.UserFlagData.userFlagData[(int)Scripts.UserData.Flag.UserFlagData.FlagType.TitleSongId];

            Array.Resize(ref datas, 3000);
            Array.Resize(ref flags1, 3000);
            Array.Resize(ref flags2, 3000);

            userData.MusicsData.Datas = datas;
            userData.UserFlagData.userFlagData[(int)Scripts.UserData.Flag.UserFlagData.FlagType.Song] = flags1;
            userData.UserFlagData.userFlagData[(int)Scripts.UserData.Flag.UserFlagData.FlagType.TitleSongId] = flags2;
            return userData;
        }
    }
}
