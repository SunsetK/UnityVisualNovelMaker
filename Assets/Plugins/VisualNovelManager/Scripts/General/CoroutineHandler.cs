using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VisualNovelManager.General
{
    public class CoroutineHandler : Singleton<CoroutineHandler> {
        IEnumerator enumerator = null;
        private void Coroutine(IEnumerator coro)
        {
            enumerator = coro;
            StartCoroutine(coro);
        }

        void Update()
        {
            if (enumerator != null) {
                if (enumerator.Current == null) {
                    Destroy(gameObject);
                }
            }
        }

        public void Stop()
        {
            StopCoroutine(enumerator.ToString());
            Destroy(gameObject);
        }

        public static GameObject Start_Coroutine(IEnumerator coro)
        {
            Instance.Coroutine(coro);
            return Instance.gameObject;
        }
    }
}