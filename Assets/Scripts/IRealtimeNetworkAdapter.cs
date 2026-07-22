// IRealtimeNetworkAdapter.cs
using System;
using Passaparola.Multiplayer;

public interface IRealtimeNetworkAdapter
{
    string CurrentRoomId { get; }
    string LocalPlayerId { get; }
    void ConnectAndFindMatch(Action<string> onMatchFound);
    void SendMyAnswer(int questionIndex, AnswerStatus status);
    void ListenToOpponentAnswers(Action<int, AnswerStatus> onOpponentAnswered);
}