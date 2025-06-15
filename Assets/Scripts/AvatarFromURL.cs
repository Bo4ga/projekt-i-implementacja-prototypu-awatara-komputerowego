using UnityEngine;
using ReadyPlayerMe.Core;
using System;

public class AvatarFromURL : MonoBehaviour
{
    [Header("https://models.readyplayer.me/6819e0de1b312db5b3f89134.glb")]
    public string avatarUrl = "https://models.readyplayer.me/your-id.glb";

    void Start()
    {
        if (!string.IsNullOrEmpty(avatarUrl))
        {
            var avatarLoader = new AvatarObjectLoader();
            avatarLoader.OnCompleted += OnAvatarLoaded;
            avatarLoader.LoadAvatar(avatarUrl);
        }
        else
        {
            Debug.LogWarning("Avatar URL is empty.");
        }
    }

    private void OnAvatarLoaded(object sender, CompletionEventArgs args)
    {
        GameObject avatar = args.Avatar;
        avatar.transform.position = Vector3.zero;
        Debug.Log("✅ Avatar loaded successfully!");
    }
}
