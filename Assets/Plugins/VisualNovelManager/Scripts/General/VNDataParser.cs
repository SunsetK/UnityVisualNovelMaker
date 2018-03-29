using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using config = VisualNovelManager.General.VNDataParserConfig;

namespace VisualNovelManager.General
{
    public class VNDataParser : EditorWindow
    {
#if UNITY_EDITOR
        static string _settingFilePath = "";
        static string _dirPath = "";
        static string _exportPath = "";
        static int _dictId = 0;

        GameObject _configObj;
        GameObject _coroutinHandlerObj;

        /// <summary>
        /// 스크립트 파일하나를 저장 할 커맨드 셋
        /// </summary>
        static Dictionary<string, Queue<VNCommand>> vnCommandSet = null;

        /// <summary>
        /// 시트정보(세팅정보)리스트
        /// </summary>
        private List<sheetData> sheetDataSet = new List<sheetData>();

        // GUI area
        Vector2 scrollPosition = Vector2.zero;

        private void OnEnable()
        {
            _configObj = config.getObj();

            // 기본설정
            _dirPath = config.GetImportPath();
            _exportPath = config.GetExporttPath();
            _settingFilePath = Application.dataPath + _exportPath + config._settingFileName;

            // VnCommandSet 초기화
            if (vnCommandSet == null) {
                vnCommandSet = new Dictionary<string, Queue<VNCommand>>();
            } else {
                vnCommandSet.Clear();
            }

            // 세팅파일 가져오기
            LoadSetting();
        }

        private void OnDisable()
        {
            // save.
            SaveSetting();

            // temporary object destroy.
            if (_coroutinHandlerObj != null)
                DestroyImmediate(_coroutinHandlerObj);
            DestroyImmediate(_configObj);
        }

        [MenuItem("Tools/VNDataParser")]
        static private void Init()
        {
            VNDataParser window = EditorWindow.GetWindow<VNDataParser>();
            window.position = new Rect(100, 100, 1000, 500);
        }

