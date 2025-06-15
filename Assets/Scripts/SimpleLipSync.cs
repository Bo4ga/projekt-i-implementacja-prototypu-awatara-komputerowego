using UnityEngine;

public class SimpleLipSync : MonoBehaviour
{
    public AudioSource audioSource;

    [SerializeField] public string mouthOpenBlendshapeName = "jawOpen";
    [SerializeField] public float maxOpen = 30f;

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private int blendshapeIndex = -1;

    private void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        if (audioSource == null)
        {
            audioSource = FindAnyObjectByType<AudioSource>();
        }

        if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
        {
            for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
            {
                if (skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i) == mouthOpenBlendshapeName)
                {
                    blendshapeIndex = i;
                    break;
                }
            }

            if (blendshapeIndex == -1)
            {
                Debug.LogWarning($"⚠️ Blendshape '{mouthOpenBlendshapeName}' not found on {gameObject.name}!");
            }
        }
    }

    private void Update()
    {
        if (skinnedMeshRenderer == null || audioSource == null || blendshapeIndex == -1)
            return;

        float loudness = GetAveragedVolume() * maxOpen;
        skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndex, Mathf.Clamp(loudness, 0, maxOpen));
    }

    private float GetAveragedVolume()
    {
        float[] data = new float[256];
        audioSource.GetOutputData(data, 0);
        float sum = 0f;
        foreach (var s in data)
        {
            sum += Mathf.Abs(s);
        }
        return sum / 256;
    }
}
