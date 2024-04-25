//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class ChessGameManager : MonoBehaviour
//{
//    private List<string> moves = new List<string>();
//    private Dictionary<string, string> gameInfo = new Dictionary<string, string>();

//    // Public variable to hold the generated PGN string
//    public string PGN { get; private set; }

//    private void Start()
//    {
//        // Initialize game info
//        gameInfo["Event"] = "Casual Game";
//        gameInfo["Site"] = "Unity Chess VisionOS";
//        gameInfo["Date"] = DateTime.Now.ToString("yyyy.MM.dd");
//        gameInfo["Round"] = "1";
//        gameInfo["White"] = "Player1";
//        gameInfo["Black"] = "Player2";
//        gameInfo["Result"] = "*"; // Result is initially unknown
//    }

//    // Method to record a move
//    public void RecordMove(string move)
//    {
//        moves.Add(move);
        
//        // Generate PGN after each move
//        PGN = GeneratePGN();
//        Debug.Log("PGN:\n" + PGN);
//    }

//    // Method to generate PGN from game info and moves
//    private string GeneratePGN()
//    {
//        // Create a PGN string builder
//        System.Text.StringBuilder pgnBuilder = new System.Text.StringBuilder();

//        // Append game info
//        foreach (var pair in gameInfo)
//        {
//            pgnBuilder.Append("[").Append(pair.Key).Append(" \"").Append(pair.Value).Append("\"]\n");
//        }

//        // Append moves
//        for (int i = 0; i < moves.Count; i++)
//        {
//            if (i % 2 == 0)
//            {
//                pgnBuilder.Append((i / 2) + 1).Append(". "); // Move number
//            }
//            string move = moves[i];
//            if (DisambiguationNeeded(move))
//            {
//                move = AddDisambiguation(move);
//            }
//            pgnBuilder.Append(move).Append(" ");
//        }

//        // Append game result if available
//        if (gameInfo.ContainsKey("Result"))
//        {
//            pgnBuilder.Append(gameInfo["Result"]);
//        }

//        return pgnBuilder.ToString();
//    }

//    // Method to end the game and set result
//    public void EndGame(string result)
//    {
//        gameInfo["Result"] = result;
//    }

//    // Method to check if disambiguation is needed for a move
//    private bool DisambiguationNeeded(string move)
//    {
//        // Extract destination square from the move
//        string destination = move.Substring(move.Length - 2);

//        // Check if the move contains a piece type that requires disambiguation
//        char pieceType = move[0];

//        // Initialize the count of pieces that can reach the destination square
//        int pieceCount = 0;

//        // Check for each piece type
//        switch (pieceType)
//        {
//            case 'R': // Rook
//                pieceCount = CountPiecesCanReachDestination<Rook>(destination);
//                break;
//            case 'B': // Bishop
//                pieceCount = CountPiecesCanReachDestination<Bishop>(destination);
//                break;
//            case 'N': // Knight
//                pieceCount = CountPiecesCanReachDestination<Knight>(destination);
//                break;
//            case 'P': // Pawn
//                pieceCount = CountPiecesCanReachDestination<Pawn>(destination);
//                break;
//            default:
//                // No disambiguation needed for other piece types
//                return false;
//        }

//        // Disambiguation is needed if there's more than one piece capable of reaching the destination square
//        return pieceCount > 1;
//    }

//    // Method to count the number of pieces of a specific type that can reach a specific destination square
//    private int CountPiecesCanReachDestination<T>(string destination) where T : ChessPiece
//    {
//        int count = 0;

//        // Iterate through the board to find pieces of the specified type
//        for (int x = 0; x < tileCountX; x++)
//        {
//            for (int y = 0; y < tileCountY; y++)
//            {
//                ChessPiece piece = board[x, y];
//                if (piece != null && piece.GetType() == typeof(T)) // Check if the piece is of the specified type
//                {
//                    // Get possible moves for the piece
//                    List<Vector2Int> validMoves = piece.GetValidMoves(board, new Vector2Int(x, y), tileCountX, tileCountY);

//                    // Check if any of the valid moves of the piece reach the destination square
//                    foreach (Vector2Int move in validMoves)
//                    {
//                        if (move.ToChessCoordinates() == destination)
//                        {
//                            count++;
//                            break; // No need to check other moves for this piece
//                        }
//                    }
//                }
//            }
//        }

//        return count;
//    }

//    // Method to add disambiguation information to a move
//    private string AddDisambiguation(string move)
//    {
//        // Extract destination square from the move
//        string destination = move.Substring(move.Length - 2);

//        // Extract piece type from the move
//        char pieceType = move[0];

//        // Initialize disambiguated move
//        string disambiguatedMove = "";

//        // Implement disambiguation logic based on the piece type
//        switch (pieceType)
//        {
//            case 'K': // King
//            case 'Q': // Queen
//            case 'R': // Rook
//            case 'B': // Bishop
//            case 'N': // Knight
//                string originRank, originFile;
//                FindOriginSquare(pieceType, destination, out originRank, out originFile);
//                disambiguatedMove = originFile + destination;
//                break;
//            case 'P': // Pawn
//                string originRankPawn, originFilePawn;
//                FindOriginSquare(pieceType, destination, out originRankPawn, out originFilePawn);
//                disambiguatedMove = originFilePawn + destination;
//                break;
//            default:
//                // If the piece type doesn't require disambiguation, return the original move
//                return move;
//        }

//        return disambiguatedMove;
//    }

//    // Method to find the origin square to disambiguate the move
//    private void FindOriginSquare(char pieceType, string destination, out string rank, out string file)
//    {
//        // Iterate through the board to find pieces of the specified type
//        for (int x = 0; x < tileCountX; x++)
//        {
//            for (int y = 0; y < tileCountY; y++)
//            {
//                ChessPiece piece = board[x, y];
//                if (piece != null && GetPieceChar(piece.GetType()) == pieceType) // Check if the piece is of the specified type
//                {
//                    // Get possible moves for the piece
//                    List<Vector2Int> validMoves = piece.GetValidMoves(board, new Vector2Int(x, y), tileCountX, tileCountY);

//                    // Check if any of the valid moves of the piece reach the destination square
//                    foreach (Vector2Int move in validMoves)
//                    {
//                        if (move.ToChessCoordinates() == destination)
//                        {
//                            // Return the rank and file of the origin square
//                            rank = GetRankFromIndex(y);
//                            file = GetFileFromIndex(x);
//                            return;
//                        }
//                    }
//                }
//            }
//        }

//        // If no piece found that can reach the destination square, return empty strings
//        rank = "";
//        file = "";
//    }

//    // Method to convert an index to rank notation (e.g., 0 -> "8", 1 -> "7", ..., 7 -> "1")
//    private string GetRankFromIndex(int index)
//    {
//        return (8 - index).ToString();
//    }

//    // Method to convert an index to file notation (e.g., 0 -> "a", 1 -> "b", ..., 7 -> "h")
//    private string GetFileFromIndex(int index)
//    {
//        return ((char)('a' + index)).ToString();
//    }
//}

////call RecordMove() for pgn to be generated
//// Assuming you have an instance of the ChessGameManager class called chessGameManager
//// eg: chessGameManager.RecordMove("e4");
