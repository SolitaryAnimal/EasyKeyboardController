using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EKT2 : MonoBehaviour
{
    [System.Serializable]
    public class KeyboardButton
    {
        public Transform Transform => meshFilter?.transform ?? null;

        public MeshRenderer meshFilter;
        public string keyName;
        public Material[] materials;
        public int materialIndex;

        //public Vector3 rotationAxis;
        [NonSerialized]
        public Collider collider;
        [NonSerialized]
        public bool isDown = false;
        [NonSerialized]
        public float targetY;
    }

    private Camera mainCamera;
    private Rigidbody rigidbody;
    private KeyboardButton mouseDownButton;
    private Dictionary<Collider, KeyboardButton> c2kDict;
    private KeyboardButton lasterHover;

    public float force, upForceRatio, jumpForce;
    public float angleSpring;
    public float positionSpring;
    public float animationSpeed;
    public float normalY, downY, hoverY;
    public KeyboardButton[] buttons;
    public Material[] materials;
    public MeshRenderer meshRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        rigidbody = GetComponent<Rigidbody>();
        
        meshRenderer.sharedMaterial = materials[0];

        foreach (var button in buttons)
        {
            // 计算所有旋转轴
            //var pos = button.Transform.localPosition;
            //pos.y = 0;
            //button.rotationAxis = Vector3.Cross(transform.up, pos).normalized;

            // 生成碰撞体
            var bound = button.meshFilter.localBounds;
            var cg = new GameObject().transform;

            cg.parent = transform;
            cg.localPosition = button.Transform.localPosition;
            cg.localRotation = button.Transform.localRotation;
            cg.localScale = button.Transform.localScale;

            var bc = cg.AddComponent<BoxCollider>();
            bc.size = bound.size;
            bc.center = bound.center;

            button.collider = bc;
            
            // 初始化材质
            button.meshFilter.sharedMaterial = button.materials[button.materialIndex];
        }

        // 生成字典
        c2kDict = buttons.ToDictionary(b => b.collider, b => b);
    }

    void ButtonDown(KeyboardButton button)
    {
        if (button.isDown) return;
        rigidbody.AddForceAtPosition(-transform.up * force, button.Transform.position, 0);
        button.targetY = downY;
        button.isDown = true;
        
        // 更新材质
        button.materialIndex = ++button.materialIndex % button.materials.Length;
        button.meshFilter.sharedMaterial = button.materials[button.materialIndex];
    }

    void ButtonUp(KeyboardButton button)
    {
        if (!button.isDown) return;
        rigidbody.AddForceAtPosition(transform.up * (force * upForceRatio), button.Transform.position, 0);
        button.targetY = normalY;
        button.isDown = false;
        
        // 更新主体材质
        if (buttons.All(b => b.materialIndex == button.materialIndex))
        {
            meshRenderer.sharedMaterial = materials[button.materialIndex];
            rigidbody.AddForce(Vector3.up * jumpForce);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 鼠标控制
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo))
        {
            if (c2kDict.TryGetValue(hitInfo.collider, out var button))
            {
                // 鼠标控制按下, 鼠标一次只能按下一个按钮
                if(mouseDownButton == null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ButtonDown(button);
                        mouseDownButton = button;
                    }
                }
                
                    
                // 更新悬停按钮动画
                // 还原上一个悬停按钮位置
                if (lasterHover != null && button != lasterHover && !lasterHover.isDown)
                {
                    lasterHover.targetY = normalY;
                }
                
                // 更新悬停高度
                if (!button.isDown)
                {
                    button.targetY = hoverY;
                    lasterHover = button;
                }
            }
        }
        else
        {
            // 更新悬停按钮动画
            // 如果没有任何被指向, 就还原高度
            if (lasterHover is { isDown: false })
            {
                lasterHover.targetY = normalY;
                lasterHover = null;
            }
        }
        
        // 鼠标抬起按钮
        if(mouseDownButton != null && Input.GetMouseButtonUp(0))
        {
            ButtonUp(mouseDownButton);
            mouseDownButton = null;
        }
        
        
        foreach (var button in buttons)
        {
            // 键盘控制
            if(mouseDownButton != button)
            {
                if (Input.GetKey(button.keyName))
                {
                    ButtonDown(button);
                }
                else if (button.isDown)
                {
                    ButtonUp(button);
                }
            }

            // 更新动画状态
            var pos = button.Transform.localPosition;
            pos.y = Mathf.MoveTowards(pos.y, button.targetY, animationSpeed * Time.deltaTime);
            button.Transform.localPosition = pos;
        }
    }

    private void FixedUpdate()
    {
        // 角度回中
        var angle = Quaternion.Inverse(rigidbody.rotation).eulerAngles;
        // for (int i = 0; i < 2; i++)
        // {
        //     if(angle[i] > 180f) angle[i] -= 360f;
        // }
        if (angle.x > 180f) angle.x -= 360f;
        if (angle.y > 180f) angle.y -= 360f;
        if (angle.z > 180f) angle.z -= 360f;
        
        rigidbody.AddTorque(angle * angleSpring);

        // 位置回中
        rigidbody.AddForce(-rigidbody.position * positionSpring, ForceMode.Acceleration);
    }
}
