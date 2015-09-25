using UnityEngine;
using System;
using System.Collections;

public class Game : MonoBehaviour {
	[Serializable]
	public class State {
		public string name;
		public AudioClip tapClip;
		public AudioClip dragClip;
		public bool peelSkin;
	}
	
	[SerializeField]
	private State[] states = new State[ 0 ];
	[SerializeField]
	private GameObject skinPrefab;
	[SerializeField]
	private Texture2D touchTexture;
	[SerializeField]
	private Texture2D skinTexture;

	private GameObject skin;
	private float touchAlpha;
	private Vector2 touchPos;

	private AudioSource audioSource;
	private Coroutine touchFadeTimer;

	protected void OnEnable() {
		audioSource = GetComponent<AudioSource>();
		ApplyState( states[ 0 ] );
	}

	private void ApplyState( State state ) {
		if( skin == null ) {
			skin = Instantiate( skinPrefab ) as GameObject;
		}

		var gesture = skin.GetComponent<GestureRecognizer>();
		gesture.OnDragStarted = ( Vector2 mousePos ) => {
			StartTimer( ref touchFadeTimer, 0.5f, ( float dt ) => { touchAlpha = dt; } );
			touchPos = mousePos;
			if( state.dragClip ) {
				audioSource.clip = state.dragClip;
				audioSource.loop = true;
				if( !audioSource.isPlaying ) {
					audioSource.Play();
				}
				audioSource.UnPause();
			}
		};

		gesture.OnDrag = ( Vector2 mousePos ) => {
			touchPos = mousePos;
			if( state.peelSkin ) {
				PeelSkin( mousePos );
			}
		};
		gesture.OnDragEnded = ( Vector2 mousePos ) => {
			StartTimer( ref touchFadeTimer, 0.5f, ( float dt ) => { touchAlpha = 1.0f - dt; } );
			audioSource.Pause();
		};
		gesture.OnTapped = () => {
			if( state.tapClip ) {
				audioSource.PlayOneShot( state.tapClip );
			}
		};
	}

	private void PeelSkin( Vector2 mousePos ) {
		Ray ray = Camera.main.ScreenPointToRay( mousePos );
		RaycastHit hit;
		if( Physics.Raycast( ray, out hit ) ) {
			var collider = skin.GetComponent<Collider>();

			if( hit.collider != collider ) {
				return;
			}

			var skinRenderer = skin.GetComponent<MeshRenderer>();
			Texture2D tex = Instantiate( skinRenderer.material.mainTexture as Texture2D );
			Color32[] pixels = tex.GetPixels32();

			Vector2 uv;
			uv.x = (hit.point.x - hit.collider.bounds.min.x) / hit.collider.bounds.size.x;
			uv.y = (hit.point.y - hit.collider.bounds.min.y) / hit.collider.bounds.size.y;

			// Now this is some butt-ugly code
			int radius = 12;
			int uvx = (int)( uv.x * tex.width), uvy = (int)( uv.y * tex.height );
			int minx = Mathf.Max( uvx - radius, 0 );
			int maxx = Mathf.Min( uvx + radius, tex.width - 1 );
			int miny = Mathf.Max( uvy - radius, 0 );
			int maxy = Mathf.Min( uvy + radius, tex.height -1 );

			for( int x = minx; x <= maxx; ++x ) {
				for( int y = miny; y <= maxy; ++y ) {
					float dist = Vector2.Distance( new Vector2( uvx, uvy ), new Vector2( x, y ) );
					if( dist > radius ) {
						continue;
					}
					int index = x + y * tex.width;
					Color32 color = pixels[ index ];
					float oldAlpha = (float)color.a;
					float newAlpha = 0;//255.0f * ( dist / radius );
					color.a = (byte)Mathf.Min( oldAlpha, newAlpha );
					pixels[ index ] = color;
				}
			}

			tex.SetPixels32( pixels );

			tex.Apply();
			skinRenderer.material.mainTexture = tex;
		}
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
			t = Time.time;
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
