using UnityEngine;
using System;
using System.Collections;

public class Game : MonoBehaviour {
	[Serializable]
	public class Audio {
		public AudioClip clip;
		public float volume = 1.0f;
		public bool vibrates;
	}

	[Serializable]
	public class AudioClips {
		public Audio catMeow;
		public Audio catPurr;
		public Audio laugh;
		public Audio moan;
		public Audio stab;
		public Audio torture;
	};

	[SerializeField]
	private GameObject skinPrefab;
	[SerializeField]
	private GameObject splatterPrefab;
	[SerializeField]
	private Texture2D touchTexture;
	[SerializeField]
	private Texture2D skinTexture;
	[SerializeField]
	private Texture2D bloodTexture;
	[SerializeField]
	private UnityEngine.UI.Text titleText;
	[SerializeField]
	private UnityEngine.UI.Text headlineText;
	[SerializeField]
	private UnityEngine.UI.Button nextButton;
	[SerializeField]
	private bool skipIntro;
	[SerializeField]
	private AudioClips clips;

	private float titleTextAlpha {
		get { return titleText.color.a; }
		set { var c = titleText.color; c.a = value; titleText.color = c; }
	}
	private float headlineTextAlpha {
		get { return headlineText.color.a; }
		set { var c = headlineText.color; c.a = value; headlineText.color = c; }
	}
	private float nextButtonAlpha {
		get { return nextButton.colors.normalColor.a; }
		set { var c = nextButton.colors; var c2 = c.normalColor; c2.a = value; c.normalColor = c2; nextButton.colors = c; }
	}

	private GameObject skinGO;
	private Skin skin;
	private float touchAlpha;
	private Vector2 touchPos;
	private bool shaking;
	private bool wound;

	private Audio tapAudio;
	private Audio strokeAudio;

	private AudioSource audioSource;
	private Coroutine touchFadeTimer;
	private Coroutine skinTimer;
	private Action OnNextButtonClicked;

	protected void OnEnable() {
		audioSource = GetComponent<AudioSource>();
		Setup();
		titleText.text = "Synecdoche";
		headlineTextAlpha = 0.0f;
		titleTextAlpha = 0.0f;
		skin.Alpha = 0.0f;
		nextButtonAlpha = 0.0f;

		if( !skipIntro ) {
			StartTimer( 2.0f, () => {
				StartTimer( 4.0f, ( float dt ) => {
					titleTextAlpha = dt;
				}, 1.0f, () => {
					StartTimer( 2.0f, ( float dt ) => {
						nextButtonAlpha = dt;
					}, () => {
						OnNextButtonClicked = () => {
							StartTimer( 4.0f, ( float dt ) => {
								titleTextAlpha = 1.0f - dt;
								skin.Alpha = dt;
							}, () => {
								InitialState();
							} );
						};
					} );
				} );
			} );
		} else {
			nextButtonAlpha = 1.0f;
			skin.Alpha = 1.0f;
			InitialState();
		}
	}

	protected void Update() {
		if( shaking ) {
			skin.transform.position = UnityEngine.Random.insideUnitCircle * 0.01f;
		} else {
			skin.transform.position = Vector3.zero;
		}
	}

	// States

	private void InitialState() {
		FadeInHeadline( "A piece of skin" );

		OnNextButtonClicked = () => {
			FadeOutHeadline( () => {
				ChangeColorState();
			} ); 
		};
	}

	private void ChangeColorState() {
		FadeInHeadline( "Shade" );

		TickTockSkinValue( 1.0f, 0.5f );

		OnNextButtonClicked = () => {
			if( skinTimer != null ) {
				StopCoroutine( skinTimer );
			}
			FadeOutHeadline( () => {
				CatState();
			} );
		};
	}

	private void CatState() {
		FadeInHeadline( "Sleek Siamese" );

		SetTapAudio( clips.catMeow );
		SetStrokeAudio( clips.catPurr );

		OnNextButtonClicked = () => {
			if( skinTimer != null ) {
				StopCoroutine( skinTimer );
			}
			FadeOutHeadline( () => {
				WomanState();
			} );
		};
	}

	private void WomanState() {
		FadeInHeadline( "Sneaky Sex" );

		SetTapAudio( clips.laugh );
		SetStrokeAudio( null );

		OnNextButtonClicked = () => {
			FadeOutHeadline( () => {
				KnifeState();
			} );
		};
	}

	private void KnifeState() {
		FadeInHeadline( "Grander story" );

		wound = true;
		SetTapAudio( clips.stab );
		SetStrokeAudio( clips.torture );

		OnNextButtonClicked = () => {
			FadeOutHeadline( () => {
				MoanState();
			} );
		};
	}

