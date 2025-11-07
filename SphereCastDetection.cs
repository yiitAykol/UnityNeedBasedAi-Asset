using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SphereCastDetection : MonoBehaviour
{
    // Sphere cast radius
    public float sphereRadius = 5f;
    // Sphere cast distance
    public float maxDistance = 10f;
    
    public LayerMask layerMask;
    public List<GameObject> detectedObjects;
    
    private List<GameObject> topThreeAction = new List<GameObject>(3);

    //public List<GameObject> topThreeAction;
    [SerializeField] TimeManagerScript timeManagerScript;

    public GameObject selectedOne;

    void OnEnable()
    {
        timeManagerScript.OnMinuteChanged += DetactCast;
    }

    void OnDisable()
    {
        timeManagerScript.OnMinuteChanged -= DetactCast;
    }
    private void Awake()
    {
        if (topThreeAction == null) topThreeAction = new List<GameObject>(3);
        while (topThreeAction.Count < 3) topThreeAction.Add(null);
        SphereCast();
    }

    private void DetactCast(int arg1, int arg2, int arg3)
    {
        SphereCast();
    }

    void Update()
    {
        /*
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(origin, sphereRadius, direction, maxDistance, layerMask);
        detectedObjects = new List<GameObject>();
        
        foreach (RaycastHit hit in hits)
        {
            GameObject obj = hit.collider.gameObject;
            if (!detectedObjects.Contains(obj))
            {
                detectedObjects.Add(obj);
            }
        }*/
        //SphereCast();
    }
    public void OrderByDescending()
    {
        int a = 0;
        foreach (GameObject obj in detectedObjects.OrderByDescending(x => x.GetComponent<InteractbleObject>()?.interactable.MultipledBasePoint ?? 0))
        {
            string objectTag = obj.tag;
            if (a < 3)
            {
                topThreeAction[a] = obj;
            }
            a++;
        }
    }
    public GameObject SelectedJobAndItem()
    {
        int randomIndex = Random.Range(0, topThreeAction.Count);

        selectedOne = topThreeAction[randomIndex];
        
        return selectedOne;
    }

    public GameObject RandomSelect(int random)
    {
        selectedOne = topThreeAction[random];
        return selectedOne;
    }

    public List<GameObject> ReturnTopThree()
    {
        return topThreeAction;
    }

    void SphereCast()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(origin, sphereRadius, direction, maxDistance, layerMask);
        detectedObjects = new List<GameObject>();

        foreach (RaycastHit hit in hits)
        {
            GameObject obj = hit.collider.gameObject;
            if (!detectedObjects.Contains(obj))
            {
                detectedObjects.Add(obj);
            }
        }
    }
}
