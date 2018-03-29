using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace VisualNovelManager.General
{
    [Serializable]
    public class CommandSet : MonoBehaviour
    {
        public Dictionary<int, Queue<VNCommand>> vnCommandSet = null;

        public CommandSet(Dictionary<int, Queue<VNCommand>> tmp)
        {
            vnCommandSet = tmp;
        }
    }
}