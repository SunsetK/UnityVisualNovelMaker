/*
 * https://github.com/SunsetK/UnityVisualNovelMaker
 * 
 * 
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using VisualNovelManager.General;
using System.Runtime.Serialization.Formatters.Binary;

namespace VisualNovelManager
{
    public class VisualNovelManager : Singleton<VisualNovelManager>
    {
        [SerializeField]
        string _FolderPath = "/Plugins/VisualNovelManager/StoryScripts/";

        [SerializeField]
        Text _commandTag;
        [SerializeField]
        Text _commandTextArea;
        [SerializeField]
        Image _touchArea;
        [SerializeField]
        GameObject _selectPanel;
        [SerializeField]
        GameObject _selector;
        [SerializeField]
        InfoBox _infoPanel;

        VNParser parser;

        bool _isSelectWait;

        public void Start()
        {
            parser = VNParser.Instance;
            if(_touchArea != null)
                _touchArea.gameObject.AddComponent<Button>().onClick.AddListener(()=> GoNextLine());

            // test
            LoadScript("sheet");
            StartStory();
        }

        public void Update()
        {
            if (_isSelectWait)
                return;

            if (Input.GetKeyDown(KeyCode.Return))
                GoNextLine();

            if(_touchArea == null)
                if(Input.GetMouseButtonDown(0))
                    GoNextLine();
        }

        public void CloseSelectMenu()
        {
            var child = _selectPanel.transform.GetComponentsInChildren<Selector>();

            foreach (var item in child) {
                if (item.name != "Selector(Template)")
                    Destroy(item.gameObject);
            }
            _selectPanel.SetActive(false);
            _isSelectWait = false;
        }

        public void ShowSelectMenu(List<VNCommand> selects)
        {
            _selectPanel.SetActive(true);
            _isSelectWait = true;

            for (int i = 0; i < selects.Count; i++) {
                var selector = Instantiate(_selector, _selectPanel.transform);
                selector.name = selects[i]._linkName;
                selector.GetComponent<Selector>().Init(selects[i]);
                selector.gameObject.SetActive(true);
            }
        }

        public void ShowInfoBox(VNCommand command)
        {
            _infoPanel.gameObject.SetActive(true);
            _infoPanel.Init(command);
        }

        public void HideInfoBox()
        {
            _infoPanel.gameObject.SetActive(false);
        }

        public void LoadScript(string script)
        {
            parser.Load(Application.dataPath + _FolderPath, script);
        }

        public void StartStory()
        {
            parser.StartStory();
            //GoNextLine();
        }

        public void EndStory()
        {
        }

        public void GoNextLine()
        {
            if (_infoPanel.gameObject.activeSelf)
                _infoPanel.gameObject.SetActive(false);

            if (parser.GetCurrentCommand()._tag == CommandTag.Text) {
                _commandTag.text = parser.GetCurrentCommand()._name;
                _commandTextArea.text = parser.GetCurrentCommand()._command;
                parser.GoNext();
            } else {
                parser.StartCommand();
            }
        }
    }
}