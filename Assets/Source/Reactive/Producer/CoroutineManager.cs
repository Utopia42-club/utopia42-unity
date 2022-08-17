using System;
using System.Collections;
using UnityEngine;

namespace Source.Reactive.Producer
{
    internal static class CoroutineManager
    {
        internal static Coroutine StartCoroutine(IEnumerator enumerator, Action complete)
        {
            return Behaviour.Instance().Execute(enumerator, complete);
        }

        internal static void StopCoroutine(Coroutine coroutine)
        {
            Behaviour.Instance().StopCoroutine(coroutine);
        }

        private class Behaviour : MonoBehaviour
        {
            private static Behaviour instance;


            internal Coroutine Execute(IEnumerator enumerator, Action complete)
            {
                return StartCoroutine(DoExecute(enumerator, complete));
            }

            internal IEnumerator DoExecute(IEnumerator enumerator, Action complete)
            {
                yield return enumerator;
                complete();
            }

            internal static Behaviour Instance()
            {
                if (instance == null)
                    instance = new GameObject("Coroutine Manager").AddComponent<Behaviour>();
                return instance;
            }
        }
    }
}