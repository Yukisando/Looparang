#region

using UnityEngine;

#endregion

public class Move : MonoBehaviour
{
    public Vector3 localMove = Vector3.forward * 5;
    public float duration = 5f;
    public LeanTweenType easeType = LeanTweenType.easeInOutExpo;
    
    [Header("Audio")]
    public float maxVolume = 1f;
    
    float maxSpeed;
    Vector3 previousPosition;
    AudioSource source;
    Vector3 startPos;
    
    void Awake() {
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.spatialBlend = 1f;
        source.playOnAwake = true;
        source.volume = 0f;
        source.maxDistance = 120f;
        source.rolloffMode = AudioRolloffMode.Linear;
    }
    
    void Start() {
        startPos = transform.position;
        transform.LeanMove(startPos + localMove, duration).setEase(easeType).setLoopPingPong();
        previousPosition = startPos;
        
        // Calculate the maximum speed
        maxSpeed = (localMove / duration).magnitude;
    }
    
    void Update() {
        // Calculate the current speed
        float speed = (transform.position - previousPosition).magnitude / Time.deltaTime;
        previousPosition = transform.position;
        
        // Adjust pitch based on speed
        source.pitch = Mathf.Clamp(speed / maxSpeed, 0f, 1f);
        
        // Adjust volume based on speed
        source.volume = Mathf.Clamp(speed / maxSpeed, 0.3f, maxVolume);
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + localMove, 1f);
        Gizmos.DrawLine(transform.position, transform.position + localMove);
    }
}