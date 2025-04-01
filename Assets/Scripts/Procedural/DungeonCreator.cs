using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;

public class DungeonCreator : MonoBehaviour
{
    [Header("Referencia al XR")]
    public GameObject xrRig;

    public GameObject[] rooms;
    public GameObject[] connectors;
    public GameObject initialRoom;
    public GameObject doorPrefab;

    public int minRooms = 6;
    public int maxRooms = 10;

    // Listas públicas para los objetos de decoración
    public List<GameObject> paintsList;
    public List<GameObject> weaponsList;
    public List<GameObject> storageList;
    public List<GameObject> statuesList;
    public List<GameObject> decorationList;
    public List<GameObject> portalList;

    public List<GameObject> enemiesList;

    [Header("NavMesh")]
    public NavMeshSurface navMeshSurface;
    // Probabilidad inicial de spawnear enemigos (20%)
    private float enemySpawnChance = 0.9f;

    // Incremento de probabilidad por cada sala generada
    private float spawnIncreasePerRoom = 0.05f;

    // Límite máximo de probabilidad de aparición de enemigos (90%)
    private float maxEnemySpawnChance = 0.9f;

    private List<GameObject> spawnedRooms = new List<GameObject>();
    private List<ExitPoint> availableExits = new List<ExitPoint>();

    private bool roomCreated = false;

    public class ExitPoint
    {
        public Transform exitTransform;  // Transform de la salida
        public GameObject room;          // Referencia a la sala o conector donde está la salida

        public ExitPoint(Transform exitTransform, GameObject room)
        {
            this.exitTransform = exitTransform;
            this.room = room;
        }
    }

    public bool GenerateDungeonWithResult()
    {
        bool isCreated = GenerateDungeon();
        PlaceDoorsOnUnconnectedExits();

        // Al final de la generación, "horneamos" el NavMesh
        if (navMeshSurface)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh reconstruido tras generar el dungeon.");
        }

        // Mover el XR al (0,0,0)
        if (xrRig != null)
        {
            xrRig.transform.position = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("No se encontró un objeto XRRig en la escena.");
        }

