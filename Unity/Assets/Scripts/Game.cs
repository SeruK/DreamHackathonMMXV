using UnityEngine;
using System;
using System.Collections;

public class Game : MonoBehaviour {
	private enum SkinState {
		None,
		Scratch,
		Cat,
		Female,
		PullOff,
		PullOffNoises
	}

	[SerializeField]
	private GameObject skinPrefab;
	[SerializeField]
	private Texture2D touchTexture;

	[SerializeField]
	private AudioClip tapClip;
	[SerializeField]
	private AudioClip dragClip;

	private GameObject skin;
	private float touchAlpha;
	private Vector2 touchPos;
	private bool peelSkin = true;

	private Coroutine touchFadeTimer;

	protected void OnEnable() {
		SetState( SkinState.Scratch );
	}

	private void SetState( SkinState state ) {
		if( skin != null ) {
			var oldSkin = skin;
			skin.GetComponent<GestureRecognizer>().Reset();
			var renderer = skin.GetComponent<MeshRenderer>();
			Vector3 from = skin.transform.position;
			Vector3 to   = from - new Vector3( 5, 0, 0 );
			Color color = renderer.material.color;
			StartTimer( 1.0f, ( float dt ) => {
				oldSkin.transform.position = Vector3.Lerp( from, to, dt );
				oldSkin.transform.localScale = Vector3.Lerp( new Vector3( 1, 1, 1 ), Vector3.zero, dt );
//				color.a = 1.0f - dt;
//				renderer.material.color = color;
			}, () => {
				Destroy( oldSkin.gameObject );
			} );
			skin = null;
		}

		if( state == SkinState.None ) {
			return;
		}

		skin = Instantiate( skinPrefab ) as GameObject;
		var gesture = skin.GetComponent<GestureRecognizer>();
		gesture.OnDragStarted = ( Vector2 mousePos ) => {
			StartTimer( ref touchFadeTimer, 0.5f, ( float dt ) => { touchAlpha = dt; } );
			touchPos = mousePos;
		};

		gesture.OnDrag = ( Vector2 mousePos ) => {
			touchPos = mousePos;
			if( peelSkin ) {
				PeelSkin( mousePos );
			}
		};
		gesture.OnDragEnded = ( Vector2 mousePos ) => {
			StartTimer( ref touchFadeTimer, 0.5f, ( float dt ) => { touchAlpha = 1.0f - dt; } );
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
