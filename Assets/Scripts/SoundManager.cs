using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	[Header("FMOD EVENTS")]
	public string lightPickupSound = "event:/lightPickup";


	// Play a single clip through the sound effects source.
	public void Play(string path)
	{
		FMODUnity.RuntimeManager.PlayOneShot(path);
	}

}