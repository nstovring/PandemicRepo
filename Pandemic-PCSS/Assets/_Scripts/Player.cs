using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;


public class Player : NetworkBehaviour
{
    [SyncVar] public int cityID;
    [SyncVar] public bool active;

    //used in the cure 
    public int[] discardArray;


    public Hand hand;
    public City CurrentCity;

    //public _roleCard roleCard;

    public _roleCard roleCard;
    private GameManager gameManager;
    public int actionsLeft;
    public int[][] actionsTaken;
    private int count;
    private int currentCard;


    private void Awake()
    {
        if (isServer)
        {
            NetworkIdentity netIdentity = GetComponent<NetworkIdentity>();
            netIdentity.localPlayerAuthority = true;
            netIdentity.AssignClientAuthority(connectionToClient);
        }
        //NetworkServer.SpawnWithClientAuthority(hand.transform.gameObject, connectionToClient);
    }

    //[ClientRpc]
    public void Initialize(int role)
    {
        //active = false;

        gameManager = GameManager.instance;

        count = 0;
        actionsTaken = new int[1000][];
        actionsLeft = 4;

        hand = new GameObject("Hand").AddComponent<Hand>();

        hand.transform.parent = transform;

        hand.Initialize(this);

        cityID = 4;
        CurrentCity = GameManager.GetCityFromID(4);

        Debug.Log(role);
        Debug.Log(GameManager.roleCardStack.cards.Count);

        Card tempRole = GameManager.roleCardStack.cards[role];

        this.roleCard = tempRole as _roleCard;

        //this.roleCard = (_roleCard) GameManager.roleCardStack.cards[roleCard]; //GameManager.roleCardStack.roleCards.Contains(roleCard); //Error?
        MoveToCity(cityID);
        CurrentCity.UpdatePawns();
    }

    public void startTurn()
    {
        active = true;
        count = 0;
        actionsTaken = new int[1000][];
        actionsLeft = 4;

    }

    public void shareKnowledge(Player[] allPlayers, int playerNo, int cardIndex)
    {
        Card tmp = null;
        tmp = this.hand.cards[cardIndex];
        this.hand.cards[cardIndex] = allPlayers[playerNo].hand.cards[cardIndex];
        allPlayers[playerNo].hand.cards[cardIndex] = tmp;
    }

    //[ClientCallback]
    private void Update()
    {
        if (isLocalPlayer)
        {
            InputMoveToCity();
        }
        else if (CurrentCity != null && CurrentCity.cityId != cityID)
        {
            MoveToCity(cityID);
        }
    }

    [Command]
    public void Cmd_UpdateSyncListCards()
    {
        for (int i = 0; i < hand.cards.Length; i++)
        {
            for (int j = 0; j < GameManager.instance.SyncListPlayerCardSort.Count; j++)
            {
                //Debug.Log(GameManager.instance.SyncListPlayerCardSort[j]);
                if (hand.cards[i].Id == GameManager.instance.SyncListPlayerCardSort[j])
                {
                    GameManager.instance.Cmd_RemoveFromCityList(hand.cards[i].Id);
                }
            }

        }
    }

    public void EndTurn()
    {
        actionsLeft = 0;
        hand.drawPlayerCards();
        GameManager.instance.InfectCities();
        //active = false;
        GameManager.instance.CmdSwitchTurn();
    }

