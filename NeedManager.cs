using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;

public class NeedManager : MonoBehaviour
{
    [SerializeField] public List<NewNeed> myNeeds;
    private Dictionary<string, NewNeed> needMap;
    private NewNeed activeNeed = null;//private
    
    [SerializeField] private TimeManagerScript timeManagerScript;
    [SerializeField] SphereCastDetection sphereCastDetection;
    public GameObject selectedObject;
    public GameObject selectedObjectIPoint;//seilen objenin interaction pointi
    public GameObject playerSelected = null;

    [SerializeField] PlayerController playerController;
    public string ActiveJob;
    public string LastActiveJob;

    //private int lastMinute = -1;
    public float minuteCount = 0;

    public float arrivedMinut = 0;

    InteractbleObject interactableComponent1;
    InteractbleObject canceledInterractbleObject = null;
    public bool actionActive = false;
    public bool finisherAction = false;

    private const float MAX_NEED_VALUE = 100f;
    private const float MIN_NEED_VALUE = -100f;

    private bool breakFlow2 = false;
    public bool inPlace = false;
    private bool automonous = true;

    public bool waitForPlayer = false;

    [SerializeField] private Light sun;
    public float degreesPerMinute = 0.25f;

    public event Action<bool> OnConditionChanged;
    private bool a = false;
    public bool A
    {
        get => a;

        set
        {
            if (a != value)
            {
                a = value;
                OnConditionChanged?.Invoke(a);
            }
        }
    }
    void OnEnable()
    {
        playerController.OnConditionChanged += HandleConditionChanged;
        timeManagerScript.OnMinuteChanged += HandleMinute;
       
    }
    void OnDisable()
    {
        playerController.OnConditionChanged -= HandleConditionChanged;
        timeManagerScript.OnMinuteChanged -= HandleMinute;
        
    }
    void Awake()
    {
        if (myNeeds != null)
        {
            needMap = myNeeds.ToDictionary(n => n.needName);

            foreach (var need in needMap.Values)
            {
                need.currentValue = MAX_NEED_VALUE;
                need.inAction = false;
            }
        }
    }
    void Start()// need manager özelliklerini tree ye daðýt 
    {
        
    }
    private void HandleMinute(int hour, int minute, int slot) // ihtiyaç seçmeyi azaltmayý falan ayýr 
    {
        //Debug.Log("Minute changed: " + minute + "slot changed:" + slot);
        //sun.transform.Rotate(Vector3.right, degreesPerMinute);
        foreach (var need in needMap.Values)
        {
            if (need.currentValue > MIN_NEED_VALUE && need.inAction == false)
            {
                need.currentValue -= need.decrementLevel * 2;//decrement
            }
        }

        if (selectedObject != null) // zaman bazlý ve fix + puan veren aksiyonlar ile uyuma gibi 100 olana kadar yapýlan aksiyonlar ayrýlabilir
        {
            if (needMap.TryGetValue(selectedObject.tag, out NewNeed currentNeed) && actionActive == true && MAX_NEED_VALUE >= currentNeed.currentValue)
            {
                if (inPlace == true)//sphereCastDetection.inPlace == true
                {
                    currentNeed.currentValue += interactableComponent1.interactable.BasePoint * currentNeed.decrementLevel;
                    finisherAction = false;
                    arrivedMinut += 1;
                }

                if (currentNeed.currentValue > MAX_NEED_VALUE)
                {
                    currentNeed.currentValue = MAX_NEED_VALUE;
                }

                if (currentNeed.actionNeededMinute == 0 && currentNeed.currentValue == MAX_NEED_VALUE)// burada max need e kadar iþleme sokmak yerinde seçilen aksiyon deðeri eklenene kadar beklenebilir 
                {
                    ResetActionState(currentNeed);
                }
                else if (arrivedMinut >= currentNeed.actionNeededMinute && currentNeed.currentValue == MAX_NEED_VALUE)
                {
                    ResetActionState(currentNeed);
                }

                if (actionActive == true)
                {
                    if (needMap.TryGetValue(selectedObject.tag, out NewNeed oppositeNeed))
                    {
                        if (inPlace == true)
                        {
                            oppositeNeed.currentValue -= oppositeNeed.decrementLevel * oppositeNeed.oppositeNeedLevel;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < myNeeds.Count; i++)
        {
            foreach (GameObject obj in sphereCastDetection.detectedObjects)
            {
                string objectTag = obj.tag;

                InteractbleObject interactableComponent = obj.GetComponent<InteractbleObject>();

                if (myNeeds[i].needName == objectTag)
                {
                    float c = interactableComponent.interactable.BasePoint;
                    float curentValue = myNeeds[i].currentValue;
                    interactableComponent.interactable.MultipledBasePoint = myNeeds[i].myCurve.Evaluate(curentValue) * c;
                }
            }
        }
        sphereCastDetection.OrderByDescending();
        minuteCount += 1;
    }

    private void LateUpdate() // saat baþý belkþ bazý ihtiyaçlarý acaltmak gerek 
    {
        

        if (actionActive == false && playerController.autonomy && !waitForPlayer)
        {
            if (breakFlow2 == true)
            {
                selectedObject = playerSelected;
            }
            else
            {
                SelectedJobAndItem();
            }

            breakFlow2 = false;
            inPlace = false;
            ActiveJob = selectedObject.tag;
            interactableComponent1 = selectedObject.GetComponent<InteractbleObject>();
            selectedObjectIPoint = interactableComponent1.EmptyPoint();//interaction pointi al

            actionActive = true;
            interactableComponent1.users.Add(this.gameObject);
            minuteCount = -1;

            if (needMap.TryGetValue(selectedObject.tag, out NewNeed currentNeed))
            {
                activeNeed = currentNeed;

                currentNeed.inAction = true;
                A = true;
            }
            playerController.SetPlaying(false);
            automonous = true;
        }
    }

    private void HandleConditionChanged()// PlayerController'dan gelen event ile tetiklenir
    {
        
        playerSelected = playerController.ReturnSelected();
        automonous = false;

        if (playerSelected != null)
        {
            breakFlow2 = true;
        }
        else// burada flag açýp varýþ noktasýna gelince false yap
        {
            actionActive = false;
            
            Debug.Log("boþa týklandý");
        }
        ResetActionState(activeNeed);

    }

    private void ResetActionState(NewNeed currentNeed)
    {
        minuteCount = -1;
        actionActive = false;
        LastActiveJob = ActiveJob;
        ActiveJob = "Empty";

        if (currentNeed != null) currentNeed.inAction = false;

        if (interactableComponent1 != null)
            interactableComponent1.users.Remove(this.gameObject);

        canceledInterractbleObject = interactableComponent1;
        arrivedMinut = -1;
        finisherAction = true;
        A = false;

        // EK: noktayý býrak
        canceledInterractbleObjectHandler();
    }

    private void SelectedJobAndItem()
    {
        int randomIndex = UnityEngine.Random.Range(0, 3);
        //GameObject a = sphereCastDetection.topThreeAction[randomIndex];
        GameObject a = sphereCastDetection.RandomSelect(randomIndex);

        List<GameObject> validObjects = new List<GameObject>();
        List<GameObject> topThreeAction = sphereCastDetection.ReturnTopThree();

        foreach (GameObject go in topThreeAction)
        {
            if (needMap.TryGetValue(go.tag, out NewNeed currentNeed))
            {
                if (currentNeed.basicNeed)
                {
                    if (currentNeed.currentValue < currentNeed.minEnterPoint)
                        validObjects.Add(go);
                }
                else
                {
                    validObjects.Add(go);
                }

            }
        }

        if (validObjects.Count > 0)
        {
            randomIndex = UnityEngine.Random.Range(0, validObjects.Count);
            selectedObject = validObjects[randomIndex];
        }
    }

    public float CurrentNeedsValue()
    {
        float x = -1;

        if (needMap.TryGetValue(ActiveJob, out NewNeed currentNeed))
        {
            x = currentNeed.currentValue;
        }

        return x;
    }

    public float CurrentNeedsMinEnterValue()
    {
        float x = -1;

        if (needMap.TryGetValue(ActiveJob, out NewNeed currentNeed))
        {
            x = currentNeed.minEnterPoint;
        }

        return x;
    }

    public void canceledInterractbleObjectHandler()
    {
        if (canceledInterractbleObject != null)
        {
            canceledInterractbleObject.ReleasePoint(selectedObjectIPoint);
            canceledInterractbleObject = null;
        }

    }

    public bool returnOrientation()
    {
        if (interactableComponent1 != null)
        {
            return interactableComponent1.ReturnOrientation(selectedObjectIPoint);
        }
        return true;
    }
}

