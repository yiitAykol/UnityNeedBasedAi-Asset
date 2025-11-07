using UnityEngine;

[CreateAssetMenu(fileName = "NewNeed", menuName = "Scriptable Objects/NewNeed")]
public class NewNeed : ScriptableObject// add saat aralıklı needler
{
    public string needName;

    // -100 ile 100 arası değer
    public float currentValue;
    public float startTime = -100;
    public float endTime = 100;
    public float decrementLevel = 0;
    public float oppositeNeedLevel = 0;
    public float actionNeededMinute = 0;

    public float startValue = 20f;
    public float endValue = 0.1f;

    public bool inAction = false;
    public float minEnterPoint = 0;
    public bool urgent = false;
    public bool basicNeed = false;

    // -100 ile 100 arasını kapsayacak şekilde ayarlı
    public AnimationCurve myCurve = new AnimationCurve(
        new Keyframe(-100f, 20f),
        new Keyframe(100f, 0.1f)
    );

    public bool parabol = false;

    // Değerlerde bir değişiklik yapıldığında Unity editörde otomatik çalışır
    private void OnValidate()
    {
        // Mevcut keyframeleri temizleyip, yeni baş ve son keyframe oluşturuyoruz
        Keyframe startKey = new Keyframe(startTime, startValue);
        Keyframe endKey = new Keyframe(endTime, endValue);

        myCurve = new AnimationCurve(startKey, endKey);
        float midTime = (startTime + endTime) / 2f;

        if( parabol )
        {
            Keyframe parabolStart = new Keyframe(startTime, 20f);
            Keyframe midKey = new Keyframe(midTime, endValue);
            Keyframe parabolEnd = new Keyframe(endTime, endValue*200);

            // Orta keyframe
            parabolStart.inTangent = .1f;
            parabolStart.outTangent = -.2f;       // Negatif outTangent => farklı yönlü kavis


            parabolEnd.inTangent = .2f;
            parabolEnd.outTangent = .1f;

            myCurve = new AnimationCurve(parabolStart, midKey, parabolEnd);

        }
        // İsterseniz tangential ayarlar ya da ek keyframeler de ekleyebilirsiniz
        // startKey.inTangent = 0f; startKey.outTangent = 0f; vb...
    }

}
