using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2> GetPossibleMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2> possibleMoves = new List<Vector2>();
        int x;
        int y;

        //Top right
        x = currentX + 1;
        y = currentY + 2;
        if(x < tileCountX && y<tileCountY)
        {
            if(board[x,y] == null || board[x,y].team!=team)
            {
                possibleMoves.Add(new Vector2Int(x, y));
            }
        }

        x = currentX + 2;
        y = currentY + 1;
        if (x < tileCountX && y < tileCountY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                possibleMoves.Add(new Vector2Int(x, y));
            }
        }

        //Top Left
        x = currentX - 1;
        y = currentY + 2;
        if (x >= 0 && y < tileCountY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                possibleMoves.Add(new Vector2Int(x, y));
            }
        }

        x = currentX - 2;
        y = currentY + 1;
        if (x >= 0 && y < tileCountY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                possibleMoves.Add(new Vector2Int(x, y));
            }
        }

        //Bottom Left
        x = currentX - 1;
        y = currentY - 2;
        if (x >= 0 && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                possibleMoves.Add(new Vector2Int(x, y));
            }
        }

        x = currentX - 2;
        y = currentY - 1;
        if (x >= 0 && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                possibleMoves.Add(new Vector2Int(x, y));
            }
        }

        //Bottom Right
        x = currentX + 1;
        y = currentY - 2;
        if (x < tileCountX && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                possibleMoves.Add(new Vector2Int(x, y));
            }
        }

        x = currentX + 2;
        y = currentY - 1;
        if (x < tileCountX && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                possibleMoves.Add(new Vector2Int(x, y));
            }
        }


        return possibleMoves;
    }
}