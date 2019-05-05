using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
 
namespace Drones.Managers
{
    public class SaveManager : MonoBehaviour
    {
        //void Start()
        //{
        //    SaveGlobalFile();
        //    LoadGlobalFile();
        //}

        //public void SaveGlobalFile()
        //{
        //    string destination = Application.persistentDataPath + "/global.dat";
        //    FileStream file;

        //    if (File.Exists(destination)) file = File.OpenWrite(destination);
        //    else file = File.Create(destination);

        //    BinaryFormatter bf = new BinaryFormatter();
        //    bf.Serialize(file, data);
        //    file.Close();
        //}

        //public void LoadGlobalFile()
        //{
        //    string destination = Application.persistentDataPath + "/global.dat";
        //    FileStream file;

        //    if (File.Exists(destination)) file = File.OpenRead(destination);
        //    else
        //    {
        //        Debug.LogError("File not found");
        //        return;
        //    }

        //    BinaryFormatter bf = new BinaryFormatter();
        //    file.Close();
        //}

    }
}