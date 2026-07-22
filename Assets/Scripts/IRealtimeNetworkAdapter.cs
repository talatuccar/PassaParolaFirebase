// IRealtimeNetworkAdapter.cs
using System;
using Passaparola.Multiplayer;

public interface IRealtimeNetworkAdapter
{
    void ConnectAndFindMatch(Action<string> onMatchFound);
    void SendMyAnswer(int questionIndex, AnswerStatus status);
    void ListenToOpponentAnswers(Action<int, AnswerStatus> onOpponentAnswered);
}