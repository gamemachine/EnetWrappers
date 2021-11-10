namespace EnetWrappers
{
    public enum EnetMessageType
    {
        Test = 1,
        Test2  = 2,
        GameMessage = 10,
        IpcMessage = 11,
        BoatIpcEnvelope = 12,
        PlayerPositionUpdate = 13,

        BoatCreate = 20,
        BoatDestroy = 21,
        BoatInfo = 22,
        BoatSetControl = 23,
        BoatTransform = 24,
        BoatInput = 25,

        AgentUpdate = 30,
        RemoveAgent = 31,
        SetAgentPosition = 32,

        AiPosition = 40
    }
}
