using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
// using WaitForFixedUpdate = UnityEngine.WaitForFixedUpdate;

public class PhisTest : MonoBehaviour
{
    public Rigidbody rigidbody;
    private Camera mainCamera;
    private Collider collider;
    private Plane plane;
    
    public float force;
    public float spring, dampingConstant;
    public InputAction action;
    public bool lastPressed;
    public Vector3 rotF;
    public Vector3 rotForce;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        mainCamera = Camera.main;
        // StartCoroutine(LateFeixeUpdate());
        action.Enable();
        plane = new Plane(Vector3.up, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddForce()
    {
        if(!Input.GetMouseButtonDown(0)) return;
        print("Fire");
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if(!collider.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) return;
        // Debug.DrawLine(mainCamera.transform.position, hit.point, Color.red);
        rigidbody.AddForceAtPosition(-hit.normal * force, hit.point, ForceMode.Acceleration);
    }
    
    private void FixedUpdate()
    {
        // print(rigidbody.linearVelocity);
        //AddForce();

        //print(action.IsPressed());


        // Up
        if (lastPressed && !action.IsPressed())
        {
            print("up");
            //rigidbody.AddForce(Vector3.up * force, ForceMode.Acceleration);
            //rigidbody.AddTorque(-rotF);
        }

        // Down
        if (action.IsPressed() && !lastPressed)
        {
            print("down");
            //rigidbody.AddForce(Vector3.down * force, ForceMode.Acceleration);
            rigidbody.AddTorque(rotF, ForceMode.Acceleration);
        }

        lastPressed = action.IsPressed();

        var pos = rigidbody.position;
        var linearVelocity = rigidbody.linearVelocity;
        //rigidbody.AddForce(-pos * spring - linearVelocity * dampingConstant);
        print(rigidbody.rotation.eulerAngles);
        
        rotForce = Quaternion.Inverse(rigidbody.rotation).eulerAngles;

        if (rotForce.x > 180f) rotForce.x -= 360f;
        if (rotForce.y > 180f) rotForce.y -= 360f;
        if (rotForce.z > 180f) rotForce.z -= 360f;

        rigidbody.AddTorque(rotForce * spring - rigidbody.angularVelocity * dampingConstant);
    }

    private void OnGUI()
    {

    }

    // IEnumerator LateFeixeUpdate()
    // {
    //     while (true)
    //     {
    //         yield return new WaitForFixedUpdate();
    //         rigidbody.rotation = Quaternion.identity;
    //         var position = rigidbody.position;
    //         rigidbody.position = new Vector3(0, position.y, 0);
    //     }
    // }
}
