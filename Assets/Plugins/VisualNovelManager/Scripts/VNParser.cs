using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using VisualNovelManager.General;
using System.Runtime.Serialization.Formatters.Binary;

namespace VisualNovelManager
{
    public enum CommandTag
    {
        Text,
        Label,
        End,
        Show,
        Bg,
        Image,
        Menu,
        Selector,
        MenuEnd,
        JumpToLabel,
        Action,
        CreateCharacter,
        AddShell,
        InfoText,
    }

    public class VNParser : Singleton<VNParser>
    {
        Dictionary<string, Queue<VNCommand>> _vnCommandSet;
        List<VNCommand> _selectMenu;
        List<object> _globalValue;
        List<object> _localValue;
        Queue<VNCommand> _vnMainQueue;
        VNCommand _currentCommand;
        bool _isStarted;
        bool _goNext;

        public void StartStory(string path, string fileName, string label = null)
        {
            Load(path, fileName);
            StartStory(label);
        }

        public void StartStory(string label = null)
        {
            if (_vnCommandSet == null)
                Debug.LogWarning("스크립트 파일이 로드되지 않았습니다.");

            if (label == null)
                label = "Start";

            _selectMenu = new List<VNCommand>();
            _vnMainQueue = _vnCommandSet[label];

            //foreach (var item in _vnMainQueue)
            //    Debug.Log(item._tag + " : " + item._command);

            _goNext = true;
            _isStarted = true;
        }

        public void EndStory()
        {
            Debug.Log("끝.");
        }

        public void PopCommandSet(string labelName)
        {
            var tmp = new Queue<VNCommand>(_vnMainQueue);
            _vnMainQueue = _vnCommandSet[labelName];

            foreach (var item in tmp) {
                _vnMainQueue.Enqueue(item);
                Debug.Log(item._tag + " : " + item._command);
            }

            GoNext();
            _currentCommand = _vnMainQueue.Dequeue();
            VisualNovelManager.Instance.GoNextLine();
        }

        public VNCommand GetCurrentCommand()
        {
            return _currentCommand;
        }

        public void StartCommand()
        {
            switch (_currentCommand._tag) {
                case CommandTag.InfoText:
                    VisualNovelManager.Instance.ShowInfoBox(_currentCommand);
                    GoNext();
                    break;
                case CommandTag.End:
                    break;
                case CommandTag.Show:
                    GoNext();
                    break;
                case CommandTag.Bg:
                    GoNext();
                    break;
                case CommandTag.Image:
                    GoNext();
                    break;
                case CommandTag.Menu:
                    _selectMenu.Clear();
                    GoNext();
                    break;
                case CommandTag.Selector:
                    _selectMenu.Add(_currentCommand);
                    GoNext();
                    break;
                case CommandTag.MenuEnd:
                    VisualNovelManager.Instance.ShowSelectMenu(new List<VNCommand>(_selectMenu));
                    _goNext = false;
                    break;
                case CommandTag.JumpToLabel:
                    PopCommandSet(_currentCommand._linkName);
                    break;
                case CommandTag.Action:
                    break;
                case CommandTag.CreateCharacter:
                    break;
                case CommandTag.AddShell:
                    break;
                default:
                    GoNext();
                    break;
            }
        }

        public void GoNext()
        {
            _goNext = true;
        }

        public void Load(string path, string fileName)
        {
            if (!Directory.Exists(path)) {
                Debug.LogError(path + "\n존재하지 않는 경로.");
                return;
            }

            if (_vnCommandSet == null)
                _vnCommandSet = new Dictionary<string, Queue<VNCommand>>();

            BinaryFormatter bf = new BinaryFormatter();
            Stream streamRead = new FileStream(path + fileName + ".vn", FileMode.Open, FileAccess.Read, FileShare.None);

            _vnCommandSet.Clear();
            _vnCommandSet = (Dictionary<string, Queue<VNCommand>>)bf.Deserialize(streamRead);
            streamRead.Close();
        }

        private void Update()
        {
            if (!_isStarted)
                return;

            if (_goNext && _vnMainQueue.Count > 0) {
                _currentCommand = _vnMainQueue.Dequeue();
                Debug.Log("current tag : " + GetCurrentCommand()._tag + ", " + GetCurrentCommand()._command);
                if (_currentCommand._tag == CommandTag.Text ||
                    _currentCommand._tag == CommandTag.Menu)
                    _goNext = false;
                else
                    StartCommand();
                
            } else if (_vnMainQueue.Count == 0) {
                _isStarted = false;
                _goNext = false;
                EndStory();
            }
        }
    }
}