	private void MoanState() {
		FadeInHeadline( "Intimidating Intimacy" );

		wound = false;
		SetTapAudio( null );
		SetStrokeAudio( clips.moan );

		OnNextButtonClicked = () => {
			titleText.text = "Fin";
			var uiTxt = nextButton.transform.FindChild( "Text" ).GetComponent<UnityEngine.UI.Text>();
			uiTxt.text = "Quit";

			StartTimer( 3.0f, ( float dt ) => {
				headlineTextAlpha = 1.0f - dt;
				skin.Alpha = 1.0f - dt;
			}, () => {
				StartTimer( 3.0f, ( float dt ) => {
					titleTextAlpha = dt;
				} );
			} );
			OnNextButtonClicked = () => {
				Application.Quit();
			};
		};
	}

	// Headline

	private void FadeInHeadline( string text ) {
		headlineText.text = text;
		StartTimer( 2.0f, ( float dt ) => {
			headlineTextAlpha = dt;
		} );
	}

	private void FadeOutHeadline( Action done ) {
		StartTimer( 2.0f, ( float dt ) => {
			headlineTextAlpha = 1.0f - dt;
		}, () => {
			done();
		} );
	}

	private void TickTockSkinValue( float from, float to ) {
		StartTimer( ref skinTimer, 2.0f, ( float dt ) => {
			var c = skin.Color;
			HSV.SetValue( ref c, Mathf.Lerp( from, to, dt ) );
			skin.Color = c;
		}, () => {
			TickTockSkinValue( to, from );
		} );
	}

	private void Setup() {
		if( skinGO == null ) {
			skinGO = Instantiate( skinPrefab, Vector3.zero, Quaternion.identity ) as GameObject;
			skin = skinGO.GetComponent<Skin>();
		}

		var gesture = skinGO.GetComponent<GestureRecognizer>();
		gesture.OnDragStarted = ( Vector2 mousePos ) => {
			StartTimer( ref touchFadeTimer, 0.5f, ( float dt ) => { touchAlpha = dt; } );
			touchPos = mousePos;
			if( !audioSource.isPlaying ) {
				audioSource.Play();
			}
			audioSource.UnPause();
		};

		gesture.OnDrag = ( Vector2 mousePos ) => {
			touchPos = mousePos;
			if( strokeAudio != null && strokeAudio.vibrates ) {
				shaking = true;
			}
		};
		gesture.OnDragEnded = ( Vector2 mousePos ) => {
			StartTimer( ref touchFadeTimer, 0.5f, ( float dt ) => { touchAlpha = 1.0f - dt; } );
			audioSource.Pause();
			shaking = false;
		};
		gesture.OnTapped = ( Vector2 touchPos, Vector2 worldPos ) => {
			audioSource.Stop();
			if( tapAudio != null ) {
				audioSource.PlayOneShot( tapAudio.clip, tapAudio.volume );
				this.touchPos = touchPos;
				StartTimer( ref touchFadeTimer, 0.5f, (float dt ) => { touchAlpha = 1.0f - dt; } );
				if( tapAudio.vibrates ) {
					shaking = true;
					StartTimer( 0.3f, () => {
						shaking = false;
					} );
				}
				if( wound ) {
					var splatterGO = Instantiate<GameObject>( splatterPrefab );
					splatterGO.transform.position = ( (Vector3)worldPos ) + new Vector3( 0.0f, 0.0f, -0.1f );
					splatterGO.transform.rotation = Quaternion.Euler( new Vector3( 0.0f, 0.0f, UnityEngine.Random.Range( 0.0f, 360.0f ) ) );
					splatterGO.transform.localScale = new Vector3( 1, 1, 1 ) * UnityEngine.Random.Range( 0.09f, 0.15f );
					splatterGO.transform.parent = skin.transform;
					Material mat = splatterGO.GetComponent<MeshRenderer>().material;
					StartTimer( 3.0f, () => {
						StartTimer( 3.0f, ( float dt ) => {
							var c = mat.color;
							c.a = 1.0f - dt;
							mat.color = c;
						}, () => {
							Destroy( splatterGO );
						} );
					} );
				}
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

	private void SetTapAudio( Audio a ) {
		tapAudio = a;
	}

	private void SetStrokeAudio( Audio a ) {
		audioSource.Stop();
		strokeAudio = a;
		if( a == null ) {
			audioSource.clip = null;
			return;
		}
		audioSource.clip = a.clip;
		audioSource.volume = a.volume;
		audioSource.loop = true;
	}

	// Timers

	private Coroutine StartTimer( ref Coroutine c, float duration, Action<float> tick ) {
		return StartTimer( ref c, duration, tick, 0.0f, null );
	}

	private Coroutine StartTimer( ref Coroutine c, float duration, Action<float> tick, Action done ) {
		return StartTimer( ref c, duration, tick, 0.0f, done );
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

	// Callback

	public void NextButtonClicked() {
		if( OnNextButtonClicked != null ) {
			OnNextButtonClicked();
		}
	}
}
