// MultiplayerEnums.cs
namespace Passaparola.Multiplayer
{
    public enum AnswerStatus
    {
        None,       // Henüz yanýtlanmadý (Varsayýlan)
        Correct,    // Doðru (Yeþil)
        Wrong,      // Yanlýþ (Kýrmýzý)
        Passed      // Pas (Sarý)
    }

    public enum MatchState
    {
        WaitingForOpponent,
        Playing,
        Finished
    }
}