using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6

}

public class ChessPiece : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;

    public ChessPieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    //important for castling
    public bool hasMoved = false;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 20);
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if(force)
        {
            transform.position = desiredPosition;
        }
    }

    public virtual void SetScale(Vector3 scale, bool force = false) 
    {
        desiredScale = scale;
        if(force)
        {
            transform.localScale = scale;
        }
    }

    public virtual List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] board, int tileCountX = 8, int tileCountY = 8)
    {
        return new List<Vector2Int>();
    }
}
