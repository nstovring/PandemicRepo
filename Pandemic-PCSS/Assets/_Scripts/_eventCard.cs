using UnityEngine;
using System.Collections;



public class _eventCard : Card
{
    private int ID;
    Player player;




    public void Airlift(int city_id, int player_id)
    {
        //MoveToCity is private has to be changed to public
        //Also the Discard in hand is also private has to be changed to public

        //GameManager.players[player_id].MoveToCity(city_id);
        //player.hand.discard(this);

    }

    public void OneQuietNight()
    {



    }

    public void Forecast()
    {
        Card[] cards = new Card[6];
        int a = 0;

        for (int i = GameManager.infectCardStack.cards.Count - 1; i > 0; i--)
        {

            if (GameManager.infectCardStack.cards[i] != null)
            {
                if (a < 6)
                {
                    cards[a] = GameManager.infectCardStack.cards[i];
                    a++;
                    GameManager.infectCardStack.cards[i] = null;
                }
            }

        }
    }

    public Card[] swap(Card[] cards, int index1, int index2)
    {
        Card temp = cards[index1];
        cards[index1] = cards[index2];
        cards[index2] = temp;
        return cards;


    }



    public void GovermentGrant(int CityId)
    {
        GameManager.GetCityFromID(CityId).hasResearchCenter = true;

    }

    public void ResilientPopulation(int index)
    {

        GameManager.infectDiscardStack.cards[index] = null;


    }
}
