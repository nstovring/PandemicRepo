using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Hand : NetworkBehaviour
{
    //public Card[] cards = new Card[3];
    public Card[] cards = new Card[5];
    public GameObject[] CardButtons = new GameObject[7];
    public GameObject cardPrefab;
    GameManager gm;
    GameObject CardsOnHand; //moved it here.

    public int currentCardValue = -1;

    public Player player;

    public SyncListInt SyncListHandSort = new SyncListInt();

    void Awake()
    {
        if (isServer)
        {
            NetworkIdentity netIdentity = GetComponent<NetworkIdentity>();
            netIdentity.localPlayerAuthority = true;
            netIdentity.AssignClientAuthority(connectionToClient);
        }
    }

    public void Initialize(Player owner)
    {
        gm = GameManager.instance;

        GameObject actionButtons = GameObject.Find("ActionButtons");
        GameObject[] actionButtonChildren = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            actionButtonChildren[i] = actionButtons.transform.GetChild(i).gameObject;
        }
        //move button
        actionButtonChildren[0].GetComponent<Button>().onClick.AddListener(delegate { DelegateMove(currentCardValue); });
        //cure button
        actionButtonChildren[2].GetComponent<Button>().onClick.AddListener(delegateCure);

        //trade button
        actionButtonChildren[1].GetComponent<Button>().onClick.AddListener(delegate { owner.startTrade(); });
        actionButtonChildren[3].GetComponent<Button>().onClick.AddListener(delegateEndTurn);

        CardsOnHand = GameObject.Find("HandArea"); //moved the actual declartion up to the... beginning part of code?

        player = owner;

        if (player.isLocalPlayer)
        {
            for (int i = 0; i < CardButtons.Length; i++)
            {
                CardButtons[i] = CardsOnHand.transform.GetChild(i).gameObject;
                CardButtons[i].SetActive(false);
            }
            drawPlayerCards();
        }

        else
        {
            drawPlayerCards();
        }
    }
    private void delegateEndTurn()
    {
        if (player.isLocalPlayer && player.active)
        {
            player.EndTurn();
        }

    }
    private void delegateCure()
    {
        if (player.isLocalPlayer && player.active)
        {
            int[] currentCardValues = new int[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                currentCardValues[i] = cards[i].Id;
            }
            player.cureDisease(currentCardValues);
            Debug.Log("calls delegate method");
        }

    }

    void DelegateMove(int inputCard)
    {
        if (player.isLocalPlayer && player.active)
        {
            if (player.isServer)
            {
                player.Rpc_MoveToCityCard(currentCardValue);
            }
            else
            {
                player.Cmd_MoveToCityCard(currentCardValue);
            }

        }
    }

    void ChooseCard(int inputCard)
    {
        if (player.isLocalPlayer && player.active) currentCardValue = cards[inputCard].Id;
    }
    public void drawPlayerCards()
    {
        int num = GameManager.instance.SyncListPlayerCardSort.Count - 1;
        List<Card> tempAllCards = GameManager.playerCardStack.cards;

        addToHand(tempAllCards[num]);
        GameManager.instance.Cmd_RemoveFromCityList(tempAllCards[num].Id);

        addToHand(tempAllCards[num - 1]);
        GameManager.instance.Cmd_RemoveFromCityList(tempAllCards[num - 1].Id);
        GameManager.instance.Cmd_TryUpdateStacks();
    }
    public void addToHand(Card inputCard)
    {

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null)
            {
                if (inputCard is _epidemicCard)
                {
                    GameManager.instance.Cmd_Epidemic();
                    GameManager.instance.Cmd_AddToCityDiscardList(inputCard.Id);
                    cards[i] = null;
                    return;
                }
                cards[i] = GameManager.AllCardsStack.cards[inputCard.Id - 1];
                Cmd_IncrementSyncList(cards[i].Id);
                if (CardButtons[i] != null)
                {
                    CardButtons[i].SetActive(true);
                    CardButtons[i].GetComponentInChildren<Text>().text = inputCard.name;
                    CardButtons[i].GetComponentInChildren<Image>().sprite = inputCard.image;
                    if (this.cards[i].GetType() == typeof(_cityCard))
                    {
                        int i1 = i;
                        CardButtons[i].GetComponent<Button>().onClick.AddListener(delegate { ChooseCard(i1); });
                    }
                }
                break;
            }
        }
    }

    [Command]
    void Cmd_IncrementSyncList(int card)
    {
        SyncListHandSort.Add(card);

    }
    [Command]
    void Cmd_ReduceSyncList(int card)
    {
        SyncListHandSort.Add(card);

    }
    //overloaded method for actionButtons
    //[ClientRpc]
    //[Command]
    public void discard(int cardID)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] is Card)
            {
                if (cardID == cards[i].Id)
                {
                    if (CardButtons[i] != null)
                    {
                        CardButtons[i].SetActive(false);
                    }
                    Cmd_ReduceSyncList(cardID);
                    cards[i] = null;
                    break;
                }
            }
        }
    }

    public void discardArray(int[] discards)
    {
        //Debug.Log("start discard");
        for (int i = 0; i < discards.Length; i++)
        {
            for (int j = 0; j < cards.Length; j++)
            {
                if (cards[j] is Card)
                {
                    if (cards[j].Id == discards[i])
                    {
                        CardButtons[i].SetActive(false);
                        cards[j] = null;
                    }
                }
            }
        }
    }

    public void discard(Card card)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (card == cards[i])
            {
                GameManager.instance.Cmd_AddToCityDiscardList(card.Id);
                GameManager.infectDiscardStack.SortCardsToList(GameManager.instance.SyncListinfectionDiscardSort);
                CardButtons[i].SetActive(false);
                cards[i] = null;
                break;
            }
        }
    }

    public void updateCards()
    {
        if (player.isLocalPlayer)
        {
            //  this.cards = cards;
            for (int i = 0; i < CardButtons.Length; i++)
            {
                CardButtons[i] = CardsOnHand.transform.GetChild(i).gameObject;


                if (i < cards.Length)
                {
                    print(CardButtons[i].GetComponentInChildren<Text>().text = cards[i].name);

                    CardButtons[i].GetComponentInChildren<Text>().text = cards[i].name;
                    CardButtons[i].GetComponent<Image>().sprite = cards[i].image;

                    addToHand(cards[i]);
                    if (cards[i].GetType() == typeof(_cityCard))
                    {
                        int i1 = i;
                        CardButtons[i].GetComponent<Button>().onClick.AddListener(delegate { ChooseCard(i1); });


                    }
                }
                else
                {
                    CardButtons[i].SetActive(false);
                }
            }
        }


    }

}



