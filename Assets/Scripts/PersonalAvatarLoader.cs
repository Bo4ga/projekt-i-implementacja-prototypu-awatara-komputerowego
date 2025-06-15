using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe.Samples.QuickStart
{
    public class PersonalAvatarLoader : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Text openPersonalAvatarPanelButtonText;
        [SerializeField] private Text linkText;
        [SerializeField] private InputField avatarUrlField;
        [SerializeField] private Button openPersonalAvatarPanelButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button linkButton;
        [SerializeField] private Button loadAvatarButton;
        [SerializeField] private GameObject personalAvatarPanel;

        [Header("Avatar Loader")]
        [SerializeField] private ThirdPersonLoader thirdPersonLoader;

        private void Start()
        {
            Debug.Log("🔧 PersonalAvatarLoader initialized.");
        }

        private void OnEnable()
        {
            openPersonalAvatarPanelButton.onClick.AddListener(OnOpenPersonalAvatarPanel);
            closeButton.onClick.AddListener(OnCloseButton);
            linkButton.onClick.AddListener(OnLinkButton);
            loadAvatarButton.onClick.AddListener(OnSaveAvatarUrl);
            avatarUrlField.onValueChanged.AddListener(OnAvatarUrlFieldValueChanged);
        }

        private void OnDisable()
        {
            openPersonalAvatarPanelButton.onClick.RemoveListener(OnOpenPersonalAvatarPanel);
            closeButton.onClick.RemoveListener(OnCloseButton);
            linkButton.onClick.RemoveListener(OnLinkButton);
            loadAvatarButton.onClick.RemoveListener(OnSaveAvatarUrl);
            avatarUrlField.onValueChanged.RemoveListener(OnAvatarUrlFieldValueChanged);
        }

        private void OnOpenPersonalAvatarPanel()
        {
            linkText.text = $"https://{ReadyPlayerMe.Core.CoreSettingsHandler.CoreSettings.Subdomain}.readyplayer.me";
            personalAvatarPanel.SetActive(true);
        }

        private void OnCloseButton()
        {
            personalAvatarPanel.SetActive(false);
        }

        private void OnLinkButton()
        {
            Application.OpenURL(linkText.text);
        }

        private void OnSaveAvatarUrl()
        {
            string cleanUrl = avatarUrlField.text.Trim();

            if (!string.IsNullOrWhiteSpace(cleanUrl))
            {
                // 🛠️ Wymuś .glb zamiast .json
                if (cleanUrl.EndsWith(".json"))
                {
                    cleanUrl = cleanUrl.Replace(".json", ".glb");
                }
                else if (!cleanUrl.EndsWith(".glb"))
                {
                    cleanUrl += ".glb";
                }

                PlayerPrefs.SetString("AvatarURL", cleanUrl);
                PlayerPrefs.Save();

                Debug.Log("✅ Avatar URL saved to PlayerPrefs: " + cleanUrl);

                if (thirdPersonLoader != null)
                {
                    thirdPersonLoader.LoadAvatar(cleanUrl);
                    Debug.Log("🎯 Avatar URL sent to ThirdPersonLoader.");
                }
                else
                {
                    Debug.LogWarning("⚠️ ThirdPersonLoader is not assigned!");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Avatar URL is empty.");
            }
        }


        private void OnAvatarUrlFieldValueChanged(string url)
        {
            loadAvatarButton.interactable = !string.IsNullOrWhiteSpace(url);
        }
    }
}
