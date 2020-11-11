using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZobristKey31Bits : MonoBehaviour
{
    protected int[,] keys;
    protected int boardPositions, numberOfPieces;
    public ZobristKey31Bits(int _boardPositions, int _numberOfPieces)
    {
        System.Random rnd = new System.Random();
        boardPositions = _boardPositions;
        numberOfPieces = _numberOfPieces;
        keys = new int[boardPositions, numberOfPieces];
        for (int i = 0; i < boardPositions; i++)
        {
            for (int j = 0; j < numberOfPieces; j++)
            {
                keys[i, j] = rnd.Next(int.MaxValue);
            }
        }
    }

    public int Get(int position, int piece)
    {

        return keys[position, piece];

    }

    public void Print()
    {
        int i, j;
        string output = "";
        output += "Claves Zobrist:\n";
        for (i = 0; i < boardPositions; i++)
        {

            for (j = 0; j < numberOfPieces; j++)
            {
               // output += "Posición " + ToString(i).PadLeft(2, '0').ToString + ", Pieza " + j + ": ";
               // output += ToString(keys[i, j], 2).PadLeft(32, '0');
               // output += "\n";
            }

        }
        Debug.Log(output);
    }
}
