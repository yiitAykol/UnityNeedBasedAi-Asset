using UnityEngine;

[System.Serializable]
public class Need : MonoBehaviour
{
    public string needName;
    public float currentValue;          // -100 ile 100 arasý deðer
    public AnimationCurve priorityCurve;

    public AnimationCurve myCurve = new AnimationCurve(
    new Keyframe(-100f, 20f),  // Zaman = -100, Deðer = -100
    new Keyframe(100f, 0.1f)     // Zaman = 100,  Deðer = 100
    );// -100 ile 100 arasýný kapsayacak þekilde ayarlý

    private void Start()
    {
        
    }
}