using Enumerations;
using System.Collections.Generic;

[System.Serializable]
public class AchievementData
{
    public EAchievementID       AchievementID = EAchievementID.None;
    public EAchieveStateID      StateID = EAchieveStateID.None;
    public int                  MaxValue;
}
