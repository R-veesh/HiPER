using UnityEngine;

public static class AudioListenerEnforcer
{
    public static void KeepOnly(AudioListener preferredListener)
    {
        AudioListener[] listeners = Object.FindObjectsOfType<AudioListener>(true);

        foreach (AudioListener listener in listeners)
        {
            if (listener == null)
            {
                continue;
            }

            listener.enabled = listener == preferredListener;
        }
    }
}
