using UnityEngine;

namespace DeadAir.Core
{
    // Проигрывает короткий звук в точке мира. Сами клипы задаются в инспекторе у тех, кто их играет.
    public static class SoundPlayer
    {
        // spatial: 0 - звук слышно одинаково везде, 1 - полностью 3D (тише вдали)
        public static void PlayAt(AudioClip clip, Vector3 position, float volume, float spatial)
        {
            if (clip == null)
                return;

            GameObject soundObject = new GameObject("Sound_" + clip.name);
            soundObject.transform.position = position;

            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.pitch = Random.Range(0.95f, 1.05f); // чтобы одинаковые звуки не надоедали
            source.spatialBlend = spatial;
            source.minDistance = 6f;
            source.maxDistance = 60f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.Play();

            // Удаляем объект, когда звук доиграл
            Object.Destroy(soundObject, clip.length / source.pitch + 0.1f);
        }
    }
}
