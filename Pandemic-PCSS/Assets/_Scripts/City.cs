
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class City : NetworkBehaviour
{

    public Player[] players = new Player[4];
    public int cityId;
    public int[] connectedCityIDs;
    public string color;
    public Color cityColor;
    public string name;
    public bool hasResearchCenter = false;
    public bool active = false;
    public SpriteRenderer[] diseaseCubes;
    public SpriteRenderer[] pawnSpriteRenderers;
    public SpriteRenderer researchCenter;

    public bool locked;
    public bool update = false;

    private int diseaseSpread;

    public int DiseaseSpread
    {
        get { return diseaseSpread; }
        set { diseaseSpread = value; }
    }

    public void ResetValues()
    {
        if (DiseaseSpread > 3 && locked)
        {
            locked = false;
            DiseaseSpread = 3;
        }
    }

    //[ClientRpc]
    public void UpdatePawns()
    {
        for (int i = 0; i < pawnSpriteRenderers.Length; i++)
        {
            for (int j = 0; j < players.Length; j++)
            {
                if (players[j])
                {
                    if (players[j].roleCard.transform.name == pawnSpriteRenderers[i].transform.name)
                    {
                        pawnSpriteRenderers[i].enabled = true;
                        break;
                    }
                    else
                    {
                        pawnSpriteRenderers[i].enabled = false;
                    }
                }
                else
                {
                    pawnSpriteRenderers[i].enabled = false;

                }
            }
        }
    }


    public void IncrementDiseaseSpread(string color, int infectRate)
    {
        //check if medic prevents disease spread in his city
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                if (players[i].roleCard.role == _roleCard.roleType.MEDIC && GameManager.instance.blueCure)
                {
                    return;
                }
                if (players[i].roleCard.role == _roleCard.roleType.MEDIC && GameManager.instance.yellowCure)
                {
                    return;
                }
                if (players[i].roleCard.role == _roleCard.roleType.MEDIC && GameManager.instance.blackCure)
                {
                    return;
                }
                if (players[i].roleCard.role == _roleCard.roleType.MEDIC && GameManager.instance.redCure)
                {
                    return;
                }
            }
        }


        for (int i = 0; i < infectRate; i++)
        {
            foreach (SpriteRenderer t in diseaseCubes)
            {
                if (!t.enabled)
                {
                    t.enabled = true;
                    t.color = GetColorFromString(color);
                    break;
                }
            }
            diseaseSpread++;
        }
    }

    //[Client]
    public void ReduceDiseaseSpread(string color, _roleCard roleCard)
    {

        if (GameManager.instance.GetCureFromString(color) || roleCard.role == _roleCard.roleType.MEDIC)
        {
            foreach (SpriteRenderer t in diseaseCubes.Where(t => t.color == GetColorFromString(color)))
            {
                t.enabled = false;
                diseaseSpread--;
                GameManager.instance.SetDiseaseSpread(color, -1);
            }
        }
        else
        {
            foreach (SpriteRenderer t in diseaseCubes.Where(t => t.color == GetColorFromString(color) && t.enabled))
            {
                t.enabled = false;
                diseaseSpread--;
                GameManager.instance.SetDiseaseSpread(color, -1);
                return;
            }
        }
    }

    public void Initialize(int cityId, int[] connectedCityIDs, string color, string name)
    {
        this.cityId = cityId;
        this.connectedCityIDs = connectedCityIDs;
        this.color = color;
        this.name = name;
        transform.name = name;
        hasResearchCenter = name == "Atlanta";
        UpdatePawns();
        GetComponent<Renderer>().material.color = GetColorFromString(color);
    }

    public static Color GetColorFromString(string color)
    {
        switch (color)
        {
            case "Blue":
                return Color.blue;
            case "Yellow":
                return Color.yellow;
            case "Black":
                return Color.black;
            case "Red":
                return Color.red;
        }
        return Color.white;
    }
    public static string GetStringFromColor(Color color)
    {
        try
        {
            if (color == Color.blue)
            {
                return "Blue";
            }
            if (color == Color.red)
            {
                return "Red";
            }
            if (color == Color.black)
            {
                return "Black";
            }
            if (color == Color.yellow)
            {
                return "Yellow";
            }
        }
        catch (NullReferenceException ex)
        {
            Debug.Log("Colour does not exist returning white");
        }
        return "White";
    }


    private void Update()
    {
        researchCenter.enabled = hasResearchCenter;
        if (hasResearchCenter)
        {
            //GameManager.researchCenterCities[cityId] = this;
        }
        if (update)
        {
            UpdatePawns();
            update = false;
        }
    }


    internal void removePlayer(Player player)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == player)
            {
                players[i] = null;
                return;
            }
        }
    }

    internal void addPlayer(Player player)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
            {
                players[i] = player;
                return;
            }
        }
    }
}
