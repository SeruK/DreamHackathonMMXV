using UnityEngine;
using System;
using System.Collections;

public class Game : MonoBehaviour {
	[SerializeField]
	private GameObject skinPrefab;
	[SerializeField]
	private Texture2D touchTexture;

	private GameObject skin;
	private float touchAlpha;
	private Vector2 touchPos;

	private Coroutine touchFadeTimer;

	protected void OnEnable() {
		skin = Instantiate( skinPrefab ) as GameObject;
		var gesture = skin.GetComponent<GestureRecognizer>();
		gesture.OnDragStarted = ( Vector2 mousePos ) => {
			StartTimer( ref touchFadeTimer, 0.5f, ( float dt ) => { touchAlpha = dt; } );
			touchPos = mousePos;
		};
		gesture.OnDrag = ( Vector2 mousePos ) => {
			touchPos = mousePos;
		};
		gesture.OnDragEnded = ( Vector2 mousePos ) => {
			StartTimer( ref touchFadeTimer, 0.5f, ( float dt ) => { touchAlpha = 1.0f - dt; } );
		};
	}

	// Timers

	private Coroutine StartTimer( ref Coroutine c, float duration, Action<float> tick ) {
		if( c != null ) {
			StopCoroutine( c );
		}
		c = StartTimer( duration, tick );
		return c;
	}

	private Coroutine StartTimer( float duration, Action<float> tick ) {
		return StartTimer( duration, tick, null );
	}
	
	private Coroutine StartTimer( float duration, Action<float> tick, Action done ) {
		return StartCoroutine( DoTimer( duration, tick, done ) );
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

	// GUI

	protected void OnGUI() {
		if( touchTexture != null ) {
			GUI.color = new Color( 1, 1, 1, touchAlpha );
			var rect = new Rect( touchPos.x - 32, Screen.height - touchPos.y - 32, 64, 64 );
			GUI.DrawTexture( rect, touchTexture );	
		}
	}
}
