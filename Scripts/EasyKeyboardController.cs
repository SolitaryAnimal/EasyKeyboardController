using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EasyKeyboardController : MonoBehaviour
{
    private List<IEnumerator<(Vector3, Quaternion)>> animations = new();
    
    public Vector3 originPosition;
    public Quaternion originRotation;
    public Plane plane;
    public Camera mainCamera;

    public InputAction mousePosition;

    public float rotationAngle = -30;
    
    public AnimationCurve clickDownCurve;
    public float clickDownDistance;
    public float clickDownTime;
    public float clickDownAngle;
    
    void Start()
    {
        originPosition = transform.position;
        originRotation = transform.rotation;
        
        plane = new Plane(transform.up, transform.position);
        if(!mainCamera) mainCamera = Camera.main;
    }

    void Update()
    {
        var targetPos = originPosition;
        var targetRot = originRotation;
        
        var mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(mouseRay, out var enter))
        {
            var enterPos = mouseRay.GetPoint(enter);
            var targetDir = enterPos - originPosition;
            var rotAxis = Vector3.Cross(targetDir, plane.normal);
            Debug.DrawRay(mainCamera.transform.position, mouseRay.direction * enter, Color.red);
            Debug.DrawRay(transform.position, rotAxis, Color.blue);
            targetRot *= Quaternion.AngleAxis(rotationAngle, rotAxis);
            
            if (Input.GetMouseButtonDown(0))
            {
                animations.Add(NormalizeTimeAnimation(t =>
                {
                    var ct = clickDownCurve.Evaluate(t);
                    var pos = Vector3.down * (ct * clickDownDistance);
                    var rot = Quaternion.Lerp(Quaternion.identity, Quaternion.AngleAxis(clickDownAngle, rotAxis), ct);
                    return (pos, rot);
                }, clickDownTime));
            }
        }

        var (pos, rot) = SampleAnimation();
        transform.position = targetPos + pos;
        transform.rotation = targetRot * rot;
    }

    private void OnGUI()
    {
        GUILayout.Label(animations.Count.ToString());
    }

    private (Vector3, Quaternion) SampleAnimation()
    {
        var posResult = Vector3.zero;
        var rotResult = Quaternion.identity;
        for (int i = 0; i < animations.Count;)
        {
            var animation = animations[i];
            if (!animation.MoveNext())
            {
                animations.RemoveAt(i);
                continue;
            }
            var (pos, rot) = animation.Current;
            posResult += pos;
            rotResult *= rot;
            i++;
        }
        return (posResult, rotResult);
    }

    IEnumerator<(Vector3, Quaternion)> NormalizeTimeAnimation(Func<float, (Vector3, Quaternion)> animationFunc, float timeLength)
    {
        float startTime = Time.time;
        float normalizedTime = 0;
        while ((normalizedTime = (Time.time - startTime) / timeLength) < 1)
        {
            yield return animationFunc(Mathf.Clamp01(normalizedTime));
        }
    }
}
