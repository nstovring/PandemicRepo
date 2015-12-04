using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SaveCityPositions : MonoBehaviour
{
    public bool updateGizmos;

    public Vector2[] positions = new Vector2[48];

    void Update()
    {
        if (updateGizmos)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = transform.GetChild(i).transform.position;
                transform.GetChild(i).transform.name = GameManager.cityNames[i];
            }
            updateGizmos = false;
        }
    }
}