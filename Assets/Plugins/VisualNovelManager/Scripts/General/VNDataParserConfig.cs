using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VisualNovelManager.General
{
    enum CommandKey{
        tmpId = 0,
        tag = 1,
        command = 2
    }

    public class VNDataParserConfig : Singleton<VNDataParserConfig>
    {
        public static readonly string _defaultPath = "/Plugins/VisualNovelManager/StoryScripts/Original/";
        public static readonly string _settingFileName = "VnDataParser.setting";
        public static readonly string _downloadPath = "https://docs.google.com/spreadsheets/d/{0}/gviz/tq?tqx=out:csv&sheet={1}";
        public static readonly string _url = "https://docs.google.com/spreadsheets/d/{0}/edit?usp=sharing";

        public static readonly string _config_prefKey_path = "DataParser_path";
        public static readonly string _config_prefKey_exportPath = "DataParser_exportPath";

        public static readonly string _originalExtensionName = ".csv";
        public static readonly string _nonuseExtensionName = ".meta";
        public static readonly string _vnExtensionName = ".vn";

        public static readonly List<string> _needRemoveCharacter = new List<string>() { "\"" };

        public static GameObject getObj()
        {
            return Instance.gameObject;
        }

        public static CommandTag GetTag(string tag)
        {
            switch (tag) {
                case "Label":
                    return CommandTag.Label;
                case "InfoText":
                    return CommandTag.InfoText;
                case "End":
                    return CommandTag.End;
                case "Show":
                    return CommandTag.Show;
                case "Bg":
                    return CommandTag.Bg;
                case "Image":
                    return CommandTag.Image;
                case "Menu":
                    return CommandTag.Menu;
                case "-":
                    return CommandTag.Selector;
                case "MenuEnd":
                    return CommandTag.MenuEnd;
                case "JumpToLabel":
                    return CommandTag.JumpToLabel;
                case "Action":
                    return CommandTag.Action;
                case "CreateCharacter":
                    return CommandTag.CreateCharacter;
                case "AddShell":
                    return CommandTag.AddShell;
                default:
                    return CommandTag.Text;
            }
        }

        public static bool IsUseableFile(string fileName)
        {
            if (fileName.Contains(_originalExtensionName) && fileName.Contains(_originalExtensionName) && !fileName.Contains(_nonuseExtensionName)) {
                return true;
            }

            return false;
        }

        public static string MakeClear(string str)
        {
            for(int i = 0; i < _needRemoveCharacter.Count; i ++)
                str = str.Replace(_needRemoveCharacter[i], "");

            return str;
        }

        public static string GetImportPath()
        {
            return Instance.GetValue(_config_prefKey_path, _defaultPath);
        }

        public static string GetExporttPath()
        {
            return Instance.GetValue(_config_prefKey_exportPath, _defaultPath);
        }

        public static void SetImportPath(string path)
        {
            Instance.SetValue(_config_prefKey_path, path);
        }

        public static void SetExporttPath(string path)
        {
            Instance.SetValue(_config_prefKey_exportPath, path);
        }

        public static void SetDefault()
        {
            Instance.SetValue(_config_prefKey_path, _defaultPath);
            Instance.SetValue(_config_prefKey_exportPath, _defaultPath);
        }

        private string GetValue(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        private void SetValue(string key, string value = "")
        {
            PlayerPrefs.SetString(key, value);
        }
    }

    [Serializable]
    public class sheetData
    {
        public string _id;
        public string _sheet;
        public string _tag;

        public sheetData()
        {
        }

        public sheetData(string id, string sheet, string tag = "")
        {
            _id = id;
            _sheet = sheet;
            _tag = tag;
        }
    }
}