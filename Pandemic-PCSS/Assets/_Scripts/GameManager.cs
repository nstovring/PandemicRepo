using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using System.Collections;

[Serializable]
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    //Stack Variables
    public static Stack infectCardStack;
    public static Stack playerCardStack;
    public static Stack infectDiscardStack;
    public static Stack playerDiscardStack;
    public static Stack roleCardStack;


    //City Variables
    public GameObject cityPrefab;

    public static City[] researchCenterCities;
    /// <summary>
    /// Returns the city coressponding to the ID provided
    /// </summary>
    /// <param name="iD"></param>
    /// <returns></returns>
    public static City GetCityFromID(int iD)
    {
        return cities[iD - 1];
    }
    public static City[] cities = new City[48];

    /// <summary>
    /// Returns the city corresponding to the string provided
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static City GetCityFromName(string name)
    {
        for (int i = 0; i < cityNames.GetLength(0); i++)
        {
            if (cityNames[i].Equals(name))
            {
                return cities[i];
            }
        }
        return null;
    }

    private Vector2[] cityPositions = new Vector2[48];

    static readonly string[] cityColors = { "Blue", "Yellow", "Black", "Red" };
    public static readonly string[] cityNames = { "San Fransisco", "Chicago", "Montreal", "Atlanta", "Washington", "New York", "London",
            "Madrid", "Paris", "Essen", "Milan", "St. Petersbug", "Los Angeles", "Mexico City",
            "Miami", "Bogota", "Lima", "Santiago", "Buenos Aires", "Sao Paulo", "Lagos",
            "Kinshasa", "Khartoum", "Johannesburg", "Algiers", "Istanbul", "Moscow", "Tehran",
            "Baghdad", "Cairo", "Riyadh", "Karrachi", "Delhi", "Mumbai", "Chennai", "Kolkata",
            "Bangkok", "Jakarta", "Ho Chi minh City", "Hong Kong", "Shanghai", "Beijing", "Seoul",
            "Tokyo", "Osaka", "Taipei", "Manila", "Sydney" };
    //The city connections correspond in order to a city in the cities array 
    public readonly int[][] connectedCities = {
            new int[] { 13, 2, 44, 47 },
            new int[] { 1, 3, 13, 14, 4 },
            new int[] { 2, 6, 5 },
            new int[] { 2, 5, 15 },
            new int[] { 4, 6, 3, 15 },
            new int[] { 5, 3, 7, 8 },
            new int[] { 6, 8, 9, 10 },
            new int[] { 6, 7, 9, 20, 25 },
            new int[] { 8, 7, 10, 11, 25 },
            new int[] { 7, 9, 11, 12, },
            new int[] { 9, 10, 26 },
            new int[] { 10, 26, 27 },
            new int[] { 1, 2, 14, 48 },
            new int[] { 2, 13, 15, 16, 17 },
            new int[] { 4, 5, 14, 16 },
            new int[] { 14, 15, 17, 19, 20 },
            new int[] { 14, 16, 18 },
            new int[] { 17 },
            new int[] { 16, 20 },
            new int[] { 8, 16, 19, 21 },
            new int[] { 20, 22, 23 },
            new int[] { 21, 23, 24 },
            new int[] { 21, 22, 23 },
            new int[] { 22, 23 },
            new int[] { 8, 9, 26, 30 },
            new int[] { 11, 12, 25, 27, 29, 30 },
            new int[] { 12, 26, 28 },
            new int[] { 27, 29, 32, 33 },
            new int[] { 26, 28, 30, 31, 32 },
            new int[] { 23, 25, 26, 29, 31 },
            new int[] { 29, 30, 32 },
            new int[] { 28, 29, 31, 33, 34 },
            new int[] { 28, 32, 34, 35, 36 },
            new int[] { 32, 33, 35 },
            new int[] { 33, 34, 36, 37, 38 },
            new int[] { 33, 35, 37, 40 },
            new int[] { 35, 36, 38, 39, 40 },
            new int[] { 35, 37, 39, 48 },
            new int[] { 37, 38, 40, 47 },
            new int[] { 36, 37, 39, 41, 46, 47 },
            new int[] { 40, 42, 43, 44, 46 },
            new int[] { 41, 43 },
            new int[] { 41, 42, 44 },
            new int[] { 1, 41, 43, 45 },
            new int[] { 44, 46 },
            new int[] { 40, 41, 45, 47 },
            new int[] { 1, 39, 40, 46, 48},
            new int[] { 13, 38, 47 }
            };

    //General Game Variables
    [SyncVar]
    int infectionRate = 2;
    [SyncVar]
    int epidemicCount = 0;
    [SyncVar]
    int outbreakCounter = 0;
    int maxSingleDisease = 24;


    /// <summary>
    /// Returns the current disease with the highest amount
    /// </summary>
    /// <returns></returns>
    int currentDiseaseSpread()
    {
        if (redDiseaseSpread >= maxSingleDisease)
        {
            return redDiseaseSpread;
        }
        if (blueDiseaseSpread >= maxSingleDisease)
        {
            return blueDiseaseSpread;
        }
        if (yellowDiseaseSpread >= maxSingleDisease)
        {
            return yellowDiseaseSpread;
        }
        if (blackDiseaseSpread >= maxSingleDisease)
        {
            return blackDiseaseSpread;
        }
        return 0;
    }
    int maxDiseaseSpread = 96;

    public SyncListInt SyncListPlayerCardSort = new SyncListInt();
    public SyncListInt SyncListinfectionSort = new SyncListInt();
    public SyncListInt SyncListPlayerDiscardSort = new SyncListInt();
    public SyncListInt SyncListinfectionDiscardSort = new SyncListInt();

    [SyncVar]
    public int redDiseaseSpread = 0;
    [SyncVar]
    public int blueDiseaseSpread = 0;
    [SyncVar]
    public int yellowDiseaseSpread = 0;
    [SyncVar]
    public int blackDiseaseSpread = 0;

    [SyncVar]
    public int turnOrder = 0;

    [SyncVar]
    public bool redCure;
    [SyncVar]
    public bool blueCure;
    [SyncVar]
    public bool yellowCure;
    [SyncVar]
    public bool blackCure;
    public bool GetCureFromString(string color)
    {
        switch (color)
        {
            case "Blue":
                return blueCure;
            case "Yellow":
                return yellowCure;
            case "Black":
                return blackCure;
            case "Red":
                return redCure;
        }
        return false;
    }

    public static List<Player> players = new List<Player>();
    private NetworkIdentity netIdentity;
    [SyncVar]
    public bool initialize = true;

    private void Start()
    {
        instance = this;
        netIdentity = GetComponent<NetworkIdentity>();
    }

    public int testingPlayers = 2;

    [ClientRpc]
    public void Rpc_TryUpdateStacks()
    {

        infectCardStack.SortCardsToList(SyncListinfectionSort);
        infectDiscardStack.SortCardsToList(SyncListinfectionDiscardSort);

        playerCardStack.SortCardsToList(SyncListPlayerCardSort);
        playerDiscardStack.SortCardsToList(SyncListPlayerDiscardSort);
    }

    [Command]
    public void Cmd_TryUpdateStacks()
    {
        Rpc_TryUpdateStacks();
    }

    void Awake()
    {

    }

    //[ServerCallback]
    private void Update()
    {
        if (isServer)
        {
            if (netIdentity.observers.Count > 1 && isServer)
            {
                NetworkServer.SpawnWithClientAuthority(this.transform.gameObject, netIdentity.observers[1]);
            }
        }

        if (Input.GetKeyUp(KeyCode.S) && initialize && isServer)
        {
            Rpc_InitializeBoard();
            initialize = false;
        }
        if (Input.GetKeyUp(KeyCode.D) && isServer)
        {
            int[] roles = new[]
            {
                        UnityEngine.Random.Range(0, 7), UnityEngine.Random.Range(0, 7), UnityEngine.Random.Range(0, 7),
                        UnityEngine.Random.Range(0, 7)
                    };
            Rpc_InitializePlayers(roles);
        }
        if (Input.GetKeyUp(KeyCode.E) && isServer)
        {
            Rpc_InitializeStacks();
        }
    }

    void Rpc_InitializeStacks()
    {
        playerCardStack = GameObject.Find("playerCardStack").GetComponent<Stack>();
        playerCardStack.addEpidemicCards();
        playerCardStack.shuffleStack();
        int[] indexInts = new int[playerCardStack.cards.Count];
        Debug.Log(indexInts.Length);
        for (int i = 0; i < indexInts.Length; i++)
        {
            indexInts[i] = playerCardStack.cards[i].Id;
        }
        Cmd_ChangePlayerSyncList(indexInts);
    }


    [Command]
    void Cmd_ChangePlayerSyncList(int[] indexInts)
    {
        int length = SyncListPlayerCardSort.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            SyncListPlayerCardSort.RemoveAt(i);
        }

        for (int i = 0; i < indexInts.Length; i++)
        {
            SyncListPlayerCardSort.Add(indexInts[i]);
        }
        Rpc_TryUpdateStacks();
    }

    [ClientRpc]
    private void Rpc_InitializePlayers(int[] roles)
    {
        GameObject[] playersGameObjects = GameObject.FindGameObjectsWithTag("Player");

        foreach (var i in playersGameObjects)
        {
            players.Add(i.GetComponent<Player>());
        }
        StartCoroutine("DelayPlayerInitialization", roles);

        players[0].active = true;
    }

    private IEnumerator DelayPlayerInitialization(int[] roles)
    {
        GameObject[] playersGameObjects = GameObject.FindGameObjectsWithTag("Player");
        playersGameObjects[0].GetComponent<Player>().Initialize(roles[0]);
        yield return new WaitForSeconds(1);
        playersGameObjects[1].GetComponent<Player>().Initialize(roles[1]);
    }

    [Command]
    public void CmdSwitchTurn()
    {
        int lastTurn = turnOrder;
        turnOrder = turnOrder == netIdentity.observers.Count + 1 ? 1 : turnOrder++;
        players[lastTurn].active = false;
        players[turnOrder].active = true;
        players[turnOrder].startTurn();

    }

    [ClientRpc]
    public void Rpc_InitializeBoard()
    {
        CreateCities();
        CreateStacks();
    }

    [ClientRpc]
    private void Rpc_Testing()
    {
        //Outbreak Testing code forthwith
        City HongKong = cities[4];
        City Shanghai = cities[5];
        City Kolkata = cities[6];
        City Bangkok = cities[7];

        InfectCity(Bangkok.cityId, Bangkok, 2);
        InfectCity(HongKong.cityId, HongKong, 2);
        InfectCity(Shanghai.cityId, Shanghai, 3);
        InfectCity(Kolkata.cityId, Kolkata, 4);
        CheckForOutbreak();
    }

    /// <summary>
    /// Instantiates four different stacks and initializes them
    /// </summary>
    //[Command]
    public static Stack AllCardsStack;

    private void CreateStacks()
    {
        AllCardsStack = new GameObject("AllCards").AddComponent<Stack>();
        AllCardsStack.Initialize(Stack.cardType.PLAYER_STACK);
        AllCardsStack.addEpidemicCards();

        infectCardStack = new GameObject("infectCardStack").AddComponent<Stack>();
        infectCardStack.Initialize(Stack.cardType.INFECTION);
        infectCardStack.shuffleStack();
        for (int j = 0; j < infectCardStack.cards.Count; j++)
        {
            if (isServer)
            {
                Debug.Log("Add");
                SyncListinfectionSort.Add(infectCardStack.cards[j].Id); //new stuff
            }
        }
        Destroy(infectCardStack.gameObject);

        playerCardStack = new GameObject("playerCardStack").AddComponent<Stack>();
        playerCardStack.Initialize(Stack.cardType.PLAYER_STACK);
        playerCardStack.shuffleStack();
        for (int j = 0; j < playerCardStack.cards.Count; j++)

        {
            if (isServer)
            {
                Debug.Log("Add");
                SyncListPlayerCardSort.Add(playerCardStack.cards[j].Id); //new stuff
            }
        }
        Destroy(playerCardStack.gameObject);

        roleCardStack = new GameObject("roleCardStack").AddComponent<Stack>();
        roleCardStack.Initialize(Stack.cardType.ROLE);
        if (isServer)
        {
            Rpc_CreateStacks();
            Rpc_InitialInfection();
        }
    }

    /// <summary>
    /// Copy the values from the server to the clients
    /// </summary>
    /// <param name="shuffledInfectInts"></param>
    /// <param name="shuffledCityInts"></param>
    [ClientRpc]
    private void Rpc_CreateStacks()
    {
        AllCardsStack = new GameObject("AllCards").AddComponent<Stack>();
        AllCardsStack.Initialize(Stack.cardType.PLAYER_STACK);
        AllCardsStack.addEpidemicCards();

        infectCardStack = new GameObject("infectCardStack").AddComponent<Stack>();
        infectCardStack.Initialize(Stack.cardType.INFECTION);

        playerCardStack = new GameObject("playerCardStack").AddComponent<Stack>();
        playerCardStack.Initialize(Stack.cardType.PLAYER_STACK);

        infectDiscardStack = new GameObject("infectDiscardStack").AddComponent<Stack>();
        infectDiscardStack.Initialize(Stack.cardType.INFECTION);
        infectDiscardStack.EmptyCards();

        playerDiscardStack = new GameObject("playerDiscardStack").AddComponent<Stack>();
        playerDiscardStack.Initialize(Stack.cardType.PLAYER_STACK);
        playerDiscardStack.EmptyCards();

        Cmd_TryUpdateStacks();
    }

    /// <summary>
    /// Method responsible for creating and initializing the cities
    /// </summary>
    private void CreateCities()
    {
        cityPositions = transform.GetChild(0).GetComponent<SaveCityPositions>().positions; // Get the position of the city from the SaveCityPositions class array positions
        int colorIncrement = 1;
        GameObject cityParent = new GameObject("CityParent"); // Instantiate an empty GameObject to serve as the parent to all cities
        for (int iD = 0, colorGroup = 0; iD < cities.Length; iD++, colorIncrement++)
        {
            GameObject cityGameObject = Instantiate(cityPrefab, cityPositions[iD], Quaternion.Euler(-90, 0, 0)) as GameObject; // Instantiate the city
            cityGameObject.transform.parent = cityParent.transform; // Set the parent
            colorIncrement = colorIncrement % 13 == 0 ? colorIncrement = 1 : colorIncrement % 13; // Color increment loops from 1->12
            cities[iD] = cityGameObject.GetComponent<City>(); // Assign the City class of the City GameObject to the cities array
            cities[iD].Initialize(iD + 1, connectedCities[iD], cityColors[colorGroup], cityNames[iD]); // Initialize the city

            colorGroup = colorIncrement >= 12 ? colorGroup + 1 : colorGroup; // If color increment oversteps or is equal to 12 increment colorGroup
        }
    }

    [Command]
    public void Cmd_Epidemic()
    {
        Card bottomCard = infectCardStack.cards[0]; // Pick the bottom card of the stack
        City infectedCity = GetCityFromID(bottomCard.Id);
        infectedCity.IncrementDiseaseSpread(infectedCity.color, 3);
        //Cmd_ReduceInfectionSyncListInt(infectedCity.cityId);
        SyncListinfectionDiscardSort.Add(infectedCity.cityId);
        SyncListinfectionSort.Remove(infectedCity.cityId);
        //Shuffle discards here
        infectDiscardStack.shuffleStack();
        //And then combine stacks
        for (int i = 0; i < SyncListinfectionDiscardSort.Count; i++)
        {
            SyncListinfectionSort.Add(SyncListinfectionDiscardSort[i]);
        }

        int length = SyncListinfectionDiscardSort.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            SyncListinfectionDiscardSort.RemoveAt(i);
        }

        infectCardStack.SortCardsToList(SyncListinfectionSort);
        infectDiscardStack.SortCardsToList(SyncListinfectionDiscardSort);
        //Set infectionRate & increment epidemicCount
        epidemicCount++;
        infectionRate = epidemicCount > 3 ? 3 : epidemicCount > 5 ? 4 : infectionRate;
        Rpc_TryUpdateStacks();
    }

    //The cities are infected from the top of the stack up during initialization

    public Card[] SortCardsToList(Card[] cards, SyncListInt sortListInt)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i] = AllCardsStack.cards[(sortListInt[i])];
        }
        return cards;
    }
    [ClientRpc]
    private void Rpc_InitialInfection()
    {
        int increment = 0;
        //Loop counts from the top nine cards down
        for (int i = infectCardStack.cards.Count - 1, infectRate = 3; i > infectCardStack.cards.Count - 10; i--, increment++)
        {
            increment = increment % 4 == 0 ? increment = 1 : increment % 4; //Infection progresses as such: 3 first cities get 3 diseaseMarker, 3 next get 2, 3 last gets 1
            Card infectionCard = infectCardStack.cards[i];
            InfectCity(infectionCard, infectRate);
            infectRate = increment >= 3 ? infectRate - 1 : infectRate;
        }
    }

    /// <summary>
    /// General Infection method meant for end of turn infection
    /// </summary>
    public void InfectCities()
    {
        for (int i = 1; i < infectionRate; i++)
        {
            Card infectionCard = infectCardStack.cards[SyncListinfectionSort.Count - i];
            InfectCity(infectionCard, 1);
        }
        CheckForOutbreak();
    }

    /// <summary>
    /// Infection method for directly infecting a specific city
    /// </summary>
    /// <param name="infectionCard"></param>
    /// <param name="infectRate"></param>
    private void InfectCity(Card infectionCard, int infectRate)
    {
        City infectedCity = GetCityFromID(infectionCard.Id);
        infectedCity.IncrementDiseaseSpread(infectedCity.color, infectRate);
        Cmd_ReduceInfectionSyncListInt(infectedCity.cityId);
        Cmd_TryUpdateStacks();
        SetDiseaseSpread(infectedCity.color, infectRate);
    }

    [Command]
    void Cmd_ReduceInfectionSyncListInt(int card)
    {
        SyncListinfectionDiscardSort.Add(card);
        SyncListinfectionSort.Remove(card);
        //Debug.Log("Infectionlist new length:" + SyncListinfectionSort.Count);
    }


    [Command]
    public void Cmd_RemoveFromCityList(int card)
    {
        SyncListPlayerCardSort.Remove(card);
        //Debug.Log("PlayerList new length:" + SyncListPlayerCardSort.Count);
    }

    [Command]
    public void Cmd_AddToCityDiscardList(int removal)
    {
        if (SyncListPlayerDiscardSort.Contains(removal))
        {
            return;
        }
        SyncListPlayerDiscardSort.Add(removal);
        Rpc_TryUpdateStacks();
        Debug.Log("PlayerListDiscard new length:" + SyncListPlayerDiscardSort.Count);

    }
    [ClientRpc]
    public void Rpc_AddToCityDiscardList(int removal)
    {
        Cmd_AddToCityDiscardList(removal);
    }

    /// <summary>
    /// Infection method for directly infecting a specific city
    /// </summary>
    /// <param name="cityID"></param>
    /// <param name="infectRate"></param>
    private void InfectCity(int cityID, City infectionSource, int infectRate)
    {
        City infectedCity = GetCityFromID(cityID);
        infectedCity.IncrementDiseaseSpread(infectionSource.color, infectRate);
        //infectedCity.diseaseSpread += infectRate;
        SetDiseaseSpread(infectedCity.color, infectRate);
    }

    //[Command]
    public void SetDiseaseSpread(string color, int infectRate)
    {
        switch (color)
        {
            case "Blue":
                blueDiseaseSpread += infectRate;
                break;
            case "Black":
                blackDiseaseSpread += infectRate;
                break;
            case "Yellow":
                yellowDiseaseSpread += infectRate;
                break;
            case "Red":
                redDiseaseSpread += infectRate;
                break;
        }
    }

    public void CheckForOutbreak()
    {
        for (int i = 0; i < cities.Length; i++)
        {
            if (cities[i].DiseaseSpread > 3 && !cities[i].locked)
            {
                cities[i].locked = true;
                Debug.Log("Outbreak happened in " + (cityNames[i]));
                SpreadOutbreak(i);
            }
        }
        //Unlock cities and set the disease counter to normal
        ResetCities();
    }
    public void SpreadOutbreak(int cityIndex)
    {
        City outbreakSource = cities[cityIndex];
        for (int j = 0; j < outbreakSource.connectedCityIDs.Length; j++)
        {
            City infectedCity = GetCityFromID(outbreakSource.connectedCityIDs[j]);
            InfectCity(outbreakSource.connectedCityIDs[j], outbreakSource, 1);
            Debug.Log("Disease has spread to " + infectedCity.name + " " + infectedCity.DiseaseSpread);
        }
        CheckForOutbreak();
        outbreakCounter++;
    }

    public void ResetCities()
    {
        foreach (City t in cities)
        {
            if (t.DiseaseSpread > 3 && t.locked)
            {
                SetDiseaseSpread(t.color, -(t.DiseaseSpread - 3));
                t.ResetValues();
            }
        }
    }
    [ClientRpc]
    public void Rpc_CheckForGameOver()
    {
        if (redCure && blackCure && blueCure && yellowCure)
        {
            WinGame();
        }
        if (outbreakCounter >= 8 || playerCardStack.cards.Count < 2 || currentDiseaseSpread() >= maxSingleDisease)
        {
            LoseGame();
        }
    }
    public void WinGame()
    {
        Debug.Log("Game Won!");
    }
    public void LoseGame()
    {
        Debug.Log("Game lost!");
    }
}

