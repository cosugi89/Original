using System;
using System.ComponentModel;
using System.IO;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.Save
{
    public class JsonGameSaveRepository : IGameSaveRepository
    {
        [Description("保存ファイル名。")]
        public string SaveFileName => SaveFileNames.MainSaveFileName;

        [Description("保存ディレクトリパス。通常は Application.persistentDataPath を使う。")]
        public string SaveDirectoryPath => Application.persistentDataPath;

        [Description("実際の保存先フルパス。")]
        public string SavePath => Path.Combine(SaveDirectoryPath, SaveFileName);

        public bool Exists()
        {
            return File.Exists(SavePath);
        }

        public GameSaveData Load()
        {
            if (!Exists())
                return new GameSaveData();

            try
            {
                var json = File.ReadAllText(SavePath);
                return GameSaveJsonSerializer.Deserialize(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[JsonGameSaveRepository] Failed to load save data. Path: {SavePath}\n{ex}");
                return new GameSaveData();
            }
        }

        public void Save(GameSaveData saveData)
        {
            try
            {
                Directory.CreateDirectory(SaveDirectoryPath);
                var json = GameSaveJsonSerializer.Serialize(saveData);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonGameSaveRepository] Failed to save data. Path: {SavePath}\n{ex}");
            }
        }
    }
}
