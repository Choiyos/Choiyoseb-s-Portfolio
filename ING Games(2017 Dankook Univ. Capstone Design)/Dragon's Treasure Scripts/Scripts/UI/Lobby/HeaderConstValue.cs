static class HeaderConstValue
{
    #region RoomData
    public const int RoomDat = 0;
    public const int RoomReqDat = 0;
    public const int RoomCreateDat = 1;
    public const int RoomJoinDat = 2;
    #endregion

    #region ClientData
    public const int ClientReadyDat = 0;
    public const int ClientLeaveDat = 0;
    public const int ClientCharacterDat = 1;
    public const int ChatDat = 1;
    public const int ClientListDat = 2;
    public const int ClientBoxDat = 3;
    public const int ClientStateDat = 3;
    #endregion

    #region SystemData
    public const int GameStartDat = 1;
    public const int NoticeDat = 2;
    public const int SystemDat = 4;
    public const int SystemEnd = 5;
    public const int SceneMoveCompleteDat = 6;
    #endregion

    #region GamePlayData
    public const int EndGameSuccess = 1;
    public const int ResultDat = 1;
    #endregion

    #region ControlData
    public const int ControlDat = 5;
    #endregion

    #region MainGameData
    public const int StatusDat = 2;
    public const int EndGameDat = 1;
    public const int ClientNameDat =0;
    #endregion

    #region MapData
    public const int HostDat = 0;
    public const int MapIndexDat = 1;
    public const int HostCheck = 7;
    public const int MapDat = 8;
    #endregion

    #region MapIndex
    public const int Maze = 0;
    public const int Temple = 1;
    #endregion

}
