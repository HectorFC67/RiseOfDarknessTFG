using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public GameObject[] rooms;   // Lista de objetos Room (RoomI.2, RoomL.2, etc.)
    public GameObject[] connectors;  // Lista de objetos Hall, Stairs o Doorway
    public GameObject initialRoom;  // Objeto RoomInitial.1, que será la sala inicial
    public GameObject doorPrefab;  // Prefab de la puerta

    public int minRooms = 6;
    public int maxRooms = 10;

    private List<GameObject> spawnedRooms = new List<GameObject>();  // Para almacenar las habitaciones generadas
    private List<ExitPoint> availableExits = new List<ExitPoint>();  // Para almacenar las salidas libres para conectar

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

    void Start()
    {
        GenerateDungeon();
        PlaceDoorsOnUnconnectedExits();  // Colocar puertas al final de la generación
    }

    void GenerateDungeon()
    {
        // Limpiar cualquier nivel generado anteriormente
        ClearDungeon();

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
                if (roomCreated == false)
                {
                    nextPrefab = GetRandomRoom();
                }
            }
            else
            {
                Debug.LogWarning("No hay más salidas disponibles para conectar.");
                break;
            }
        }

        // Verificar si se han generado suficientes salas
        if (spawnedRooms.Count < minRooms)
        {
            Debug.LogWarning($"Número de salas generado ({spawnedRooms.Count}) es menor al mínimo ({minRooms}). Reintentando generación...");
            ClearDungeon();  // Borrar el nivel actual
            GenerateDungeon();  // Volver a generar
        }
        else
        {
            // Colocar puertas al final si todo está bien
            PlaceDoorsOnUnconnectedExits();
        }
    }

    // Función para destruir todas las salas generadas
    void ClearDungeon()
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

        Debug.Log($"Rotación deseada para {roomName} en {exitName}: {desiredRotation}");
        return Quaternion.Euler(desiredRotation);
    }

    // Función para colocar puertas en los exits no conectados
    void PlaceDoorsOnUnconnectedExits()
    {
        foreach (ExitPoint exit in availableExits)
        {
            // Instanciar el prefab de la puerta en la posición de la salida no conectada
            Instantiate(doorPrefab, exit.exitTransform.position, exit.exitTransform.rotation);
            Debug.Log("Puerta colocada en: " + exit.exitTransform.position);
        }
    }

    // Función para verificar que el lugar donde se va a instanciar está libre
    bool IsSpaceFree(GameObject roomPrefab, Vector3 position, Quaternion rotation)
    {
        // Obtener el collider del prefab de la sala o conector
        Collider roomCollider = roomPrefab.GetComponentInChildren<Collider>();

        if (roomCollider == null)
        {
            Debug.LogWarning("El prefab no tiene un Collider.");
            return false;
        }

        // Calcular los límites del collider en la posición y rotación deseadas
        Vector3 halfExtents = roomCollider.bounds.extents / 2;
        Vector3 roomCenter = position + rotation * roomCollider.bounds.center;

        // Realizar un chequeo de colisión con un OverlapBox para detectar si colisiona con algo
        Collider[] hitColliders = Physics.OverlapBox(roomCenter, halfExtents, rotation);

        // Devolver true si no hay colisiones
        return hitColliders.Length == 0;
    }
}
