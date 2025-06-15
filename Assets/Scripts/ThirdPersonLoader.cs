using System;
using System.IO;
using UnityEngine;
using ReadyPlayerMe.Core;

namespace ReadyPlayerMe.Samples.QuickStart
{
    public class ThirdPersonLoader : MonoBehaviour
    {
        [SerializeField] private string avatarUrl;
        public string AvatarUrl => avatarUrl;

        [SerializeField] private RuntimeAnimatorController animatorController;
        [SerializeField] private GameObject previewAvatar;
        [SerializeField] private GameObject loadingScreen;

        private GameObject avatar;
        private AvatarObjectLoader avatarObjectLoader;
        public event Action OnLoadComplete;

        private bool isLoading = false;

        private void Start()
        {
            avatarObjectLoader = new AvatarObjectLoader();
            avatarObjectLoader.OnCompleted += OnLoadCompleted;
            avatarObjectLoader.OnFailed += OnLoadFailed;

            if (previewAvatar != null)
            {
                SetupAvatar(previewAvatar);
            }

            // Pobierz zapisany URL
            string savedUrl = PlayerPrefs.GetString("AvatarURL", "").Trim();
            if (!string.IsNullOrEmpty(savedUrl))
            {
                avatarUrl = savedUrl;
                LoadAvatar(avatarUrl);
            }
            else
            {
                Debug.LogWarning("❌ Brak zapisanego URL avatara. Sprawdź czy został ustawiony w Main Menu.");
            }

            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }
        }

        public void LoadAvatar(string url)
        {
            if (isLoading)
            {
                Debug.Log("⏳ Avatar już się ładuje – pomijam duplikat.");
                return;
            }

            isLoading = true;

            string path = Application.dataPath + "/Ready Player Me/Avatars";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                foreach (var file in Directory.GetFiles(path, "*.glb"))
                {
                    File.Delete(file);
                }
                Debug.Log("🧹 Usunięto stare pliki .glb");
            }
            catch (Exception e)
            {
                Debug.LogWarning("⚠️ Nie udało się wyczyścić folderu avatara: " + e.Message);
            }

            avatarObjectLoader.LoadAvatar(url);
        }

        private void OnLoadCompleted(object sender, CompletionEventArgs args)
        {
            isLoading = false;

            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }

            if (previewAvatar != null)
            {
                Destroy(previewAvatar);
                previewAvatar = null;
            }

            SetupAvatar(args.Avatar);

            // Szukamy obiektu z Renderer_Avatar
            var meshRenderer = avatar.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (meshRenderer != null)
            {
                var lipSyncObj = meshRenderer.gameObject;
                var lipSync = lipSyncObj.AddComponent<SimpleLipSync>();
                lipSync.audioSource = FindAnyObjectByType<AudioSource>();
                lipSync.mouthOpenBlendshapeName = "jawOpen";
                lipSync.maxOpen = 10f;
            }
            else
            {
                Debug.LogWarning("⚠️ Renderer_Avatar nie znaleziony w hierarchii avatara.");
            }

            OnLoadComplete?.Invoke();
        }

        private void OnLoadFailed(object sender, FailureEventArgs args)
        {
            isLoading = false;

            Debug.LogError("❌ Nie udało się załadować avatara: " + args.Message);
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }

            OnLoadComplete?.Invoke();
        }

        private void SetupAvatar(GameObject targetAvatar)
        {
            if (avatar != null)
            {
                Destroy(avatar);
            }

            avatar = targetAvatar;
            avatar.transform.parent = transform;
            avatar.transform.localPosition = Vector3.zero;
            avatar.transform.localRotation = Quaternion.Euler(0, 180, 0);

            var animator = avatar.GetComponent<Animator>();
            if (animator != null && animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
            }
            else
            {
                Debug.LogWarning("⚠️ Brakuje Animatora lub AnimatorController do przypisania.");
            }

            var controller = GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.Setup(avatar, animatorController);
            }
        }
    }
}
