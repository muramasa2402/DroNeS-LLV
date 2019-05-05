using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System;
using Drones.UI;

namespace Drones.Managers
{
    using Serializable;
    using static Singletons;
    public static class SaveManager
    {
        //TODO Change to windows
        public static readonly string savePath = Path.Combine(new string[] { Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Documents", "DroNeS" });

        private static void Obfuscate(string filepath, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            for (int i = 0; i < bytes.Length; i++) bytes[i] ^= 0x5a;
            File.WriteAllText(filepath, Convert.ToBase64String(bytes));
        }

        private static string Deobfuscate(string filepath)
        {
            var bytes = Convert.FromBase64String(File.ReadAllText(filepath));
            for (int i = 0; i < bytes.Length; i++) bytes[i] ^= 0x5a;
            return Encoding.UTF8.GetString(bytes);
        }

        public static string FilePath(string filename)
        {
            return Path.Combine(savePath, filename + ".drn");
        }

        public static string FileName(string filepath)
        {
            return Path.GetFileName(filepath);
        }

        public static string FileNameNoExtension(string filepath)
        {
            return Path.GetFileNameWithoutExtension(filepath);
        }

        public static void OpenSaveWindow()
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            GameObject win = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Windows/SaveLoad/SaveLoad Window") as GameObject);
            win.transform.SetParent(UICanvas, false);
            win.GetComponent<SaveLoadWindow>().SetSaveMode();
        }

        public static void OpenLoadWindow()
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            GameObject win = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Windows/SaveLoad/SaveLoad Window") as GameObject);
            win.transform.SetParent(UICanvas, false);
            win.GetComponent<SaveLoadWindow>().SetLoadMode();
        }

        public static void OpenOverwriteConfirmation(string filepath)
        {
            GameObject win = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Windows/SaveLoad/Overwrite Dialog") as GameObject);
            win.transform.SetParent(UICanvas, false);
            win.GetComponent<OverwriteConfirmation>().SetFilepath(filepath);
        }

        public static void Save(string filepath)
        {
            Obfuscate(filepath, JsonUtility.ToJson(SimManager.SerializeSimulation()));
        }

        public static void Load(string filepath)
        {
            string data = Deobfuscate(filepath);
            SimManager.LoadSimulation(JsonUtility.FromJson<SSimulation>(data));
        }

    }
}