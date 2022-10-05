using UnityEngine;
using System.Collections;


public class SelfDestruct : MonoBehaviour {
	public float selfdestruct_in = 4; // Setting this to 0 means no selfdestruct.

    public AudioClip[] BodyEffectAudioClip;
    private AudioSource audioSource;

    void Start ()
    {
        if ( selfdestruct_in != 0){ 
			Destroy (gameObject, selfdestruct_in);
		}

        audioSource = this.GetComponent<AudioSource>();
        if(audioSource == null) return;

        //int i = 0;      
        //audioSource.clip = BodyEffectAudioClip[Random.Range(0, 2)];
    }
}
