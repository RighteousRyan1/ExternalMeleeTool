using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Fighter;

namespace ExternalMeleeTool.SlpParser; 
public struct SlpFile {
    public string FileName;

    public TimeSpan GameDuration;
    public DateTime GameDate;
    public ExternalStageId StageId;

    // each player's respective data
    public string[] Username;
    public string[] ConnectCode;
    public float[] ELO;
    public Rank[] Rank;
    public FtKind[] Fighter;
}

public enum Rank : byte {
    Bronze1, Bronze2, Bronze3,
    Silver1, Silver2, Silver3,
    Gold1, Gold2, Gold3,
    Platinum1, Platinum2, Platinum3,
    Diamond1, Diamond2, Diamond3,
    Master1, Master2, Master3,
    Grandmaster
}