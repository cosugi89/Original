using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Systems.Save
{
    public interface IUserDataStore
    {
        string SavePath { get; }

        bool Exists();

        UserData Load();

        void Save(UserData userData);
    }
}
