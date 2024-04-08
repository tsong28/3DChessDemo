using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2> GetPossibleMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2> possibleMoves = new List<Vector2>();

        //move diagonal up-right
        int i = 0;
        while (currentX + 1 + i < tileCountX && currentY + 1 + i < tileCountY)
        {
            if (board[currentX + 1 + i, currentY + 1 + i] == null)
            {
                possibleMoves.Add(new Vector2(currentX + 1 + i, currentY + 1 + i));
            }
            else
            {
                if (board[currentX + 1 + i, currentY + 1 + i].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2(currentX + 1 + i, currentY + 1 + i));
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
                possibleMoves.Add(new Vector2(currentX - 1 - i, currentY + 1 + i));
            }
            else
            {
                if (board[currentX - 1 - i, currentY + 1 + i].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2(currentX - 1 - i, currentY + 1 + i));
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
                possibleMoves.Add(new Vector2(currentX + 1 + i, currentY - 1 - i));
            }
            else
            {
                if (board[currentX + 1 + i, currentY - 1 - i].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2(currentX + 1 + i, currentY - 1 - i));
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
                possibleMoves.Add(new Vector2(currentX - 1 - i, currentY - 1 - i));
            }
            else
            {
                if (board[currentX - 1 - i, currentY - 1 - i].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2(currentX - 1 - i, currentY - 1 - i));
                break;
            }
            i++;

        }

        //right
        for (int j = currentX + 1; j < tileCountX; j++)
        {
            if (board[j, currentY] == null)
            {
                possibleMoves.Add(new Vector2(j, currentY));
            }
            else
            {
                if (board[j, currentY].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2(j, currentY));
                break;
            }
        }

        //left
        for (int j = currentX - 1; j >= 0; j--)
        {
            if (board[j, currentY] == null)
            {
                possibleMoves.Add(new Vector2(j, currentY));
            }
            else
            {
                if (board[j, currentY].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2(j, currentY));
                break;
            }
        }

        //up
        for (int j = currentY + 1; j < tileCountY; j++)
        {
            if (board[currentX, j] == null)
            {
                possibleMoves.Add(new Vector2(currentX, j));
            }
            else
            {
                if (board[currentX, j].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2(currentX, j));
                break;
            }
        }

        //down
        for (int j = currentY - 1; j >= 0; j--)
        {
            if (board[currentX, j] == null)
            {
                possibleMoves.Add(new Vector2(currentX, j));
            }
            else
            {
                if (board[currentX, j].team == team)
                {
                    break;
                }
                possibleMoves.Add(new Vector2(currentX, j));
                break;
            }
        }


        return possibleMoves;
    }
}
