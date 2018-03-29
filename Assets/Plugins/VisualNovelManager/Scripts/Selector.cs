using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovelManager
{
    public class Selector : MonoBehaviour
    {
        VNCommand _command;

        public void Init(VNCommand command)
        {
            _command = command;
            transform.Find("Text").GetComponent<Text>().text = _command._command;
        }

        public void OnClick()
        {
            VNParser.Instance.PopCommandSet(_command._linkName);
            VisualNovelManager.Instance.CloseSelectMenu();
        }
    }
}
