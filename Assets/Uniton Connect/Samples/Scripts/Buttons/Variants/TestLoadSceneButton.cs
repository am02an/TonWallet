using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnitonConnect.Core.Demo
{
    public sealed class TestLoadSceneButton : TestBaseButton
    {
        [SerializeField, Space] private int _sceneId;

        public sealed override void OnClick()
        {
            LoadingScreen.Instance.StartLoading();
            SceneManager.LoadScene(_sceneId);
        }
    }
}