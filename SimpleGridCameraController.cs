///<summary>
///Copyright 2021 Jussi Mattila a.k.a John Rudy Permission is hereby granted, free of charge, to any person 
///obtaining a copy of this software and associated documentation files (the "Software"), to deal in the 
///Software without restriction, including without limitation the rights to use, copy, modify, merge, 
///publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the 
///Software is furnished to do so, subject to the following conditions: The above copyright notice and this 
///permission notice shall be included in all copies or substantial portions of the Software. 
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
/// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
/// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER 
/// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
/// USE OR OTHER DEALINGS IN THE SOFTWARE.
/// </summary>


///<summary>
/// A simple "3rd person" camera controller meant to be used in grid based envireonments.
/// Place the camera in the scene view at the position you want the camera to follow the target.
/// Alignment offset is an additional offset.
/// </summary>

using UnityEngine;

public class SimpleGridCameraController : MonoBehaviour {
    #region variables
    [Header("Target to follow")]
    [SerializeField] private Transform target;
    public Transform Target {
        set {
            if( target != value ) {
                target = value;
            }
        }
    }
    [Tooltip("Add offset if needed for the target")]
    [SerializeField] private Vector3 alignmentOffset = Vector3.up;
    private Vector3 offset;
    private Vector2 mouse;
    private Vector3 mouseOffset;
    [Header("Rotate amount angle")]
    [Range(0f, 180f)]
    [SerializeField] private float angle = 90f;
    [Header("Invert")]
    [SerializeField] private bool invertX = false;
    public bool InvertX {
        set {
            invertX = value;
        }
    }
    [SerializeField] private bool invertY = false;
    public bool InvertY {
        set {
            invertY = value;
        }
    }
    [Header("Delta times")]
    [Tooltip("Mouse strength")]
    [Range(0f, 0.004f)]
    [SerializeField] private float mouseDelta = 0.0015f;
    [Tooltip("Following speed")]
    [SerializeField] private float lerpDelta = 5f;
    [Tooltip("rotating speed")]
    [SerializeField] private float rotDelta = 3f;
    [Header("Zoom")]
    [SerializeField] private bool invertZoom = true;
    public bool InvertZoom {
        set {
            invertZoom = value;
        }
    }
    [Tooltip("Zoom strength")]
    [Range(-1f, 1f)]
    [SerializeField] private float zoomDelta = 0.5f;
    [Range(10, 40)]
    [SerializeField] private float maxZoom = 20;
    [Range(1, 20)]
    [SerializeField] private float minZoom = 5;
    #endregion

    #region Unity Callbacks
    private void OnValidate () {
        if( maxZoom < minZoom ) { maxZoom = minZoom + 1; }
        if( minZoom > maxZoom ) { minZoom = maxZoom - 1; }
        if( minZoom <= 0 ) { minZoom = 1; }
        if( maxZoom <= 0 ) { maxZoom = 1; }
    }

    private void Start () { offset = target.position - transform.position + alignmentOffset; }

    private void Update () { Inputs(); MouseOffset(); }

    private void LateUpdate () { TransformOffsets(); }
    #endregion

    #region Camera Methods
    private void Inputs () {
        if( Input.GetKeyDown(KeyCode.Q) ) { RotateCamera(angle); }
        if( Input.GetKeyDown(KeyCode.E) ) { RotateCamera(-angle); }
        if( Input.mouseScrollDelta.y != 0 ) { Zoom(Input.mouseScrollDelta.y); }
    }

    private void Zoom ( float dir ) {
        if( invertZoom ) { dir *= -1; }
        Vector3 zoomOffset = transform.TransformDirection(Vector3.forward) * dir * zoomDelta;
        if( ( offset + zoomOffset ).magnitude < maxZoom && ( offset + zoomOffset ).magnitude > minZoom ) {
            offset += zoomOffset;
        }
    }

    //Adding a small mouse offset to the camera movement for "smooth interest"
    private void MouseOffset () {
        mouse = Input.mousePosition;
        mouse.x -= Screen.width / 2;
        mouse.y -= Screen.height / 2;

        if( invertX ) {
            mouse.x = -mouse.x;
        }

        if( invertY ) {
            mouse.y = -mouse.y;
        }

        mouseOffset = (
            transform.TransformDirection(Vector3.right) * mouse.x * mouseDelta ) + (
            transform.TransformDirection(Vector3.up) * mouse.y * mouseDelta );
    }

    //position lerping and transform rotation slerping
    private void TransformOffsets () {
        //Rotation 
        Quaternion targetRot = Quaternion.LookRotation(offset);
        Quaternion rot = Quaternion.Slerp(transform.rotation, targetRot, rotDelta * Time.deltaTime);
        transform.rotation = rot;

        //Position
        transform.position = Vector3.Lerp(transform.position, target.position - offset + mouseOffset, lerpDelta * Time.deltaTime);
    }

    //Rotates the transform acording to the angle given and offsets it
    private void RotateCamera ( float angle ) {
        Vector3 newVec = Quaternion.AngleAxis(angle, Vector3.up) * offset;
        offset = newVec;
    }
    #endregion
}
