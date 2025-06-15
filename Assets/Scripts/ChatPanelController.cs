using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatPanelController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chatPanel;
    public Button chatToggleButton;
    public TMP_InputField inputField;
    public TMP_Text chatText;

    private bool isVisible = false;

    void Start()
    {
        chatPanel.SetActive(isVisible);
        chatToggleButton.onClick.AddListener(ToggleChatPanel);
    }

    void ToggleChatPanel()
    {
        isVisible = !isVisible;
        chatPanel.SetActive(isVisible);
    }

    public void AppendBotResponse(string response)
    {
        chatText.text += $"\n<color=#00ffff><b>AI:</b></color> {response}";
    }

    public void AppendMessage(string sender, string message)
    {
        chatText.text += $"<b>{sender}:</b> {message}\n";
    }

    public void OnSubmitText()
    {
        string input = inputField.text.Trim();
        if (string.IsNullOrEmpty(input)) return;

        AppendMessage("You", input);
        // Tutaj przekaż dane do twojego systemu rozmowy np. SakuraAI.Process(input);

        inputField.text = "";
        inputField.ActivateInputField();
    }
}