    private void InputMoveToCity()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100);
        if (Input.GetMouseButtonUp(0))
        {
            if (hit.collider != null)
            {
                if (hit.transform.tag == "City" && actionsLeft > 0 &&
                    CityIsConnected(hit.transform.GetComponent<City>().cityId))
                {
                    MoveToCity(hit.transform.GetComponent<City>().cityId);
                }
                if (hit.transform.tag == "DiseaseCube" && actionsLeft > 0)
                {
                    Cmd_RemoveDiseaseCubes(City.GetStringFromColor(hit.transform.GetComponent<SpriteRenderer>().color));
                }
            }
        }
    }


    public void deactivateCities()
    {
        for (int i = 0; i < GameManager.cities.GetLength(0); i++)
        {
            GameManager.GetCityFromID(i).active = false;
        }
    }

    public void activateCities()
    {
        if (cardEqualToCity())
        {
            for (int i = 0; i < GameManager.cities.GetLength(0); i++)
            {
                GameManager.GetCityFromID(i).active = true;
            }
        }
        else
        {
            int[] cityIDs = GameManager.GetCityFromID(cityID).connectedCityIDs;

            for (int i = 0; i < cityIDs.GetLength(0); i++)
            {
                GameManager.GetCityFromID(cityIDs[i]).active = true;
            }
            for (int i = 0; i < hand.cards.GetLength(0); i++)
            {
                if (hand.cards[i] is _cityCard)
                {
                    GameManager.GetCityFromID(hand.cards[i].Id).active = true;
                }
            }
            for (int i = 0; i < GameManager.researchCenterCities.GetLength(0); i++)
            {
                GameManager.researchCenterCities[i].active = true;
            }
        }
    }

    public bool cardEqualToCity()
    {
        for (int i = 0; i < hand.cards.GetLength(0); i++)
        {
            if (hand.cards[i] is _cityCard && hand.cards[i].Id == cityID)
            {
                return true;
            }
        }
        return false;
    }

    public void MoveToConnectedCity(int newID)
    {
        actionsTaken[count] = new int[] {0, cityID};
        count++;
        actionsLeft--;
        MoveToCity(gameManager.connectedCities[cityID][newID]);
    }

    public void MoveToResearchCity(int ID)
    {
        if (GameManager.GetCityFromID(cityID).researchCenter)
        {
            actionsTaken[count] = new int[] {1, cityID};
            count++;
            actionsLeft--;
            int choose = 0;
            MoveToCity(GameManager.researchCenterCities[choose].cityId);
        }
    }

    [ClientRpc]
    public void Rpc_MoveToCityCard(int cityCardID)
    {
        Debug.Log("move to city: " + cityCardID);
        // int ID = cityCard.Id;
        int ID = cityCardID;
        actionsTaken[count] = new int[] {2, cityID};
        count++;
        actionsLeft--;

        MoveToCity(ID);
        hand.discard(ID);
        GameManager.instance.Cmd_AddToCityDiscardList(ID);
    }

    [Command]
    public void Cmd_MoveToCityCard(int cityCardID)
    {
        Rpc_MoveToCityCard(cityCardID);
    }

    [Command]
    private void Cmd_ChangeCityID(int newID)
    {
        cityID = newID;
    }

    [Client]
    public void MoveToCity(int ID)
    {

        //If medic roleCard and cure, remove disease cubes
        if (roleCard.role == _roleCard.roleType.MEDIC && gameManager.blueCure)
        {
            CurrentCity.ReduceDiseaseSpread("Blue", roleCard);
        }
        if (roleCard.role == _roleCard.roleType.MEDIC && gameManager.yellowCure)
        {
            CurrentCity.ReduceDiseaseSpread("Yellow", roleCard);
        }
        if (roleCard.role == _roleCard.roleType.MEDIC && gameManager.blackCure)
        {
            CurrentCity.ReduceDiseaseSpread("Black", roleCard);
        }
        if (roleCard.role == _roleCard.roleType.MEDIC && gameManager.redCure)
        {
            CurrentCity.ReduceDiseaseSpread("Red", roleCard);
        }

        CurrentCity.removePlayer(this);
        CurrentCity.UpdatePawns();

        City newCity = GameManager.GetCityFromID(ID);
        newCity.addPlayer(this);
        newCity.UpdatePawns();
        CurrentCity = newCity;
        //Move
        cityID = ID;
        //Synchronize the cityID
        Cmd_ChangeCityID(cityID);
    }

    [Command]
    private void Cmd_RemoveDiseaseCubes(string colour)
    {
        Rpc_RemoveDiseaseCubes(colour);
    }

    [ClientRpc]
    private void Rpc_RemoveDiseaseCubes(string colour)
    {
        switch (colour)
        {
            case "Blue":
                actionsTaken[count] = new int[] {3, 1, cityID};
                count++;
                actionsLeft--;
                break;
            case "Yellow":
                actionsTaken[count] = new int[] {3, 2, cityID};
                count++;
                actionsLeft--;
                break;
            case "Black":
                actionsTaken[count] = new int[] {3, 3, cityID};
                count++;
                actionsLeft--;
                break;
            case "Red":
                actionsTaken[count] = new int[] {3, 4, cityID};
                count++;
                actionsLeft--;
                break;
        }
        GameManager.GetCityFromID(cityID).ReduceDiseaseSpread(colour, roleCard);
    }

    private void buildResearchCenter(int cityID, _cityCard city)
    {
        //gameManager.GetCityFromID(cityID).hasResearchCenter = true;
        if (cityID == city.Id)
        {
            GameManager.GetCityFromID(cityID).hasResearchCenter = true;
            hand.discard(city);
            actionsTaken[count] = new int[] {4, cityID};
            count++;
            actionsLeft--;
        }

    }

    public void cureDisease(int[] cardIDs)
    {
        int[] checker = checkForCure(5, cardIDs);

        if (GameManager.GetCityFromID(cityID).researchCenter && checker[0] == 1)
        {

            hand.discardArray(discardArray);

            switch (checker[1])
            {
                case 0:
                    GameManager.instance.blueCure = true;

                    break;
                case 1:
                    GameManager.instance.yellowCure = true;
                    break;
                case 2:
                    GameManager.instance.blackCure = true;
                    break;

                case 3:
                    GameManager.instance.redCure = true;
                    break;
            }
            count++;
            actionsLeft--;
        }

    }

    private int[] checkForCure(int counter, int[] hand)
    {

        int[] counters = new int[4];
        int[] discardBlue = new int[counter];
        int[] discardYellow = new int[counter];
        int[] discardBlack = new int[counter];
        int[] discardRed = new int[counter];
        for (int i = 0; i < hand.GetLength(0); i++)
        {
            if (hand[i] != null)
            {
                String colour = GameManager.GetCityFromID(hand[i]).color;
                switch (GameManager.GetCityFromID(hand[i]).color)
                {
                    case "Blue":
                        counters[0]++;
                        discardBlue[i] = hand[i];
                        break;
                    case "Yellow":
                        counters[1]++;
                        discardYellow[i] = hand[i];
                        break;
                    case "Black":
                        counters[2]++;
                        discardBlack[i] = hand[i];
                        break;
                    case "Red":
                        counters[3]++;
                        discardRed[i] = hand[i];
                        break;
                }
            }

        }
        for (int i = 0; i < counters.GetLength(0); i++)
        {
            Debug.Log(counters[i]);
            if (counters[i] >= counter)
            {
                switch (i)
                {
                    case 0:
                        discardArray = discardBlue;
                        break;
                    case 1:
                        discardArray = discardYellow;
                        break;
                    case 2:
                        discardArray = discardBlack;
                        break;
                    case 3:
                        discardArray = discardRed;
                        Debug.Log("discard array 3 " + discardArray[2]);
                        break;
                }
                return new int[] {1, i};
            }
        }
        return new int[] {0, 0};
    }

    private bool CityIsConnected(int ID)
    {

        for (int i = 0; i < CurrentCity.connectedCityIDs.Length; i++)
        {
            if (CurrentCity.connectedCityIDs[i] == ID)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// From this point, all the code is related to trading of cards
    /// </summary>

    GameObject playerSelection;
    GameObject otherPlayerArea;
    GameObject[] playerCardButtons = new GameObject[7];
    GameObject[] playerSelectionButtons = new GameObject[3];

    //When called, checks cityID with cards of the players, expand for further info.
    public void startTrade()
    {
        for (int i = 0; i < GameManager.players.Count; i++)
        {
            Debug.Log(GameManager.players.Count);
            if (GameManager.players[i] == isLocalPlayer)
            {
                for (int j = 0; j < GameManager.players[i].hand.cards.Length; j++)
                {
                    //print(GameManager.players[i].hand.cards[j].Id);
                    Debug.Log(GameManager.players[i].hand.cards.Length);
                }
            }
        }
        GameObject[] playersGameObjects = GameObject.FindGameObjectsWithTag("Player");
        List<Player> players = new List<Player>();

        foreach (var i in playersGameObjects)
        {
            players.Add(i.GetComponent<Player>());
        }

        playerSelection = GameObject.Find("PlayerSelection");
        //playerSelection.SetActive(true);
        bool trade = false;

        //Runs through all the players, inluding yourself. lel
        for (int i = 0; i < players.Count; i++)
        {
            for (int j = 0; j < players[i].hand.cards.Length; j++)
            {
                if (players[i].hand.cards[j] != null && players[i].cityID == players[i].hand.cards[j].Id)
                {
                    trade = true;
                    //If you dont have the card, display the one who has it, and if you click on them, you get that card
                    if (players[i] != isLocalPlayer)
                    {
                        playerSelectionButtons[i] = playerSelection.transform.GetChild(i).gameObject;
                        playerSelectionButtons[i].GetComponentInChildren<Text>().text = players[i].name;
                        var i1 = i;
                        playerSelectionButtons[i].GetComponent<Button>().onClick.AddListener(delegate { takeCard(players[i1].hand.cards[j].Id, players[i1]); });
                    }

                    //If you have the card, disply all the other players. If you click on one of em, they get the card
                    else {
                        for (int x = 0; x < players.Count; x++)
                        {
                            if (players[i] != isLocalPlayer)
                            {
                                playerSelectionButtons[i] = playerSelection.transform.GetChild(i).gameObject;
                                playerSelectionButtons[i].GetComponentInChildren<Text>().text = players[i].name;
                                var i1 = i;
                                playerSelectionButtons[i].GetComponent<Button>().onClick.AddListener(delegate { giveCard(players[i1].hand.cards[j].Id, players[i1]); });
                            }
                        }
                    }
                    trade = true;
                    break;
                }
            }
            if (trade == true) { break; }
        }
    }


    //Gives a card to the other player
    public void giveCard(int cardID, Player player)
    {
        for (int i = 0; i < player.hand.cards.Length; i++)
        {
            if (hand.cards[i] != null && hand.cards[i].Id == cardID)
            {
                hand.discard(i);
            }
        }
        for (int i = 0; i < this.hand.cards.Length; i++)
        {
            if (player.hand.cards[i] == null)
            {
                //player.hand.addToHand(GameManager.AllCardsStack.cards[cardID]);
                player.hand.cards[i] = GameManager.AllCardsStack.cards[cardID];
                exitTrade();
                break;
            }
        }

    }

    //Takes a card from the other player
    private void takeCard(int cardID, Player player)
    {

        for (int i = 0; i < player.hand.cards.Length; i++)
        {
            if (player.hand.cards[i] != null && player.hand.cards[i].Id == cardID)
            {
                player.hand.discard(i);
                //  player.hand.updateCards();
            }
        }

        for (int i = 0; i < this.hand.cards.Length; i++)
        {
            if (hand.cards[i] == null)
            {
                hand.cards[i] = GameManager.AllCardsStack.cards[cardID];
                //  this.hand.updateCards();
                exitTrade();
                break;
            }
        }
    }

    //Exits the entire trading debacle
    public void exitTrade()
    {
        playerSelection.SetActive(false);
    }




}




