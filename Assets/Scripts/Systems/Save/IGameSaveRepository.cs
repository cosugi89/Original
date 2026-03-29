using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Systems.Save
{
    public interface IGameSaveRepository
    {
        string SavePath { get; }

        bool Exists();

        GameSaveData Load();

        void Save(GameSaveData saveData);
    }
}
