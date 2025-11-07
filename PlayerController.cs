using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] 
    private Camera mainCamera;
    [SerializeField]
    private NavMeshAgent agent;
    
    public bool autonomy { get; private set; } = true;

    private int counter = 0;
   
    private GameObject selectedObject = null;
    private GameObject previouslySelected = null;
    private Vector3 hitPoint = Vector3.zero;
    
    [SerializeField] private Animator animatorController;
    [SerializeField] private NeedManager needManager;
    public bool isPlaying = false;
    [SerializeField] private SubBehave SubBehave;
    [SerializeField] private TimeManagerScript timeManagerScript;

    public bool cancelled = false;
    
    public event Action OnConditionChanged;
    
    private void Start()
    {
    }
    void OnEnable()
    {
        timeManagerScript.OnMinuteChanged += HandleMinute;
    }
    void OnDisable()
    {
        timeManagerScript.OnMinuteChanged -= HandleMinute;
    }

    private void HandleMinute(int arg1, int arg2, int arg3)
    {
        if (autonomy == false && counter > 0)
        {
            autonomy = true;
        }
        counter += 1;
    }

    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (mainCamera == null)
        {
            Debug.LogWarning("Camera = null");
        }

        if (agent == null)
        {
            Debug.LogWarning("agent = null");
        }
    }
    void Update()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                
                var obj = hit.collider.GetComponent<InteractbleObject>();
                if (obj != null)
                {

                    selectedObject = obj.gameObject; //seçilen obje ayný ise seçtirmez , aktiv iþle karþýlaþtýr
                    
                    if(needManager.inPlace != true && selectedObject.tag == needManager.ActiveJob)
                    {
                        Debug.Log("In place is false, selected object is active job, autonomy is true and counter is 0");
                    }
                    else
                    {
                        counter = 0;
                        autonomy = false;
                        
                        OnConditionChanged.Invoke();
                    }
                    
                    previouslySelected = selectedObject; //seçilen objeyi önceki olarak atar

                } 
                else
                {
                   
                    selectedObject = null;
                    hitPoint = hit.point;
                    counter = 0;
                    autonomy = false;
                    
                    SubBehave.setMousePoint(hit.point);

                    OnConditionChanged.Invoke();
                } 
            }
        }
        
    }

    public GameObject ReturnSelected() { return selectedObject; }

    public Vector3 GetHitPoint() { return hitPoint; }

    public void SetPlaying(bool isPlaying)
    { 
      this.isPlaying = isPlaying;
    }
}
