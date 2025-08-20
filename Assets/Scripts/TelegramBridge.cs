using UnityEngine;
using System.Runtime.InteropServices;

public class TelegramBridge : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetTelegramUsername();

    [DllImport("__Internal")]
    private static extern string GetTelegramUserImage();
#endif

    public string GetUsername()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GetTelegramUsername();
#else
        return "EditorMode"; // Fallback when testing in Editor
#endif
    }

    public string GetUserImage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GetTelegramUserImage();
#else
        return ""; // Empty when testing in Editor
#endif
    }
}
