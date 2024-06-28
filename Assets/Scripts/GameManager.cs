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
    [SerializeField] GameObject endUI;
    [SerializeField] GameObject playerUI;

    [Header("Settings")]
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
    Move[] movingObjects;
    NodePickupVolume[] nodeVolumes;
    ResetVolume[] resetVolumes;

    void Awake() {
        if (I == null) I = this;
        else Destroy(gameObject);

        InitializeTriggerVolumes();
        LoadLastCheckpoint();
    }

    void Update() {
        CheckInputs();
    }

    void CheckInputs() {
        if (Input.GetKeyDown(quitKey)) Application.Quit();
        if (Input.GetKeyDown(clearSaveKey)) checkpointManager.DeleteSaveFile();
        if (Input.GetKeyDown(respawnKey)) ResetGameState(false);
        if (Input.GetKeyDown(restartKey)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void InitializeTriggerVolumes() {
        checkpointVolumes = FindObjectsByType<CheckpointVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        resetVolumes = FindObjectsByType<ResetVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        nodeVolumes = FindObjectsByType<NodePickupVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        killVolumes = FindObjectsByType<KillVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        emancipationVolumes = FindObjectsByType<EmancipationVolume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        movingObjects = FindObjectsByType<Move>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var checkpointVolume in checkpointVolumes) {
            checkpointVolume.onEnterVolume += OnPlayerEnteredCheckpointVolume;
        }

        foreach (var resetVolume in resetVolumes) {
            resetVolume.onEnterVolume += ResetGameState;
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
    }

    void LoadLastCheckpoint() {
        var checkpointPosition = checkpointManager.LoadLastCheckpoint();
        if (checkpointPosition != Vector3.zero) {
            respawnPoint.position = checkpointPosition;
            ResetGameState();
            Debug.Log($"Loaded checkpoint at {checkpointPosition}!");
        }
        else {
            Debug.Log("No checkpoint found.");
            respawnPoint.position = playerDependencies.transform.position;
        }
    }

    public void EndGame() {
        playerDependencies.rb.gameObject.SetActive(false);
        playerUI.SetActive(false);
        endUI.SetActive(true);
    }

    void OnPlayerEnterEmancipationVolume(RopeType _ropeType) {
        playerDependencies.grapplingHook.DestroyRopes(_ropeType);
    }

    void ResetGameState(bool _playSound = true) {
        //Resets player
        if (_playSound) playerDependencies.audioSourceTop.PlayOneShot(resetSound);
        playerDependencies.grapplingHook.DestroyRopes();
        playerDependencies.rb.linearVelocity = Vector3.zero;
        playerDependencies.rb.angularVelocity = Vector3.zero;
        playerDependencies.rb.MovePosition(respawnPoint.position);
        playerDependencies.perspective.ForceOrientation(respawnPoint.rotation);

        //Resets all moving objects
        foreach (var movingObject in movingObjects) {
            movingObject.Reset();
        }
    }

    void OnPlayerEnteredCheckpointVolume(Transform _spawnPoint) {
        playerDependencies.audioSourceTop.PlayOneShot(checkpointSound);
        checkpointManager.SaveCheckpoint(_spawnPoint.position);
        respawnPoint.position = _spawnPoint.position;
        respawnPoint.forward = _spawnPoint.forward;
        infoPopup.ShowPopup("Checkpoint reached!");
    }

    void OnPlayerEnteredNodePickupVolume(NodeData _nodeData) {
        scratchManager.AddNode(_nodeData);
        infoPopup.ShowPopup($"Node collected: {_nodeData.id}");
        AudioSource.PlayClipAtPoint(nodePickupSound, transform.position);
    }

    void OnPlayerEnteredKillVolume() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        infoPopup.ShowPopup("Crap!");
    }
}