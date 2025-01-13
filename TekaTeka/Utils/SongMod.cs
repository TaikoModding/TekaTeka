using Scripts.UserData;

namespace TekaTeka.Utils
{
    [Serializable]
    internal struct CleanMusicInfoEx
    {
        [Serializable]
        public struct CleanEnsoDonChanBandRecordInfo
        {
            public int bestFanValue;
            public int clearCount;
            public int playCount;
        }

        [Serializable]
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
        public struct CleanEnsoWarRecordInfo
        {
            public int playCount;
            public int winCount;
        }

        public CleanEnsoDonChanBandRecordInfo donChanBandRecordInfo;
        public MusicFlags musicFlag;
        public bool isBattleSong => (this.musicFlag | MusicFlags.IsBattleSong) != 0;
        public bool isDownloaded => (this.musicFlag | MusicFlags.IsDownloaded) != 0;
        public bool IsNew => (this.musicFlag | MusicFlags.IsNew) != 0;

        public CleanEnsoRecordInfo[] normalRecordInfo;
        public int playCount;

        public const MusicFlags playlistMask = MusicFlags.IsPlaylist1 | MusicFlags.IsPlaylist2 |
                                               MusicFlags.IsPlaylist3 | MusicFlags.IsPlaylist4 | MusicFlags.IsPlaylist5;

        public bool isFavorite => (this.musicFlag | playlistMask) != 0;
        public int usageOrder;
        public CleanEnsoWarRecordInfo[] warRecordInfo;

        public Scripts.UserData.MusicInfoEx ToMusicInfoEx()
        {
            Scripts.UserData.MusicInfoEx musicInfo = new Scripts.UserData.MusicInfoEx();
            musicInfo.SetDefault();

            musicInfo.MusicFlag = this.musicFlag;
            musicInfo.playCount = this.playCount;
            musicInfo.usageOrder = this.usageOrder;

            musicInfo.donChanBandRecordInfo =
                new EnsoDonChanBandRecordInfo { bestFanValue = this.donChanBandRecordInfo.bestFanValue,
                                                clearCount = this.donChanBandRecordInfo.clearCount,
                                                playCount = this.donChanBandRecordInfo.playCount };

            EnsoRecordInfo[] ensoRecordInfos = new EnsoRecordInfo[5];
            EnsoData.EnsoLevelType j = 0;
            int k = 0;
            foreach (var record2 in this.normalRecordInfo)
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

        public void FromMusicInfoEx(MusicsData musicsData, int uniqueId)
        {
            this.musicFlag = musicsData.Datas[uniqueId].MusicFlag;
            this.playCount = musicsData.Datas[uniqueId].playCount;
            this.usageOrder = musicsData.Datas[uniqueId].usageOrder;

            this.donChanBandRecordInfo = new CleanEnsoDonChanBandRecordInfo {
                bestFanValue = musicsData.Datas[uniqueId].donChanBandRecordInfo.bestFanValue,
                clearCount = musicsData.Datas[uniqueId].donChanBandRecordInfo.clearCount,
                playCount = musicsData.Datas[uniqueId].donChanBandRecordInfo.playCount
            };

            this.normalRecordInfo = new CleanEnsoRecordInfo[5];
            for (EnsoData.EnsoLevelType i = 0; i < EnsoData.EnsoLevelType.Num; i++)
            {
                EnsoRecordInfo record;
                musicsData.GetNormalRecordInfo(uniqueId, i, out record);
                this.normalRecordInfo[(int)i] =
                    new CleanEnsoRecordInfo { allGood = record.allGood,
                                              cleared = record.cleared,
                                              crown = record.crown,
                                              ensoGamePlayCount = record.ensoGamePlayCount,
                                              normalHiScore =
                                                  new CleanHiScoreRecordInfo {
                                                      bad = record.normalHiScore.bad,
                                                      combo = record.normalHiScore.combo,
                                                      excellent = record.normalHiScore.excellent,
                                                      good = record.normalHiScore.good,
                                                      renda = record.normalHiScore.renda,
                                                      score = record.normalHiScore.score,
                                                  },
                                              playCount = record.playCount,
                                              shinuchiHiScore =
                                                  new CleanHiScoreRecordInfo {
                                                      bad = record.shinuchiHiScore.bad,
                                                      combo = record.shinuchiHiScore.combo,
                                                      excellent = record.shinuchiHiScore.excellent,
                                                      good = record.shinuchiHiScore.good,
                                                      renda = record.shinuchiHiScore.renda,
                                                      score = record.shinuchiHiScore.score,
                                                  },
                                              trainingPlayCount = record.trainingPlayCount };
            }

            this.warRecordInfo = new CleanEnsoWarRecordInfo[(int)EnsoData.WarPlayMode.Num];
            for (EnsoData.WarPlayMode i = 0; i < EnsoData.WarPlayMode.Num; i++)
            {
                EnsoWarRecordInfo warRecord;
                musicsData.GetWarRecordInfo(uniqueId, i, out warRecord);
                this.warRecordInfo[(int)i] =
                    new CleanEnsoWarRecordInfo { playCount = warRecord.playCount, winCount = warRecord.winCount };
            }
        }
    }

    internal abstract class SongMod
    {
        public bool enabled { get; set; }
        public string name { get; set; }
        protected string modFolder { get; set; }

        public abstract void AddMod(ModdedSongsManager manager);

        public abstract bool IsValidMod();

        public string GetModFolder()
        {
            return this.modFolder;
        }

        public abstract SongEntry GetSongEntry(string id, bool idIsSongFile = false);

        public abstract void SaveUserData(UserData userData);

        public abstract Scripts.UserData.MusicInfoEx LoadUserData();
    }
}
