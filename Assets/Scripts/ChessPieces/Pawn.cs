using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetPossibleMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {


        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        //move up if white, move down if blck
        int direction = (team == 0) ? 1 : -1;

        //edge of board has no moves (would promote anyways)
        if ((team == 0 && currentY != 7) || (team == 1 && currentY != 0))
        {
            //Space in front is clear
            if (board[currentX, currentY + direction] == null)
            {
                possibleMoves.Add(new Vector2Int(currentX, currentY + direction));
            }

            //Can move two if first move
            if ((team == 0 && currentY == 1) || (team == 1 && currentY == 6))
            {
                if (board[currentX, currentY + (direction * 2)] == null && board[currentX, currentY + (direction)] == null)
                {
                    possibleMoves.Add(new Vector2Int(currentX, currentY + (direction * 2)));
                }
                
            }
            //Diagonal take

            //right take
            if (currentX != tileCountX - 1)
            {
                if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
                {
                    possibleMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                }
            }

            //left take
            if (currentX != 0)
            {
                if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
                {
                    possibleMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                }
            }

            
        }
        return possibleMoves;
    }
    
}
