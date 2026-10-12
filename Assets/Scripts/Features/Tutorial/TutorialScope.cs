using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using UnityEngine;

namespace Assets.Scripts.Features.Tutorial
{
    [Serializable]
    public sealed class TutorialTargetBinding
    {
        public string Key;
        public GameObject Target;
    }

    /// <summary>画面インスタンス単位の操作対象。非表示時に候補と参照が失効する。</summary>
    public sealed class TutorialScope : MonoBehaviour
    {
        [SerializeField] private string scopeKey;
        [SerializeField] private TutorialSignal readySignal;
        [SerializeField] private List<TutorialTargetBinding> targets = new();
        public string ScopeKey => scopeKey;
        public bool IsReady { get; private set; }
        public int Generation { get; private set; }

        private void OnEnable()
        {
            Generation++;
            StartCoroutine(PrepareRoutine());
        }

        private IEnumerator PrepareRoutine()
        {
            // OnEnable内で行う一覧生成やレイアウト更新を待つ。
            yield return null;
            Canvas.ForceUpdateCanvases();
            // TutorialSystemを配置していないSceneでも、共通DialogのScopeは利用できる。
            var manager = TutorialManager.Instance;
            if (manager == null) yield break;
            IsReady = true;
            manager.Register(this);
            if (readySignal != TutorialSignal.None)
                TutorialManager.Report(readySignal, scopeKey, gameObject);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            IsReady = false;
            if (TutorialManager.Current != null)
                TutorialManager.Current.Unregister(this);
        }

        public void Bind(string key, GameObject target)
        {
            var binding = targets.Find(item => item.Key == key);
            if (binding == null) targets.Add(new TutorialTargetBinding { Key = key, Target = target });
            else binding.Target = target;
        }

        public GameObject Resolve(string key)
        {
            var target = targets.Find(item => item.Key == key)?.Target;
            return target != null && target.activeInHierarchy ? target : null;
        }
    }
}
