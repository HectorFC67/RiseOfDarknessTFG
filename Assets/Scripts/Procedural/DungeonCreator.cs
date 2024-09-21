using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public GameObject[] rooms;   // Lista de objetos Room (RoomI.2, RoomL.2, etc.)
    public GameObject[] connectors;  // Lista de objetos Hall, Stairs o Doorway
    public GameObject initialRoom;  // Objeto RoomInitial.1, que será la sala inicial

    public int minRooms = 6;
    public int maxRooms = 10;

    private List<GameObject> spawnedRooms = new List<GameObject>();  // Para almacenar las habitaciones generadas
    private List<ExitPoint> availableExits = new List<ExitPoint>();  // Para almacenar las salidas libres para conectar

    // Clase para definir un punto de salida
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

    // Clase para definir las salidas de una sala o conector
    [System.Serializable]
    public class RoomExits
    {
        public string roomName;          // Nombre de la sala o conector (e.g., RoomI.2, HallTForm.3)
        public Transform[] exits;        // Posiciones de las salidas
    }

    public RoomExits[] roomExitDefinitions;  // Lista de las definiciones de salidas para cada tipo de sala o conector

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        // 1. Instanciar la sala inicial
        GameObject currentRoom = Instantiate(initialRoom, transform.position, initialRoom.transform.rotation);  // Usar la rotación original del prefab
        spawnedRooms.Add(currentRoom);
        AddExits(currentRoom);

        int roomCount = Random.Range(minRooms, maxRooms + 1);
        Debug.Log("Generando " + roomCount + " salas.");

        bool lastWasRoom = true; // Bandera para alternar entre Room y Connector

        // 2. Generar el resto de las salas
        for (int i = 1; i < roomCount * 2; i++)  // *2 para alternar entre Room y Connector
        {
            GameObject nextPrefab;

            // Alternar entre Room y Connector
            if (lastWasRoom)
            {
                nextPrefab = GetRandomConnector();
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
                SpawnRoomAtExit(nextPrefab, exit);
            }
            else
            {
                Debug.LogWarning("No hay más salidas disponibles para conectar.");
                break;
            }
        }
    }

    GameObject GetRandomRoom()
    {
        return rooms[Random.Range(0, rooms.Length)];
    }

    GameObject GetRandomConnector()
    {
        return connectors[Random.Range(0, connectors.Length)];
    }

    // Esta función obtiene las salidas de una sala o conector según su tipo
    void AddExits(GameObject room)
    {
        string roomName = room.name.Replace("(Clone)", "").Trim();
        RoomExits roomExits = System.Array.Find(roomExitDefinitions, re => re.roomName == roomName);

        if (roomExits != null)
        {
            foreach (Transform exit in roomExits.exits)
            {
                // Crear un nuevo ExitPoint y añadirlo a la lista de salidas disponibles
                ExitPoint exitPoint = new ExitPoint(exit, room);
                availableExits.Add(exitPoint);
            }
        }
        else
        {
            Debug.LogWarning("No se encontraron definiciones de salidas para " + roomName);
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

    void SpawnRoomAtExit(GameObject roomPrefab, ExitPoint exitPoint)
    {
        RoomExits roomExits = System.Array.Find(roomExitDefinitions, re => re.roomName == roomPrefab.name.Replace("(Clone)", "").Trim());

        if (roomExits != null && roomExits.exits.Length > 0)
        {
            // Usamos la primera salida como referencia para posicionar y rotar la nueva sala
            Transform newRoomExit = roomExits.exits[0];

            // Calcular la posición y rotación que alinee las dos salidas
            Vector3 offset = exitPoint.exitTransform.position - newRoomExit.position;
            Quaternion rotation = Quaternion.LookRotation(exitPoint.exitTransform.forward, Vector3.up) * Quaternion.Inverse(newRoomExit.rotation);

            // Instanciar la nueva sala
            GameObject newRoom = Instantiate(roomPrefab, exitPoint.exitTransform.position - offset, rotation);
            spawnedRooms.Add(newRoom);

            // Añadir las salidas de la nueva sala
            AddExits(newRoom);
        }
        else
        {
            Debug.LogWarning("No se encontraron salidas para " + roomPrefab.name);
        }
    }
}