        return isCreated;
    }

    bool GenerateDungeon()
    {
        // Limpiar cualquier nivel generado anteriormente
        ClearDungeon();

        // Contador de reintentos
        int retryCount = 0;
        int maxRetries = 10;  // Máximo número de reintentos para evitar bucles infinitos

        // 1. Instanciar la sala inicial
        GameObject currentRoom = Instantiate(initialRoom, transform.position, initialRoom.transform.rotation);
        spawnedRooms.Add(currentRoom);
        AddExits(currentRoom);

        int roomCount = Random.Range(minRooms, maxRooms + 1);
        Debug.Log("Generando " + roomCount + " salas.");

        bool lastWasRoom = true; // Bandera para alternar entre Room y Connector

        while (retryCount < maxRetries)
        {
            // 2. Generar el resto de las salas
            for (int i = 1; i < roomCount * 2; i++)  // *2 para alternar entre Room y Connector
            {
                GameObject nextPrefab;

                if (lastWasRoom)
                {
                    nextPrefab = GetRandomConnector();  // Alternar con un conector
                    lastWasRoom = false;
                }
                else
                {
                    nextPrefab = GetRandomRoom();  // Alternar con una sala
                    lastWasRoom = true;
                }

                // 3. Intentar conectar la sala con una salida disponible
                if (availableExits.Count > 0)
                {
                    ExitPoint exit = GetRandomExit();
                    StartCoroutine(SpawnRoomAtExit(nextPrefab, exit));

                    // Si la sala no fue creada, probar con la siguiente
                    if (!roomCreated)
                    {
                        nextPrefab = GetRandomRoom();
                    }
                    else
                    {
                        // Intentar spawnear un enemigo tras crear una sala
                        TrySpawnEnemy(nextPrefab);
                    }
                }
                else
                {
                    Debug.LogWarning("No hay más salidas disponibles para conectar.");
                    break;
                }
            }

            // Verificar si se han generado suficientes salas
            if (spawnedRooms.Count >= minRooms)
            {
                return true;
            }
            else
            {
                retryCount++;
            }
        }

        Debug.LogError("Generación fallida tras varios intentos. Por favor revisa las configuraciones.");
        return false;
    }

    void TrySpawnEnemy(GameObject room)
    {
        if (enemiesList.Count > 0 && Random.value < enemySpawnChance)
        {
            Transform enemySpawnPoint = room.transform.Find("enemySpawn");

            if (enemySpawnPoint)
            {
                GameObject enemyPrefab = enemiesList[Random.Range(0, enemiesList.Count)];
                Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
                Debug.Log("Enemigo generado.");
            }
        }

        // Incrementar la probabilidad de spawn de enemigos, pero no exceder el límite máximo
        enemySpawnChance = Mathf.Min(enemySpawnChance + spawnIncreasePerRoom, maxEnemySpawnChance);
        Debug.Log($"Probabilidad de spawnear enemigo incrementada a: {enemySpawnChance * 100}%");
    }

    // Función para destruir todas las salas generadas
    public void ClearDungeon()
    {
        foreach (GameObject room in spawnedRooms)
        {
            Destroy(room);  // Destruir la sala
        }
        spawnedRooms.Clear();  // Limpiar la lista de habitaciones generadas
        availableExits.Clear();  // Limpiar las salidas disponibles
    }

    GameObject GetRandomRoom()
    {
        return rooms[Random.Range(0, rooms.Length)];
    }

    GameObject GetRandomConnector()
    {
        return connectors[Random.Range(0, connectors.Length)];
    }

    void AddExits(GameObject room)
    {
        Transform[] exits = room.GetComponentsInChildren<Transform>();

        foreach (Transform exit in exits)
        {
            if (exit.name.Contains("Exit"))
            {
                ExitPoint exitPoint = new ExitPoint(exit, room);
                availableExits.Add(exitPoint);
            }
        }

        Debug.Log("Salidas disponibles: " + availableExits.Count);
    }

    ExitPoint GetRandomExit()
    {
        ExitPoint exit = availableExits[Random.Range(0, availableExits.Count)];
        availableExits.Remove(exit);  // Elimina la salida de la lista para no reutilizarla
        Debug.Log("Conectando a una salida. Salidas restantes: " + availableExits.Count);
        return exit;
    }

    IEnumerator SpawnRoomAtExit(GameObject roomPrefab, ExitPoint exitPoint)
    {
        // Variables to hold temporary rooms
        GameObject tempHall = null;
        GameObject tempRoom = null;

        // Check if roomPrefab is Stairs.2 or StairsL.2
        if (roomPrefab.name.Contains("Stairs.2"))
        {
            // Try to instantiate HallIForm.2 and RoomI.2
            bool canBuild = TryBuildTemporarySequence(exitPoint, "HallIForm.2", "RoomI.2", out tempHall, out tempRoom);

            // Destroy temporary rooms after checking
            if (tempHall != null)
                Destroy(tempHall);
            if (tempRoom != null)
                Destroy(tempRoom);

            if (!canBuild)
            {
                roomCreated = false;
                // Mantener la salida no conectada en la lista
                availableExits.Add(exitPoint);
                yield break;
            }
        }
        else if (roomPrefab.name.Contains("StairsL.2"))
        {
            // Try to instantiate HallLForm.2 and RoomL.2
            bool canBuild = TryBuildTemporarySequence(exitPoint, "HallLForm.2", "RoomL.2", out tempHall, out tempRoom);

            // Destroy temporary rooms after checking
            if (tempHall != null)
                Destroy(tempHall);
            if (tempRoom != null)
                Destroy(tempRoom);

            if (!canBuild)
            {
                roomCreated = false;
                // Mantener la salida no conectada en la lista
                availableExits.Add(exitPoint);
                yield break;
            }
        }

        // Proceed to instantiate roomPrefab normally
        GameObject newRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
        Transform[] newRoomExits = newRoom.GetComponentsInChildren<Transform>();
        Transform newRoomExit = null;

        foreach (Transform exit in newRoomExits)
        {
            if (exit.name.Contains("Exit1"))
            {
                newRoomExit = exit;
                break;
            }
        }

        if (newRoomExit != null)
        {
            Quaternion baseRotation = exitPoint.room.transform.rotation;
            newRoom.transform.position = exitPoint.exitTransform.position;
            Quaternion rotationAdjustment = CalculateRotation(exitPoint.room, exitPoint.exitTransform);
            newRoom.transform.rotation = baseRotation * rotationAdjustment;

            Vector3 offset = exitPoint.exitTransform.position - newRoomExit.position;
            newRoom.transform.position += offset;

            // Verificar si el espacio está libre antes de agregar la sala
            if (IsSpaceFree(newRoom, newRoom.transform.position, newRoom.transform.rotation))
            {
                spawnedRooms.Add(newRoom);
                AddExits(newRoom);
                // Eliminamos solo las salidas correctamente conectadas
                availableExits.RemoveAll(e => e.exitTransform == newRoomExit || e.exitTransform == exitPoint.exitTransform);
                PlaceDecorations(newRoom);
                roomCreated = true;
            }
            else
            {
                Debug.LogWarning("Colisión detectada. No se puede instanciar la nueva sala aquí.");
                Destroy(newRoom);  // Destruir la sala si está colisionando
                roomCreated = false;  // Indicar que la sala no se pudo crear

                // Mantener la salida no conectada en la lista
                availableExits.Add(exitPoint);
            }
        }
        else
        {
            Debug.LogWarning("No se encontraron salidas en la nueva sala.");
        }

        yield return null;
    }

    Quaternion CalculateRotation(GameObject connectedRoom, Transform exitTransform)
    {
        string roomName = connectedRoom.name;
        string exitName = exitTransform.name;
        Vector3 desiredRotation = Vector3.zero;

        // Condiciones para establecer la rotación exacta según el room y el exit
        if (roomName.Contains("RoomInitial.1") && exitName.Contains("Exit1"))
        {
            desiredRotation = new Vector3(0, 0, 0);
        }
        else if (roomName.Contains("RoomI.2") && exitName.Contains("Exit2"))
        {
            desiredRotation = new Vector3(0, 0, 0);
        }
        else if (roomName.Contains("RoomL.2") && exitName.Contains("Exit2"))
        {
            desiredRotation = new Vector3(0, -90, 0);
        }
        else if (roomName.Contains("RoomT.3"))
        {
            if (exitName.Contains("Exit2"))
            {
                desiredRotation = new Vector3(0, -90, 0);
            }
            else if (exitName.Contains("Exit3"))
            {
                desiredRotation = new Vector3(0, 90, 0);
            }
        }
        else if (roomName.Contains("RoomX.4"))
        {
            if (exitName.Contains("Exit2"))
            {
                desiredRotation = new Vector3(0, -90, 0);
            }
            else if (exitName.Contains("Exit3"))
            {
                desiredRotation = new Vector3(0, 0, 0);
            }
            else if (exitName.Contains("Exit4"))
            {
                desiredRotation = new Vector3(0, 90, 0);
            }
        }
        else if (roomName.Contains("Doorway.2") && exitName.Contains("Exit2"))
        {
            desiredRotation = new Vector3(0, 0, 0);
        }
        else if (roomName.Contains("HallIForm.2") && exitName.Contains("Exit2"))
        {
            desiredRotation = new Vector3(0, 0, 0);
        }
        else if (roomName.Contains("HallLForm.2") && exitName.Contains("Exit2"))
        {
            desiredRotation = new Vector3(0, -90, 0);
        }
        else if (roomName.Contains("HallShort.2") && exitName.Contains("Exit2"))
        {
            desiredRotation = new Vector3(0, 0, 0);
        }
        else if (roomName.Contains("HallTForm.3"))
        {
            if (exitName.Contains("Exit2"))
            {
                desiredRotation = new Vector3(0, -90, 0);
            }
            else if (exitName.Contains("Exit3"))
            {
                desiredRotation = new Vector3(0, 90, 0);
            }
        }
        else if (roomName.Contains("HallXForm.4"))
        {
            if (exitName.Contains("Exit2"))
            {
                desiredRotation = new Vector3(0, 90, 0);
            }
            else if (exitName.Contains("Exit3"))
            {
                desiredRotation = new Vector3(0, 0, 0);
            }
            else if (exitName.Contains("Exit4"))
            {
                desiredRotation = new Vector3(0, -90, 0);
            }
        }
        else if (roomName.Contains("Stairs.2") && exitName.Contains("Exit2"))
        {
            desiredRotation = new Vector3(0, 0, 0);
        }
        else if (roomName.Contains("StairsL.2") && exitName.Contains("Exit2"))
        {
            desiredRotation = new Vector3(0, 90, 0);
        }

        Debug.Log($"Rotación deseada para {roomName} en {exitName}: {desiredRotation}");
        return Quaternion.Euler(desiredRotation);
    }

    void PlaceDecorations(GameObject room)
    {
        // Spawns para cada tipo de decoración
        Transform paintsSpawn = room.transform.Find("paintsSpawn");
        Transform storageSpawn = room.transform.Find("storageSpawn");
        Transform statueSpawn = room.transform.Find("statueSpawn");
        Transform decorationSpawn = room.transform.Find("decorationSpawn");
        //Transform portalSpawn = room.transform.Find("portalSpawn");

        // Instanciar aleatoriamente objetos de cada lista en su spawn correspondiente
        if (paintsSpawn && Random.value < 0.6f && paintsList.Count > 0)
        {
            Debug.Log("Instanciando pintura en paintsSpawn.");

            GameObject paintPrefab = paintsList[Random.Range(0, paintsList.Count)];
            Quaternion originalRotation = paintPrefab.transform.rotation;
            Quaternion spawnRotation = paintsSpawn.rotation;

            // Crear una nueva rotación que conserve el Z del prefab original pero use X e Y del spawn
            Quaternion finalRotation = Quaternion.Euler(spawnRotation.eulerAngles.x, spawnRotation.eulerAngles.y, originalRotation.eulerAngles.z);

            Instantiate(paintPrefab, paintsSpawn.position, finalRotation);
        }
        else
        {
            Debug.Log("No se instanció pintura en paintsSpawn.");
        }

        if (storageSpawn && Random.value < 0.5f && storageList.Count > 0)
        {
            Debug.Log("Instanciando storage en storageSpawn.");
            GameObject storageItem = Instantiate(storageList[Random.Range(0, storageList.Count)], storageSpawn.position, Quaternion.identity);

            // Instanciar arma dentro del storage
            Transform weaponSpawn = storageItem.transform.Find("weaponSpawn");
            if (weaponSpawn && weaponsList.Count > 0)
            {
                Debug.Log("Instanciando arma dentro del storage en weaponSpawn.");

                // Toma la rotación del weaponSpawn para instanciar el arma
                GameObject weapon = Instantiate(
                    weaponsList[Random.Range(0, weaponsList.Count)],
                    weaponSpawn.position,
                    weaponSpawn.rotation
                );
            }
            else
            {
                Debug.Log("No se instanció arma en weaponSpawn dentro del storage.");
            }
        }
        else
        {
            Debug.Log("No se instanció storage en storageSpawn.");
        }

        if (statueSpawn && Random.value < 0.7f && statuesList.Count > 0)
        {
            Debug.Log("Instanciando estatua en statueSpawn.");
            GameObject statuePrefab = statuesList[Random.Range(0, statuesList.Count)];

            // Usar la rotación original del prefab
            Instantiate(statuePrefab, statueSpawn.position, statuePrefab.transform.rotation);
        }
        else
        {
            Debug.Log("No se instanció estatua en statueSpawn.");
        }

        if (decorationSpawn && Random.value < 0.8f && decorationList.Count > 0)
        {
            Debug.Log("Instanciando decoración en decorationSpawn.");
            Instantiate(decorationList[Random.Range(0, decorationList.Count)], decorationSpawn.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("No se instanció decoración en decorationSpawn.");
        }

        /*if (portalSpawn && Random.value < 0.5f && portalList.Count > 0)
        {
            Debug.Log("Instanciando portal en portalSpawn.");
            Instantiate(portalList[Random.Range(0, portalList.Count)], portalSpawn.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("No se instanció portal en portalSpawn.");
        }*/
    }

    // Función para colocar puertas en los exits no conectados
    void PlaceDoorsOnUnconnectedExits()
    {
        foreach (ExitPoint exit in availableExits)
        {
            // Verificar si la salida pertenece a RoomInitial.1 y es Exit1
            if (exit.room.name.Contains("RoomInitial.1") && exit.exitTransform.name.Contains("Exit1"))
            {
                Debug.Log("No se colocará puerta en Exit1 de RoomInitial.1.");
                continue; // Saltar esta iteración para que no se instancie la puerta
            }

            // Instanciar el prefab de la puerta en la posición de la salida no conectada
            Instantiate(doorPrefab, exit.exitTransform.position, exit.exitTransform.rotation);
            Debug.Log("Puerta colocada en: " + exit.exitTransform.position);
        }
    }


    // Función para verificar que el lugar donde se va a instanciar está libre
    bool IsSpaceFree(GameObject roomPrefab, Vector3 position, Quaternion rotation)
    {
        Collider[] colliders = roomPrefab.GetComponentsInChildren<Collider>();

        foreach (Collider roomCollider in colliders)
        {
            Vector3 colliderCenter = position + rotation * (roomCollider.transform.localPosition);
            Quaternion colliderRotation = rotation * roomCollider.transform.localRotation;

            Vector3 halfExtents = roomCollider.bounds.extents;

            // Perform an overlap check for each collider
            Collider[] hitColliders = Physics.OverlapBox(colliderCenter, halfExtents, colliderRotation);

            foreach (Collider hitCollider in hitColliders)
            {
                if (!hitCollider.transform.IsChildOf(roomPrefab.transform))
                {
                    return false;
                }
            }
        }

        return true;
    }

    bool TryBuildTemporarySequence(ExitPoint exitPoint, string hallName, string roomName, out GameObject tempHall, out GameObject tempRoom)
    {
        tempHall = null;
        tempRoom = null;

        // Find the prefabs
        GameObject hallPrefab = FindPrefabByName(hallName);
        GameObject roomPrefab = FindPrefabByName(roomName);

        if (hallPrefab == null || roomPrefab == null)
        {
            Debug.LogError("Could not find prefabs for " + hallName + " or " + roomName);
            return false;
        }

        // Instantiate tempHall
        tempHall = Instantiate(hallPrefab, Vector3.zero, Quaternion.identity);

        // Get the exits of tempHall
        Transform tempHallExit1 = GetExitTransform(tempHall, "Exit1");
        Transform tempHallExit2 = GetExitTransform(tempHall, "Exit2");

        if (tempHallExit1 == null || tempHallExit2 == null)
        {
            Debug.LogError("Temp Hall does not have Exit1 or Exit2");
            Destroy(tempHall);
            return false;
        }

        // Align tempHall's Exit1 to exitPoint.exitTransform
        AlignRoomToExit(tempHall, tempHallExit1, exitPoint.exitTransform);

        // Instantiate tempRoom
        tempRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);

        // Get the exit of tempRoom
        Transform tempRoomExit = GetExitTransform(tempRoom, "Exit1");

        if (tempRoomExit == null)
        {
            Debug.LogError("Temp Room does not have Exit1");
            Destroy(tempHall);
            Destroy(tempRoom);
            return false;
        }

        // Align tempRoom's Exit1 to tempHall's Exit2
        AlignRoomToExit(tempRoom, tempRoomExit, tempHallExit2);

        // Check if the space is free for tempHall and tempRoom
        bool spaceFree = IsSpaceFree(tempHall, tempHall.transform.position, tempHall.transform.rotation)
                         && IsSpaceFree(tempRoom, tempRoom.transform.position, tempRoom.transform.rotation);

        return spaceFree;
    }

    GameObject FindPrefabByName(string prefabName)
    {
        foreach (GameObject prefab in rooms)
        {
            if (prefab.name.Contains(prefabName))
            {
                return prefab;
            }
        }
        foreach (GameObject prefab in connectors)
        {
            if (prefab.name.Contains(prefabName))
            {
                return prefab;
            }
        }
        return null;
    }

    Transform GetExitTransform(GameObject room, string exitName)
    {
        Transform[] roomExits = room.GetComponentsInChildren<Transform>();
        foreach (Transform exit in roomExits)
        {
            if (exit.name.Contains(exitName))
            {
                return exit;
            }
        }
        return null;
    }

    void AlignRoomToExit(GameObject room, Transform roomExit, Transform targetExit)
    {
        // Calculate rotation to align roomExit's forward to targetExit's backward
        Vector3 forwardToMatch = -targetExit.forward;
        Quaternion rotation = Quaternion.LookRotation(forwardToMatch, Vector3.up);

        // Adjust for the exit's local rotation
        Quaternion exitRotationOffset = Quaternion.Inverse(roomExit.rotation) * room.transform.rotation;

        room.transform.rotation = rotation * exitRotationOffset;

        // Position the room so that roomExit.position matches targetExit.position
        Vector3 positionOffset = targetExit.position - roomExit.position;
        room.transform.position += positionOffset;
    }

}