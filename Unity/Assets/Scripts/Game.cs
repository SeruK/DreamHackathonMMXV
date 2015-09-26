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
	[SerializeField]
	private UnityEngine.UI.Text titleText;
	[SerializeField]
	private UnityEngine.UI.Text headlineText;

	private float titleTextAlpha {
		get { return titleText.color.a; }
		set { var c = titleText.color; c.a = value; titleText.color = c; }
	}
	private float headlineTextAlpha {
		get { return headlineText.color.a; }
		set { var c = headlineText.color; c.a = value; headlineText.color = c; }
	}

	private GameObject skinGO;
	private Skin skin;
	private float touchAlpha;
	private Vector2 touchPos;

	private AudioSource audioSource;
	private Coroutine touchFadeTimer;
	private Coroutine skinTimer;

	protected void OnEnable() {
		audioSource = GetComponent<AudioSource>();
		ApplyState( states[ 0 ] );
		headlineTextAlpha = 0.0f;
		titleTextAlpha = 0.0f;
		skin.Alpha = 0.0f;

		StartTimer( 2.0f, () => {
			StartTimer( 4.0f, ( float dt ) => {
				titleTextAlpha = dt;
			}, 3.0f, () => {
				StartTimer( 4.0f, ( float dt ) => {
					titleTextAlpha = 1.0f - dt;
					skin.Alpha = dt;
				}, () => {
					ChangeColor();
				} );
			} );
		} );
	}

	private void ChangeColor() {
		headlineText.text = "A change of value";
		StartTimer( 2.0f, ( float dt ) => {
			headlineTextAlpha = dt;
		} );

		TickTockSkinValue( 1.0f, 0.5f );
	}

	private void TickTockSkinValue( float from, float to ) {
		StartTimer( ref skinTimer, 2.0f, ( float dt ) => {
			var c = skin.Color;
			HSV.SetValue( ref c, Mathf.Lerp( from, to, dt ) );
			skin.Color = c;
		}, 2.0f, () => {
			TickTockSkinValue( to, from );
		} );
	}

	private void ApplyState( State state ) {
		if( skinGO == null ) {
			skinGO = Instantiate( skinPrefab, Vector3.zero, Quaternion.identity ) as GameObject;
			skin = skinGO.GetComponent<Skin>();
		}

		var gesture = skinGO.GetComponent<GestureRecognizer>();
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
			var collider = skinGO.GetComponent<Collider>();

			if( hit.collider != collider ) {
				return;
			}

			var skinRenderer = skinGO.GetComponent<MeshRenderer>();
			Texture2D tex = Instantiate( skinRenderer.material.mainTexture as Texture2D );
			Color32[] pixels = tex.GetPixels32();

			Vector2 uv;
			uv.x = (hit.point.x - hit.collider.bounds.min.x) / hit.collider.bounds.size.x;
			uv.y = (hit.point.y - hit.collider.bounds.min.y) / hit.collider.bounds.size.y;

			// Now this is some butt-ugly code
			int radius = 16;
			int peelRadius = 12;
			int uvx = (int)( uv.x * tex.width), uvy = (int)( uv.y * tex.height );
			int minx = Mathf.Max( uvx - radius, 0 );
			int maxx = Mathf.Min( uvx + radius, tex.width - 1 );
			int miny = Mathf.Max( uvy - radius, 0 );
			int maxy = Mathf.Min( uvy + radius, tex.height -1 );

			for( int x = minx; x <= maxx; ++x ) {
				for( int y = miny; y <= maxy; ++y ) {
					float dist = Vector2.Distance( new Vector2( uvx, uvy ), new Vector2( x, y ) );
					bool peel = dist < peelRadius;

					if( dist > radius ) {
						continue;
					}
					int index = x + y * tex.width;
					Color32 color = pixels[ index ];
					float oldAlpha = (float)color.a;
					float newAlpha = peel ? 0.0f : (float)( ( dist - peelRadius ) / (float)peelRadius );//255.0f * ( dist / radius );
					color.r = (byte)0;
					color.g = (byte)0;
					color.b = (byte)0;
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

	private Coroutine StartTimer( ref Coroutine c, float duration, Action<float> tick, float wait, Action done ) {
		if( c != null ) {
			StopCoroutine( c );
		}
		c = StartTimer( duration, tick, wait, done );
		return c;
	}

	private Coroutine StartTimer( float duration, Action done ) {
		return StartTimer( duration, null, done );
	}

	private Coroutine StartTimer( float duration, float wait, Action done ) {
		return StartTimer( duration, null, wait, done );
	}

	private Coroutine StartTimer( float duration, Action<float> tick ) {
		return StartTimer( duration, tick, null );
	}
	
	private Coroutine StartTimer( float duration, Action<float> tick, Action done ) {
		return StartTimer( duration, tick, 0.0f, done );
	}

	private Coroutine StartTimer( float duration, Action<float> tick, float wait, Action done ) {
		return StartCoroutine( DoTimer( duration, tick, wait, done ) );
	}
	
	private IEnumerator DoTimer( float duration, Action<float> tick, float wait, Action done ) {
		if( tick == null ) {
			yield return new WaitForSeconds( duration + wait );
			if( done != null ) done();
			yield break;
		}

		float t = Time.time;
		float elapsed = 0.0f;
		while( elapsed < duration ) {
			yield return null;
			float dt = Time.time - t;
			t = Time.time;
			elapsed += dt;
			tick( Mathf.Min( elapsed / duration, 1.0f ) );
		}
		tick( 1.0f );
		yield return new WaitForSeconds( wait );
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
