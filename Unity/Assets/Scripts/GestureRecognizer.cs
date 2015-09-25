using UnityEngine;
using System;
using System.Collections;

public class GestureRecognizer : MonoBehaviour {
	[SerializeField]
	private Collider coll;

	public Action OnTapped;
	public Action<Vector2> OnDragStarted;
	public Action<Vector2> OnDrag;
	public Action<Vector2> OnDragEnded;

	private float startTimeStamp;
//	private float lastTimeStamp;
	private float lastDragTimeStamp;

	private Vector2 lastTouchPos;
	private bool isDragging;
	private bool didMove;

	protected void Update() {
		// MouseButton( 0 ) == Finger
		if( Input.GetMouseButtonDown( 0 ) ) {
			if( coll != null ) {
				Ray mouseRay = Camera.main.ScreenPointToRay( Input.mousePosition );
				RaycastHit hitInfo;
				if( Physics.Raycast( mouseRay, out hitInfo ) ) {
					if( hitInfo.collider != coll ) {
						return;
					}
				} else {
					return;
				}
			}

			startTimeStamp = Time.time;
//			lastTimeStamp = Time.time;
			lastTouchPos = Input.mousePosition;
			didMove = false;
			isDragging = false;
		} else if( Input.GetMouseButton( 0 ) ) {
			Vector2 touchDelta = ( (Vector2)Input.mousePosition ) - lastTouchPos;

			if( touchDelta.magnitude > 0.001f ) {
				lastDragTimeStamp = Time.time;

				didMove = true;
				if( !isDragging ) {
					isDragging = true;
					if( OnDragStarted != null ) {
						OnDragStarted( Input.mousePosition );
					}
				}

				if( OnDrag != null ) {
					OnDrag( Input.mousePosition );
				}
			} else {
				float deltaTime = Time.time - lastDragTimeStamp;

				if( isDragging && deltaTime > 0.1f ) {
					isDragging = false;
					if( OnDragEnded != null ) {
						OnDragEnded( Input.mousePosition );
					}
				}
			}

//			lastTimeStamp = Time.time;
			lastTouchPos = Input.mousePosition;
		} else if( Input.GetMouseButtonUp( 0 ) ) {
			float deltaTime = Time.time - startTimeStamp;

			if( !didMove ) {
				// Tap
				if( deltaTime < 0.5f ) {
					if( OnTapped != null ) {
						OnTapped();
					}
				}
			} else {
				if( isDragging ) {
					isDragging = false;
					if( OnDragEnded != null ) {
						OnDragEnded( Input.mousePosition );
					}
				}
			}
		}
	}
}
