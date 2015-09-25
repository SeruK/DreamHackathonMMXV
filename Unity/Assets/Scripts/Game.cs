using UnityEngine;
using System;
using System.Collections;

public class Game : MonoBehaviour {
	protected void OnEnable() {
	}

	protected void Update() {

	}

	protected void UpdateInput() {

	}

	// Timers

	private void StartTimer( float duration, Action<float> tick ) {
		StartTimer( duration, tick, null );
	}
	
	private void StartTimer( float duration, Action<float> tick, Action done ) {
		StartCoroutine( DoTimer( duration, tick, done ) );
	}
	
	private IEnumerator DoTimer( float duration, Action<float> tick, Action done ) {
		float t = Time.time;
		float elapsed = 0.0f;
		tick( 0.0f );
		while( elapsed < duration ) {
			yield return null;
			float dt = Time.time - t;
			elapsed += dt;
			tick( Mathf.Min( elapsed / duration, 1.0f ) );
		}
		tick( 1.0f );
		if( done != null ) {
			done();
		}
	}
}
