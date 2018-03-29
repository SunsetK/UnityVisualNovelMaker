using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace VisualNovelManager.General
{
    public class VNBinaryManager
    {
        public static object LoadFile(string path)
        {
            object target = null;

            if (File.Exists(path)) {
                BinaryFormatter bf = new BinaryFormatter();
                Stream streamRead = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                target = bf.Deserialize(streamRead);
                streamRead.Close();
            }

            return target;
        }

        public static void SaveFile(string path, object target)
        {
            BinaryFormatter bf = new BinaryFormatter();
            Stream file = File.Create(path);
            bf.Serialize(file, target);
            file.Close();
        }
    }
}
