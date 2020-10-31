using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casilla : MonoBehaviour
{
    public byte ID;
    [SerializeField] TresEnRaya game; // Esto lo podemos hacer también con un delegado

    private void OnMouseDown()
    {
        game.SeleccionarCasilla(ID, this.gameObject);
    }
}
