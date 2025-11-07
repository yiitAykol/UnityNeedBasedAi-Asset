using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
public class SubBehave : MonoBehaviour
{
    [SerializeField] BehaviorGraph behaviorGraph;
    [SerializeField] BehaviorGraphAgent agent;
    [SerializeField] SphereCastDetection sphereCastDetection;
    [SerializeField] NeedManager needManager;
    [SerializeField] PlayerController playerController; 
    [SerializeField] private SliderController sliderController;
    GameObject nullJect = null;
    public string state = null;
    private Vector3 mouseHitPoint = Vector3.zero;
    bool sameAction = false;
    //public bool condition = false;

    void OnEnable()
    {
        needManager.OnConditionChanged += HandleConditionChanged;
        playerController.OnConditionChanged += FlowConditionChanged;
    }
    void OnDisable()
    {
        needManager.OnConditionChanged -= HandleConditionChanged;
        playerController.OnConditionChanged -= FlowConditionChanged;
    }
    void Start()
    {
        agent.SetVariableValue("navigated", false);
        agent.SetVariableValue("SecondaryFind", false);
        agent.SetVariableValue("States", States.Empty);
    }
    private void HandleConditionChanged(bool currentCondition)//otomatik gelen ile , seçilen hadle condition ý ayýr
    {
        if (currentCondition)
        {
            string tagValue = needManager.selectedObject.tag;
            //bool sameAction;
            //string tagValue = needManager.ActiveJob;
            if (state == tagValue) 
            { 
                //agent.SetVariableValue("ContinuesAction", true);
                //sameAction = true;

                if (agent.GetVariable("navigated", out BlackboardVariable<bool> navigatedVariable))
                {
                    /*
                    if (navigatedVariable != tr)
                    {
                        agent.SetVariableValue("speed", sliderController.GetValue());
                    }*/
                    if(navigatedVariable.Value == true)
                    {
                        agent.SetVariableValue("ContinuesAction", true);
                        sameAction = true;
                    }
                    else
                    {
                        agent.SetVariableValue("ContinuesAction", false);
                        sameAction = false;
                    }

                }
            }
            else
            {
                agent.SetVariableValue("ContinuesAction", false);
                sameAction = false;
            }

            if (Enum.TryParse(tagValue, out States stateValue))
            {
                //condition=true;
                agent.SetVariableValue("States", stateValue);
                agent.SetVariableValue("NeedCurrentValue", needManager.CurrentNeedsValue());
                agent.SetVariableValue("MinEnterPoint", needManager.CurrentNeedsMinEnterValue());
                agent.SetVariableValue("navigated", sameAction);
                agent.SetVariableValue("NeededObject", needManager.selectedObject);

                Vector3 hitPoint = needManager.selectedObject.transform.position;
                agent.SetVariableValue("InputHitPoint", hitPoint);
                agent.SetVariableValue("SelectedObjectIPoint", needManager.selectedObjectIPoint);

                agent.SetVariableValue("BreakFlow", false);
            }
            else
            {
                Debug.LogWarning("Tag value '" + tagValue + "' States not in enum class.");
            }

            state = tagValue;
        }
        else
        {
            Change();
        }
    }
    private void FlowConditionChanged()//bool currentCondition
    {
        agent.SetVariableValue("BreakFlow", true);
    }
    void Update()
    {
        if(agent.GetVariable("navigated" , out BlackboardVariable<bool> navigatedVariable))
        {
            needManager.inPlace = navigatedVariable.Value;//value yoktu eklememi söyledi
            if(sliderController != null)
            {
                agent.SetVariableValue("speed", sliderController.GetValue());
            }
        }

        if (agent.GetVariable("EmptyHit", out BlackboardVariable<bool> emptyHit))
        {
            //needManager.inPlace = emptyHit;
            needManager.waitForPlayer = emptyHit.Value;
        }
    }

    public void setMousePoint(Vector3 point)
    {
        mouseHitPoint = point;
        agent.SetVariableValue("InputHitPoint", mouseHitPoint);
        agent.SetVariableValue("EmptyHit", true);
        agent.SetVariableValue("navigated", false);
        state = "Empty";
        Change();

    }
    
    void Change()
    {
        agent.SetVariableValue("States", States.Empty);
        //agent.SetVariableValue("navigated", false);
        //agent.SetVariableValue("NeededObject", nullJect);
        agent.SetVariableValue("SecondaryObjects", nullJect);
        agent.SetVariableValue("SecondaryFind", false);
        agent.SetVariableValue("NeedCurrentValue", -1f);
        agent.SetVariableValue("MinEnterPoint", -1f);
        needManager.inPlace = false;
    }
}