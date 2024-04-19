using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessGameManager : MonoBehaviour
{
    private List<string> moves = new List<string>();
    private Dictionary<string, string> gameInfo = new Dictionary<string, string>();

    // Public variable to hold the generated PGN string
    public string PGN { get; private set; }

    private void Start()
    {
        // Initialize game info
        gameInfo["Event"] = "Casual Game";
        gameInfo["Site"] = "Unity Chess VisionOS";
        gameInfo["Date"] = DateTime.Now.ToString("yyyy.MM.dd");
        gameInfo["Round"] = "1";
        gameInfo["White"] = "Player1";
        gameInfo["Black"] = "Player2";
        gameInfo["Result"] = "*"; // Result is initially unknown
    }

    // Method to record a move
    public void RecordMove(string move)
    {
        moves.Add(move);
        
        // Generate PGN after each move
        PGN = GeneratePGN();
        Debug.Log("PGN:\n" + PGN);
    }

    //public string MovetoString(Vector2Int moveVec, Vector2Int prevPosition, ChessPiece piece, bool takes, bool checks, int castle = 0)
    //{
    //    string file = ((char)(97+moveVec.x)).ToString();
    //    string rank = ((char)(moveVec.y + 1)).ToString();
    //    string move = file + rank;
    //    string x = "";
    //    string check = "";
    //    if(takes)
    //    {
    //        x = "x";
    //    }
    //    if (checks)
    //    {
    //        check = "+";
    //    }

    //    switch (piece.type)
    //    {
    //        case ChessPieceType.Pawn:
    //            return x+ move + check;
                
    //    }
    //}

    // Method to generate PGN from game info and moves
    private string GeneratePGN()
    {
        // Create a PGN string builder
        System.Text.StringBuilder pgnBuilder = new System.Text.StringBuilder();

        // Append game info
        foreach (var pair in gameInfo)
        {
            pgnBuilder.Append("[").Append(pair.Key).Append(" \"").Append(pair.Value).Append("\"]\n");
        }

        // Append moves
        for (int i = 0; i < moves.Count; i++)
        {
            if (i % 2 == 0)
            {
                pgnBuilder.Append((i / 2) + 1).Append(". "); // Move number
            }
            pgnBuilder.Append(moves[i]).Append(" ");
        }

        // Append game result if available
        if (gameInfo.ContainsKey("Result"))
        {
            pgnBuilder.Append(gameInfo["Result"]);
        }

        return pgnBuilder.ToString();
    }

    // Method to end the game and set result
    public void EndGame(string result)
    {
        gameInfo["Result"] = result;
    }
}
//call record move for pgn to be generated
// Assuming you have an instance of the ChessGameManager class called chessGameManager
// eg: chessGameManager.RecordMove("e4");

