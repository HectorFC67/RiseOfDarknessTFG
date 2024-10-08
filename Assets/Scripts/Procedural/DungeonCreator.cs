using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public GameObject[] rooms;   // Lista de objetos Room (RoomI.2, RoomL.2, etc.)
    public GameObject[] connectors;  // Lista de objetos Hall, Stairs o Doorway
    public GameObject initialRoom;  // Objeto RoomInitial.1, que sera la sala inicial
    public GameObject doorPrefab;  // Prefab de la puerta

    public int minRooms = 6;
    public int maxRooms = 10;

    private List<GameObject> spawnedRooms = new List<GameObject>();  // Para almacenar las habitaciones generadas
    private List<ExitPoint> availableExits = new List<ExitPoint>();  // Para almacenar las salidas libres para conectar

    private int stairAttempts = 0;  // Contador de intentos de instanciar sin escaleras

    private bool roomCreated = false;

    public class ExitPoint
    {
        public Transform exitTransform;  // Transform de la salida
        public GameObject room;          // Referencia a la sala o conector donde esta la salida

        public ExitPoint(Transform exitTransform, GameObject room)
        {
            this.exitTransform = exitTransform;
            this.room = room;
        }
    }

    void Start()
    {
        GenerateDungeon();
        PlaceDoorsOnUnconnectedExits();  // Colocar puertas al final de la generacion
    }

    void GenerateDungeon()
    {
        // 1. Instanciar la sala inicial
        GameObject currentRoom = Instantiate(initialRoom, transform.position, initialRoom.transform.rotation);
        spawnedRooms.Add(currentRoom);
        AddExits(currentRoom);

        int roomCount = Random.Range(minRooms, maxRooms + 1);
        Debug.Log("Generando " + roomCount + " salas.");

        bool lastWasRoom = true; // Bandera para alternar entre Room y Connector

        // 2. Generar el resto de las salas
        for (int i = 1; i < roomCount * 2; i++)  // *2 para alternar entre Room y Connector
        {
            GameObject nextPrefab;

            if (lastWasRoom)
            {
                // Si se han hecho 3 intentos sin instanciar escaleras, forzar la creacion de una
                if (stairAttempts >= 3)
                {
                    nextPrefab = GetRandomStairs();  // Forzar Stairs
                    stairAttempts = 0;  // Reiniciar el contador
                }
                else
                {
                    nextPrefab = GetRandomConnector();
                    if (nextPrefab.name.Contains("Stairs"))
                    {
                        stairAttempts = 0;  // Si es escalera, reiniciar contador
                    }
                    else
                    {
                        stairAttempts++;  // Incrementar el contador si no es escalera
                    }
                }
                lastWasRoom = false;
            }
            else
            {
                nextPrefab = GetRandomRoom();
                lastWasRoom = true;
            }

            // 3. Conectar la sala al azar con una salida disponible
            if (availableExits.Count > 0)
            {
                ExitPoint exit = GetRandomExit();            
               StartCoroutine(SpawnRoomAtExit(nextPrefab, exit));
               if(roomCreated==false){
                    nextPrefab = GetRandomRoom();
                }
            }
            else
            {
                Debug.LogWarning("No hay mas salidas disponibles para conectar.");
                break;
            }
        }
    }

    GameObject GetRandomStairs()
    {
        // Filtra el array de conectores para obtener solo las escaleras
        List<GameObject> stairsOptions = new List<GameObject>();
        foreach (GameObject connector in connectors)
        {
            if (connector.name.Contains("Stairs"))
            {
                stairsOptions.Add(connector);
            }
        }
        return stairsOptions[Random.Range(0, stairsOptions.Count)];
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

        // Condiciones para establecer la rotacion exacta segun el room y el exit
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

        Debug.Log($"Rotacion deseada para {roomName} en {exitName}: {desiredRotation}");
        return Quaternion.Euler(desiredRotation);
    }

    // Funcion para colocar puertas en los exits no conectados
    void PlaceDoorsOnUnconnectedExits()
    {
        foreach (ExitPoint exit in availableExits)
        {
            // Instanciar el prefab de la puerta en la posicion de la salida no conectada
            Instantiate(doorPrefab, exit.exitTransform.position, exit.exitTransform.rotation);
            Debug.Log("Puerta colocada en: " + exit.exitTransform.position);
        }
    }

    // Funcion para verificar que el lugar donde se va a instanciar esta libre
    bool IsSpaceFree(GameObject roomPrefab, Vector3 position, Quaternion rotation)
    {
        // Obtener el collider del prefab de la sala o conector
        Collider roomCollider = roomPrefab.GetComponentInChildren<Collider>();

        if (roomCollider == null)
        {
            Debug.LogWarning("El prefab no tiene un Collider.");
            return false;
        }

        // Calcular los limites del collider en la posicion y rotacion deseadas
        Vector3 halfExtents = roomCollider.bounds.extents / 2;
        Vector3 roomCenter = position + rotation * roomCollider.bounds.center;

        // Realizar un chequeo de colision con un OverlapBox para detectar si colisiona con algo
        Collider[] hitColliders = Physics.OverlapBox(roomCenter, halfExtents, rotation);

        // Devolver true si no hay colisiones
        return hitColliders.Length == 0;
    }

}
