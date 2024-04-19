using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        //up
        if (currentY != 7)
        {
            if (board[currentX, currentY + 1] == null || board[currentX, currentY + 1].team != team)
            {
                possibleMoves.Add(new Vector2Int(currentX, currentY + 1));
            }
        }

        //down
        if (currentY != 0)
        {
            if (board[currentX, currentY - 1] == null || board[currentX, currentY - 1].team != team)
            {
                possibleMoves.Add(new Vector2Int(currentX, currentY - 1));
            }
        }
        //left
        if (currentX != 0)
        {
            if (board[currentX - 1, currentY] == null || board[currentX - 1, currentY].team != team)
            {
                possibleMoves.Add(new Vector2Int(currentX-1, currentY));
            }
        }

        //right
        if (currentX != 7)
        {
            if (board[currentX + 1, currentY] == null || board[currentX + 1, currentY].team != team)
            {
                possibleMoves.Add(new Vector2Int(currentX + 1, currentY));
            }
        }

        //diagonal upright
        if (currentY != 7&&currentX !=7)
        {
            if (board[currentX+1, currentY + 1] == null || board[currentX + 1, currentY + 1].team != team)
            {
                possibleMoves.Add(new Vector2Int(currentX+1, currentY + 1));
            }
        }

        //diagonal upleft
        if (currentY != 7 && currentX != 0)
        {
            if (board[currentX - 1, currentY + 1] == null || board[currentX - 1, currentY + 1].team != team)
            {
                possibleMoves.Add(new Vector2Int(currentX - 1, currentY + 1));
            }
        }

        //diagonal downleft
        if (currentY != 0 && currentX != 0)
        {
            if (board[currentX - 1, currentY - 1] == null || board[currentX - 1, currentY - 1].team != team)
            {
                possibleMoves.Add(new Vector2Int(currentX - 1, currentY - 1));
            }
        }

        //diagonal downright
        if (currentY != 0 && currentX != 7)
        {
            if (board[currentX + 1, currentY - 1] == null || board[currentX + 1, currentY - 1].team != team)
            {
                
                possibleMoves.Add(new Vector2Int(currentX + 1, currentY - 1));
            }
        }
        return possibleMoves;

    }
}
