using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAudio : MonoBehaviour
{
    [Header(" ==== Footstep====")]
    public AudioSource FootstepSrc;
    public AudioClip[] FootstepClip;

    [Header(" ==== Model====")]
    public AudioSource ModelSrc;
    public AudioClip[] ModelClip;

    [Header(" ==== Weapon====")]
    public AudioSource WeaponSrc;
    public AudioClip[] WeaponClip;

    [Header(" ==== Voice====")]
    public AudioSource VoiceSrc;
    public AudioClip[] VoiceClip;

    [Header(" ==== Object====")]
    public AudioSource[] ObjectSrc;
    public AudioClip[] ObjectClip;

    [Header(" ==== Instance====")]
    public AudioClip[] InstanceClip;


    // ==================== Footstep=======================

    public void PlayFootstepSound(int index)
    {
        FootstepSrc.clip = FootstepClip[index];
        FootstepSrc.Play();
    }

    public void PlayJumpstepSound(int index)
    {
        FootstepSrc.clip = FootstepClip[index];
        FootstepSrc.Play();
    }
    // ==================== Weapon =======================

    public void PlayWeaponSound(int index)
    {
        WeaponSrc.clip = WeaponClip[index];
        WeaponSrc.Play();
    }

    // ==================== Object =======================

    public void PlayObjectSound(int srcIndex, int clipIndex)
    {
        ObjectSrc[srcIndex].clip = ObjectClip[clipIndex];
        ObjectSrc[srcIndex].Play();
    }
    public void PlayObjectSound(GameObject obj, int clipIndex)
    {
        obj.GetComponent<AudioSource>().clip = ObjectClip[clipIndex];
        obj.GetComponent<AudioSource>().Play();
    }

    // ==================== Model =======================

    public void PlayModelSound(int index)
    {
        ModelSrc.clip = ModelClip[index];
        ModelSrc.Play();
    }

    // ==================== Voice =======================

    public void PlayVoiceSound(int index)
    {
        VoiceSrc.clip = VoiceClip[index];
        VoiceSrc.Play();
    }
}
