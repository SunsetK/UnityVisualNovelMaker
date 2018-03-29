using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VisualNovelManager
{
    public class InfoBox : MonoBehaviour
    {
        VNCommand _command;

        public void Init(VNCommand command)
        {
            _command = command;
            transform.Find("Text").GetComponent<Text>().text = _command._command;
        }
    }
}