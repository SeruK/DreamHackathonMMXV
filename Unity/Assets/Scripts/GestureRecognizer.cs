﻿using UnityEngine;
using System;
using System.Collections;

public class GestureRecognizer : MonoBehaviour {
	[SerializeField]
	private Collider coll;

	public Action<Vector2, Vector2> OnTapped;
	public Action<Vector2> OnDragStarted;
	public Action<Vector2> OnDrag;
	public Action<Vector2> OnDragEnded;

	private float startTimeStamp;
//	private float lastTimeStamp;
	private float lastDragTimeStamp;

	private Vector2 origTouchPos;
	private Vector2 origTouchHitPos;
	private Vector2 lastTouchPos;
	private bool isDragging;
	private bool didMove;
	private bool started;

	public void Reset() {
		OnTapped = null;
		OnDragStarted = null;
		OnDrag = null;
		OnDragEnded = null;
	}

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
				origTouchHitPos = hitInfo.point;
			}

			started = true;
			startTimeStamp = Time.time;
//			lastTimeStamp = Time.time;
			origTouchPos = Input.mousePosition;
			lastTouchPos = Input.mousePosition;
			didMove = false;
			isDragging = false;
		} else if( started && Input.GetMouseButton( 0 ) ) {
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
		} else if( started && Input.GetMouseButtonUp( 0 ) ) {
			started = false;
			float deltaTime = Time.time - startTimeStamp;

			if( !didMove ) {
				// Tap
				if( deltaTime < 0.5f ) {
					if( OnTapped != null ) {
						OnTapped( origTouchPos, origTouchHitPos );
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
