﻿using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class CreateNewSong : MonoBehaviour
{
    [SerializeField] private SongList list;

    public void CreateSong() => PersistentUI.Instance.ShowInputBox("SongSelectMenu", "newmap.dialog", HandleNewSongName,
        "newmap.dialog.default");

    private void HandleNewSongName(string res)
    {
        if (string.IsNullOrWhiteSpace(res)) return;

        if (string.Compare(res, "chromapper chan", StringComparison.InvariantCultureIgnoreCase) == 0 || 
            string.Compare(res, "CM chan", StringComparison.InvariantCultureIgnoreCase) == 0)
        {
            Settings.Instance.Waifu = !Settings.Instance.Waifu;
            
            PersistentUI.Instance.DisplayMessage(
                Settings.Instance.Waifu
                    ? "CM Chan unlocked!"
                    : "CM Chan disabled...",
                PersistentUI.DisplayMessageType.Bottom);

            SceneTransitionManager.Instance.LoadScene("01_SongSelectMenu");
            return;
        }

        var song = new BeatSaberSong(list.WipLevels, res);

        if (list.Songs.Any(x => Path.GetFullPath(x.Directory).Equals(
            Path.GetFullPath(Path.Combine(
                list.WipLevels ? Settings.Instance.CustomWIPSongsFolder : Settings.Instance.CustomSongsFolder,
                song.CleanSongName)),
            StringComparison.CurrentCultureIgnoreCase
        )))
        {
            PersistentUI.Instance.ShowInputBox("SongSelectMenu", "newmap.dialog.duplicate", HandleNewSongName,
                "newmap.dialog.default");
            return;
        }

        var standardSet = new BeatSaberSong.DifficultyBeatmapSet();
        song.DifficultyBeatmapSets.Add(standardSet);
        BeatSaberSongContainer.Instance.SelectSongForEditing(song);
        PersistentUI.Instance.ShowDialogBox("SongSelectMenu", "newmap.message", null,
            PersistentUI.DialogBoxPresetType.Ok);
    }
}
