using System;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class TimeManagerScript : MonoBehaviour
{
    [Header("Hýz")]
    [Min(0.001f)] public float realSecondsPerGameMinute = 1f;
    [Tooltip("timeScale==0 iken de aktýrsýn mý?")]
    public bool useUnscaledTime = true;

    [Header("Baþlangýç Zamaný")]
    [Range(0, 23)] public int startHour = 8;
    [Range(0, 59)] public int startMinute = 0;

    [Header("Baþlangýç Zamaný")]
    [Range(0, 23)] public int endHourMorning = 8;
    [Header("Baþlangýç Zamaný")]
    [Range(0, 23)] public int endHourNoon = 8;
    [Header("Baþlangýç Zamaný")]
    [Range(0, 23)] public int endHourEvening = 8;

    [Header("Akýþ")]
    public bool autoStart = true;
    [SerializeField] private TextMeshProUGUI clockText;
    // Sabitler
    public const int MinutesPerDay = 24 * 60;    // 1440
    public const int SlotMinutes = 15;
    public const int SlotsPerDay = MinutesPerDay / SlotMinutes; // 96

    // Eventler
    public event Action OnDayChanged;
    public event Action<int> OnHourChanged;                 // hour
    public event Action<int, int, int> OnMinuteChanged;       // (hour, minute, slot)
    public event Action<int> OnSlotChanged;                 // slot 0..95
    
    // Ýç durum
    int _totalMinutes;    // 0..1439
    int _lastHour;        // 0..23
    int _lastSlot;        // 0..95
    float _acc;             // gerçek saniye biriktirici
    bool _running;

    // Okunabilir anlýk deðerler
    public int CurrentTotalMinutes => _totalMinutes;
    public int CurrentHour => _totalMinutes / 60;
    public int CurrentMinute => _totalMinutes % 60;
    public int CurrentSlot => _totalMinutes / SlotMinutes;

    void Start()
    {
        _totalMinutes = Mathf.Clamp(startHour, 0, 23) * 60 + Mathf.Clamp(startMinute, 0, 59);
        _lastHour = CurrentHour;
        _lastSlot = CurrentSlot;

        // Baþlangýç anýný tek sefer ilan et
        OnSlotChanged?.Invoke(_lastSlot);
        OnHourChanged?.Invoke(_lastHour);
        OnMinuteChanged?.Invoke(CurrentHour, CurrentMinute, CurrentSlot);

        _running = autoStart;
        _acc = 0f;

        UpdateClockUI();//  for clock ui update
    }

    void Update()
    {
        if (!_running) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _acc += dt;

        // Her bir "oyun dakikasý" doldukça 1 dakikalýk step at
        while (_acc >= realSecondsPerGameMinute)
        {
            _acc -= realSecondsPerGameMinute;
            StepOneMinute();
        }
    }

    void StepOneMinute()
    {
        _totalMinutes++;
        if (_totalMinutes >= MinutesPerDay)
        {
            _totalMinutes = 0;
            OnDayChanged?.Invoke(); // tam bir kez
        }

        int slot = CurrentSlot;
        if (slot != _lastSlot)
        {
            OnSlotChanged?.Invoke(slot);
            _lastSlot = slot;
        }

        int hour = CurrentHour;
        if (hour != _lastHour)
        {
            OnHourChanged?.Invoke(hour);
            _lastHour = hour;
        }
        UpdateClockUI();//  for clock ui update
        // Dakika: her adýmda tam 1 kez
        OnMinuteChanged?.Invoke(hour, CurrentMinute, slot);
    }

    // --- Dýþ API ---
    public void SetTime(int hour, int minute, bool dispatchImmediately = false)
    {
        hour = Mathf.Clamp(hour, 0, 23);
        minute = Mathf.Clamp(minute, 0, 59);
        _totalMinutes = hour * 60 + minute;
        _acc = 0f;

        _lastHour = CurrentHour;
        _lastSlot = CurrentSlot;

        if (dispatchImmediately)
        {
            OnSlotChanged?.Invoke(_lastSlot);
            OnHourChanged?.Invoke(_lastHour);
            OnMinuteChanged?.Invoke(CurrentHour, CurrentMinute, CurrentSlot);
        }
    }

    public void SetRunning(bool running) => _running = running;

    public void SetMinuteSpeed(float realSecondsPerMinute)
    {
        realSecondsPerGameMinute = Mathf.Max(0.001f, realSecondsPerMinute);
    }

    // Yardýmcýlar
    public static int TimeToSlot(int hour, int minute)
    {
        hour = Mathf.Clamp(hour, 0, 23);
        minute = Mathf.Clamp(minute, 0, 59);
        return (hour * 60 + minute) / SlotMinutes; // 0..95
    }

    public static void SlotToTime(int slot, out int hour, out int minute)
    {
        slot = Mathf.FloorToInt(Mathf.Repeat(slot, SlotsPerDay));
        int total = slot * SlotMinutes;
        hour = total / 60;
        minute = total % 60;
    }

    public string ClockString() => $"{CurrentHour:00}:{CurrentMinute:00}";

    private void UpdateClockUI()
    {
        if (clockText != null)
            clockText.text = ClockString();
        // Alternatif: clockText.text = ClockString();
    }
}
