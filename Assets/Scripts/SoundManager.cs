using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	// Audio players components.
	public AudioSource EffectsSource;
	public AudioSource MusicSource;

	[Header("Pickup Sound Effects")]
	public List<AudioClip> lightOrbPickups = new List<AudioClip>();
	public AudioClip darklightOrbPickup;
	public AudioClip goldenOrbPickup;
	public AudioClip etherealOrbPickup;

	[Header("Music")]
	public AudioClip darkTheme;


	public void Start()
	{
		
	}


	// Play a single clip through the sound effects source.
	public void Play(AudioClip clip)
	{
		EffectsSource.clip = clip;
		EffectsSource.Play();
	}

	// Play a single clip through the music source.
	public void PlayMusic(AudioClip clip)
	{
		MusicSource.clip = clip;
		MusicSource.Play();
	}

	public void PlayRandomEffectFromList(List<AudioClip> clips)
	{
		AudioClip clip = clips[Random.Range(0, clips.Count)];
		EffectsSource.PlayOneShot(clip);
	}
}