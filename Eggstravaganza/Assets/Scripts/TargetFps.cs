using UnityEngine;

public class TargetFps : MonoBehaviour
{
    [SerializeField] private int m_Target = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = m_Target;
    }
}
