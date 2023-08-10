using ChessChallenge.API;
using Raylib_cs;
using System;
using System.Text.RegularExpressions;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();

        int highestScore = int.MinValue;
        int currentScore = 0;
        // Pick a random move to play if nothing better is found
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        foreach (Move move in allMoves)
        {
            // Console.Write($"{move}\t");
            currentScore = 0;
            // Always play checkmate in one
            if (MoveIsCheckmate(board, move))
            {
                // Console.Write("Checkmate!\n");
                return move;
            }

            //if (timer.MillisecondsRemaining <= 1) {
            //    return moveToPlay;
            //}
            if (move.IsCapture)
            {
                currentScore += pieceValues[(int)board.GetPiece(move.TargetSquare).PieceType];
                // Console.Write($"CAP:{currentScore}\t");
            }
            if (move.IsPromotion)
            {
                currentScore += pieceValues[(int)move.PromotionPieceType];
                // Console.Write($"PRO:{currentScore}\t");
            }


            if (board.SquareIsAttackedByOpponent(move.TargetSquare))
            {
                currentScore -= pieceValues[(int)move.MovePieceType];
                // Console.Write($"ATK:{currentScore}\t");
            }

            // Add points for enabling more moves next turn
            currentScore += DevelopScore(board, allMoves, move);

            // Subtract the value of whatever the opponent could attack if this move is made
            currentScore -= FindMoveLoss(board, move);

            if (currentScore > highestScore)
            {
                moveToPlay = move;
                highestScore = currentScore;
                // Console.Write($"TOP:{currentScore}\t");
            }
            // Console.Write("\n");
        }
        // Console.WriteLine($"CHOSE: {moveToPlay}\n");
        return moveToPlay;

    }
    
    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    // Get list of available moves after this move
    int DevelopScore(Board board, Move[] currentMoves, Move move)
    {
        int developedBefore = 0;
        foreach (Move available in currentMoves)
        {
            developedBefore += pieceValues[(int)available.MovePieceType];
        }
        board.MakeMove(move);
        board.ForceSkipTurn();
        int developedAfter = 0;
        Move[] newMoves = board.GetLegalMoves();
        foreach (Move nowAvailable in newMoves)
        {
            developedAfter += pieceValues[(int)nowAvailable.MovePieceType];
        }
        int score = developedAfter - developedBefore;
        score = score / (10 * currentMoves.Length);
        // Console.Write($"DEV:{score}\t");

        board.UndoSkipTurn();
        board.UndoMove(move);

        return score;

    }

    // Find biggest loss as a result of this move
    int FindMoveLoss(Board board, Move move)
    {
        board.MakeMove(move);
        Move[] opponentCaptures = board.GetLegalMoves(true);
        int largestLoss = 0;
        int currentLoss = 0;
        foreach (Move loss in opponentCaptures)
        {
            if(MoveIsCheckmate(board, loss))
            {
                largestLoss = pieceValues[6];
                break;
            }
             currentLoss = pieceValues[(int)board.GetPiece(loss.TargetSquare).PieceType];

            if (board.SquareIsAttackedByOpponent(loss.TargetSquare))
            {
                currentLoss -= pieceValues[(int)move.MovePieceType];
            }

            if ( currentLoss > largestLoss)
            {
                largestLoss = currentLoss;
            }
        }
        board.UndoMove(move);
        
        // Console.Write($"LOS:{largestLoss}\t");
        return largestLoss;
    }

}