using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class sound_util {

	public static void play_sound(AudioClip clip){ data.mem.audioSource.PlayOneShot(clip); }
	
}