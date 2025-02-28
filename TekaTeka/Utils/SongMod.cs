using Scripts.UserData;
using System.Xml;
using System.Xml.Serialization;

namespace TekaTeka.Utils
{
#pragma warning disable 0649
    [Serializable]
    [XmlRoot("MusicInfoEx")]
    public struct CleanMusicInfoEx
    {
        [Serializable]
        [XmlType("EnsoDonChanBandRecordInfo")]
        public struct CleanEnsoDonChanBandRecordInfo
        {
            public int bestFanValue;
            public int clearCount;
            public int playCount;
        }

        [Serializable]
        [XmlType("HiScoreRecordInfo")]
        public struct CleanHiScoreRecordInfo
        {
            public int score;
            public short excellent;
            public short good;
            public short bad;
            public short combo;
            public short renda;
        }

        [Serializable]
        [XmlType("EnsoRecordInfo")]
        public struct CleanEnsoRecordInfo
        {
            public bool allGood;
            public bool cleared;
            public DataConst.CrownType crown;
            public int ensoGamePlayCount;
            public CleanHiScoreRecordInfo normalHiScore;
            public int playCount;
            public CleanHiScoreRecordInfo shinuchiHiScore;
            public int trainingPlayCount;
        }

        [Serializable]
        [XmlType("EnsoWarRecordInfo")]
        public struct CleanEnsoWarRecordInfo
        {
            public int playCount;
            public int winCount;
        }

        public EnsoDonChanBandRecordInfo donChanBandRecordInfo;
        public MusicFlags MusicFlag;
        public bool isBattleSong => (this.MusicFlag & MusicFlags.IsBattleSong) != 0;
        public bool isDownloaded => (this.MusicFlag & MusicFlags.IsDownloaded) != 0;
        public bool IsNew => (this.MusicFlag & MusicFlags.IsNew) != 0;

        public CleanEnsoRecordInfo[][] normalRecordInfo;
        public int playCount;

        public const MusicFlags playlistMask = MusicFlags.IsPlaylist1 | MusicFlags.IsPlaylist2 |
                                               MusicFlags.IsPlaylist3 | MusicFlags.IsPlaylist4 | MusicFlags.IsPlaylist5;

        public bool isFavorite => (this.MusicFlag & playlistMask) != 0;
        public int usageOrder;
        public CleanEnsoWarRecordInfo[] warRecordInfo;

#pragma warning restore 0649

        public Scripts.UserData.MusicInfoEx ToMusicInfoEx()
        {
            Scripts.UserData.MusicInfoEx musicInfo = new Scripts.UserData.MusicInfoEx();
            musicInfo.SetDefault();

            musicInfo.MusicFlag = this.MusicFlag;
            musicInfo.playCount = this.playCount;
            musicInfo.usageOrder = this.usageOrder;

            musicInfo.donChanBandRecordInfo =
                new EnsoDonChanBandRecordInfo { bestFanValue = this.donChanBandRecordInfo.bestFanValue,
                                                clearCount = this.donChanBandRecordInfo.clearCount,
                                                playCount = this.donChanBandRecordInfo.playCount };

            EnsoRecordInfo[] ensoRecordInfos = new EnsoRecordInfo[5];
            EnsoData.EnsoLevelType j = 0;
            int k = 0;
            foreach (var record2 in this.normalRecordInfo[0])
            {

                var shinuchiHiScore = new HiScoreRecordInfo {
                    bad = record2.shinuchiHiScore.bad,
                    combo = record2.shinuchiHiScore.combo,
                    excellent = record2.shinuchiHiScore.excellent,
                    good = record2.shinuchiHiScore.good,
                    renda = record2.shinuchiHiScore.renda,
                    score = record2.shinuchiHiScore.score,
                };
                var normalHiScore = new HiScoreRecordInfo {
                    bad = record2.normalHiScore.bad,
                    combo = record2.normalHiScore.combo,
                    excellent = record2.normalHiScore.excellent,
                    good = record2.normalHiScore.good,
                    renda = record2.normalHiScore.renda,
                    score = record2.normalHiScore.score,
                };

                musicInfo.UpdateNormalRecordInfo(j, false, ref normalHiScore, record2.crown);
                musicInfo.UpdateNormalRecordInfo(j, true, ref shinuchiHiScore, record2.crown);

                j++;
                k++;
            }

            EnsoData.WarPlayMode i = 0;
            foreach (var warRecord in this.warRecordInfo)
            {
                var ensoWarRecordInfo =
                    new EnsoWarRecordInfo { playCount = warRecord.playCount, winCount = warRecord.winCount };

                // musicData.UpdateWarRecordInfo(id, i, true);
                i++;
            }

            return musicInfo;
        }

        public void FromMusicInfoEx(XmlNode musicsData)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CleanMusicInfoEx));

            CleanMusicInfoEx ? result;
            using (TextReader reader = new StringReader(musicsData.OuterXml))
            {
                result = (CleanMusicInfoEx?)serializer.Deserialize(reader);
            }
            if (result != null)
            {
                this = (CleanMusicInfoEx)result;
            }
        }
    }

    internal abstract class SongMod
    {
        public bool enabled { get; set; }
        public string name { get; set; }
        protected string modFolder { get; set; }

        public abstract void AddMod(ModdedSongsManager manager);
        public abstract void RemoveMod(string songId, ModdedSongsManager manager);
        public abstract bool IsValidMod();

        public string GetModFolder()
        {
            return this.modFolder;
        }

        public abstract SongEntry GetSongEntry(string id, bool idIsSongFile = false);

        public abstract void SaveUserData(XmlElement userData);

        public abstract Scripts.UserData.MusicInfoEx LoadUserData();
    }
}