        private void OnGUI()
        {
            // 경로 관련 영역.
            GUILayout.Label("<< Original file path & export path >>", EditorStyles.boldLabel);
            _dirPath = EditorGUILayout.TextField("Impot Path", _dirPath);
            _exportPath = EditorGUILayout.TextField("Export Path", _exportPath);

            GUILayout.BeginHorizontal();
            GUILayout.Space(100f);
            if (GUILayout.Button("Save Path")) {
                config.SetImportPath(_dirPath);
                config.SetExporttPath(_exportPath);
            }
            GUILayout.Space(100f);
            if (GUILayout.Button("Set Default Path")) {
                config.SetDefault();
                _dirPath = config.GetImportPath();
                _dirPath = config.GetExporttPath();
            }
            GUILayout.Space(100f);
            GUILayout.EndHorizontal();

            // 시트 데이터 관련 영역 시작.==========
            GUILayout.Space(20f);
            GUILayout.Label("<< Google spread sheet Id & sheet name >> ", EditorStyles.boldLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(1000), GUILayout.Height(300));
            for (int i = 0; i < sheetDataSet.Count; i ++) {
                int index = i;
                GUILayout.BeginHorizontal();
                // 시트 주소 id
                var textDimensions = GUI.skin.label.CalcSize(new GUIContent("[" + string.Format("{0:D3}", index) + "] ID "));
                EditorGUIUtility.labelWidth = textDimensions.x;
                sheetDataSet[index]._id = EditorGUILayout.TextField("[" + index + "] ID", sheetDataSet[index]._id);
                // 시트 이름
                textDimensions = GUI.skin.label.CalcSize(new GUIContent("Sheet Name "));
                EditorGUIUtility.labelWidth = textDimensions.x;
                sheetDataSet[index]._sheet = EditorGUILayout.TextField("Sheet Name", sheetDataSet[index]._sheet, GUILayout.Width(200));
                // 시트 태그(필터링용)
                textDimensions = GUI.skin.label.CalcSize(new GUIContent("Tag "));
                EditorGUIUtility.labelWidth = textDimensions.x;
                sheetDataSet[index]._tag = EditorGUILayout.TextField("Tag", sheetDataSet[index]._tag, GUILayout.Width(100));
                // 시트정보 삭제 버튼
                if (GUILayout.Button("X", GUILayout.Width(30))) {
                    sheetDataSet.Remove(sheetDataSet[index]);
                }
                // 시트오픈 버튼
                if (GUILayout.Button("Open", GUILayout.Width(50))) {
                    Application.OpenURL(string.Format(config._url, sheetDataSet[index]._id));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space(300f);
            // 시트 정보 아이템 추가
            if (GUILayout.Button("Add Item")) {
                sheetDataSet.Add(new sheetData());
            }
            GUILayout.Space(300f);
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            //==========시트 데이터 관련 영역 끝.

            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(200f);
            // 파싱 시작 버튼.
            if (GUILayout.Button("Parse All")) {
                _coroutinHandlerObj = CoroutineHandler.Start_Coroutine(Download());
            }
            GUILayout.Space(200f);
            GUILayout.EndHorizontal();
        }

        private IEnumerator Download()
        {
            yield return DownloadCvs();
            Parse();
        }

        private void Parse()
        {
            string directoryPath = Application.dataPath + _dirPath;

            if (!Directory.Exists(directoryPath)) {
                Debug.LogError(directoryPath + "\n존재하지 않는 경로.");
                return;
            }

            var dInfo = new DirectoryInfo(directoryPath);
            foreach (var file in dInfo.GetFiles()) {
                if (!config.IsUseableFile(file.Name))
                    continue;

                string strFile = directoryPath + file.Name;
                FileStream fs = new FileStream(strFile, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.UTF8, false);
                Queue<string> storyStream = new Queue<string>();
                string strLineValue = null;

                while ((strLineValue = sr.ReadLine()) != null) {
                    //strLineValue = config.MakeClear(strLineValue);
                    storyStream.Enqueue(strLineValue);
                }

                ReadData(storyStream);
                foreach (var item in vnCommandSet) {
                    Debug.Log("============================= key : " + item.Key + " =============================");
                    foreach (var it2 in item.Value) {
                        Debug.Log(it2._tag + " : " + it2._command);
                    }
                }
                SaveData(file.Name.Replace(config._originalExtensionName, config._vnExtensionName));
                _dictId = 0;

                sr.Close();
                fs.Close();
            }
        }

        private void ReadData(Queue<string> stream, bool instantly = false, string label = null)
        {
            Queue<VNCommand> commands = new Queue<VNCommand>();

            int id = 0;
            bool breakReservation = false; // 루프를 빠져나갈것을 예약하는 유무.
            bool menuStart = false;
            List<string> keys = new List<string>();
            string labelName = "";
            string strLineValue = null;

            string command = null;
            string name = null;
            CommandTag tag = CommandTag.Text;

            if (label != null)
                labelName = label;

            Debug.Log(labelName + "  menuStart : " + menuStart);
            Debug.Log("--------------------------------------------------------");

            while (stream.Count> 0 && (strLineValue = stream.Dequeue()) != null) {
                // 다음 큐를 확인해서, 셀렉터나 메뉴끝이라면 루프브레이크를 true로 해 준다.
                if (stream.Count > 0 && NeedBreakReservation(stream.Peek(), instantly)) {
                    breakReservation = true;
                }

                if (!strLineValue.Substring(1, 1).Equals("#")) {
                    keys = GetSplitedKeyValues(strLineValue);
                }
                
                if (keys != null && keys.Count > 1) {
                    tag = config.GetTag(keys[(int)CommandKey.tag]);
                    command = keys[(int)CommandKey.command];
                    name = keys[(int)CommandKey.tag];
                    command = command.Replace("\\n", "\n");

                    Debug.Log("[" + labelName + "]  " + tag + " : " + command);

                    switch (tag) {
                        case CommandTag.Text:
                            commands.Enqueue(new VNCommand(id++, tag, command, name));
                            break;
                        case CommandTag.Label:
                            labelName = command;
                            break;
                        case CommandTag.End:
                            vnCommandSet.Add(labelName, new Queue<VNCommand>(commands));
                            commands.Clear();
                            break;
                        case CommandTag.InfoText:
                            commands.Enqueue(new VNCommand(id++, tag, command));
                            break;
                        case CommandTag.Show:
                            commands.Enqueue(new VNCommand(id++, tag, command));
                            break;
                        case CommandTag.Bg:
                            commands.Enqueue(new VNCommand(id++, tag, command));
                            break;
                        case CommandTag.Image:
                            commands.Enqueue(new VNCommand(id++, tag, command));
                            break;
                        case CommandTag.Menu:
                            Debug.Log(labelName + " Is memu");
                            commands.Enqueue(new VNCommand(id++, tag));
                            menuStart = true;
                            break;
                        case CommandTag.Selector:
                            if((instantly && menuStart) || !instantly) {
                                commands.Enqueue(new VNCommand(id++, tag, command, linkName: labelName + _dictId));
                                ReadData(stream, true, labelName + _dictId++);
                            } else if (instantly) {
                                vnCommandSet.Add(labelName, new Queue<VNCommand>(commands));
                                commands.Clear();
                                return;
                            }
                            break;
                        case CommandTag.MenuEnd:
                            Debug.Log("menu end : " + labelName + "  instantly : " + instantly + "  menuStart : " + menuStart);
                            if ((instantly && menuStart) || !instantly) {
                                commands.Enqueue(new VNCommand(id++, tag));
                            } else if (instantly) {
                                vnCommandSet.Add(labelName, new Queue<VNCommand>(commands));
                                return;
                            }
                            menuStart = false;
                            break;
                        case CommandTag.JumpToLabel:
                            commands.Enqueue(new VNCommand(id++, tag, command));
                            break;
                        case CommandTag.Action:
                            commands.Enqueue(new VNCommand(id++, tag, command));
                            break;
                        case CommandTag.CreateCharacter:
                            break;
                        case CommandTag.AddShell:
                            break;
                        default:
                            continue;
                    }

                    keys = null;
                }

                if (breakReservation && !menuStart) {
                    vnCommandSet.Add(labelName, new Queue<VNCommand>(commands));
                    commands.Clear();
                    return;
                }
            }

            if (commands != null && !vnCommandSet.ContainsKey(labelName)) {
                vnCommandSet.Add(labelName, new Queue<VNCommand>(commands));
            }
        }

        private List<string> GetSplitedKeyValues(string str)
        {
            List<string> tmp = new List<string>();
            string tmpString = "";
            bool startAdd = false;

            for (int i = 0; i < str.Length; i++) {
                if ((str[i] == '\"' && i < str.Length-1 && str[i+1] == ',')) {
                    startAdd = false;
                    tmp.Add(tmpString);
                    tmpString = "";
                }

                if (str[i] == '\"' || (str[i] == ',' && i > 0 && str[i-1] == '\"')) {
                    startAdd = true;
                    continue;
                }

                if (startAdd)
                    tmpString += str[i];
            }

            return tmp;
        }

        private bool NeedBreakReservation(string check, bool isInstantly)
        {
            List<string>keys = GetSplitedKeyValues(check);
            CommandTag tag = config.GetTag(keys[(int)CommandKey.tag]);

            if ((tag == CommandTag.Selector || tag == CommandTag.MenuEnd) && isInstantly) //  || tag == CommandTag.MenuEnd
                return true;

            return false;
        }

        private void SaveData(string fileName)
        {
            string filePath = Application.dataPath + _exportPath + fileName;
            VNBinaryManager.SaveFile(filePath, vnCommandSet);
            
            _dictId = 0;
            vnCommandSet.Clear();

            Debug.Log("Save success! : " + filePath);
        }

        private IEnumerator DownloadCvs()
        {
            for (int i = 0; i < sheetDataSet.Count; i++) {
                if (string.IsNullOrEmpty(sheetDataSet[i]._id) || string.IsNullOrEmpty(sheetDataSet[i]._sheet))
                    continue;

                string str = string.Format(config._downloadPath, sheetDataSet[i]._id, sheetDataSet[i]._sheet);

                Debug.Log("[" + sheetDataSet[i]._sheet + "] Waite for Download... : " + str);

                WWW www = new WWW(str);
                yield return www;

                Debug.Log("[" + sheetDataSet[i]._sheet + "] Download Success!");

                System.IO.File.WriteAllBytes(Application.dataPath + _dirPath + sheetDataSet[i]._sheet + VNDataParserConfig._originalExtensionName, www.bytes);
            }
        }

        private void LoadSetting()
        {
            var tmp = (List<sheetData>)VNBinaryManager.LoadFile(_settingFilePath);
            if(tmp != null)
                sheetDataSet = tmp;
        }

        private void SaveSetting()
        {
            Debug.Log("save setting.");
            VNBinaryManager.SaveFile(_settingFilePath, sheetDataSet);
        } 
#endif
    }
}