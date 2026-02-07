using UnityEngine;
using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnCarChanged))]
    public int selectedCarIndex = 0;

    [SyncVar(hook = nameof(OnReadyStateChanged))]
    public bool isReady = false;

    [SyncVar]
    public string playerName = "Player";

    [Header("Prefabs")]
    public GameObject[] carPrefabs;

    private GameObject previewCar;
    private Transform assignedPlate;

    public override void OnStartClient()
    {
        SpawnPreviewCar();
        Debug.Log($"LobbyPlayer spawned for {playerName}");
    }

    public void SetPlatePosition(Transform plateTransform)
    {
        assignedPlate = plateTransform;
        if (isServer)
        {
            RpcUpdatePosition(plateTransform.position, plateTransform.rotation);
        }
    }

    [ClientRpc]
    void RpcUpdatePosition(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    void SpawnPreviewCar()
    {
        if (previewCar != null)
            Destroy(previewCar);

        if (carPrefabs != null && carPrefabs.Length > 0)
        {
            previewCar = Instantiate(
                carPrefabs[selectedCarIndex],
                transform.position + Vector3.up * 0.5f,
                Quaternion.identity,
                transform
            );
        }
    }

    void OnCarChanged(int oldIndex, int newIndex)
    {
        SpawnPreviewCar();
    }

    void OnReadyStateChanged(bool oldState, bool newState)
    {
        Debug.Log($"{playerName} ready state: {newState}");
        // Update UI here if needed
    }

    [Command]
    public void CmdNextCar()
    {
        if (isReady) return; // Can't change car when ready
        
        selectedCarIndex = (selectedCarIndex + 1) % carPrefabs.Length;
    }

    [Command]
    public void CmdPrevCar()
    {
        if (isReady) return; // Can't change car when ready
        
        selectedCarIndex--;
        if (selectedCarIndex < 0)
            selectedCarIndex = carPrefabs.Length - 1;
    }

    [Command]
    public void CmdSetReady()
    {
        isReady = !isReady;
        Debug.Log($"{playerName} set ready to: {isReady}");
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    public override void OnStopClient()
    {
        if (previewCar != null)
            Destroy(previewCar);
    }
}