#region

using PrototypeFPC;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    
    [Header("Game dependencies")]
    [SerializeField] PlayerDependencies playerDependencies;
    [SerializeField] CheckpointManager checkpointManager;
    [SerializeField] Transform respawnPoint;
    [SerializeField] ScratchManager scratchManager;
    [SerializeField] InfoPopup infoPopup;
    
    [Header("Settings")]
    [SerializeField] bool loadLastCheckpointOnStart = true;
    [SerializeField] KeyCode respawnKey = KeyCode.Q;
    [SerializeField] KeyCode restartKey = KeyCode.F5;
    [SerializeField] KeyCode quitKey = KeyCode.Escape;
    [SerializeField] KeyCode clearSaveKey = KeyCode.F6;
    
    [Header("Audio")]
    public AudioClip resetSound;
    public AudioClip checkpointSound;
    public AudioClip nodePickupSound;
    public AudioClip platformSound;
    
    CheckpointVolume[] checkpointVolumes;
    EmancipationVolume[] emancipationVolumes;
    KillVolume[] killVolumes;
    NodePickupVolume[] nodeVolumes;
    ResetVolume[] resetVolumes;
    
    void Awake() {
        if (I == null) I = this;
        else Destroy(gameObject);
        checkpointVolumes = FindObjectsByType<CheckpointVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        resetVolumes = FindObjectsByType<ResetVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        nodeVolumes = FindObjectsByType<NodePickupVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        killVolumes = FindObjectsByType<KillVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        emancipationVolumes = FindObjectsByType<EmancipationVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var checkpointVolume in checkpointVolumes) {
            checkpointVolume.onEnterVolume += OnPlayerEnteredCheckpointVolume;
        }
        
        foreach (var resetVolume in resetVolumes) {
            resetVolume.onEnterVolume += ResetPlayer;
        }
        
        foreach (var nodeVolume in nodeVolumes) {
            nodeVolume.onEnterVolume += OnPlayerEnteredNodePickupVolume;
        }
        
        foreach (var killVolume in killVolumes) {
            killVolume.onEnterVolume += OnPlayerEnteredKillVolume;
        }
        
        foreach (var emancipationVolume in emancipationVolumes) {
            emancipationVolume.onEnterVolume += OnPlayerEnterEmancipationVolume;
        }
        
        if (loadLastCheckpointOnStart) LoadLastCheckpoint();
    }
    
    void Update() {
        if (Input.GetKeyDown(quitKey)) Application.Quit();
        if (Input.GetKeyDown(clearSaveKey)) checkpointManager.DeleteSaveFile();
        if (Input.GetKeyDown(respawnKey)) ResetPlayer();
        if (Input.GetKeyDown(restartKey)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    [Command("reload", "Go to last checkpoint.")]
    void LoadLastCheckpoint() {
        var checkpointPosition = checkpointManager.LoadLastCheckpoint();
        if (checkpointPosition != Vector3.zero) {
            respawnPoint.position = checkpointPosition;
            playerDependencies.rb.MovePosition(checkpointPosition);
        }
    }
    
    void OnPlayerEnterEmancipationVolume(RopeType _ropeType) {
        playerDependencies.GetComponent<GrapplingHook>().DestroyRopes(_ropeType);
    }
    
    void ResetPlayer() {
        playerDependencies.GetComponent<GrapplingHook>().DestroyRopes();
        playerDependencies.audioSourceTop.PlayOneShot(resetSound);
        playerDependencies.rb.linearVelocity = Vector3.zero;
        playerDependencies.rb.angularVelocity = Vector3.zero;
        playerDependencies.rb.MovePosition(respawnPoint.position);
        Debug.Log("Player got reset!");
    }
    
    void OnPlayerEnteredCheckpointVolume(CheckpointVolume _v) {
        playerDependencies.audioSourceTop.PlayOneShot(checkpointSound);
        _v.gameObject.SetActive(false);
        checkpointManager.SaveCheckpoint(_v.transform.position);
        respawnPoint.position = _v.transform.position;
        infoPopup.ShowPopup($"{_v.id} checkpoint reached!");
    }
    
    void OnPlayerEnteredNodePickupVolume(NodeData _nodeData) {
        scratchManager.AddNode(_nodeData);
        infoPopup.ShowPopup($"Node collected: {_nodeData.id}");
        AudioSource.PlayClipAtPoint(nodePickupSound, transform.position);
        Debug.Log("Node collected!");
    }
    
    void OnPlayerEnteredKillVolume() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Player died!");
    }
}