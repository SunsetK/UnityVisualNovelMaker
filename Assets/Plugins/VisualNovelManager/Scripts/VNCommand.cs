using System.Collections;
using System.Collections.Generic;
using System;

namespace VisualNovelManager
{
    [Serializable]
    public class VNCommand
    {
        int _id;
        public CommandTag _tag { get; private set; }
        public string _name { get; private set; }
        public string _command { get; private set; }
        public string _linkName { get; private set; }

        public VNCommand(int id, CommandTag tag, string command = null, string name = null, string linkName = null)
        {
            _id = id;
            _tag = tag;
            _command = command;
            _name = name;
            _linkName = linkName;

            if (tag == CommandTag.JumpToLabel) {
                _linkName = _command;
                _command = null;
            }
        }

        public void Init(int id, CommandTag tag, string command = null)
        {
            _id = id;
            _tag = tag;
            _command = command;
        }
    }
}