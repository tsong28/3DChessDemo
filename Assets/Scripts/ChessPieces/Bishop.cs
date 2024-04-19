using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    

    public override List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        //move diagonal up-right
        int i = 0;
        while (currentX + 1 + i < tileCountX && currentY + 1 + i < tileCountY)
        {
            if (board[currentX + 1 + i, currentY + 1 + i] == null)
            {
                possibleMoves.Add(new Vector2Int(currentX + 1 + i, currentY + 1 + i));
            }
            else
            {
                if (board[currentX + 1 + i, currentY + 1 + i].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2Int(currentX + 1 + i, currentY + 1 + i));
                break;
            }
            i++;

        }

        //move diagonal up-left
        i = 0;
        while (currentX - 1 - i >= 0 && currentY + 1 + i < tileCountY)
        {
            if (board[currentX - 1 - i, currentY + 1 + i] == null)
            {
                possibleMoves.Add(new Vector2Int(currentX - 1 - i, currentY + 1 + i));
            }
            else
            {
                if (board[currentX - 1 - i, currentY + 1 + i].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2Int(currentX - 1 - i, currentY + 1 + i));
                break;
            }
            i++;

        }

        //move diagonal down-right
        i = 0;
        while (currentX + 1 + i < tileCountX && currentY - 1 - i >= 0)
        {
            if (board[currentX + 1 + i, currentY - 1 - i] == null)
            {
                possibleMoves.Add(new Vector2Int(currentX + 1 + i, currentY - 1 - i));
            }
            else
            {
                if (board[currentX + 1 + i, currentY - 1 - i].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2Int(currentX + 1 + i, currentY - 1 - i));
                break;
            }
            i++;

        }

        //move diagonal down-left
        i = 0;
        while (currentX - 1 - i >= 0 && currentY - 1 - i >= 0)
        {
            if (board[currentX - 1 - i, currentY - 1 - i] == null)
            {
                possibleMoves.Add(new Vector2Int(currentX - 1 - i, currentY - 1 - i));
            }
            else
            {
                if (board[currentX - 1 - i, currentY - 1 - i].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2Int(currentX - 1 - i, currentY - 1 - i));
                break;
            }
            i++;

        }




        return possibleMoves;
    }
    
}
