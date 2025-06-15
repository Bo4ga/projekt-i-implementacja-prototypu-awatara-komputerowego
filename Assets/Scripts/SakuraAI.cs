using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using SimpleJSON;
using ReadyPlayerMe.Core;
using ReadyPlayerMe.Samples.QuickStart;
using TMPro;

public class SakuraAI : MonoBehaviour
{


    [Header("Wake Word Settings")]
    public string[] wakeWords = { "ok sakura", "okay sakura", "okej sakura" };

    [Header("UI References")]
    public TMP_InputField userInput;
    public TMP_Text responseText;
    public ChatPanelController chatPanelController;
    public AudioSource audioSource;

    [Header("TTS Settings")]
    public AudioClip thinkingSound;
    public AudioClip listeningSound;
    public string openAiVoice = "nova";

    [Header("Avatar Reference")]
    public GameObject avatar;
    public ThirdPersonLoader avatarLoader;

    [Header("Loading UI")]
    public GameObject loadingScreen;

    private DictationRecognizer dictationRecognizer;
    private int restartCount = 0;
    private bool isReady = false;

    private void Start()
    {
        Debug.Log("🟢 SakuraAI initialized.");

        foreach (var device in Microphone.devices)
        {
            Debug.Log("🎤 Microphone detected: " + device);
        }

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        if (avatarLoader != null)
        {
            avatarLoader.OnLoadComplete += OnAvatarLoaded;
        }
        else
        {
            Debug.LogError("❌ ThirdPersonLoader not assigned in SakuraAI.");
        }
    }

    private IEnumerator PresentTextStepByStep(string content)
    {
        string[] lines = content.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            yield return SpeakTextWithOpenAI(line);
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("✅ Prezentacja zakończona.");
    }

    public void PlayPresentation(string text)
    {
        Debug.Log("🎥 Rozpoczynam prezentację...");
        StartCoroutine(PresentTextStepByStep(text));
    }

    private void OnAvatarLoaded()
    {
        Debug.Log("✅ Avatar loaded via ThirdPersonLoader");
        isReady = true;
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
        CreateAndStartRecognizer();
    }

    private void CreateAndStartRecognizer()
    {
        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.InitialSilenceTimeoutSeconds = 8;
        dictationRecognizer.AutoSilenceTimeoutSeconds = 6;

        dictationRecognizer.DictationResult += (text, confidence) =>
        {
            if (!isReady) return;
            Debug.Log("🗣️ STT Result: " + text);
            userInput.text = text;
            OnSend();
        };

        dictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (!isReady) return;
            Debug.LogWarning("Dictation ended: " + completionCause);

            if (completionCause != DictationCompletionCause.Complete)
            {
                restartCount++;
                Debug.Log("🔁 Restarting recognizer (count: " + restartCount + ")");
                StartCoroutine(RestartRecognizer());
            }
        };

        dictationRecognizer.Start();
        if (listeningSound != null && audioSource != null && isReady)
        {
            audioSource.PlayOneShot(listeningSound);
        }
    }

    private IEnumerator RestartRecognizer()
    {
        yield return StartCoroutine(WaitForSpeechToEnd());

        if (dictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            dictationRecognizer.Stop();
        }

        yield return new WaitForSeconds(0.5f);

        if (dictationRecognizer.Status != SpeechSystemStatus.Running && isReady)
        {
            dictationRecognizer.Start();

            if (listeningSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(listeningSound);
            }

            Debug.Log("🎧 Dictation restarted. Ready to listen.");
        }
    }

    private IEnumerator WaitForSpeechToEnd()
    {
        yield return new WaitUntil(() => audioSource != null && !audioSource.isPlaying);
    }

    private void OnSend()
    {
        if (!isReady) return;
        string input = userInput.text;
        if (!string.IsNullOrWhiteSpace(input))
        {
            Debug.Log("User: " + input);

            if (IsIdentityQuestion(input))
            {
                string identityAnswer = "I'm a personal assistant made by Boguslav Pacyno to assist you in your work or have a chat.";
                responseText.text = identityAnswer;
                chatPanelController.AppendBotResponse(identityAnswer);
                StartCoroutine(SpeakTextWithOpenAI(identityAnswer));
                return;
            }

            if (IsWakeWordPresent(input))
            {
                string prompt = RemoveWakeWord(input);
                StartCoroutine(SendChatRequest(prompt));
            }
            else
            {
                responseText.text = "(Wake word not detected)";
            }

            chatPanelController.AppendBotResponse(responseText.text);
        }
    }

    private bool IsWakeWordPresent(string text)
    {
        text = text.ToLower();
        return wakeWords.Any(word => text.Contains(word));
    }

    private bool IsIdentityQuestion(string text)
    {
        string[] identityPrompts = new string[]
        {
            "who are you", "what are you", "what is your purpose",
            "why were you made", "why are you here", "what do you do",
            "who created you", "who made you", "what's your purpose"
        };

        text = text.ToLower();

        return identityPrompts.Any(prompt => text.Contains(prompt));
    }

    private string RemoveWakeWord(string text)
    {
        foreach (var word in wakeWords)
        {
            text = text.Replace(word, "");
        }
        return text.Trim();
    }

    private IEnumerator SendChatRequest(string prompt)
    {
        if (!isReady) yield break;
        string endpoint = "https://api.openai.com/v1/chat/completions";
        string json = $"{{\"model\":\"{model}\",\"messages\":[{{\"role\":\"user\",\"content\":\"{EscapeJson(prompt)}\"}}]}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {openAiApiKey}");

            if (thinkingSound != null && audioSource != null)
                audioSource.PlayOneShot(thinkingSound);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("OpenAI Error: " + request.error);
                responseText.text = "Error contacting OpenAI.";
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                string reply = ExtractContentFromResponse(responseJson);
                responseText.text = reply;
                StartCoroutine(SpeakTextWithOpenAI(reply));
            }
        }
    }

    private string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
    }

    private string ExtractContentFromResponse(string json)
    {
        try
        {
            var parsed = JSON.Parse(json);
            return parsed["choices"][0]["message"]["content"];
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to parse OpenAI response: " + e.Message);
            return "Error parsing response.";
        }
    }

    private IEnumerator SpeakTextWithOpenAI(string text)
    {
        if (!isReady) yield break;
        string endpoint = "https://api.openai.com/v1/audio/speech";
        string json = $"{{\"model\":\"tts-1\",\"input\":\"{EscapeJson(text)}\",\"voice\":\"{openAiVoice}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(endpoint, AudioType.MPEG))
        {
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerAudioClip(endpoint, AudioType.MPEG);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {openAiApiKey}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("TTS Error: " + request.error);
            }
            else
            {
                var clip = DownloadHandlerAudioClip.GetContent(request);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (dictationRecognizer != null)
        {
            dictationRecognizer.Stop();
            dictationRecognizer.Dispose();
        }

        if (avatarLoader != null)
        {
            avatarLoader.OnLoadComplete -= OnAvatarLoaded;
        }
    }
}
