using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThrowManager : MonoBehaviour
{
    #region Public Fields
    [SerializeField]
    private GameObject throwable;

    [SerializeField]
    private float strength = 5f;

    [SerializeField]
    private float maxCharge = 2f;

    [SerializeField]
    private float chargeSpeed = 5000f;

    [SerializeField]
    public float kunaiCount = 1f;

    [SerializeField]
    public CameraFollow cameraFollow;

    public List<GameObject> kunaiList;
    #endregion

    #region Delegates
    event Action onUpdate = () => { };
    #endregion

    #region Private Fields
    float chargeTime = 0f;
    float chargeStrength;
    Vector3 direction;
    Rigidbody playerRigidbody;
    Pointer pointer;
    Coroutine pointerCoroutine;
    #endregion

    private void Awake()
    {
        playerRigidbody = this.GetComponent<Rigidbody>();
        pointer = GetComponent<Pointer>();
        kunaiList = new List<GameObject>();

        Physics.IgnoreLayerCollision(3, 6, true);
        this.onUpdate = CaptureInput;
    }   

    private void Update() => onUpdate();

    void CaptureInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (pointerCoroutine != null)
                StopCoroutine(pointerCoroutine);

            pointerCoroutine = StartCoroutine(pointer.Charge());
            chargeTime = Time.time;
            onUpdate += Charged;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (kunaiList.Count == 0)
                return;
            
            GameObject kunaiLast = kunaiList.Last();

            if (!kunaiLast)
                return;

            Teleport(kunaiLast);
        }

        if(Input.GetKeyDown(KeyCode.LeftControl) && kunaiList.Count > 0 )
        {
            cameraFollow.Target = kunaiList[kunaiList.Count - 1].transform;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
            cameraFollow.Target = transform;

    }

    void Regular()
    {
        Throw(strength);
        onUpdate -= Regular;
    }

    void Charged()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse1))
        {
            if(pointerCoroutine != null)
                StopCoroutine(pointerCoroutine);

            pointerCoroutine = StartCoroutine(pointer.Discharge());
            chargeStrength = Time.time - chargeTime;
            chargeStrength *= chargeSpeed;
            
            if(chargeStrength >= maxCharge)
            {
                chargeStrength = maxCharge;
            }

            Throw(chargeStrength);

            onUpdate -= Charged;
        }
    }

    private void Throw(float throwStrength)
    {
        if (kunaiCount > 0)
        {
            // Decrease kunai count
            kunaiCount--;
            GameObject throwable = Spawn();

            // Get throwable's Rigidbody
            Rigidbody throwableRigidbody = throwable.GetComponent<Rigidbody>();

            // Direction of a kunai
            direction = pointer.Direction;

            throwable.transform.rotation = Quaternion.Euler(direction);

            // Perfrorm a throw
            throwableRigidbody.AddForce(direction * throwStrength);

            kunaiList.Add(throwable);
        }

        // Replace with UI notifier
        Debug.Log("Out of kunai");
    }

    GameObject Spawn()
    {
        // Spawn a throwable
        GameObject throwable = Instantiate(this.throwable, transform.position, Quaternion.identity);

        // Mark thrown object as a Throwable
        throwable.gameObject.tag = "Throwable";
        return throwable;
    }

    void Teleport(GameObject kunai)
    {
        cameraFollow.Target = transform;

        Debug.Log("Kunai Stack: " + kunaiList.Count);

        // Change player's position to kunai
        transform.position = kunai.transform.position;

        // Reset player's velocity on teleport
        playerRigidbody.velocity = Vector3.zero;
    }
}
