using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class Stack : MonoBehaviour
{


    public enum cardType
    {
        PLAYER_STACK,
        INFECTION,
        ROLE,
        DISCARD
    };

    public List<Card> cards;

    //On creation, create stack of type...
    public void Initialize(cardType type)
    {
        Sprite sprite = Instantiate(Resources.Load("Citycard_blue", typeof(Sprite))) as Sprite;
        produceCards(type);
    }

    public void produceCards(cardType type)
    {
        switch (type)
        {
            case cardType.PLAYER_STACK:
                createPlayerStack();
                break;
            case cardType.INFECTION:
                createInfectionCards();
                break;
            case cardType.ROLE:
                createRoleCards();
                break;
            case cardType.DISCARD:
                createDiscardStack();
                break;
            default:
                break;
        }
    }

    //Functions you can call
    public void EmptyCards()
    {
       cards = new List<Card>();
    }

    public void shuffleStack()
    {
        // Knuth shuffle algorithm
        for (int i = 0; i < cards.Count; i++)
        {
            Card tmp = cards[i];
            int r = UnityEngine.Random.Range(i, cards.Count);
            cards[i] = cards[r];
            cards[r] = tmp;
        }
    }

    public void addEpidemicCards()
    {
        _epidemicCard[] epidemicCards = new _epidemicCard[5];

        for (int i = 0; i < epidemicCards.Length; i++)
        {
            epidemicCards[i] = new GameObject().AddComponent<_epidemicCard>();
            epidemicCards[i].transform.parent = transform;
        }
        for (int i = 0; i < epidemicCards.Length; i++)
        {
            epidemicCards[i].name = "Epidemic";
            epidemicCards[i].Id = 54 + i;
        }

        for (int i = 0; i < epidemicCards.Length; i++)
        {
            cards.Add(epidemicCards[i]);
        }

    }

    public static Stack combineStacks(Stack one, Stack two)
    {

        Stack newStack = new Stack();
        for (int i = 0; i < one.cards.Count; i++)
        {
            newStack.cards.Add(one.cards[i]);
        }
        for (int i = 0; i < one.cards.Count; i++)
        {
            newStack.cards.Add(two.cards[i]);
        }
        return newStack;
    }

    //Creates cards, remember to call addEpidemicCards function later on the playerStack.
    public void createDiscardStack()
    {
        cards = new List<Card>();
    }

    public void createInfectionCards()
    {

        cards = new List<Card>();
        _infectionCard[] infectionCards = new _infectionCard[48];

        //Nikolaj - another unity change
        for (int i = 0; i < infectionCards.Length; i++)
        {
            infectionCards[i] = new GameObject().AddComponent<_infectionCard>();
            infectionCards[i].transform.parent = transform;
        }
        for (int i = 0; i < infectionCards.Length; i++)
        {
            infectionCards[i].name = GameManager.cityNames[i];
            infectionCards[i].Id = i + 1;
        }


        for (int i = 0; i < infectionCards.Length; i++)
        {
            cards.Add(infectionCards[i]);

        }
    }

    public void createRoleCards()
    {
        _roleCard[] roleCards = new _roleCard[7];
        cards = new List<Card>();

        for (int i = 0; i < roleCards.Length; i++)
        {
            roleCards[i] = new GameObject().AddComponent<_roleCard>();
            roleCards[i].transform.parent = transform;

        }

        Debug.Log("Creating Role Cards");
        roleCards[0].name = ("MEDIC");
        roleCards[1].name = ("DISPATCHER");
        roleCards[2].name = ("QURANTINE SPECIALIST");
        roleCards[3].name = ("CONTINGENCY");
        roleCards[4].name = ("RESEARCHER");
        roleCards[5].name = "SCIENTIST";
        roleCards[6].name = "OPERATIONS EXPERT";

        roleCards[0].role = _roleCard.roleType.MEDIC;
        roleCards[1].role = _roleCard.roleType.DISPATCHER;
        roleCards[2].role = _roleCard.roleType.QURANTINE_SPECIALIST;
        roleCards[3].role = _roleCard.roleType.CONTINGENCY;
        roleCards[4].role = _roleCard.roleType.RESEARCHER;
        roleCards[5].role = _roleCard.roleType.SCIENTIST;
        roleCards[6].role = _roleCard.roleType.OPERATIONS_EXPERT;


        for (int i = 0; i < roleCards.Length; i++)
        {
            cards.Add(roleCards[i]);
        }
    }

    public void createPlayerStack()
    {

        _cityCard[] cityCards = new _cityCard[48];
        _eventCard[] eventCards = new _eventCard[5];
        cards = new List<Card>();

        //Create the city cards
        for (int i = 0; i < cityCards.Length; i++)
        {
            cityCards[i] = new GameObject().AddComponent<_cityCard>();
            //Debug.Log(Resources.Load("Citycard_blue"));

            if (i < 12)
            {
                cityCards[i].image = Resources.Load<Sprite>("Citycard_blue");
                //Debug.Log("does it run?");
            }
            else if (i >= 12 && i < 24)
            {
                cityCards[i].image = Resources.Load<Sprite>("Citycard_yellow");
            }
            else if (i >= 24 && i < 36)
            {
                cityCards[i].image = Resources.Load<Sprite>("Citycard_black");
            }
            else if (i >= 36 && i < 48)
            {
                cityCards[i].image = Resources.Load<Sprite>("Citycard_red");
            }
            cityCards[i].transform.parent = transform;
        }


        for (int i = 0; i < cityCards.Length; i++)
        {
            cityCards[i].name = GameManager.cityNames[i];
            cityCards[i].Id = i + 1;
        }
        for (int i = 0; i < cityCards.Length; i++)
        {
            //cards[i] = cityCards[i];
            cards.Add(cityCards[i]);

        }

        //Create the event cards
        for (int i = 0; i < eventCards.Length; i++)
        {
            eventCards[i] = new GameObject().AddComponent<_eventCard>();
            eventCards[i].transform.parent = transform;
        }
        eventCards[0].name = "RESILIENT POPULATION";
        eventCards[1].name = "ONE QUIET NIGHT";
        eventCards[2].name = "FORECAST";
        eventCards[3].name = "AIRLIFT";
        eventCards[4].name = "GOVERNMENT GRANT";

        eventCards[0].Id = 49;
        eventCards[1].Id = 50;
        eventCards[2].Id = 51;
        eventCards[3].Id = 52;
        eventCards[4].Id = 53;

        for (int i = 0; i < eventCards.Length; i++)
        {
            cards.Add(eventCards[i]);
        }
    }

    public void SortCardsToList(SyncListInt sortListInt)
    {
        List<Card> tempCityList = new List<Card>();
        for (int i = 0; i < sortListInt.Count; i++)
        {
            tempCityList.Add(GameManager.AllCardsStack.cards[sortListInt[(i)]-1]);
        }
        cards = tempCityList;
    }


}

